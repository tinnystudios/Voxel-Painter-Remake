using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreationManager : Singleton<CreationManager> {
    public CreateBlockAction createAction;
    public Text textSize;
    public float mSize = 1;
    public List<float> sizes;
    
    private void Awake()
    {
        float s = 0.125F;
        for (int i = 1; i < 9; i++) {
            sizes.Add(s);
            s *= 2;
        }
    }

    public void SetSize(float index) {
        mSize = sizes[(int)index];
        createAction.size = mSize;
        textSize.text = mSize.ToString();
    }
}
