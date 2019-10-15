using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTextSortingLayer : MonoBehaviour
{
	private const string _TextSortingLayer = "OverlayText";
	void Awake () {
		GetComponent<MeshRenderer>().sortingLayerName = _TextSortingLayer;
	}
}
