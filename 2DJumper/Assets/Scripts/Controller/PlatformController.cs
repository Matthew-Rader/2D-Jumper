using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
	[SerializeField] private LayerMask passengerMask;

	[Header("Platform Path Variables")] // -----------
	[SerializeField] private Vector3[] localWaypoints;
	[SerializeField] private float speed;
	[SerializeField] private bool cyclicPath;
	[SerializeField] private float waitTimeBetweenWaypoints;
	[SerializeField] [Range(1, 3)] private float easeAmount;
	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;
	// Store the localWaypoints in global space
	Vector3[] globalWaypoints;

	[SerializeField] private GameObject pathLineRenderer;

	// PRIVATE VARIABLES -----------------------------
	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
		base.Start();

		globalWaypoints = new Vector3[localWaypoints.Length];

		for (int i = 0; i < localWaypoints.Length; ++i)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		if (localWaypoints != null) {
			float size = 0.3f;

			LineRenderer newLine = Instantiate(pathLineRenderer, transform).GetComponent<LineRenderer>(); ;
			newLine.positionCount = 1;

			for (int i = 0; i < localWaypoints.Length; ++i) {
				Vector3 globalWaypointPositionA = (Application.isPlaying) ?
					globalWaypoints[i] : localWaypoints[i] + transform.position;

				int nextWaypoint = (i + 1) % localWaypoints.Length;
				Vector3 globalWaypointPositionB = (Application.isPlaying) ?
					globalWaypoints[nextWaypoint] : localWaypoints[nextWaypoint] + transform.position;

				if ((i < localWaypoints.Length - 1) || (i == localWaypoints.Length - 1 && cyclicPath)) {
					newLine.positionCount += 1;
					newLine.SetPosition(i, globalWaypointPositionA);
					newLine.SetPosition(i + 1, globalWaypointPositionB);
				}
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
		UpdateRaycastOrigins();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);

		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
    }

	// This function is an implementation of the equation y = (x^a) / (x^a + (1-x)^a)
	float Ease(float x)
	{
		float a = easeAmount;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow((1 - x), a));
	}

	Vector3 CalculatePlatformMovement()
	{
		if (Time.time < nextMoveTime)
		{
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length; ;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1)
		{
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;

			if (!cyclicPath)
			{
				if (fromWaypointIndex >= globalWaypoints.Length - 1)
				{
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints);
				}
			}
			nextMoveTime = Time.time + waitTimeBetweenWaypoints;
		}

		return newPos - transform.position;
	}

	void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in passengerMovement)
		{
			if (!passengerDictionary.ContainsKey(passenger.transform))
			{
				passengerDictionary.Add(passenger.transform , passenger.transform.GetComponent<Controller2D>());
			}
			if (passenger.moveBeforePlatform == beforeMovePlatform)
			{
				passengerDictionary[passenger.transform ].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);

		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; ++i)
			{
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			for (int i = 0; i < horizontalRayCount; ++i)
			{
				Vector2 rayOrigin = raycastOrigins.bottomLeft;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * -1, rayLength, passengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * -1;
						// Adding a minute downward force resolves an issue where the passenger would not be checking vertical
						// collisions since it's Y movement was 0.
						float pushY = -skinWidth;

						if (hit.transform.GetComponent<Controller2D>().collInfo.below) {
							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}
						else {
							Vector3 playerNewPosition = hit.transform.position + new Vector3(pushX, 0f);
							hit.transform.position = playerNewPosition;
						}
					}
				}
			}

			for (int i = 0; i < horizontalRayCount; ++i) 
			{
				Vector2 rayOrigin = raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * 1, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * 1;
						// Adding a minute downward force resolves an issue where the passenger would not be checking vertical
						// collisions since it's Y movement was 0.
						float pushY = -skinWidth;

						if (hit.transform.GetComponent<Controller2D>().collInfo.below) {
							passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
						}
						else {
							Vector3 playerNewPosition = hit.transform.position + new Vector3(pushX, 0f);
							hit.transform.position = playerNewPosition;
						}
					}
				}
			}
		}

		// Passenger on top of a horizontally or donward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRayCount; ++i)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x;
						float pushY = velocity.y;
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerMovement
	{
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	void OnDrawGizmos()
	{
		if (localWaypoints != null)
		{
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; ++i)
			{
				Vector3 globalWaypointPositionA = (Application.isPlaying) ? 
					globalWaypoints[i] : localWaypoints[i] + transform.position;

				int nextWaypoint = (i + 1) % localWaypoints.Length;
				Vector3 globalWaypointPositionB = (Application.isPlaying) ?
					globalWaypoints[nextWaypoint] : localWaypoints[nextWaypoint] + transform.position;

				Gizmos.color = Color.red;
				Gizmos.DrawLine(globalWaypointPositionA - Vector3.up * size, globalWaypointPositionA + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPositionA - Vector3.left * size, globalWaypointPositionA + Vector3.left * size);

				if ((i < localWaypoints.Length - 1) || (i == localWaypoints.Length - 1 && cyclicPath))
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(globalWaypointPositionA, globalWaypointPositionB);
				}
			}
		}
	}
}
