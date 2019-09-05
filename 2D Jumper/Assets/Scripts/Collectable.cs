using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
	[SerializeField] private GameObject parent;

	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log(other.tag);
		if (other.gameObject.tag == "Player")
		{
			Destroy(parent);
		}
	}
}
