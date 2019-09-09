using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterController2D : MonoBehaviour
{
	// PRIVATE ----------------------------------
	private Collision playerColl;
	private Rigidbody2D characterRigi;
	private bool facingRight = true;
	private Vector3 velocity = Vector3.zero;
	private float moveX;
	private float moveY;
	private bool jumpInput = false;
	private bool jumping = false;
	private bool grabWall = false;
	private bool applyWallSlide = true;
	private bool canMove = true;

	// SERIALIZED PRIVATE -----------------------
	[Header("Movement")]
	[SerializeField] private float runSpeed = 40f;
	[Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;
	[SerializeField] private float controllerDeadZone = 0.25f;

	[Header("Jump Parameters")]
	[SerializeField] private float standardJumpForce = 15f;
	[SerializeField] private float wallJumpForce = 13f;
	[SerializeField] private float wallJumpVerticalControlDelay = 0.1f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[Tooltip("0 will result in a vertical jump and 1 ~45*")]
	[Range (0, 1)] [SerializeField] private float wallJumpAwayAngleModifier = 0.5f;
	[SerializeField] private float jumpOffLedgeDelay = 0.5f;
	private float jumpOffLedgeCounter = 0.0f;
	private bool canLedgeDelayJump = false; 

	[Header("Better Jumping Gravity Multiplier")]
	[SerializeField] private float fallMultiplier = 3.5f;
	[SerializeField] private float lowJumpMultiplier = 5f;

	[Header("Wall Mechanics")]
	[SerializeField] private float slideRate = 2f;
	[SerializeField] private float wallGrabStaminaMax = 3.0f;
	private float wallGrabStamina = 0.0f;
	private bool wallGrabDepleted = false;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Awake()
	{
		characterRigi = GetComponent<Rigidbody2D>();
		playerColl = GetComponent<Collision>();
	}

	void Update()
	{
		GetMovementInput();

		JumpOffLedgeDelay();

		if (Input.GetButtonDown("Jump"))
			jumpInput = true;

		grabWall = Input.GetAxis("LT") == 0 ? false : true;

		WallGrabStamina();
	}

	void FixedUpdate()
	{
		Move(moveX, moveY, jumpInput, grabWall);
		jumpInput = false;
	}

	public void Move (float x, float y, bool jump, bool grabWall)
	{
		// If the player is grounded and no longer moving the character null the player x velocity
		if (playerColl.onGround && x == 0.0f)
		{
			characterRigi.velocity = new Vector2(0.0f, characterRigi.velocity.y);
		}
		// If the player is grabbing a wall then lock x movement and allow them to move up or down the wall
		else if (playerColl.onWall && grabWall && !playerColl.onGround && canMove && !wallGrabDepleted)
		{
			//characterRigi.velocity = new Vector2(0f, 0f);
			characterRigi.velocity = new Vector2(0f, (y * 10f * Time.fixedDeltaTime));
		}
		else
		{
			if (canMove)
			{
				// Move the character by finding the target velocity
				Vector3 targetVelocity = new Vector2(x * 10f * Time.fixedDeltaTime, characterRigi.velocity.y);

				// And then smoothing it out and applying it to the character
				characterRigi.velocity = Vector3.SmoothDamp(characterRigi.velocity, targetVelocity, ref velocity, movementSmoothing);
			}

			if (!playerColl.onGround && playerColl.onWall && (x != 0f))
				WallSlide();
		}

		HandlePlayerSpriteFlip(x, y);

		// If the player should jump
		if (jump)
			HandleJump();

		ApplyGravityScale(grabWall);
	}

	// Affect gravity scale for better jumping
	private void ApplyGravityScale(bool grabWall)
	{
		if (characterRigi.velocity.y < 0)
		{
			characterRigi.gravityScale = fallMultiplier;
			applyWallSlide = true;
		}
		else if (characterRigi.velocity.y > 0)
		{
			characterRigi.gravityScale = lowJumpMultiplier;
			applyWallSlide = false;
		}
		else if (grabWall && playerColl.onWall && !playerColl.onGround && !wallGrabDepleted)
		{
			characterRigi.gravityScale = 0f;
		}
		else
		{
			characterRigi.gravityScale = 1f;
			applyWallSlide = false;
		}
	}

	// Determines which type of jump that needs to be performed
	private void HandleJump()
	{
		if (playerColl.onGround)
		{
			//Debug.Log("Standard Jump");
			playerColl.onGround = false;
			DoJump(Vector2.up, standardJumpForce);
		}
		else if (!playerColl.onGround && canLedgeDelayJump)
		{
			//Debug.Log("Delay Jump");
			characterRigi.velocity = new Vector2(characterRigi.velocity.x, 0.0f);
			DoJump(Vector2.up, standardJumpForce);
		}
		else if (playerColl.onWall && !playerColl.onGround) // Some sort of wall jump
		{
			// Vertical jump up a wall
			if (grabWall && !wallGrabDepleted && (moveX == 0 || (moveX < 0f && playerColl.onWallLeft) || (moveX > 0f && playerColl.onWallRight)))
			{
				//Debug.Log("Vertical Jump");
				StopCoroutine(DisableMovementWallJumpUp(0));
				StartCoroutine(DisableMovementWallJumpUp(wallJumpVerticalControlDelay));

				DoJump(Vector2.up, wallJumpForce);
			}
			// Jump away from a wall
			else
			{
				//Debug.Log("Wall Jump");
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				//wallSide is -1 for left and 1 for right
				Vector2 wallDir = playerColl.onWallRight ? Vector2.left : Vector2.right; 
				DoJump((Vector2.up + wallDir * wallJumpAwayAngleModifier), wallJumpForce);
			}
		}
	}

	private void DoJump(Vector2 dir, float jumpForce)
	{
		characterRigi.velocity = new Vector2(characterRigi.velocity.x, 0);
		characterRigi.velocity += dir * jumpForce;
		jumping = true;
	}

	IEnumerator DisableMovementWallJumpOff(float time)
	{
		canMove = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		canMove = true;
	}

	IEnumerator DisableMovementWallJumpUp(float time)
	{
		canMove = false;
		grabWall = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		grabWall = true;
		canMove = true;
	}

	private void HandlePlayerSpriteFlip(float x, float y)
	{
		// If the input is moving the player right and the player is facing left...
		if (x > 0 && !facingRight)
		{
			// ... flip the player.
			Flip();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (x < 0 && facingRight)
		{
			// ... flip the player.
			Flip();
		}
	}

	// Flips the player sprite
	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	private void WallSlide()
	{
		if (applyWallSlide)
		{
			characterRigi.velocity = new Vector2(characterRigi.velocity.x, -slideRate);
		}
	}

	private void GetMovementInput()
	{
		Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		if (movementInput.magnitude < controllerDeadZone)
			movementInput = Vector2.zero;

		moveX = movementInput.x * runSpeed;
		moveY = movementInput.y * runSpeed;
	}

	private void JumpOffLedgeDelay()
	{
		if (playerColl.onGround)
		{
			jumpOffLedgeCounter = 0.0f;
			canLedgeDelayJump = true;
			jumping = false;
		}
		else if (!playerColl.onGround && !playerColl.onWall && !jumping && (jumpOffLedgeCounter < jumpOffLedgeDelay))
		{
			jumpOffLedgeCounter += Time.deltaTime;
		}
		else
		{
			canLedgeDelayJump = false;
		}
	}

	private void WallGrabStamina()
	{
		if (playerColl.onWall && grabWall && !playerColl.onGround)
			wallGrabStamina += Time.deltaTime;
		else if (playerColl.onGround)
			wallGrabStamina = 0.0f;

		if (wallGrabStamina >= wallGrabStaminaMax)
		{
			wallGrabDepleted = true;
			GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
		}
		else
		{
			wallGrabDepleted = false;
			GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
		}
	}
	}
