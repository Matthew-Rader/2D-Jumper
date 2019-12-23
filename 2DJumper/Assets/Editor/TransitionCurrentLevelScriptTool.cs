using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TransitionCurrentLevel))]
public class TransitionCurrentLevelScriptTool : Editor {
	[SerializeField] int newSpawnPointIndex = 0;
	enum Level { LevelA, LevelB }

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		EditorGUILayout.Space();

		TransitionCurrentLevel transitionScript = (TransitionCurrentLevel)target;

		if (GUILayout.Button("Grab Level SpawnPoints")) {
			Debug.Log("Grab spawnpoints button");
			GrabLevelSpawnPoints(transitionScript);
		}

		EditorGUILayout.Space();

		if (!transitionScript.noSpawnPointA) {
			Level level = Level.LevelA;
			List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelA;

			if (CheckIfThereAreSpawnPoints(transitionScript, level, levelSpawnPoints)) {
				CreateSpawnPointPopupTool(transitionScript, level, levelSpawnPoints, ref transitionScript.choosenSpawnPointA, ref transitionScript.choosenSpawnPointAIndex);
			}
		}

		if (!transitionScript.noSpawnPointB) {
			Level level = Level.LevelA;
			List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelB;

			if (CheckIfThereAreSpawnPoints(transitionScript, level, levelSpawnPoints)) {
				CreateSpawnPointPopupTool(transitionScript, level, levelSpawnPoints, ref transitionScript.choosenSpawnPointB, ref transitionScript.choosenSpawnPointBIndex);
			}
		}
	}

	void CreateSpawnPointPopupTool (TransitionCurrentLevel transitionScript, Level level, List<Transform> levelSpawnPoints, ref Transform choosenSpawnPoint, ref int choosenSpawnPointIndex) {
		string levelName = (level == Level.LevelA) ? "Level A" : "Level B";
		int spawnPointCount = levelSpawnPoints.Count;
		string[] spawnPointNames = new string[spawnPointCount];

		for (int i = 0; i < spawnPointCount; ++i) {
			if (levelSpawnPoints[i] != null) {
				spawnPointNames[i] = levelSpawnPoints[i].name;
			}
			else {
				Debug.LogError("Grab spawnpoints " + levelName + ": index null");
				GrabLevelSpawnPoints(transitionScript);
				return;
			}
		}

		// Create the popup boxes to select Level A and B's spawn point
		if (spawnPointNames.Length > 0) {
			newSpawnPointIndex = choosenSpawnPointIndex;

			newSpawnPointIndex = EditorGUILayout.Popup(
				"Spawn Point " + levelName,
				newSpawnPointIndex,
				spawnPointNames);

			Undo.RecordObject(transitionScript, "Setting Spawn Point " + levelName);
			choosenSpawnPoint = levelSpawnPoints[newSpawnPointIndex];
			choosenSpawnPointIndex = newSpawnPointIndex;
			EditorUtility.SetDirty(transitionScript);

		}
		else {
			EditorGUILayout.LabelField("Spawn Point " + levelName, "No Spawn Points Found!");
		}
	}

	bool CheckIfThereAreSpawnPoints (TransitionCurrentLevel transitionScript, Level level, List<Transform> levelSpawnPoints) {
		string levelName = (level == Level.LevelA) ? "Level A" : "Level B";

		// Since we are calling this function we should be expecting spawnpoints, if 
		// the count is zero then try to gather the necessary spawnpoints
		if (levelSpawnPoints.Count == 0) {
			Debug.LogError("Grab spawnpoints " + levelName + ": count equals 0");
			GrabLevelSpawnPoints(transitionScript);

			// Check to see if we have found the spawn points, else return
			if (levelSpawnPoints.Count == 0) {
				Debug.LogError("No spawnpoints found for " + levelName);
				return false;
			}
		}

		return true;
	}

	void GrabLevelSpawnPoints (TransitionCurrentLevel transitionScript) {
		Debug.Log("Grabbing SpawnPoints");

		foreach (Transform child in transitionScript.levelA.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				Undo.RecordObject(transitionScript, "Some Random Text");
				transitionScript.spawnPointsLevelA = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				EditorUtility.SetDirty(transitionScript);
				break;
			}
		}

		foreach (Transform child in transitionScript.levelB.transform) {
			if (child.CompareTag("SpawnPointParent")) {
				Undo.RecordObject(transitionScript, "Some Random Text");
				transitionScript.spawnPointsLevelB = child.GetComponent<LevelSpawnPoints>().spawnPoints;
				EditorUtility.SetDirty(transitionScript);
				break;
			}
		}
	}
}
