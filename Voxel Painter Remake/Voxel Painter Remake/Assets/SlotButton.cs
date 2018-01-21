using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using core;
using UnityEngine.UI;
public class SlotButton : MonoBehaviour, IPointerClickHandler {
    public Image image;
    public core.Action action;
    public GameObject highlight;
    public void OnPointerClick(PointerEventData eventData)
    {
        ActionManager.Instance.SelectAction(action);
    }
}
