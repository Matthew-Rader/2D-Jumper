﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

/* 
 * TODO:
 * - There is a stange bug that occurs sometimes when grabbing a wall the player will instanly 
 *   turn to the opposite direction when letting go.
 * - When detecting collision with spikes (if I even need to do this) detect a hit only with
 *   hit.distance == 0.
 */

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
	// SERIALIZED PRIVATE
	[SerializeField] private GameManager gameManager;

	[Header("Movement")]
	[SerializeField] private float moveSpeed = 9.75f;
	[SerializeField] private float accelerationTimeAirborne = 0.1f;
	[SerializeField] private float decelerationTimeAirborne = 0.1f;
	[SerializeField] private float accelerationTimeGrounded = 0.15f;
	[SerializeField] private float decelerationTimeGrounded = 0.05f;

	[Header("Jump Parameters")]
	[SerializeField] private float maxJumpHeight = 2.25f;
	[SerializeField] private float minJumpHeight = 1.0f;
	[SerializeField] private float timeToJumpApex = 0.25f;
	[SerializeField] private Vector2 wallJumpAway = new Vector2(50.0f, 10.0f);
	[SerializeField] private Vector2 wallJumpUp = new Vector2(16.0f, 20.0f);
	//[SerializeField] private Vector2 wallJumpClimb;
	[SerializeField] private float wallGrabJump = 15.0f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[SerializeField] private float jumpQueueTimer = 0.15f;
	[SerializeField] private float coyoteJumpTimer = 1.0f;

	[Header("Better Jumping Gravity Multiplier")]
	[SerializeField] private float gravityFallMultiplier = 10;

	[Header("Wall-Slide Parameters")]
	[SerializeField] private float wallSlideSpeed = 1.5f;
	[SerializeField] private float wallClimbSpeed = 4.5f;
	[SerializeField] private float wallGrabStaminaMax = 5.0f;
	private float wallGrabStamina = 0.0f;
	private bool wallGrabDepleted = false;

	[Header("Game Events")]
	// Should call DeathFade.StartDeathFadeCoroutine()
	public UnityEvent touchedHazard;
	public UnityEvent onGroundJump;
	public UnityEvent onWallGrabJump;
	public UnityEvent onWallJumpAway;
	public UnityEvent onWallJumpUp;
	public UnityEvent onMove;
	public UnityEvent onPlayerReset;
	public UnityEvent onLandingOnGround;

	// PRIVATE VARIABLES
	private float moveX, moveY;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private float gravity;
	private Vector2 velocity;
	private float velocitySmoothing;
	private bool wallSliding;
	private Controller2D controller;
	private DeathFade deathFade;
	private CameraEffects camEffects;
	private bool grabWall = false;
	private bool canMove = true;
	private int wallDirX;
	private bool onLeftWall, onRightWall;
	private float timeToWallUnstick;
	private float wallStickTime = 0.15f;
	private bool jumpInputDown;
	private bool jumpInputUp;
	private bool jumping;
	private bool applyJumpQueue;
	private bool canCoyoteJump;
	private bool grounded;

	void Start() {
		controller = GetComponent<Controller2D>();
		deathFade = GetComponent<DeathFade>();
		camEffects = GetComponent<CameraEffects>();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

		jumping = onLeftWall = onRightWall = applyJumpQueue = false;

		transform.position = gameManager.currentReSpawnPoint.transform.position;
    }

	void Update () {
		GetInput();

		// Check if the player is grounded based on collision info from last frame
		CheckForGrounded();

		wallDirX = (controller.collInfo.left) ? -1 : 1;

		DetermineIfCanCoyoteJump();

		if (jumpInputDown ||
			(applyJumpQueue && (grounded || ((controller.collInfo.left || controller.collInfo.right) && !grounded))))
		{
			HandleJump();
		}

		if (jumpInputUp) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

		HandleWallGrab();

		if (grabWall && !wallGrabDepleted) {
			float targetVelocityY = moveY * wallClimbSpeed;
			velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocitySmoothing, 0.05f);

			velocity.x = 0;
		}
		else {
			DetermineVelocityY();

			DetermineVelocityX();

			HandleWallSliding();

			HandleStickyWallDelay();
		}

		// Move the player
		controller.Move(velocity * Time.deltaTime, new Vector2(moveX, moveY));

		CheckForCollisions();

		// TODO: Remove once animations are made.
		GetComponent<SpriteRenderer>().flipX = (controller.collInfo.movementDirection == 1f) ? false : true;
	}

	private void GetInput() {
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		moveX = input.x;
		moveY = input.y;

		jumpInputDown = (Input.GetButtonDown("Jump")) ? true : false;
		jumpInputUp = (Input.GetButtonUp("Jump")) ? true : false;
	}

	private void CheckForGrounded () {
		if (!grounded && controller.collInfo.below) {
			onLandingOnGround.Invoke();
		}

		grounded = controller.collInfo.below;
	}

	private void DetermineIfCanCoyoteJump () {
		if (!grounded && !onLeftWall && !onRightWall && !jumping && velocity.y < 0f &&
			(Time.time <= (controller.collInfo.timeLeftGround + coyoteJumpTimer))) {
			canCoyoteJump = true;
		}
		else {
			canCoyoteJump = false;
		}
	}

	private void HandleJump() {
		// Standrad Jump
		if (grounded) {
			velocity.y = maxJumpVelocity;
			jumping = true;

			onGroundJump.Invoke();
		}
		else if (canCoyoteJump) {
			// Debug.Log("coyoteJump");

			velocity.y = maxJumpVelocity;
			jumping = true;

			onGroundJump.Invoke();
		}
		// Some sort of wall jump
		else if ((controller.collInfo.left || controller.collInfo.right) && !grounded) {
			// Jump vertically up wall
			if (grabWall && (moveX == 0 || (moveX < 0f && controller.collInfo.left) || (moveX > 0f && controller.collInfo.right))) {
				velocity.x = 0;
				velocity.y = wallGrabJump;
				jumping = true;

				onWallGrabJump.Invoke();
			}
			// Jump up a wall
			else if ((wallDirX == -1 && moveX < 0f) || (wallDirX == 1 && moveX > 0f)) {
				// Debug.Log("Jump up a wall");

				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				velocity.x = -wallDirX * wallJumpUp.x;
				velocity.y = wallJumpUp.y;
				jumping = true;

				onWallJumpUp.Invoke();
			}
			// Jump away from a wall
			else {
				// Debug.Log(" Jump away from a wall");

				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				velocity.x = -wallDirX * wallJumpAway.x;
				velocity.y = wallJumpAway.y;
				jumping = true;

				onWallJumpAway.Invoke();
			}
		}
		else if (!applyJumpQueue) {
			StartCoroutine(JumpQueueTimer(jumpQueueTimer));
		}
	}

	private void HandleWallGrab() {
		bool grabWallInput = Input.GetAxis("LT") == 0 ? false : true;

		if (grabWallInput && (controller.collInfo.left || controller.collInfo.right) && canMove) {
			if (!grounded) {
				wallGrabStamina += Time.deltaTime;
			}
			else if (grounded) {
				wallGrabStamina = 0.0f;
			}

			if (wallGrabStamina >= wallGrabStaminaMax) {
				grabWall = false;
				//GetComponent<SpriteRenderer>().color = new Color(70, 70, 70);
			}
			else {
				grabWall = true;
				//GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
			}
		}
		else {
			grabWall = false;
			wallGrabStamina = 0.0f;
		}
	}

	private void DetermineVelocityY () {
		if (velocity.y < 0) {
			velocity.y += (gravity - gravityFallMultiplier) * Time.deltaTime;
		}
		else if (jumping || !grounded) {
			velocity.y += gravity * Time.deltaTime;
		}
		else if (grounded) {
			velocity.y = 0;
		}
	}

	private void DetermineVelocityX () {
		float targetVelocityX = moveX * moveSpeed;
		if (targetVelocityX != 0) {
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocitySmoothing,
				(grounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
		}
		else {
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocitySmoothing,
				(grounded) ? decelerationTimeGrounded : decelerationTimeAirborne);
		}
	}

	private void HandleWallSliding () {
		wallSliding = false;
		onLeftWall = onRightWall = false;

		if ((controller.collInfo.left && moveX == -1 || controller.collInfo.right && moveX == 1) &&
			!grounded && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeed) {
				velocity.y = -wallSlideSpeed;
			}

			if (controller.collInfo.left) {
				onLeftWall = true;
				onRightWall = false;
			}
			else if (controller.collInfo.right) {
				onRightWall = true;
				onLeftWall = false;
			}
		}
	}

	private void HandleStickyWallDelay () {
		if ((controller.collInfo.right || controller.collInfo.left) && !grounded && velocity.y < 0) {
			if (timeToWallUnstick > 0) {
				velocitySmoothing = 0;
				velocity.x = 0;

				if (moveX != wallDirX && moveX != 0) {
					timeToWallUnstick -= Time.deltaTime;

					// Wall slide rate needs to be applied so the player doesn't fall at the rate of gravity
					if (velocity.y < -wallSlideSpeed) {
						velocity.y = -wallSlideSpeed;
					}
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
		}
		else {
			timeToWallUnstick = wallStickTime;
		}
	}

	private void CheckForCollisions () {
		if (controller.collInfo.above || controller.collInfo.below) {
			velocity.y = 0;
		}

		if (controller.collInfo.above || controller.collInfo.below ||
			controller.collInfo.left || controller.collInfo.right) {
			jumping = false;
		}

		if (controller.collInfo.touchedHazard) {
			touchedHazard.Invoke();
		}

		if (controller.collInfo.hitJumpPlatform) {
			HandleJumpPlatform();
		}
	}

	private void HandleJumpPlatform () {
		velocity.y = maxJumpVelocity * 1.5f;
		jumping = true;
		camEffects.ApplyCameraShake();
	}

	IEnumerator DisableMovementWallJumpOff(float time) {
		canMove = false;
		yield return new WaitForSeconds(time);
		canMove = true;
	}

	IEnumerator JumpQueueTimer (float time) {
		applyJumpQueue = true;
		yield return new WaitForSeconds(time);
		applyJumpQueue = false;
	}

	public void ResetPlayer () {
		jumping = onLeftWall = onRightWall = false;
		controller.collInfo.below = false;
		velocity = Vector2.zero;

		onPlayerReset.Invoke();
	}

}
