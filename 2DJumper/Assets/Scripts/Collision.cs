using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
	private Vector2 bottomOffset = new Vector2(0f, -0.5f);
	private Vector2 bottomOverlapBox = new Vector2(0.95f, 0.05f);
	private Vector2 leftOffset = new Vector2(-0.5f, 0f);
	private Vector2 leftOverlapBox = new Vector2(0.05f, 0.95f);
	private Vector2 rightOffset = new Vector2(0.5f, 0f);
	private Vector2 rightOverlapBox = new Vector2(0.05f, 0.95f);

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent;

	void Awake()
	{
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		bool wasGrounded = onGround;
		
		// Check for ground collision
		onGround = Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, bottomOverlapBox, 0.0f, whatIsGround);

		if (!wasGrounded && onGround)
			OnLandEvent.Invoke();

		// Check for left wall collision
		onWallLeft = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, leftOverlapBox, 0.0f, whatIsWall);

		// Check for right wall collision
		onWallRight = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, rightOverlapBox, 0.0f, whatIsWall);

		onWall = onWallLeft || onWallRight ? true : false;

		wallSide = onWallRight ? -1 : 1;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		Gizmos.DrawWireCube(transform.position + (Vector3)bottomOffset, new Vector3(0.95f, 0.05f, 0.0f));
		Gizmos.DrawWireCube(transform.position + (Vector3)leftOffset, new Vector3(0.05f, 0.95f, 0.0f));
		Gizmos.DrawWireCube(transform.position + (Vector3)rightOffset, new Vector3(0.05f, 0.95f, 0.0f));
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Spike")
		{
			RestartLevel();
		}
	}

	void RestartLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
