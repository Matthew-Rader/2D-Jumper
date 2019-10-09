using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
	[SerializeField]
	protected LayerMask collisionMask;

	protected const float skinWidth = 0.015f;

	// The amount of rays that shot out from the sides of the player
	protected int horizontalRayCount;

	// The amount of rays that shot out from the top and bottom of the player
	protected int verticalRayCount;

	// The relative spacing of all the rays being shot out from the player
	[HideInInspector]
	protected float horizontalRaySpacing;
	[HideInInspector]
	protected float verticalRaySpacing;

	const float distanceBetweenRays = 0.25f;

	[HideInInspector]
	protected BoxCollider2D collider;
	[HideInInspector]
	protected RaycastOrigins raycastOrigins;

	public virtual void Start()
	{
		collider = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
	}

	public void CalculateRaySpacing()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt(boundsHeight  /distanceBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

}
