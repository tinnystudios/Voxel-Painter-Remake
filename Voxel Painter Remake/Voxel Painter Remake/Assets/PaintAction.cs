using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintAction : MonoBehaviour, IAction
{
    public List<PaintContainerList> undoList = new List<PaintContainerList>();
    public List<PaintContainerList> redoList = new List<PaintContainerList>();

    public void Undo()
    {
        var element = undoList[undoList.Count - 1];

        foreach (var paintContainer in element.list) {
            paintContainer.face.SetColor(paintContainer.initialColor);
        }

        undoList.Remove(element);
        redoList.Add(element);
    }

    public void Redo()
    {
        var element = redoList[redoList.Count - 1];

        foreach (var paintContainer in element.list)
        {
            paintContainer.face.SetColor(paintContainer.newColor);
        }

        undoList.Add(element);
        redoList.Remove(element);
    }

    public void Select()
    {

    }

    public void Deselect()
    {

    }

    public bool Use()
    {
        var paintContainers = new PaintContainerList();
        List<GameObject> objectsToPaint = new List<GameObject>(SelectionManager.Instance.selectedGameObjects);

        if (SelectionManager.Instance.selectedGameObjects.Count <= 0)
        {
            ISelectable iSelectable = SelectionManager.Instance.TryFindSelectableFromRay();
            if(iSelectable != null)
                objectsToPaint.Add(iSelectable.gameObject);
            else
                return false;
        }

        foreach (GameObject selectable in objectsToPaint) {
            Face face = selectable.gameObject.GetComponent<Face>(); //IColor interface?
            if (face != null)
            {
                var paintContainer = new PaintContainer();

                paintContainer.face = face;
                paintContainer.initialColor = face.color;
                paintContainer.newColor = ColorManager.Instance.primaryColor;

                face.color = ColorManager.Instance.primaryColor;

                paintContainers.list.Add(paintContainer);
            }
        }

        undoList.Add(paintContainers);
        SelectionManager.Instance.Clear();

        return true;
    }

    public void AddFace(Face face) {
        face.SetColor(ColorManager.Instance.primaryColor);
    }

    [System.Serializable]
    public class PaintContainer {
        public Face face;
        public Color initialColor;
        public Color newColor;
    }

    [System.Serializable]
    public class PaintContainerList {
        public List<PaintContainer> list = new List<PaintContainer>();
    }
}
