using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
	[SerializeField]
	protected LayerMask collisionMask;
	[SerializeField]
	protected const float skinWidth = 0.015f;

	// The amount of rays that shot out from the sides of the player
	[SerializeField]
	protected int horizontalRayCount = 4;

	// The amount of rays that shot out from the top and bottom of the player
	[SerializeField]
	protected int verticalRayCount = 4;

	// The relative spacing of all the rays being shot out from the player
	[HideInInspector]
	protected float horizontalRaySpacing;
	[HideInInspector]
	protected float verticalRaySpacing;

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

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

}
