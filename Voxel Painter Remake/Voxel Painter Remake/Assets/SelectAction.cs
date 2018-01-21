using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//You probably still need to make it revert base on the state though
public class SelectAction : MonoBehaviour, IAction
{
    public List<SelectContainer> undoList = new List<SelectContainer>();
    public List<SelectContainer> redoList = new List<SelectContainer>();
    public HistoryTracker<List<ISelectable>> history = new HistoryTracker<List<ISelectable>>();

    public void Undo()
    {
        var element = history.Undo();

        foreach (var iSelectable in element.mInitial)
        {
            if (element.isActive)
                SelectionManager.Instance.Deselect(iSelectable);
            else
                SelectionManager.Instance.Select(iSelectable);
        }
    }

    public void Redo()
    {
        var element = history.Redo();

        foreach (var iSelectable in element.mInitial) {
            if (!element.isActive)
                SelectionManager.Instance.Deselect(iSelectable);
            else
                SelectionManager.Instance.Select(iSelectable);
        }
    }

    public void Deselect()
    {

    }

    public void Select()
    {

    }

    public bool Use()
    {
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            //SelectionManager.Instance.Clear();
        }

        //The thing is you are only selecting a face atm.
        ISelectable iSelectable = SelectionManager.Instance.OnClickDown();
        List<ISelectable> selectables = new List<ISelectable>();

        if (iSelectable != null)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                Block block = iSelectable.gameObject.transform.parent.GetComponent<Block>();
                foreach (Face face in block.faces)
                {
                    if (face.gameObject != iSelectable.gameObject)
                    {
                        SelectionManager.Instance.Select(face);
                        selectables.Add(face);
                    }
                }
            }

            selectables.Add(iSelectable);
            var instance = history.NewInstance<List<ISelectable>>();
            instance.isActive = SelectionManager.Instance.hashSelectable.Contains(iSelectable);
            instance.mInitial = selectables;
            history.undoList.Add(instance);

            return true;
        }
        else
        {
            return false;
        }
    }

    [System.Serializable]
    public class SelectContainer
    {
        public ISelectable lastSelected;
        public bool active;

        public SelectContainer(ISelectable iSelectable){
            lastSelected = iSelectable;
            active = SelectionManager.Instance.hashSelectable.Contains(lastSelected);
        }

    }
}
