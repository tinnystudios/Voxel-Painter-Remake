using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core;
public class ToolManager : MonoBehaviour {
    public List<SlotContainer> slots;

    private void Awake()
    {
        ActionManager.Instance.OnActionChanged += Instance_OnActionChanged;

        foreach (SlotContainer slot in slots)
            slot.Init();
    }
    private void Update()
    {

        foreach (var slot in slots)
        {
            if (slot.action.Object == ActionManager.Instance.selectedAction.Object)
                slot.slotButton.highlight.SetActive(true);
            else
                slot.slotButton.highlight.SetActive(false);
        }

    }

    private void Instance_OnActionChanged(Action action)
    {

    }

    [System.Serializable]
    public class SlotContainer {
        public core.Action action;
        public SlotButton slotButton;
        public Sprite sprite;

        public void Init() {
            slotButton.image.sprite = sprite;
            slotButton.action = action;
        }
    }
}
