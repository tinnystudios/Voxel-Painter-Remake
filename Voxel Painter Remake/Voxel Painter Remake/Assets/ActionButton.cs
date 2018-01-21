using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using core;
using System;
using Action = core.Action;

public class ActionButton : MonoBehaviour, IPointerClickHandler {
    public OnClick m_OnClick;
    public Action action;

    public void OnPointerClick(PointerEventData eventData)
    {
        m_OnClick.Invoke(action);
    }

    [System.Serializable]
    public class OnClick : UnityEvent<Action> {
        
    }
}
