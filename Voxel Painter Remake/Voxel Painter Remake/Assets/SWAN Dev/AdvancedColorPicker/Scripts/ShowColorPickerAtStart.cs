using UnityEngine;
using System.Collections;

public class ShowColorPickerAtStart : MonoBehaviour
{
	AColorPicker picker;

	void Start()
	{
		picker = AColorPicker.Create(null);
		picker.Setup(null);
	}

	void Update()
	{
		if(picker)
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				Debug.Log("Pressed ESCAPE to close color picker. Press SPACE to create color picker.");
				picker.Close();
			}
		}
		else
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				Debug.Log("Pressed SPACE to create color picker.");
				Start();
			}
		}
	}

}
