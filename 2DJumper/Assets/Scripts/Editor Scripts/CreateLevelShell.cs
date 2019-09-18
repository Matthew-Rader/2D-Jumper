using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateLevelShell : MonoBehaviour
{
	[MenuItem("PrefabGeneration")]

	static void Create()
    {
		for (int x = 0; x != 10; x++)
		{
			GameObject go = new GameObject("MyCreatedGO" + x);
			go.transform.position = new Vector3(x, 0, 0);
		}
	}
}
