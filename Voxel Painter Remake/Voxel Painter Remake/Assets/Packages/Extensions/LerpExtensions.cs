using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//GoTween. 
public static class LerpExtensions {
    #region Lerp Float with CoroutineInfo

    public static IEnumerator _LerpFloat(CoroutineInfo<float> info, float b, float length) {
        float a = info.value;
        for (float i = 0; i < 1f; i += Time.deltaTime/length) {
            info.value = Mathf.Lerp(a, b, i);
            info.progress = i;
            yield return null;
        }

        info.progress = 1;
        info.value = b;
    }

    public static CoroutineInfo<float> LerpFloat(MonoBehaviour mono,float b,float length) {
        CoroutineInfo<float> info = new CoroutineInfo<float>();
        info.coroutine = mono.StartCoroutine(_LerpFloat(info,b, length));
        return info; 
    }

    #endregion

    public static IEnumerator _LFloat(System.Action<float> callback, float a,float b,float length) {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        for (float i = 0; i <= 1f; i += Time.deltaTime / length)
        {
            //Use Clamp01
            float value = Mathf.Lerp(a, b, curve.Evaluate(i)); //Sub with a curve later

            if (callback != null)
                callback(value);

            yield return null;
        }

        if (callback != null)
            callback(b);
    }

    //Beware of garbage...
    //Return coroutine 
    public static Coroutine LFloat(MonoBehaviour mono, System.Action<float> callback,float a, float b, float length) {
        return mono.StartCoroutine(_LFloat(callback,a,b, length));
    }


    public static Coroutine LerpColor(MonoBehaviour mono, System.Action<Color> callback, Color a, Color b, float length) {
        return mono.StartCoroutine(_LerpColor(callback, a, b, length));
    }

    public static IEnumerator _LerpColor(System.Action<Color> callback, Color a, Color b, float length)
    {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        for (float i = 0; i <= 1f; i += Time.deltaTime / length)
        {
            Color value = Color.Lerp(a, b, curve.Evaluate(i)); //Sub with a curve later

            if (callback != null) callback(value);

            yield return null;
        }

        if (callback != null)
            callback(b);
    }

    public static Coroutine LerpVector3(MonoBehaviour mono, System.Action<Vector3> callback, Vector3 a, Vector3 b, float length)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        return mono.StartCoroutine(_LerpVector3(callback, a, b, curve, length));
    }

    public static Coroutine LerpVector3(MonoBehaviour mono, System.Action<Vector3> callback, Vector3 a, Vector3 b,AnimationCurve curve, float length)
    {
        return mono.StartCoroutine(_LerpVector3(callback, a, b,curve, length));
    }

    public static IEnumerator _LerpVector3(System.Action<Vector3> callback, Vector3 a, Vector3 b,AnimationCurve curve, float length)
    {
        
        for (float i = 0; i <= 1f; i += Time.deltaTime / length)
        {
            Vector3 value = Vector3.Lerp(a, b, curve.Evaluate(i)); //Sub with a curve later
            if (callback != null) callback(value);
            yield return null;
        }

        if (callback != null)
            callback(b);
    }
}

//Pass in both
public class CoroutineInfo<T>
{
    public T value;
    public float progress = 0;
    public Coroutine coroutine;
}

