using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSpawnPoints))]
public class LevelSpawnPointsToolHandle : Editor
{
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		LevelSpawnPoints spawnPointsScript = (LevelSpawnPoints)target;

		if (GUILayout.Button("Gather SpawnPoints")) {
			spawnPointsScript.CollectChildSpawnPoints();
		}
	}
}
