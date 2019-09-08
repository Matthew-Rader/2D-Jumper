using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterJumping : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2f;
	private Rigidbody2D characterRigi;

	// Start is called before the first frame update
	void Awake()
    {
		characterRigi = GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if (characterRigi.velocity.y < 0)
		{
			characterRigi.gravityScale = fallMultiplier;
		}
		//else if (characterRigi.velocity.y > 0 && !Input.GetButton("Jump"))
		//{
		//	characterRigi.gravityScale = lowJumpMultiplier;
		//}
		else
		{
			characterRigi.gravityScale = 1f;
		}
	}
}
