using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Liminal;

public static class AudioExtensions{
    public static void PlayNewSourceInstance(AudioSource audioSource) {
        if (audioSource.isPlaying)
            PoolManager.Instance.NewGameObject(audioSource.gameObject, true);
        else
        {
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
        }
    }

    public static void SetVolume(AudioSource source, float v)
    {
        source.volume = v;
    }

    public static Coroutine SetVolume(MonoBehaviour mono, AudioSource source, float v, float length)
    {
        return mono.StartCoroutine(_LerpVolume(source, v, length));
    }

    public static IEnumerator _LerpVolume(AudioSource source, float v, float length)
    {
        float sourceVolume = source.volume;
        for (float i = 0; i < 1f; i += Time.deltaTime / length) {
            source.volume = Mathf.Lerp(sourceVolume, v, i);
            yield return null;
        }
    }

    public static Coroutine CrossFade(MonoBehaviour mono, AudioSource sourceA, AudioSource sourceB, float volume,float length) {
        return mono.StartCoroutine(_CrossFade(sourceA,sourceB,volume,length));
    }

    //Fades sourceA out while facing sourceB in
    public static IEnumerator _CrossFade(AudioSource sourceA, AudioSource sourceB,float volume,float length) {
        float sourceAVolume = sourceA.volume;
        float souorceBVolume = sourceB.volume;

        for (float i = 0; i < 1f; i += Time.deltaTime / length)
        {
            sourceA.volume = Mathf.Lerp(sourceAVolume, 0, i);
            sourceB.volume = Mathf.Lerp(souorceBVolume, volume, i);
            yield return null;
        }

        sourceA.volume = Mathf.Lerp(sourceAVolume, 0, 1);
        sourceB.volume = Mathf.Lerp(souorceBVolume, volume, 1);
    }

}
