using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransitionCurrentLevel))]
public class TransitionCurrentLevelScriptTool : Editor {
	[SerializeField] int levelASpawnPointIndex = 0;
	[SerializeField] int levelBSpawnPointIndex = 0;

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		EditorGUILayout.Space();

		TransitionCurrentLevel transitionPointScript = (TransitionCurrentLevel)target;

		if (GUILayout.Button("Grab Level SpawnPoints")) {
			Debug.Log("Grab spawnpoints button");
			/*transitionPointScript.*/GrabLevelSpawnPoints(transitionPointScript);
		}

		EditorGUILayout.Space();

		if (!transitionPointScript.noSpawnPointA) {
			if (transitionPointScript.spawnPointsA.Count == 0) {
				Debug.Log("Grab spawnpoints Level A count equals 0");
				/*transitionPointScript.*/GrabLevelSpawnPoints(transitionPointScript);
				return;
			}

			int levelASpawnPointCount = transitionPointScript.spawnPointsA.Count;

			string[] levelASpawnPointNames = new string[levelASpawnPointCount];

			for (int i = 0; i < levelASpawnPointCount; ++i) {
				if (transitionPointScript.spawnPointsA[i] != null) {
					levelASpawnPointNames[i] = transitionPointScript.spawnPointsA[i].name;
				}
				else {
					Debug.Log("Grab spawnpoints Level A index null");
					/*transitionPointScript.*/GrabLevelSpawnPoints(transitionPointScript);
					return;
				}
			}

			if (levelASpawnPointNames.Length > 0) {
				//Debug.Log(levelASpawnPointIndex);
				levelASpawnPointIndex = transitionPointScript.choosenSpawnPointAIndex;

				levelASpawnPointIndex = EditorGUILayout.Popup(
					"Spawn Point Level A",
					levelASpawnPointIndex,
					levelASpawnPointNames);

				Undo.RecordObject(this, "Some Random Text");
				transitionPointScript.choosenSpawnPointA = transitionPointScript.spawnPointsA[levelASpawnPointIndex];
				transitionPointScript.choosenSpawnPointAIndex = levelASpawnPointIndex;
				EditorUtility.SetDirty(this);

			}
			else {
				EditorGUILayout.LabelField("Spawn Point Level A", "No Spawn Points Found!");
			}
		}

		if (!transitionPointScript.noSpawnPointB) {
			if (transitionPointScript.spawnPointsB.Count == 0) {
				Debug.Log("Grab spawnpoints Level B count equals 0");
				/*transitionPointScript.*/GrabLevelSpawnPoints(transitionPointScript);
				return;
			}

			int levelBSpawnPointCount = transitionPointScript.spawnPointsB.Count;

			string[] levelBSpawnPointNames = new string[levelBSpawnPointCount];

			for (int i = 0; i < levelBSpawnPointCount; ++i) {
				if (transitionPointScript.spawnPointsB[i] != null) {
					levelBSpawnPointNames[i] = transitionPointScript.spawnPointsB[i].name;
				}
				else {
					Debug.Log("Grab spawnpoints Level B index null");
					/*transitionPointScript.*/GrabLevelSpawnPoints(transitionPointScript);
					return;
				}
			}

			// Create the popup boxes to select Level A an B's spawn point
			if (levelBSpawnPointNames.Length > 0) {
				levelBSpawnPointIndex = transitionPointScript.choosenSpawnPointBIndex;

				levelBSpawnPointIndex = EditorGUILayout.Popup(
					"Spawn Point Level B",
					levelBSpawnPointIndex,
					levelBSpawnPointNames);

				Undo.RecordObject(this, "Some Random Text");
				transitionPointScript.choosenSpawnPointB = transitionPointScript.spawnPointsB[levelBSpawnPointIndex];
				transitionPointScript.choosenSpawnPointBIndex = levelBSpawnPointIndex;
				EditorUtility.SetDirty(this);

			}
			else {
				EditorGUILayout.LabelField("Spawn Point Level B", "No Spawn Points Found!");
			}
		}

		//if (GUILayout.Button("Set SpawnPoints")) {
		//	Undo.RecordObject(this, "Some Random Text");
		//	transitionPointScript.SetLevelSpawnPoint('a', levelASpawnPointIndex);
		//	transitionPointScript.SetLevelSpawnPoint('b', levelBSpawnPointIndex);
		//	EditorUtility.SetDirty(this);
		//}
	}

	public void GrabLevelSpawnPoints (TransitionCurrentLevel transitionPointScript) {
		Debug.Log("Grabbing SpawnPoints");

		foreach (Transform child in transitionPointScript.levelA.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				Undo.RecordObject(this, "Some Random Text");
				transitionPointScript.spawnPointsA = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				EditorUtility.SetDirty(this);
				break;
			}
		}

		foreach (Transform child in transitionPointScript.levelB.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				Undo.RecordObject(this, "Some Random Text");
				transitionPointScript.spawnPointsB = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				EditorUtility.SetDirty(this);
				break;
			}
		}
	}
}
