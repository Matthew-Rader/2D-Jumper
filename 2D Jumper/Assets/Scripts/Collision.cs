using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collision : MonoBehaviour
{
	[HideInInspector] public bool onGround;
	[HideInInspector] public bool onWall;
	[HideInInspector] public bool onWallLeft;
	[HideInInspector] public bool onWallRight;
	[HideInInspector] public int wallSide;

	[Header("Collision Layers")]
	[SerializeField] private LayerMask whatIsGround; // A mask determining what is ground to the character
	[SerializeField] private LayerMask whatIsWall; // A mask determining what is wall to the character

	[Header("Collision check points")]
	[SerializeField] private float collisionRadius = .2f; // Radius of the overlap circle to determine if grounded
	[SerializeField] private Vector2 bottomOffset, rightOffset, leftOffset;

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent;

	void Awake()
	{
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		rightOffset = new Vector2(0.5f, 0f);
		leftOffset = new Vector2(-0.5f, 0f);
		bottomOffset = new Vector2(0f, -0.5f);
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		bool wasGrounded = onGround;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, whatIsGround);
		if (!wasGrounded && onGround)
			OnLandEvent.Invoke();

		// Check for left wall collision
		onWallLeft = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, whatIsWall);

		// Check for right wall collision
		onWallRight = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, whatIsWall);

		onWall = onWallLeft || onWallRight ? true : false;

		wallSide = onWallRight ? -1 : 1;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

		Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
	}
}
