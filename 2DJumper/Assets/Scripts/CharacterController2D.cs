using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterController2D : MonoBehaviour
{
	private Collision playerColl;
	private Rigidbody2D characterRigi;
	private bool facingRight = true;
	private Vector3 velocity = Vector3.zero;

	[Header("Jump Parameters")]
	[SerializeField] private float jumpForce = 15f;
	[SerializeField] private float wallJumpForce = 13f;
	[SerializeField] private float wallJumpVerticalControlDelay = 0.1f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[Tooltip("0 will result in a vertical jump and 1 ~45*")]
	[Range (0, 1)] [SerializeField] private float wallJumpAwayAngleModifier = 0.5f;

	[Space] 

	[Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f; // How much to smooth out the movement

	[Header("Better Jumping Gravity Multiplier")]
	[Space]
	[SerializeField] private float fallMultiplier = 3.5f;
	[SerializeField] private float lowJumpMultiplier = 5f;

	[Header("Wall Mechanics")]
	[Space]
	[SerializeField] private float slideRate = 2f;
	private bool canMove = true;

	[SerializeField] private float controllerDeadZone = 0.25f;
	[SerializeField] private float runSpeed = 40f;
	float moveX;
	float moveY;
	bool jumping = false;
	bool grabWall = false;
	bool applyWallSlide = true;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Awake()
	{
		characterRigi = GetComponent<Rigidbody2D>();
		playerColl = GetComponent<Collision>();
	}

	void Update()
	{
		Vector2 stickInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		if (stickInput.magnitude < controllerDeadZone)
			stickInput = Vector2.zero;

		moveX = stickInput.x * runSpeed;
		moveY = stickInput.y * runSpeed;

		if (Input.GetButtonDown("Jump"))
			jumping = true;

		grabWall = Input.GetAxis("LT") == 0 ? false : true;
	}

	void FixedUpdate()
	{
		//Move(moveX * Time.fixedDeltaTime, jumping, grabWall);
		Move(moveX, moveY, jumping, grabWall);
		jumping = false;
	}

	//public void Move(float move, bool jump, bool grabWall)
	public void Move (float x, float y, bool jump, bool grabWall)
	{
		//Debug.Log("On Wall: " + playerColl.onWall + " GrabWall: " + grabWall);
		if (playerColl.onWall && grabWall && !playerColl.onGround && canMove)
		{
			//characterRigi.velocity = new Vector2(0f, 0f);
			characterRigi.velocity = new Vector2(0f, (y * 10f * Time.fixedDeltaTime));
		}
		else
		{
			//only control the player if grounded or airControl is turned on
			if (canMove)
			{
				// Move the character by finding the target velocity
				Vector3 targetVelocity = new Vector2(x * 10f * Time.fixedDeltaTime, characterRigi.velocity.y);

				// And then smoothing it out and applying it to the character
				characterRigi.velocity = Vector3.SmoothDamp(characterRigi.velocity, targetVelocity, ref velocity, movementSmoothing);
			}

			if (!playerColl.onGround && playerColl.onWall && (x != 0f))
			{
				WallSlide();
			}
		}

		HandlePlayerSpriteFlip(x, y);

		// If the player should jump...
		if (jump)
		{
			HandleJump();
		}

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
		else if (grabWall && playerColl.onWall && !playerColl.onGround)
		{
			characterRigi.gravityScale = 0f;
		}
		else
		{
			characterRigi.gravityScale = 1f;
			applyWallSlide = false;
		}
	}

	private void HandleJump()
	{
		if (playerColl.onGround)
		{
			// Add a vertical force to the player.
			playerColl.onGround = false;
			characterRigi.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
		}
		else if (playerColl.onWall && !playerColl.onGround) // Some sort of wall jump
		{
			// Vertical jump up a wall
			if (grabWall && (moveX == 0 || (moveX < 0f && playerColl.onWallLeft) || (moveX > 0f && playerColl.onWallRight)))
			{
				StopCoroutine(DisableMovementWallJumpUp(0));
				StartCoroutine(DisableMovementWallJumpUp(wallJumpVerticalControlDelay));

				DoJump(Vector2.up, true);
			}
			// Jump away from a wall
			else
			{
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				Vector2 wallDir = playerColl.onWallRight ? Vector2.left : Vector2.right; //wallSide is -1 for left and 1 for right

				DoJump((Vector2.up + wallDir * wallJumpAwayAngleModifier), true);
			}
		}
	}

	private void DoJump(Vector2 dir, bool offWall)
	{
		characterRigi.velocity = new Vector2(characterRigi.velocity.x, 0);
		characterRigi.velocity += dir * wallJumpForce;
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
}
