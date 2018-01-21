using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public Transform blockGroup;
	public GameObject gridPrefab;
	public float row = 20;
	public float col = 20;

	// Use this for initialization
	void Start ()
	{
		for (int y = 0; y < col; y++) {
			for (int x = 0; x < row; x++) {
				GameObject newGrid = Instantiate (gridPrefab);
				newGrid.transform.SetParent (blockGroup);
				newGrid.transform.position = new Vector3 (x, -1, y);
			}
		}
	}
}
