using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorManager : Singleton<ColorManager> {
    
    public Color primaryColor = Color.black;
    public Color secondaryColor = Color.white;
    public GameObject colorInfoMenu;
    public Image colorSelectorImage;
    public AColorPicker picker;
    public GameObject[] disableThese;

    private void Awake()
    {
        colorInfoMenu.SetActive(false);
        picker.selectedColorDisplay.color = primaryColor;
    }

    public void ToggleColorInfoMenu() {
        colorInfoMenu.SetActive(!colorInfoMenu.activeInHierarchy);
        foreach (GameObject g in disableThese)
            g.SetActive(!colorInfoMenu.activeInHierarchy);
    }

    public void Ok() {
        primaryColor = picker.CurrentColor;
        ToggleColorInfoMenu();
    }

    public void Cancel() {
        ToggleColorInfoMenu();
    }
}
