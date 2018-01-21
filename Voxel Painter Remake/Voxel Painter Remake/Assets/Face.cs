using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour, ISelectable {
    public MaterialBlockField materialBlock;
    public Color color = Color.white;
    private bool isSelected = false;

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
    }

    void Awake() {
        materialBlock.Init(gameObject);
    }

    public void Deselect()
    {
        materialBlock.SetColor("_Color", color);
        isSelected = false;
    }

    public void HoverExit() {
        materialBlock.SetColor("_Color", color);
    }

    public void Select()
    {
        materialBlock.SetColor("_Color",Color.green);
        isSelected = true;
    }

    public void Hover()
    {
        materialBlock.SetColor("_Color", Color.yellow);
    }

    public void SetColor(Color c) {
        color = c;
        materialBlock.SetColor("_Color", color);
    }
}
