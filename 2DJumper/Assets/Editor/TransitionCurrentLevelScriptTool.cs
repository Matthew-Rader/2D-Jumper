/*
 * This script is in charge of creating the spawnpoint selection tool. It collects the spawnpoints
 * that can be found in the PlayerSpawnPoints object on stages provided by LevelA and LevelB. It
 * then populates Popups to select which spawnpoint should be used and assigns it on the main script.
 * 
 * Apendix ----------------------------------------------------------------------------------------
 * 
 * A: Undo and SetDirty
 * In several places you will find the functions Undo.RecordObject() and EditorUtility.SetDirty().
 * These functions are in charge of manually telling Unity that a recordable change has occured.
 * - Undo.RecordObject() tells Unity to allow us to undo this change; we can also pass in a message.
 * - EditorUtility.SetDirty() tells Unity a change has occured that should/can be saved. 
 * - EditorSceneManager.MarkSceneDirty() tells Unity that the scene is dirty and can be saved.
 * 
 * Author: Matthew Rader
 */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[CustomEditor(typeof(TransitionCurrentLevel))]
public class TransitionCurrentLevelScriptTool : Editor {
	enum Level { LevelA, LevelB }
	const Level _LevelA = Level.LevelA;
	const Level _LevelB = Level.LevelB;
	[SerializeField] int newSpawnPointIndex = 0;
	const int _defaultSpawnPointIndex = 0;
	TransitionCurrentLevel transitionScript;

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		EditorGUILayout.Space();

		transitionScript = (TransitionCurrentLevel)target;

		if (GUILayout.Button("Grab Level Spawnpoints")) {
			Debug.Log("Grab spawnpoints button pressed");
			GrabLevelSpawnPoints();
			SetDefaultSpawnPoint(_LevelA);
			SetDefaultSpawnPoint(_LevelB);
		}

		EditorGUILayout.Space();

		if (!transitionScript.noSpawnPointA && transitionScript.levelA != null) {
			Level level = _LevelA;
			List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelA;

			if (CheckIfThereAreSpawnPoints(level, levelSpawnPoints)) {
				CreateSpawnPointPopupTool(level, levelSpawnPoints, ref transitionScript.choosenSpawnPointA, ref transitionScript.choosenSpawnPointAIndex);
			}
		}

		if (!transitionScript.noSpawnPointB && transitionScript.levelB != null) {
			Level level = _LevelB;
			List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelB;

			if (CheckIfThereAreSpawnPoints(level, levelSpawnPoints)) {
				CreateSpawnPointPopupTool(level, levelSpawnPoints, ref transitionScript.choosenSpawnPointB, ref transitionScript.choosenSpawnPointBIndex);
			}
		}
	}

	bool CheckIfThereAreSpawnPoints (Level level, List<Transform> levelSpawnPoints) {
		string levelName = (level == _LevelA) ? "Level A (" + transitionScript.levelA.name + ")" : 
			                                    "Level B (" + transitionScript.levelB.name + ")";

		// Since we are calling this function we should be expecting spawnpoints, if 
		// the count is zero then try to gather the necessary spawnpoints
		if (levelSpawnPoints.Count == 0) {
			Debug.LogError("Grab spawnpoints " + levelName + ": count equals 0");
			GrabLevelSpawnPoints();

			// Check to see if we have found the spawnpoints, else return
			if (levelSpawnPoints.Count == 0) {
				Debug.LogError("No spawnpoints found for " + levelName);
				return false;
			}

			// If we have succesfully found spawnpoints then set them to default
			SetDefaultSpawnPoint(level);
		}

		return true;
	}

	void CreateSpawnPointPopupTool (Level level, List<Transform> levelSpawnPoints, ref Transform choosenSpawnPoint, ref int choosenSpawnPointIndex) {
		string levelName = (level == _LevelA) ? "Level A (" + transitionScript.levelA.name + ")" :
												"Level B (" + transitionScript.levelB.name + ")";

		int spawnPointCount = levelSpawnPoints.Count;
		string[] spawnPointNames = new string[spawnPointCount];

		for (int i = 0; i < spawnPointCount; ++i) {
			if (levelSpawnPoints[i] != null) {
				spawnPointNames[i] = levelSpawnPoints[i].name;
			}
			else {
				Debug.LogError("Grab spawnpoints " + levelName + ": index null");
				GrabLevelSpawnPoints();
				SetDefaultSpawnPoint(level);
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

			if (newSpawnPointIndex != choosenSpawnPointIndex) {
				Undo.RecordObject(transitionScript, "Setting Spawnpoint " + levelName);
				choosenSpawnPoint = levelSpawnPoints[newSpawnPointIndex];
				choosenSpawnPointIndex = newSpawnPointIndex;
				Debug.Log("Setting Spawnpoint for " + levelName + " to " + choosenSpawnPoint.name);
				EditorUtility.SetDirty(transitionScript);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			else if (spawnPointNames.Length == 1 && choosenSpawnPoint == null) {
				Undo.RecordObject(transitionScript, "Setting Spawnpoint " + levelName);
				choosenSpawnPoint = levelSpawnPoints[_defaultSpawnPointIndex];
				choosenSpawnPointIndex = _defaultSpawnPointIndex;
				Debug.Log("Setting Spawnpoint for " + levelName + " to " + choosenSpawnPoint.name);
				EditorUtility.SetDirty(transitionScript);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}
		else {
			EditorGUILayout.LabelField("Spawnpoint " + levelName, "No Spawnpoints Found!");
		}
	}

	void GrabLevelSpawnPoints () {
		Debug.Log("Grabbing Spawnpoints Start");

		if (transitionScript.levelA != null) {
			Debug.Log("Grabbing Spawnpoints: Level A (" + transitionScript.levelA.name + ")");

			foreach (Transform child in transitionScript.levelA.transform) {
				if (child.CompareTag("SpawnPointParent")) {
					Undo.RecordObject(transitionScript, "Some Random Text");
					transitionScript.spawnPointsLevelA = child.GetComponent<LevelSpawnPoints>().spawnPoints;
					EditorUtility.SetDirty(transitionScript);
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					break;
				}
			}
		}

		if (transitionScript.levelB != null) {
			Debug.Log("Grabbing Spawnpoints: Level B (" + transitionScript.levelB.name + ")");
			foreach (Transform child in transitionScript.levelB.transform) {
				if (child.CompareTag("SpawnPointParent")) {
					Undo.RecordObject(transitionScript, "Some Random Text");
					transitionScript.spawnPointsLevelB = child.GetComponent<LevelSpawnPoints>().spawnPoints;
					EditorUtility.SetDirty(transitionScript);
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					break;
				}
			}
		}

		Debug.Log("Grabbing Spawnpoints End");
	}

	void SetDefaultSpawnPoint (Level level) {
		if (level == _LevelA) {
			SetDefaultSpawnPointA(level);
		}
		else if (level == _LevelB) {
			SetDefaultSpawnPointB(level);
		}
	}

	void SetDefaultSpawnPointA (Level level) {
		List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelA;
		AssignDefaultSpawnPoint(level, levelSpawnPoints, ref transitionScript.choosenSpawnPointA, ref transitionScript.choosenSpawnPointAIndex);
	}

	void SetDefaultSpawnPointB (Level level) {
		List<Transform> levelSpawnPoints = transitionScript.spawnPointsLevelB;
		AssignDefaultSpawnPoint(level, levelSpawnPoints, ref transitionScript.choosenSpawnPointB, ref transitionScript.choosenSpawnPointBIndex);
	}

	void AssignDefaultSpawnPoint (Level level, List<Transform> levelSpawnPoints, ref Transform choosenSpawnPoint, ref int choosenSpawnPointIndex) {
		string levelName = (level == _LevelA) ? "Level A" : "Level B";

		Undo.RecordObject(transitionScript, "Setting Spawn Point " + levelName);
		choosenSpawnPoint = levelSpawnPoints[_defaultSpawnPointIndex];
		choosenSpawnPointIndex = _defaultSpawnPointIndex;
		EditorUtility.SetDirty(transitionScript);
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}
}
