using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LevelSpawnPoints))]
public class LevelSpawnPointsToolHandle : Editor
{
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		if (GUILayout.Button("Gather SpawnPoints")) {			
			CollectChildSpawnPoints();
		}
	}

	public void CollectChildSpawnPoints () {
		LevelSpawnPoints spawnPointsScript = (LevelSpawnPoints)target;

		spawnPointsScript.spawnPoints.Clear();

		foreach (Transform spawnPoint in spawnPointsScript.transform) {
			if (spawnPoint.CompareTag("SpawnPoint")) {
				Undo.RecordObject(spawnPointsScript, "Added spawnpoint");
				spawnPointsScript.spawnPoints.Add(spawnPoint);
				EditorUtility.SetDirty(spawnPointsScript);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

				Debug.Log("Added: " + spawnPoint.name + " to " + spawnPointsScript.transform.name + " SpawnPoints list");
			}
		}
	}
}
