using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelSpawnPoints : MonoBehaviour {
	public List<Transform> spawnPoints = new List<Transform>();

	public void CollectChildSpawnPoints () {
		spawnPoints.Clear();

		foreach (Transform spawnPoint in this.transform) {
			if (spawnPoint.CompareTag("SpawnPoint")) {
				spawnPoints.Add(spawnPoint);
				Debug.Log("Added: " + spawnPoint.name + " to " + this.transform.name + " SpawnPoints list");
			}
		}
	}
}
