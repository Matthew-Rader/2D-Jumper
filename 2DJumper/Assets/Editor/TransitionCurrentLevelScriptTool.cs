using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransitionCurrentLevel))]
public class TransitionCurrentLevelScriptTool : Editor {
	int levelASpawnPointIndex = 0;
	int levelBSpawnPointIndex = 0;

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		EditorGUILayout.Space();

		TransitionCurrentLevel transitionPointScript = (TransitionCurrentLevel)target;

		if (GUILayout.Button("Grab Level SpawnPoints")) {
			transitionPointScript.GrabLevelSpawnPoints();
		}

		EditorGUILayout.Space();

		if (!transitionPointScript.noSpawnPointA) {
			int levelASpawnPointCount = transitionPointScript.spawnPointsA.Count;

			string[] levelASpawnPointNames = new string[levelASpawnPointCount];

			for (int i = 0; i < levelASpawnPointCount; ++i) {
				levelASpawnPointNames[i] = transitionPointScript.spawnPointsA[i].name;
			}

			if (levelASpawnPointNames.Length > 0) {
				levelASpawnPointIndex = EditorGUILayout.Popup(
					"Spawn Point Level A",
					levelASpawnPointIndex,
					levelASpawnPointNames);
			
			}
			else {
				EditorGUILayout.LabelField("Spawn Point Level A", "No Spawn Points Found!");
			}
		}

		if (!transitionPointScript.noSpawnPointB) {
			int levelBSpawnPointCount = transitionPointScript.spawnPointsB.Count;

			string[] levelBSpawnPointNames = new string[levelBSpawnPointCount];

			for (int i = 0; i < levelBSpawnPointCount; ++i) {
				levelBSpawnPointNames[i] = transitionPointScript.spawnPointsB[i].name;
			}

			// Create the popup boxes to select Level A an B's spawn point
			if (levelBSpawnPointNames.Length > 0) {
				levelBSpawnPointIndex = EditorGUILayout.Popup(
					"Spawn Point Level B",
					levelBSpawnPointIndex,
					levelBSpawnPointNames);
				
			}
			else {
				EditorGUILayout.LabelField("Spawn Point Level B", "No Spawn Points Found!");
			}
		}

		if (GUILayout.Button("Set SpawnPoints")) {
			transitionPointScript.SetLevelSpawnPoint('a', levelASpawnPointIndex);
			transitionPointScript.SetLevelSpawnPoint('b', levelBSpawnPointIndex);
		}
	}
}
