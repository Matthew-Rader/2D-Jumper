using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionCurrentLevel : MonoBehaviour
{
	// This script is in charage of updating the gamemanger and letting it know
	// the player has transitioned to a new level. It does this by updating the 
	// player spawn point and the known current level.

	[SerializeField] private GameObject spawnPointA;
	[SerializeField] private GameObject spawnPointB;
	[SerializeField] private GameObject levelA;
	[SerializeField] private GameObject levelB;
	[SerializeField] private GameManager gameManager;

	void Awake()
	{
		if (spawnPointA == null)
		{
			Debug.LogError("TransitionCurrentLevel is missing requiered reference spawnPointA");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (spawnPointB == null)
		{
			Debug.LogError("TransitionCurrentLevel is missing requiered reference spawnPointB");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (levelA == null)
		{
			Debug.LogError("TransitionCurrentLevel is missing requiered reference levelA");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (levelB == null)
		{
			Debug.LogError("TransitionCurrentLevel is missing requiered reference levelB");
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player")
		{
			if (gameManager.currentLevel == levelA)
			{
				gameManager.currentLevel = levelB;
				gameManager.currentReSpawnPoint = spawnPointB;
			}
			else if (gameManager.currentLevel == levelB)
			{
				gameManager.currentLevel = levelA;
				gameManager.currentReSpawnPoint = spawnPointA;
			}
		}
	}
}
