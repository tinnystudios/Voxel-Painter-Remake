/// <summary>
/// Created by SWAN Dev
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Dynamic UI Label.
/// </summary>
public class DLabel : MonoBehaviour {
	public Text title;

	#if UNITY_EDITOR
	//for checking label height at development runtime, 
	//check the boolean forceUpdate in inspector to update value.
	public float m_LabelHeight = 0f;
	public bool forceUpdate = false; 
	void Update()
	{
		if(forceUpdate)
		{
			forceUpdate = false;
			m_LabelHeight = LabelHeight;
		}
	}
	#endif

	public void SetText(string text)
	{
		title.text = text;
	}

	public string GetText()
	{
		return title.text;
	}

	public int GetInt()
	{
		string tempText = title.text;

		int num = 255;
		int.TryParse(tempText, out num);
		return num;
	}

	public void ClearText()
	{
		title.text = "";
	}

	public float LabelHeight
	{
		get{
			// Update the canvases to get the updated rect of the text
			Canvas.ForceUpdateCanvases();
			return title.preferredHeight;
		}
	}
}
