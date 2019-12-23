using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class TransitionCurrentLevel : MonoBehaviour {
	// This script is in charage of updating the gamemanger and letting it know
	// the player has transitioned to a new level. It does this by updating the 
	// player spawn point and the known current level.

	//[SerializeField] private GameObject spawnPointA;

	//[SerializeField] private GameObject spawnPointB;

	[SerializeField] public GameObject levelA;
	[SerializeField] public GameObject levelB;
	[SerializeField] private bool verticalTransition = false;
	public bool noSpawnPointA;
	public bool noSpawnPointB;
	private CinemachineVirtualCamera cameraA;
	private CinemachineVirtualCamera cameraB;
	private GameManager gameManager;

	public List<Transform> spawnPointsA = new List<Transform>();
	public List<Transform> spawnPointsB = new List<Transform>();
	public Transform choosenSpawnPointA;
	public int choosenSpawnPointAIndex = 0;
	public Transform choosenSpawnPointB;
	public int choosenSpawnPointBIndex = 0;

	private const float _coroutinePauseTime = 0.25f;
	private const float _playerTransitionHorizontalDistance = 3f;
	private const float _playerTransitionVerticalDistance = 4f;
	private const float _TotalLerpTime = 0.1f;

	void Awake()
	{
		//if (spawnPointA == null && !noSpawnPointA)
		//{
		//	Debug.LogError("TransitionCurrentLevel is missing requiered reference spawnPointA");
		//	UnityEditor.EditorApplication.isPlaying = false;
		//}

		//if (spawnPointB == null && !noSpawnPointB)
		//{
		//	Debug.LogError("TransitionCurrentLevel is missing requiered reference spawnPointB");
		//	UnityEditor.EditorApplication.isPlaying = false;
		//}

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

		foreach (Transform child in levelA.transform) {
			if (child.CompareTag("vcam")) {
				cameraA = child.GetComponent<CinemachineVirtualCamera>();
				break;
			}
		}

		foreach (Transform child in levelB.transform) {
			if (child.CompareTag("vcam")) {
				cameraB = child.GetComponent<CinemachineVirtualCamera>();
				break;
			}
		}

		if (cameraA == null) {
			Debug.LogError("ChangeVirutalCameraPriority is missing requiered component cameraA");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (cameraB == null) {
			Debug.LogError("ChangeVirutalCameraPriority is missing requiered component cameraB");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

		if (gameManager == null) {
			Debug.LogError("GameManager is missing");
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		//  1: A -> B
		// -1: B -> A
		int direction = 0;
		bool swapped = false;

		if (col.gameObject.tag == "Player")
		{
			if (cameraA.Priority == 1) {
				cameraA.Priority = 0;
				cameraB.Priority = 1;
				swapped = true;
				direction = 1;
			}
			else if (cameraB.Priority == 1) {
				cameraA.Priority = 1;
				cameraB.Priority = 0;
				swapped = true;
				direction = -1;
			}

			if (swapped) {
				UpdateGameManager();

				Player playerController = col.gameObject.GetComponent<Player>();
				Transform playerTransform = col.gameObject.GetComponent<Transform>();
				StartCoroutine(DoPlayerTransition(playerController, playerTransform, direction));
			}
		}
	}

	void UpdateGameManager () {
		if (gameManager.currentLevel == levelA) {
			gameManager.currentLevel = levelB;
			gameManager.currentReSpawnPoint = choosenSpawnPointB;
		}
		else if (gameManager.currentLevel == levelB) {
			gameManager.currentLevel = levelA;
			gameManager.currentReSpawnPoint = choosenSpawnPointA;
		}
	}

	IEnumerator DoPlayerTransition (Player playerController, Transform playerTransform, int direction) {
		// Freeze the players physic opperations
		playerController.enabled = false;

		// Leave the player floating while the virtual cameras start to transition
		// _coroutinePauseTime = 0.25f
		yield return new WaitForSeconds(_coroutinePauseTime);

		// Lerp the player to a position just past the transition trigger.
		// _playerTransitionDistance = 3f
		Vector3 newPosition;
		if (verticalTransition)
			newPosition = playerTransform.position + new Vector3(0.0f, _playerTransitionVerticalDistance * direction, 0.0f);
		else
			newPosition = playerTransform.position + new Vector3(_playerTransitionHorizontalDistance * direction, 0.0f, 0.0f);

		Vector3 startingPosition = playerTransform.position;
		float elapsedTime = 0.0f;
		while (elapsedTime < _TotalLerpTime) {
			playerTransform.position = Vector3.Lerp(startingPosition, newPosition, (elapsedTime / _TotalLerpTime));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		// Unfreeze the player's rigidbody
		playerController.enabled = true;
	}

	// Used by the tool extension 
	public void GrabLevelSpawnPoints () {
		foreach (Transform child in levelA.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				spawnPointsA = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				break;
			}
		}

		foreach (Transform child in levelB.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				spawnPointsB = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				break;
			}
		}
	}

	public void SetLevelSpawnPoint (char level, int index) {
		switch (level) {
			case 'a':
				choosenSpawnPointA = spawnPointsA[index];
				choosenSpawnPointAIndex = index;
				break;
			case 'b':
				choosenSpawnPointB = spawnPointsB[index];
				choosenSpawnPointBIndex = index;
				break;
		}
	}
}
