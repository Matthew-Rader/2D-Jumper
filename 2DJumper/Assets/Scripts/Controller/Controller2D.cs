using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D :	RaycastController
{
	float maxClimbAngle = 50f;
	float maxDescendAngle = 75f;
	bool grounded;

	public CollisionInfo collInfo;
	Vector2 playerInput;

	public override void Start()
	{
		// This runs the start method within the RaycastController
		base.Start();

		collInfo.movementDirection = 1;
	}

	public void Move (Vector2 movementDistance, bool standingOnPlatform) {
		Move(movementDistance, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector2 movementDistance, Vector2 input, bool standingOnPlatform = false)
	{
		playerInput = input;

		UpdateRaycastOrigins();
		collInfo.Reset();
		collInfo.movementDistanceOld = movementDistance;

		if (movementDistance.x != 0)
			collInfo.movementDirection = (int)Mathf.Sign(movementDistance.x);

		if (movementDistance.y < 0)
			DescendSlope(ref movementDistance);

		HorizontalCollisions(ref movementDistance);

		VerticalCollisions(ref movementDistance);

		transform.Translate(movementDistance);

		if (standingOnPlatform) {
			collInfo.below = true;
		}
	}

	void HorizontalCollisions (ref Vector2 movementDistance)
	{
		float directionX = collInfo.movementDirection;
		float rayLength = Mathf.Abs(movementDistance.x) + skinWidth;

		if (Mathf.Abs(movementDistance.x) < skinWidth)
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
						movementDistance = collInfo.movementDistanceOld;
					}

					float distanceToSlopeStart = 0;
					if (slopeAngle != collInfo.slopeAngleOld)
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						movementDistance.x -= distanceToSlopeStart * directionX;
					}

					ClimbSlope(ref movementDistance, slopeAngle);

					movementDistance.x += distanceToSlopeStart * directionX;
				}

				if (!collInfo.climbingSlope || slopeAngle > maxClimbAngle)
				{
					movementDistance.x = Mathf.Min(Mathf.Abs(movementDistance.x), (hit.distance - skinWidth)) * directionX;
					rayLength = Mathf.Min(Mathf.Abs(movementDistance.x) + skinWidth, hit.distance);

					if (collInfo.climbingSlope)
					{
						movementDistance.y = Mathf.Tan(collInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(movementDistance.x);
					}

					collInfo.left = directionX == -1;
					collInfo.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions (ref Vector2 movementDistance)
	{
		float directionY = Mathf.Sign(movementDistance.y);
		float rayLength = Mathf.Abs(movementDistance.y) + skinWidth;

		if (Mathf.Abs(movementDistance.y) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		float movementAmountY = 0.0f;
		bool rayHit = false;

		for (int i = 0; i < verticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + movementDistance.x);

			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (hit)
			{
				if (hit.collider.tag == "Passable Platform") {
					if (directionY == 1 || hit.distance == 0 || collInfo.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1 && playerInput.x == 0) {
						collInfo.fallingThroughPlatform = true;
						Invoke("ResetFallingThroughPlatform", 0.5f);
						continue;
					}
				}

				collInfo.above = directionY == 1;
				collInfo.below = directionY == -1;
				collInfo.grounded = true;
				collInfo.onEdge = (i == 0 || i == verticalRayCount - 1);

				movementAmountY = (hit.distance - skinWidth) * directionY;
				//movementDistance.y = (hit.distance - skinWidth) * directionY;

				rayLength = hit.distance;

				if (collInfo.climbingSlope) {
					movementDistance.x = movementDistance.y / Mathf.Tan(collInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(movementDistance.x);
				}

				rayHit = true;
			}
		}

		// If the ground was not detected then check if we are near a ledge.
		if (!rayHit) {
			LedgeDetection(ref movementDistance);
		}
		else {
			movementDistance.y = movementAmountY;
		}

		if (collInfo.climbingSlope)
		{
			float directionX = Mathf.Sign(movementDistance.x);
			rayLength = Mathf.Abs(movementDistance.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * movementDistance.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collInfo.slopeAngle)
				{
					movementDistance.x = (hit.distance - skinWidth) * directionX;
					collInfo.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope (ref Vector2 movementDistance, float slopeAngle)
	{
		float moveDistance = Mathf.Abs(movementDistance.x);
		float climbmovementDistanceY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (movementDistance.y <= climbmovementDistanceY)
		{
			movementDistance.y = climbmovementDistanceY;
			movementDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(movementDistance.x);
			collInfo.below = true;
			collInfo.climbingSlope = true;
			collInfo.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope (ref Vector2 movementDistance)
	{
		float directionX = Mathf.Sign(movementDistance.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
			{
				if (Mathf.Sign(hit.normal.x) == directionX)
				{
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(movementDistance.x))
					{
						float moveDistance = Mathf.Abs(movementDistance.x);

						float descendmovementDistanceY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						movementDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(movementDistance.x);
						movementDistance.y -= descendmovementDistanceY;

						collInfo.slopeAngle = slopeAngle;
						collInfo.descendingSlope = true;
						collInfo.below = true;
					}
				}

			}
		}
	}

	void LedgeDetection (ref Vector2 movementDistance) {
		if (!(movementDistance.y > 0) && !collInfo.left && !collInfo.right) {
			float directionX = Mathf.Sign(movementDistance.x);
			float rayLength = Mathf.Abs(movementDistance.x) + skinWidth;

			//for (int i = 1; i <= 2; ++i) {
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
				if (directionX > 0)
					rayOrigin -= Vector2.right * ((verticalRaySpacing * 0.7f) + movementDistance.x);
				else
					rayOrigin += Vector2.right * ((verticalRaySpacing * 0.7f) + movementDistance.x);

				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * -1, rayLength, collisionMask);

				Debug.DrawRay(rayOrigin, Vector2.up * -1 * 2, Color.blue);

				if (hit) {
					movementDistance.y = 0;
					collInfo.below = true;
					collInfo.grounded = true;
					collInfo.overEdge = true;
				}
			//}
		}
	}

	void ResetFallingThroughPlatform () {
		collInfo.fallingThroughPlatform = false;
	}

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 movementDistanceOld;
		public bool fallingThroughPlatform;

		public int movementDirection;
		public bool grounded;

		public bool onEdge;
		public bool overEdge;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
			onEdge = false;
			overEdge = false;
			grounded = false;
		}
	}
}
