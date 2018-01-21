using core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : Singleton<HistoryManager> {
    public delegate void HistoryAction(HistoryActionContainer action);
    public delegate void HistoryActionList(List<HistoryActionContainer> actionContainers);

    public event HistoryAction OnAdd; // When a history has been added
    public event HistoryAction OnUndo; // When a history has been added
    public event HistoryAction OnRedo; // When a history has been added
    public event HistoryActionList OnClearRedo; // When a history has been added

    public List<HistoryActionContainer> undoList;
    public List<HistoryActionContainer> redoList;

    public void AddAction(Action action) {
        HistoryActionContainer actionContainer = new HistoryActionContainer(action);

        undoList.Add(actionContainer);

        if (OnAdd != null)
            OnAdd.Invoke(actionContainer);

        ClearRedoList();
    }

    public void Undo(HistoryActionContainer actionContainer) {

        redoList.Add(actionContainer);
        undoList.Remove(actionContainer);
        actionContainer.action.Result.Undo();

        if (OnUndo != null)
            OnUndo.Invoke(actionContainer);
    }

    public void OrderedUndo(HistoryActionContainer actionContainer) {
        //Find where it is on the list.
        int index = undoList.IndexOf(actionContainer);
        int lastIndex = undoList.Count - 1;
        List<HistoryActionContainer> cacheList = new List<HistoryActionContainer>(undoList);

        for (int i = lastIndex; i > index - 1; i--)
            Undo(cacheList[i]);
    }


    public void Undo() {
        if (undoList.Count <= 0)
            return;

        Undo(undoList[undoList.Count - 1]);
    }

    public void Redo() {
        if (redoList.Count <= 0)
            return;

        HistoryActionContainer actionContainer = redoList[redoList.Count - 1];
        Redo(actionContainer);
    }

    public void Redo(HistoryActionContainer actionContainer) {
        undoList.Add(actionContainer);
        redoList.Remove(actionContainer);
        actionContainer.action.Result.Redo();

        if (OnRedo != null)
            OnRedo.Invoke(actionContainer);
    }

    public void OrderedRedo(HistoryActionContainer actionContainer)
    {
        //Find where it is on the list.
        int index = redoList.IndexOf(actionContainer);
        int lastIndex = redoList.Count - 1;
        List<HistoryActionContainer> cacheList = new List<HistoryActionContainer>(redoList);

        for (int i = lastIndex; i > index - 1; i--)
            Redo(cacheList[i]);
    }

    //When you do an action, clear the redo list.
    public void ClearRedoList() {
        if (OnClearRedo != null)
            OnClearRedo.Invoke(redoList);

        redoList.Clear();
    }
}

[System.Serializable]
public class HistoryActionContainer
{
    public Action action;
    public HistoryActionContainer(Action a) {
        action = a;
    }
}
