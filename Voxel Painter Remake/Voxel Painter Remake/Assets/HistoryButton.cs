using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HistoryButton : MonoBehaviour, IPointerUpHandler, IPointerClickHandler
{
    public HistoryActionContainer actionContainer;
    public Text textName;
    public Image image;
    private Color imageColor;
    private Color currentColor;
    private bool isUndo = false;

    public void Init (HistoryActionContainer container) {
        actionContainer = container;
        textName.text = container.action.Result.ToString();
        imageColor = image.color;
        Show();

        gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(isUndo)
            HistoryManager.Instance.OrderedUndo(actionContainer);
        else
            HistoryManager.Instance.OrderedRedo(actionContainer);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void Show() {
        isUndo = true;
        currentColor = imageColor;
        currentColor.a = 1.0F;

        image.color = currentColor;
    }

    public void Hide() {
        isUndo = false;

        currentColor = imageColor;
        currentColor.a = 0.2F;

        image.color = currentColor;
    }

    public void Clear() {
        gameObject.SetActive(false);
    }
}
