using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class ChangeVirtualCameraPriority : MonoBehaviour
{
	[SerializeField] private CinemachineVirtualCamera cameraA;
	[SerializeField] private CinemachineVirtualCamera cameraB;

	private const float _coroutinePauseTime = 0.25f;
	private const float _playerTransitionDistance = 3f;
	private const float _TotalLerpTime = 0.1f;

	void Awake()
	{
		if (cameraA == null)
		{
			Debug.LogError("ChangeVirutalCameraPriority is missing requiered component cameraA");
			UnityEditor.EditorApplication.isPlaying = false;
		}

		if (cameraB == null)
		{
			Debug.LogError("ChangeVirutalCameraPriority is missing requiered component cameraB");
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		//  1: A -> B
		// -1: B -> A
		int direction = 0;
		bool swapped = false;
		if (col.gameObject.tag == "Player")
		{
			if (cameraA.Priority == 1)
			{
				cameraA.Priority = 0;
				cameraB.Priority = 1;
				swapped = true;
				direction = 1;
			}
			else if (cameraB.Priority == 1)
			{
				cameraA.Priority = 1;
				cameraB.Priority = 0;
				swapped = true;
				direction = -1;
			}

			if (swapped)
			{
				Rigidbody2D playerRigi = col.gameObject.GetComponent<Rigidbody2D>();
				Transform playerTransform = col.gameObject.GetComponent<Transform>();
				StartCoroutine(DoTransition(playerRigi, playerTransform, direction));
			}
		}
	}

	IEnumerator DoTransition(Rigidbody2D playerRigi, Transform playerTransform, int direction)
	{
		// Freeze the players physic opperations
		playerRigi.constraints = RigidbodyConstraints2D.FreezeAll;

		// Leave the player floating while the virtual cameras start to transition
		// _coroutinePauseTime = 0.25f
		yield return new WaitForSeconds(_coroutinePauseTime);

		// Lerp the player to a position just past the transition trigger.
		// _playerTransitionDistance = 3f
		Vector3 newPosition = playerTransform.position + new Vector3(_playerTransitionDistance* direction, 0.0f, 0.0f);
		Vector3 startingPosition = playerTransform.position;
		float elapsedTime = 0.0f;
		while (elapsedTime < _TotalLerpTime)
		{
			playerTransform.position = Vector3.Lerp(startingPosition, newPosition, (elapsedTime / _TotalLerpTime));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		// Unfreeze the player's rigidbody
		playerRigi.constraints = RigidbodyConstraints2D.None;
		playerRigi.constraints = RigidbodyConstraints2D.FreezeRotation;
	}
}
