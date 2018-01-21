using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionClearAction : MonoBehaviour, IAction {

    public HistoryTracker<List<ISelectable>> history = new HistoryTracker<List<ISelectable>>();

    public void Undo()
    {
        var element = history.Undo();

        foreach (var iSelectable in element.mInitial) {
            SelectionManager.Instance.Select(iSelectable);
        }
    }

    public void Redo()
    {
        var element = history.Redo();

        foreach (var iSelectable in element.mInitial)
            SelectionManager.Instance.Deselect(iSelectable);
    }

    public void Deselect()
    {

    }

    public void Select()
    {

    }

    public bool Use()
    {
        var newInstance = history.NewInstance<List<ISelectable>>();
        newInstance.mInitial = SelectionManager.Instance.hashSelectable.ToList();
        history.undoList.Add(newInstance);
        return true;
    }

}

[System.Serializable]
public class HistoryTracker<T>
{
    public ContainerList<Container<T>> undoList = new ContainerList<Container<T>>();
    public ContainerList<Container<T>> redoList = new ContainerList<Container<T>>();

    public Container<T> GetLast(ContainerList<Container<T>> list)
    {
        return list[list.Count - 1];
    }

    public Container<T> NewInstance<T>()
    {
        Container<T> newContainer = new Container<T>();
        return newContainer;
    }

    public Container<T> Undo()
    {
        var element = GetLast(undoList);

        undoList.Remove(element);
        redoList.Add(element);

        return element;
    }
    public Container<T> Redo()
    {
        var element = GetLast(redoList);

        undoList.Add(element);
        redoList.Remove(element);

        return element;
    }
}

public class ContainerList<T> : List<T>
{
    public List<T> list = new List<T>();
}

public class Container<T>
{
    public T mInitial;
    public bool isActive = false;
}
