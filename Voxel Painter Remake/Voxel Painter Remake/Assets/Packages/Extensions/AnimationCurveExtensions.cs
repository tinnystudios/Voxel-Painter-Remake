using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationCurveExtensions{

    public static AnimationCurve InverseCurve(AnimationCurve curve) {
        AnimationCurve reversedCurve = new AnimationCurve();

        //Get Time/Value and reverse the value
        float[] time = new float[curve.keys.Length];
        float[] value = new float[curve.keys.Length];

        int index = curve.keys.Length - 1;
        for (int i = 0; i < curve.keys.Length; i++)
        {
            time[i] = curve.keys[i].time;
            value[i] = curve.keys[index].time;
            reversedCurve.AddKey(time[i], value[i]);
            index--;
        }

        return reversedCurve;
    }

    public static AnimationCurve LinearCurve() {
        return AnimationCurve.Linear(0, 0, 1, 1);
    }
    public static AnimationCurve EaseInOut()
    {
        return AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}
