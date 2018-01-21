using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformExtensions{
    public static Coroutine FixedLerpTransform(MonoBehaviour mono, Transform t, Vector3 b, float length, AnimationCurve curve,TransformType transformType)
    {
        return mono.StartCoroutine(_FixedLerpTransform(t, b, length, curve,transformType));
    }

    public static Coroutine FixedLerpTransform(MonoBehaviour mono, Transform t, Vector3 b, float length, TransformType transformType)
    {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        return mono.StartCoroutine(_FixedLerpTransform(t, b, length, curve,transformType));
    }

    public static IEnumerator _FixedLerpTransform(Transform t, Vector3 b, float length, AnimationCurve curve, TransformType transformType)
    {
        Vector3 a = GetTransformValue(t, transformType);

        for (float i = 0; i < 1f; i += Time.deltaTime / length)
        {
            if (i + (Time.deltaTime / length) >= 1)
                i = 1;
            Vector3 value = Vector3.Lerp(a, b, curve.Evaluate(i));
            ApplyToTransform(t, transformType, value);
            yield return null;
        }
    }

    public static void ApplyToTransform(Transform t, TransformType transType, Vector3 value) {
        switch (transType) {
            case TransformType.position:
                t.position = value;
                break;
            case TransformType.scale:
                t.localScale = value;
                break;
        }
    }

    public static Vector3 GetTransformValue(Transform t,TransformType transType)
    {
        Vector3 value = Vector3.zero;

        switch (transType)
        {
            case TransformType.position:
                value = t.position;
                break;
            case TransformType.scale:
                value = t.localScale;
                break;
        }
        return value;
    }

    public static Coroutine LerpTransform(MonoBehaviour mono, Transform t, Vector3 b, float length)
    {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        return mono.StartCoroutine(_LerpTransform(t, b, length, curve));
    }

    public static Coroutine LerpTransform(MonoBehaviour mono, Transform t, Vector3 b, float length, AnimationCurve curve)
    {
        return mono.StartCoroutine(_LerpTransform(t, b, length,curve));
    }
    public static IEnumerator _LerpTransform(Transform t, Vector3 b, float length) {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        yield return _LerpTransform(t, b, length, curve);
    }

    public static IEnumerator _LerpTransform(Transform t, Vector3 b, float length, AnimationCurve curve)
    {
        Vector3 a = t.position;
        Vector3 displacement = Vector3.zero;
        Vector3 curPosition = a;
        Vector3 prevPosition = a;

        for (float i = 0; i < 1f; i += Time.deltaTime / length) {

            if (i + (Time.deltaTime / length) >= 1)
                i = 1;

            curPosition = Vector3.Lerp(a, b, curve.Evaluate(i));
            displacement = curPosition - prevPosition;
            t.position += displacement;
            prevPosition = curPosition;

            yield return null;
        }
    }

    public static Coroutine LerpLocalPosition(MonoBehaviour mono, Transform t, Vector3 b, AnimationCurve curve,float length)
    {
        return mono.StartCoroutine(_LerpLocalPosition(t, b, length, curve));
    }

    public static Coroutine LerpLocalPosition(MonoBehaviour mono, Transform t, Vector3 b, float length)
    {
        AnimationCurve curve = AnimationCurveExtensions.EaseInOut();
        return mono.StartCoroutine(_LerpLocalPosition(t, b, length, curve));
    }

    public static IEnumerator _LerpLocalPosition(Transform t, Vector3 b, float length, AnimationCurve curve)
    {
        Vector3 a = t.localPosition;
        Vector3 displacement = Vector3.zero;
        Vector3 curPosition = a;
        Vector3 prevPosition = a;

        for (float i = 0; i < 1f; i += Time.deltaTime / length)
        {

            if (i + (Time.deltaTime / length) >= 1)
                i = 1;

            curPosition = Vector3.Lerp(a, b, curve.Evaluate(i));
            displacement = curPosition - prevPosition;
            t.localPosition += displacement;
            prevPosition = curPosition;

            yield return null;
        }
    }

    public static void LookAt(Transform t, Vector3 position) {
        position.Normalize();
        t.LookAt(position);
    }

    public static Transform[] GetTransformArray(MonoBehaviour[] list) {
        Transform[] transforms = new Transform[list.Length];
        for (int i = 0; i < list.Length; i++) 
            transforms[i] = list[i].transform;
        return transforms;
    }
}

public enum TransformType {
    position,
    rotation,
    scale
}
