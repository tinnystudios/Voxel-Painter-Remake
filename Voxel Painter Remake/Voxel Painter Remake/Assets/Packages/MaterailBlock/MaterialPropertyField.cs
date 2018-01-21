using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The only important thing is to call Init on Awake/Start
[System.Serializable]
public class MaterialBlockField
{
    public Renderer mRenderer;
    public List<string> properties = new List<string>();

    private Dictionary<string, int> propertyLookUps = new Dictionary<string, int>();
    private MaterialPropertyBlock mPropertyBlock = null;

    public void Init()
    {
        mPropertyBlock = new MaterialPropertyBlock();
        CacheIds();
    }

    public void Init(GameObject go) {
        mRenderer = go.GetComponentInChildren<Renderer>();
        Init();
    }

    public void CacheIds()
    {
        foreach (string s in properties)
        {
            if(!propertyLookUps.ContainsKey(s))
                propertyLookUps.Add(s, Shader.PropertyToID(s));
        }
    }

    public void SetColor(string id, Color c)
    {
        if (mPropertyBlock == null)
            Init();

        CheckLookUp(id);
        mPropertyBlock.SetColor(propertyLookUps[id], c);
        mRenderer.SetPropertyBlock(mPropertyBlock);
    }

    //Using the defined ID, if you call 0
    public void SetColor(int id, Color c)
    {
        SetColor(properties[id], c);
    }

    //Using the defined ID, if you call 0
    public Color GetColor(string id)
    {
        return mPropertyBlock.GetVector(propertyLookUps[id]);
    }

    public void SetFloat(string id, float f)
    {
        CheckLookUp(id);
        mPropertyBlock.SetFloat(propertyLookUps[id], f);
        mRenderer.SetPropertyBlock(mPropertyBlock);
    }

    public void CheckLookUp(string s)
    {
        if (!propertyLookUps.ContainsKey(s))
            propertyLookUps.Add(s, Shader.PropertyToID(s));
    }
}
