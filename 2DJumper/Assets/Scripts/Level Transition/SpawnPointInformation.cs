using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointInformation : MonoBehaviour
{
	[SerializeField] private enum orientation {
		Left,
		Right
	};

	[SerializeField] private orientation spawnOrientation = orientation.Left;

	//  1 == RIGHT
	// -1 == LEFT
	[HideInInspector]
	public int spwnOrien;

	void Start () {
		spwnOrien = (spawnOrientation == orientation.Left) ? -1 : 1;
	}
}
