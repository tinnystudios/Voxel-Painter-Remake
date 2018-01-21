/// <summary>
/// Created by SWAN DEV
/// </summary>

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SelfAnimation : MonoBehaviour
{
	public enum SelfAnimType{
		None = 0,
		Move,
		Rotate,
		Scale,
	}
	public SelfAnimType m_SelfAnimType = SelfAnimType.None;

	public Vector3 fromValue;
	public Vector3 toValue;
	public float time = 0.5f;
	public SDemoAnimation.LoopType loop = SDemoAnimation.LoopType.None;

	public UnityEvent onComplete;

	// Use this for initialization
	void Start () {
		switch(m_SelfAnimType)
		{
		case SelfAnimType.Move:
			SDemoAnimation.Instance.Move(gameObject, fromValue, toValue, time, loop, OnComplete);
			break;

		case SelfAnimType.Rotate:
			SDemoAnimation.Instance.Rotate(gameObject, fromValue, toValue, time, loop, OnComplete);
			break;

		case SelfAnimType.Scale:
			SDemoAnimation.Instance.Scale(gameObject, fromValue, toValue, time, loop, OnComplete);
			break;
		}
	}

	void OnComplete()
	{
		if(onComplete != null) onComplete.Invoke();
	}

}
