using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D :	RaycastController
{
	float maxClimbAngle = 50f;
	float maxDescendAngle = 75f; 

	public CollisionInfo collInfo;
	Vector2 playerInput;

	public override void Start()
	{
		// This runs the start method within the RaycastController
		base.Start();

		collInfo.movementDirection = 1;
	}

	public void Move (Vector3 velocity, bool standingOnPlatform) {
		Move(velocity, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
	{
		playerInput = input;

		UpdateRaycastOrigins();
		collInfo.Reset();
		collInfo.velocityOld = velocity;

		if (velocity.x != 0)
			collInfo.movementDirection = (int)Mathf.Sign(velocity.x);

		if (velocity.y < 0)
			DescendSlope(ref velocity);

		HorizontalCollisions(ref velocity);

		if (velocity.y != 0)
			VerticalCollisions(ref velocity);

		transform.Translate(velocity);

		if (standingOnPlatform)
			collInfo.below = true;
	}

	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = collInfo.movementDirection;
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth)
		{
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit)
			{
				// If the character is inside another collider
				if (hit.distance == 0)
				{
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle)
				{
					if (collInfo.descendingSlope)
					{
						collInfo.descendingSlope = false;
						velocity = collInfo.velocityOld;
					}

					float distanceToSlopeStart = 0;
					if (slopeAngle != collInfo.slopeAngleOld)
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}

					ClimbSlope(ref velocity, slopeAngle);

					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!collInfo.climbingSlope || slopeAngle > maxClimbAngle)
				{
					velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
					rayLength = Mathf.Min(Mathf.Abs(velocity.x) + skinWidth, hit.distance);

					if (collInfo.climbingSlope)
					{
						velocity.y = Mathf.Tan(collInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collInfo.left = directionX == -1;
					collInfo.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity)
	{
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (hit)
			{
				if (hit.collider.tag == "Passable Platform") {
					if (directionY == 1 || hit.distance == 0 || (playerInput.y == -1 && playerInput.x == 0)) {
						continue;
					}
				}

				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collInfo.climbingSlope)
				{
					velocity.x = velocity.y / Mathf.Tan(collInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				collInfo.above = directionY == 1;
				collInfo.below = directionY == -1;
			}
		}

		if (collInfo.climbingSlope)
		{
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collInfo.slopeAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					collInfo.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY)
		{
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collInfo.below = true;
			collInfo.climbingSlope = true;
			collInfo.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign(velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
			{
				if (Mathf.Sign(hit.normal.x) == directionX)
				{
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
					{
						float moveDistance = Mathf.Abs(velocity.x);

						float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
						velocity.y -= descendVelocityY;

						collInfo.slopeAngle = slopeAngle;
						collInfo.descendingSlope = true;
						collInfo.below = true;
					}
				}

			}
		}
	}

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;

		public int movementDirection;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
