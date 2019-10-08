using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * TODO:
 * - There is a stange bug that occurs sometimes when grabbing a wall the player will instanly 
 *   turn to the opposite direction when letting go.
 */

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
	// SERIALIZED PRIVATE
	[SerializeField] private GameManager gameManager;

	[Header("Movement")]
	[SerializeField] private float moveSpeed = 6.0f;
	[SerializeField] private float controllerDeadZone = 0.25f;
	[SerializeField] private float accelerationTimeAirborne = 0.1f;
	[SerializeField] private float accelerationTimeGrounded = 0.1f;

	[Header("Jump Parameters")]
	[SerializeField] private float maxJumpHeight = 4.0f;
	[SerializeField] private float minJumpHeight = 4.0f;
	[SerializeField] private float timeToJumpApex = 0.4f;
	[SerializeField] private Vector2 wallJumpAway;
	[SerializeField] private Vector2 wallJumpClimb;
	[SerializeField] private float wallJumpUp;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;

	[Header("Better Jumping Gravity Multiplier")]

	[Header("Wall-Slide Parameters")]
	[SerializeField] private float wallSlideSpeed = 3.0f;
	[SerializeField] private float wallClimbSpeed = 4.0f;
	[SerializeField] private float wallGrabStaminaMax = 3.0f;
	private float wallGrabStamina = 0.0f;
	private bool wallGrabDepleted = false;

	// PRIVATE VARIABLES
	private float moveX, moveY;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private float gravity;
	private Vector3 velocity;
	private float velocitySmoothing;
	private bool wallSliding;
	private Controller2D controller;
	private bool grabWall = false;
	private bool canMove = true;
	private int wallDirX;

	void Start() {
		controller = GetComponent<Controller2D>();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update() {
		GetMovementInput();

		wallDirX = (controller.collInfo.left) ? -1 : 1;

		if (Input.GetButtonDown("Jump"))
			HandleJump();

		if (Input.GetButtonUp("Jump")) {
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
			velocity.y += gravity * Time.deltaTime;

			float targetVelocityX = moveX * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocitySmoothing,
				(controller.collInfo.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

			HandleWallSliding();
		}

		controller.Move(velocity * Time.deltaTime, new Vector2(moveX, moveY));

		if (controller.collInfo.above || controller.collInfo.below)
			velocity.y = 0;

		// TODO: Remove once animations are made.
		GetComponent<SpriteRenderer>().flipX = (controller.collInfo.movementDirection == 1f) ? false : true;
	}

	private void GetMovementInput() {
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		moveX = input.x;
		moveY = input.y;
	}

	void HandleWallSliding() {
		wallSliding = false;

		if ((controller.collInfo.left && moveX == -1 || controller.collInfo.right && moveX == 1) &&
			!controller.collInfo.below && velocity.y < 0)
		{
			wallSliding = true;

			if (velocity.y < -wallSlideSpeed) {
				velocity.y = -wallSlideSpeed;
			}
		}
	}

	void HandleJump() {
		// Standrad Jump
		if (controller.collInfo.below) {
			velocity.y = maxJumpVelocity;
		}
		// Some sort of wall jump
		else if ((controller.collInfo.left || controller.collInfo.right) && !controller.collInfo.below) {
			// Jump vertically up wall
			if (grabWall && (moveX == 0 || (moveX < 0f && controller.collInfo.left) || (moveX > 0f && controller.collInfo.right))) {
				Debug.Log(moveX);
				velocity.x = 0;
				velocity.y = wallJumpUp;
			}
			else {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				velocity.x = -wallDirX * wallJumpAway.x;
				velocity.y = wallJumpAway.y;
			}
		}
	}

	private void HandleWallGrab() {
		bool grabWallInput = Input.GetAxis("LT") == 0 ? false : true;

		if (grabWallInput && (controller.collInfo.left || controller.collInfo.right) && canMove) {
			if (!controller.collInfo.below) {
				wallGrabStamina += Time.deltaTime;
			}
			else if (controller.collInfo.below) {
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
		}
	}

	IEnumerator DisableMovementWallJumpOff(float time) {
		canMove = false;
		yield return new WaitForSeconds(time);
		canMove = true;
	}
}
