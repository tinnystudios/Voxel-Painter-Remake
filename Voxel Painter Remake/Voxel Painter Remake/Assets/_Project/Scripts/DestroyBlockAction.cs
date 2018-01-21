using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBlockAction : MonoBehaviour, IAction
{
    public SelectionManager selectionManager;

    public List<List<GameObject>> undoList = new List<List<GameObject>>();
    public List<List<GameObject>> redoList = new List<List<GameObject>>();

    public void Undo()
    {
        var gos = undoList[undoList.Count - 1];

        foreach (var go in gos)
            go.SetActive(true);

        undoList.Remove(gos);
        redoList.Add(gos);
    }

    public void Redo()
    {
        var gos = redoList[redoList.Count - 1];

        foreach (var go in gos)
            go.SetActive(false);

        undoList.Add(gos);
        redoList.Remove(gos);
    }



    public void Deselect()
    {

    }

    public void Reset()
    {
  
    }

    public void Select()
    {

    }

    public bool Use()
    {
        List<GameObject> undoObjects = new List<GameObject>();

        if (selectionManager.selectedGameObjects.Count <= 0)
        {
            ISelectable iSelectable = SelectionManager.Instance.OnClickDown();

            if (iSelectable == null)
                return false;
        }

        foreach (GameObject selectable in selectionManager.selectedGameObjects) {
            if (selectable.GetComponent<Face>())
            {
                GameObject block = selectable.transform.parent.gameObject;
                block.SetActive(false);
                undoObjects.Add(block);
            }
        }

        undoList.Add(undoObjects);
        redoList.Clear();
        selectionManager.Clear();

        return true;
    }

}

