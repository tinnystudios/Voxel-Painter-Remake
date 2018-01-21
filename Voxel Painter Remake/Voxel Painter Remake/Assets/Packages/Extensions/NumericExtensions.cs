using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinMaxFloat
{
    public float min = 0;
    public float max = 1;

    public MinMaxFloat(float m, float mM){
        min = m;
        max = mM;
    }

    public float GetRandomBetween() {
        return Random.Range(min, max);
    }
}

[System.Serializable]
public class MinMaxInt
{
    public int min = 0;
    public int max = 1;
}