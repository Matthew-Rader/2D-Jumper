using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathFade : MonoBehaviour
{
	[SerializeField] private Animator fadeAnim;
	[SerializeField] private GameManager gameManager;

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Spike")
		{
			StartCoroutine(ReSpawnPlayer());
		}
	}

	public void StartDeathFadeCoroutine () {
		StartCoroutine(ReSpawnPlayer());
	}

	// This could eventually trigger a fuller death sequence to play.
	// PlayerDeathAnim -> FadeOut -> Wait -> FadeIn -> PlayerEntraceAnim
	public IEnumerator ReSpawnPlayer()
	{
		fadeAnim.SetTrigger("FadeOut");

		// Freeze the player
		transform.GetComponent<Player>().enabled = false;

		yield return new WaitForSeconds(0.75f);

		transform.position = gameManager.currentReSpawnPoint.transform.position;

		// Unfreeze the player
		transform.GetComponent<Player>().enabled = true;

		fadeAnim.SetTrigger("FadeIn");
	}
}
