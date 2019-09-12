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

	// This could eventually trigger a fuller death sequence to play.
	// PlayerDeathAnim -> FadeOut -> Wait -> FadeIn -> PlayerEntraceAnim
	IEnumerator ReSpawnPlayer()
	{
		fadeAnim.SetTrigger("FadeOut");
		yield return new WaitForSeconds(0.75f);
		transform.position = gameManager.currentReSpawnPoint.transform.position;
		fadeAnim.SetTrigger("FadeIn");
	}
}
