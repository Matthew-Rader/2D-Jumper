using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
	[SerializeField] private float jumpHeight = 4;
	[SerializeField] private float timeToJumpApex = 0.4f;
	[SerializeField] private float moveSpeed = 6;
	[SerializeField] private float accelerationTimeAirborne = 0.2f;
	[SerializeField] private float accelerationTimeGrounded = 0.1f;


	float jumpVelocity;
	float gravity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

    void Start()
    {
		controller = GetComponent<Controller2D>();

		gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update()
    {
		if (controller.collInfo.above || controller.collInfo.below)
		{
			velocity.y = 0;
		}

		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		if (Input.GetButtonDown("Jump") && controller.collInfo.below)
		{
			velocity.y = jumpVelocity;
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, 
			(controller.collInfo.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
    }
}
