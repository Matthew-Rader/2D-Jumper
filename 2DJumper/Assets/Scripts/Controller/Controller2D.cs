using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D :	RaycastController
{
	// Formerly used slope detection variables
	//float maxClimbAngle = 50f;
	//float maxDescendAngle = 75f;

	[SerializeField] private LayerMask hazardCollisionMask;
	[SerializeField] private bool allowPassablePlatforms = false;
	public CollisionInfo collInfo;
	Vector2 playerInput;

	struct ClosestRayHit {
		public float distance;
		public string colliderTag;

		public ClosestRayHit (float rayLength) {
			distance = rayLength + 1;
			colliderTag = "";
		}
	}

	public override void Start()
	{
		// This runs the start method within the RaycastController
		base.Start();

		collInfo.movementDirection = 1;
		collInfo.belowLastFrame = false;
		collInfo.coyoteJumpPossible = false;
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

		if (movementDistance.x != 0) {
			collInfo.movementDirection = (int)Mathf.Sign(movementDistance.x);
		}

		HorizontalCollisions(ref movementDistance);

		if (!collInfo.touchedHazard) {
			VerticalCollisions(ref movementDistance);
		}

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

		bool terrainDetected, hazardDetected;
		terrainDetected = hazardDetected = false;

		ClosestRayHit closestHazardHit = new ClosestRayHit();
		ClosestRayHit closestTerrainHit = new ClosestRayHit();

		// Cast out rays and determine the closest piece of terrain and / or a hazard
		for (int i = 0; i < horizontalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			RaycastHit2D terrainHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			RaycastHit2D hazardHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, hazardCollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (terrainHit) {
				if (terrainDetected) {
					if ((int)(terrainHit.distance * 1000) < (int)(closestTerrainHit.distance * 1000)) {
						closestTerrainHit.colliderTag = terrainHit.collider.tag;
						closestTerrainHit.distance = terrainHit.distance;
					}

				}
				else {
					terrainDetected = true;
					closestTerrainHit.colliderTag = terrainHit.collider.tag;
					closestTerrainHit.distance = terrainHit.distance;
				}
			}

			if (hazardHit) {
				if (hazardDetected) {
					if ((int)(hazardHit.distance * 1000) < (int)(closestHazardHit.distance * 1000)) {
						closestTerrainHit.colliderTag = hazardHit.collider.tag;
						closestTerrainHit.distance = hazardHit.distance;
					}

				}
				else {
					hazardDetected = true;
					closestHazardHit.colliderTag = hazardHit.collider.tag;
					closestHazardHit.distance = hazardHit.distance;
				}
			}
		}

		if (hazardDetected) {
			if (closestHazardHit.distance == 0) {
				collInfo.touchedHazard = true;
				movementDistance = Vector2.zero;
				return;
			}
		}

		if (terrainDetected) {
			collInfo.left = directionX == -1;
			collInfo.right = directionX == 1;

			if (closestTerrainHit.colliderTag == "JumpPlatform") {
				if (closestTerrainHit.distance == 0) {
					collInfo.hitJumpPlatform = true;
				}
				return;
			}

			// If the character is inside another collider
			if (closestTerrainHit.distance == 0) {
				return;
			}

			movementDistance.x = Mathf.Min(Mathf.Abs(movementDistance.x), (closestTerrainHit.distance - skinWidth)) * directionX;
		}
	}

	void VerticalCollisions (ref Vector2 movementDistance)
	{
		float directionY = (movementDistance.y > 0) ? 1.0f : -1.0f;
		float rayLength = Mathf.Abs(movementDistance.y) + skinWidth;

		if (Mathf.Abs(movementDistance.y) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		bool terrainDetected = false;
		bool hazardDetected = false;

		ClosestRayHit closestHazardHit = new ClosestRayHit();
		ClosestRayHit closestTerrainHit = new ClosestRayHit();

		// Cast out rays and determine the closest piece of terrain and / or a hazard
		for (int i = 0; i < verticalRayCount; ++i) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + movementDistance.x);

			RaycastHit2D terrainHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			RaycastHit2D hazardHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, hazardCollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (terrainHit) {
				if (terrainDetected) {
					if ((int)(terrainHit.distance * 1000) < (int)(closestTerrainHit.distance * 1000)) {
						closestTerrainHit.colliderTag = terrainHit.collider.tag;
						closestTerrainHit.distance = terrainHit.distance;
					}

				}
				else {
					terrainDetected = true;
					closestTerrainHit.colliderTag = terrainHit.collider.tag;
					closestTerrainHit.distance = terrainHit.distance;
				}
			}

			if (hazardHit) {
				if (hazardDetected) {
					if ((int)(hazardHit.distance * 1000) < (int)(closestHazardHit.distance * 1000)) {
						closestTerrainHit.colliderTag = hazardHit.collider.tag;
						closestTerrainHit.distance = hazardHit.distance;
					}

				}
				else {
					hazardDetected = true;
					closestHazardHit.colliderTag = hazardHit.collider.tag;
					closestHazardHit.distance = hazardHit.distance;
				}
			}
		}

		if (hazardDetected) {
			if (closestHazardHit.distance == 0) {
				collInfo.touchedHazard = true;
				movementDistance = Vector2.zero;
				return;
			}
		}

		if (terrainDetected) {
			// Check for passable platforms
			if (closestTerrainHit.colliderTag == "Passable Platform" && allowPassablePlatforms) {
				if (directionY == 1 || closestTerrainHit.distance == 0 || collInfo.fallingThroughPlatform) {
					return;
				}

				if (playerInput.y == -1 && playerInput.x == 0) {
					collInfo.fallingThroughPlatform = true;
					Invoke("ResetFallingThroughPlatform", 0.5f);
					return;
				}
			}

			collInfo.above = directionY == 1;
			collInfo.below = directionY == -1;
			collInfo.belowLastFrame = collInfo.below ? true : false;

			if (closestTerrainHit.colliderTag == "JumpPlatform") {
				collInfo.hitJumpPlatform = true;
				return;
			}

			movementDistance.y = (closestTerrainHit.distance - skinWidth) * directionY;
		}
		else if (collInfo.belowLastFrame && !collInfo.below) {
			collInfo.belowLastFrame = false;
			collInfo.timeLeftGround = Time.time;
		}
		//else {
		//	LedgeDetection(ref movementDistance);
		//}
	}

	void LedgeDetection (ref Vector2 movementDistance) {
		if (!(movementDistance.y > 0) && !collInfo.left && !collInfo.right) {
			float directionX = Mathf.Sign(movementDistance.x);
			float rayLength = Mathf.Abs(movementDistance.x) + skinWidth;

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
				collInfo.overEdge = true;
			}
		}
	}

	void ResetFallingThroughPlatform () {
		collInfo.fallingThroughPlatform = false;
	}

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;
		public bool belowLastFrame;
		public bool coyoteJumpPossible;
		public float timeLeftGround;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 movementDistanceOld;
		public bool fallingThroughPlatform;

		public int movementDirection;

		public bool onEdge;
		public bool overEdge;

		public bool touchedHazard;

		public bool hitJumpPlatform;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
			onEdge = false;
			overEdge = false;
			touchedHazard = false;
			hitJumpPlatform = false;
		}
	}
}
