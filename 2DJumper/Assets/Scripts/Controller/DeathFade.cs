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

		// Freeze the players physic opperations
		transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

		transform.GetComponent<CharacterController2D>().enabled = false;

		yield return new WaitForSeconds(0.75f);

		transform.position = gameManager.currentReSpawnPoint.transform.position;
		transform.GetComponent<CharacterController2D>().enabled = true;

		// Unfreeze the player's rigidbody
		transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
		transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

		fadeAnim.SetTrigger("FadeIn");
	}
}
