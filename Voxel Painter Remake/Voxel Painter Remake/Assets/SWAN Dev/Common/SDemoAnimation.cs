/// <summary>
/// Created by SWAN DEV
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class SDemoAnimation : MonoBehaviour
{
	private static SDemoAnimation _instance = null;
	public static SDemoAnimation Instance
	{
		get{
			if(_instance == null)
			{
				_instance = new GameObject("[SDemoAnimation]").AddComponent<SDemoAnimation>();
			}
			return _instance;
		}
	}

	public enum LoopType
	{
		None = 0,
		Loop,
		PingPong,
	}

	public void Move(GameObject targetGO, Vector3 fromPosition, Vector3 toPosition, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		StartCoroutine(_Move(targetGO, fromPosition, toPosition, time, loop, onComplete));
	}

	public IEnumerator _Move(GameObject targetGO, Vector3 fromPosition, Vector3 toPosition, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		targetGO.transform.localPosition = fromPosition;
		float elapsedTime = 0;
		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;
			targetGO.transform.localPosition = Vector3.Lerp(fromPosition, toPosition, (elapsedTime / time));
			yield return new WaitForEndOfFrame();
		}
		if(onComplete != null) onComplete(); 

		if(loop == LoopType.Loop) StartCoroutine(_Move(targetGO, fromPosition, toPosition, time, loop, onComplete));
		else if(loop == LoopType.PingPong) StartCoroutine(_Move(targetGO, toPosition, fromPosition, time, loop, onComplete));
	}

	public void Scale(GameObject targetGO, Vector3 fromScale, Vector3 toScale, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		StartCoroutine(_Scale(targetGO, fromScale, toScale, time, loop, onComplete));
	}
	public IEnumerator _Scale(GameObject targetGO, Vector3 fromScale, Vector3 toScale, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		targetGO.transform.localScale = fromScale;
		float elapsedTime = 0;
		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;
			targetGO.transform.localScale = Vector3.Lerp(fromScale, toScale, (elapsedTime / time));
			yield return new WaitForEndOfFrame();
		}
		if(onComplete != null) onComplete(); 

		if(loop == LoopType.Loop) StartCoroutine(_Scale(targetGO, fromScale, toScale, time, loop, onComplete));
		else if(loop == LoopType.PingPong) StartCoroutine(_Scale(targetGO, toScale, fromScale, time, loop, onComplete));
	}

	public void Rotate(GameObject targetGO, Vector3 fromEulerAngle, Vector3 toEulerScale, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		StartCoroutine(_Rotate(targetGO, fromEulerAngle, toEulerScale, time, loop, onComplete));
	}
	public IEnumerator _Rotate(GameObject targetGO, Vector3 fromEulerAngle, Vector3 toEulerAngle, float time, LoopType loop = LoopType.None, Action onComplete = null)
	{
		targetGO.transform.localEulerAngles = fromEulerAngle;
		float elapsedTime = 0;
		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;
			targetGO.transform.localEulerAngles = Vector3.Lerp(fromEulerAngle, toEulerAngle, (elapsedTime / time));
			yield return new WaitForEndOfFrame();
		}
		if(onComplete != null) onComplete(); 

		if(loop == LoopType.Loop) StartCoroutine(_Rotate(targetGO, fromEulerAngle, toEulerAngle, time, loop, onComplete));
		else if(loop == LoopType.PingPong) StartCoroutine(_Rotate(targetGO, toEulerAngle, fromEulerAngle, time, loop, onComplete));
	}
}