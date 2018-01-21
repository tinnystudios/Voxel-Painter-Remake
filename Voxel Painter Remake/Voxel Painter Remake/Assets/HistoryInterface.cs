using core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryInterface : MonoBehaviour
{
    private HistoryManager historyManager;
    public HistoryButton buttonPrefab;
    public RectTransform content;
    public Dictionary<HistoryActionContainer, HistoryButton> undoLookUp = new Dictionary<HistoryActionContainer, HistoryButton>();

    private void Awake()
    {
        historyManager = GetComponent<HistoryManager>();
        historyManager.OnAdd += OnAdd;
        historyManager.OnUndo += HistoryManager_OnUndo;
        historyManager.OnRedo += HistoryManager_OnRedo;
        historyManager.OnClearRedo += HistoryManager_OnClearRedo;
    }

    //OnAdd
    private void OnAdd(HistoryActionContainer actionContainer)
    {
        HistoryButton button = Instantiate(buttonPrefab, content);
        button.Init(actionContainer);
        undoLookUp.Add(actionContainer, button);
        button.actionContainer = actionContainer;
    }

    //Clean up redo
    private void HistoryManager_OnClearRedo(List<HistoryActionContainer> actionContainers)
    {
        foreach (HistoryActionContainer actionContainer in actionContainers)
        {
            undoLookUp[actionContainer].Clear();
            undoLookUp.Remove(actionContainer);
        }
    }

    //Onredo
    private void HistoryManager_OnRedo(HistoryActionContainer action)
    {
        undoLookUp[action].Show(); 
    }

    //On undo
    private void HistoryManager_OnUndo(HistoryActionContainer action)
    {
        undoLookUp[action].Hide();
    }

}
