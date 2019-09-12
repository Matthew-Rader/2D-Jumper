using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject currentLevel;
	public GameObject currentReSpawnPoint;

    // Start is called before the first frame update
    void Awake()
    {
		if (currentLevel == null)
		{
			Debug.LogError("GameManager is missing requiered reference currentLevel");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (currentReSpawnPoint == null)
		{
			Debug.LogError("GameManager is missing requiered reference currentReSpawnPoint");
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}
}
