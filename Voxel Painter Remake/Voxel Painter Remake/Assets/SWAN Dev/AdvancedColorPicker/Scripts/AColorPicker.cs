/// <summary>
/// Created by SWAN Dev
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using System.IO;

public class AColorPicker : MonoBehaviour 
{
	public Transform containerT;
	public SaveFileFormat saveFileFormat = SaveFileFormat.JPG;

	public bool hasAlpha = false;
	/// <summary>
	/// Show/Hide the alpha setting in the palette.
	/// </summary>
	/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
	public void ToggleAlphaGradient(bool isEnabled)
	{
		sliderA.gameObject.SetActive(isEnabled);
		labelA.transform.parent.gameObject.SetActive(isEnabled);
		hasAlpha = isEnabled;
	}

	public bool hasPickerRect = true;
	public int pickRectSize = 128;
	public InputField pickRectSizeInputField;
	/// <summary>
	/// Show/Hide the picker rect for picking color on the screen.
	/// </summary>
	/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
	public void TogglePickerRect(bool isEnabled)
	{
		rectPicker.gameObject.SetActive(isEnabled);
		displayReadPixel.transform.parent.gameObject.SetActive(isEnabled);
	}
	/// <summary>
	/// Set the size of the touch picker rect for reading pixel from screen
	/// </summary>
	/// <param name="size">Size.</param>
	public void SetPickerRectSize(int size)
	{
		if(size > Screen.width || size > Screen.height)
		{
			size = Mathf.Min(Screen.width, Screen.height);
		}

		int border = 0;
		rectPickerReadPixelArea.sizeDelta = new Vector2(size + border, size + border) / canvasScale;
		pickRectSize = size;
		if(pickRectSizeInputField != null && !pickRectSizeInputField.text.Equals(pickRectSize.ToString()))
		{
			pickRectSizeInputField.text = pickRectSize.ToString();
		}
	}
	public void OnPickerRectInputFieldEndEdit(InputField inputField)
	{
		int newSize = 1;
		if(!int.TryParse(inputField.text, out newSize))
		{
			newSize = 1;
		}

		if(newSize < 1)
		{
			newSize = 1;
			inputField.text = newSize.ToString();
		}

		pickRectSize = newSize;
		SetPickerRectSize(pickRectSize);
	}

	private bool enableRectPicker = false;
	private Texture2D readTex;
	private float nextUpdateTime_DisplayReadPixel = 0;

	public DLabel labelHexCode;
	private Texture2D colorSpaceTex;
	public RawImage colorSpace;
	private Texture2D alphaGradientTex;
	public RawImage alphaGradient;

	// Palette textures, add your textures in these lists, 
	// call ChangePaletteColorTexture()/ChangePaletteAlphaGradientTexture to change textures
	public List<Texture2D> paletteColorTextures = new List<Texture2D>();
	public List<Texture2D> paletteAlphaGradientTextures = new List<Texture2D>();

	public Transform defaultColorContainer;	
	private Image[] defaultColors;

	public Image selectedColorDisplay;
	public Image displayReadPixel;

	public Slider sliderR;
	public Slider sliderG;
	public Slider sliderB;
	public Slider sliderA;

	public DLabel labelR;
	public DLabel labelG;
	public DLabel labelB;
	public DLabel labelA;

	public RectTransform rectPicker;				// Touch Picker Button
	public RectTransform rectPickerReadPixelArea;	// Read pixel rect of Touch Picker
	private Vector3 originPickerPos;
	public Text pickerBtnMessage;
	private Vector3 rectPos;

	public GameObject pallettePanelGO;

	public Button btn_Close;
	public Button btn_Save;

	public Image appCursor;

	private float canvasScale = 1f;

	/// <summary>
	/// Get the current picked color.
	/// </summary>
	/// <value>The color of the current.</value>
	public Color CurrentColor
	{
		get{
			return selectedColorDisplay.color;
		}
	}

	/// <summary>
	/// Get the current picked color sample texture.
	/// </summary>
	/// <value>The current picked color sample.</value>
	public Texture2D CurrentPickedColorSample
	{
		get{
			return readTex;
		}
	}


	/// <summary>
	/// Create an instance of AColorPicker, and set parent.
	/// </summary>
	/// <param name="parentT">The container/parent for this instance.</param>
	/// <param name="prefabName">The prefab in resources folder, name must match</param>
	public static AColorPicker Create(Transform parentT, string prefabName = "AColorPickerUGUI_AutoFit_Prefab")
	{
		GameObject prefab = Resources.Load(prefabName) as GameObject;
		AColorPicker gifPanel = _InstantiatePrefab<AColorPicker>(prefab);
		gifPanel.transform.SetParent(parentT);
		if(parentT) gifPanel.transform.rotation = parentT.rotation;
		gifPanel.transform.localScale = Vector3.one;
		gifPanel.transform.localPosition = Vector3.zero;
		return gifPanel;
	}

	private static T _InstantiatePrefab<T>(GameObject prefab) where T: MonoBehaviour
	{
		if(prefab != null)
		{
			GameObject go = GameObject.Instantiate(prefab) as GameObject;
			if(go != null)
			{
				go.name = "[Prefab]" + prefab.name;
				go.transform.localScale = Vector3.one;
				return go.GetComponent<T>();
			}
			else
			{	
				Debug.Log("prefab is null!") ;
				return null ;
			}
		}
		else
			return null ;
	}

	bool hasSetup = false;
	void Start()
	{
		Setup();
	}

	public void Setup(bool hasAlpha, Action onCloseAction = null)
	{
		Setup(hasAlpha, false, onCloseAction);
	}

	public void Setup(bool hasAlpha, bool hasPickerRect, Action onCloseAction = null)
	{
		this.hasAlpha = hasAlpha;
		this.hasPickerRect = hasPickerRect;
		Setup(onCloseAction);
	}

	public void Setup(bool hasAlpha, int pickerRectSize = 128, Action onCloseAction = null)
	{
		this.hasAlpha = hasAlpha;
		this.hasPickerRect = true;
		this.pickRectSize = Mathf.Max(1, pickerRectSize);
		Setup(onCloseAction);
	}

	// Use this for initialization
	public void Setup(Action onCloseAction = null) 
	{
		if(hasSetup) return;
		hasSetup = true;

		_onCloseAction = onCloseAction;

		transform.localScale = Vector3.one;

		colorSpaceTex = (Texture2D) colorSpace.mainTexture;
		alphaGradientTex = (Texture2D) alphaGradient.mainTexture;

		StartCoroutine(_SetScales());
		btn_Save.transform.parent.gameObject.SetActive(false);

		defaultColors = defaultColorContainer.GetComponentsInChildren<Image>();

		if(EventSystem.current == null)
		{
			GameObject go = new GameObject("[EventSystem]");
			go.AddComponent<EventSystem>();
			go.AddComponent<StandaloneInputModule>();
			cursor = new PointerEventData(EventSystem.current);
		}

		ToggleAlphaGradient(hasAlpha);
		TogglePickerRect(hasPickerRect);

		gameObject.SetActive(true);
	}

	/// <summary>
	/// Change the palette color texture (by index).
	/// </summary>
	/// <param name="index">Index of the texture in the paletteColorTextures list.</param>
	public void ChangePaletteColorTexture(int index)
	{
		if(paletteColorTextures != null && paletteColorTextures.Count > index)
		{
			ChangePaletteColorTexture(paletteColorTextures[index]);
		}
		else
		{
			Debug.LogWarning("Texture not found! Index: " + index);
		}
	}

	/// <summary>
	/// Change the palette alpha gradient texture (by index).
	/// </summary>
	/// <param name="index">Index of the texture in the paletteAlphaGradientTextures list.</param>
	public void ChangePaletteAlphaGradientTexture(int index)
	{
		if(paletteAlphaGradientTextures != null && paletteAlphaGradientTextures.Count > index)
		{
			ChangePaletteAlphaGradientTexture(paletteAlphaGradientTextures[index]);
		}
		else
		{
			Debug.LogWarning("Texture not found! Index: " + index);
		}
	}

	/// <summary>
	/// Change the palette color texture with a new texture.
	/// </summary>
	public void ChangePaletteColorTexture(Texture2D newTexture)
	{
		colorSpaceTex = newTexture;
		colorSpace.texture = colorSpaceTex;
		colorSpace.SetNativeSize();
	}

	/// <summary>
	/// Change the palette alpha gradient texture with a new texture.
	/// </summary>
	public void ChangePaletteAlphaGradientTexture(Texture2D newTexture)
	{
		alphaGradientTex = newTexture;
		alphaGradient.texture = alphaGradientTex;
		alphaGradient.SetNativeSize();
	}

	IEnumerator _SetScales()
	{
		//waiting for next frame for setting scales, ensure the prefab transform scales are updated
		yield return new WaitForEndOfFrame();
		canvasScale = transform.lossyScale.x;

		SetPickerRectSize(pickRectSize);
	}

	PointerEventData cursor = new PointerEventData(EventSystem.current);
	List<RaycastResult> objectsHit = new List<RaycastResult> ();
	Color color;
	//Update is called once per frame
	void Update () 
	{
		bool isHeldDown = Input.GetMouseButton(0);

		cursor.position = Input.mousePosition;
		if(enableRectPicker)
		{
			rectPos = cursor.position;
			rectPicker.position = new Vector2(Mathf.Clamp(rectPos.x, pickRectSize/2f, Screen.width-pickRectSize/2f), 
				Mathf.Clamp(rectPos.y, pickRectSize/2f, Screen.height-pickRectSize/2f));
		}

		EventSystem.current.RaycastAll(cursor, objectsHit);
		if(isHeldDown && objectsHit.Count > 0)
		{
			foreach(RaycastResult r in objectsHit)
			{
				//Colors
				if(r.gameObject.Equals(colorSpace.gameObject))
				{
					appCursor.transform.position = new Vector3(cursor.position.x, cursor.position.y, appCursor.transform.position.z);
					color = colorSpaceTex.GetPixel((int)((cursor.position.x - colorSpace.transform.position.x)/canvasScale), 
						(int)((cursor.position.y - colorSpace.transform.position.y)/canvasScale));

					if(color.a > 0){
						_SetColor(color);
					}
				}	
				else if(r.gameObject.name.Equals(alphaGradient.name))
				{
					appCursor.transform.position = new Vector3(cursor.position.x, cursor.position.y, appCursor.transform.position.z);
					if(hasAlpha)
					{
						//Alpha
						color = alphaGradientTex.GetPixel((int)((cursor.position.x - alphaGradient.transform.position.x)/canvasScale), 
							(int)((cursor.position.y - alphaGradient.transform.position.y)/canvasScale));
						_SetAlpha(color.r);
					}
					else
					{//Grey scale
						color = alphaGradientTex.GetPixel((int)((cursor.position.x - alphaGradient.transform.position.x)/canvasScale), 
							(int)((cursor.position.y - alphaGradient.transform.position.y)/canvasScale));
						_SetColor(color);
					}
				}
				else
				{
					if(defaultColors != null && defaultColors.Length > 0)
					{
						foreach(Image img in defaultColors)
						{
							if(r.gameObject.Equals(img.gameObject))
							{
								appCursor.transform.position = (new Vector3(cursor.position.x, cursor.position.y, appCursor.transform.position.z));
								_SetColor(img.color);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Save the current color sample texture as JPG/PNG.
	/// (Call this at the Save button (Btn_Save) is clicked)
	/// </summary>
	public void SaveCurrentColorSample()
	{
		if(CurrentPickedColorSample != null)
		{
			SaveColorSample(CurrentPickedColorSample);

			btn_Save.enabled = false;
			btn_Save.transform.parent.gameObject.SetActive(false);
		}
	}
	private void SaveColorSample(Texture2D tex2D)
	{
		_SaveImageToDataPath(tex2D, "ColorSamples", "ColorSample_" + DateTime.Now.ToString("yyyyMMddHHmmss"), DataPathType.PersistentDataPath);
	}

	private void _ClearPickedSampleTexture()
	{
		//Clear last picked texture before killing AColorPicker to avoid memory leak
		if(readTex) Texture2D.DestroyImmediate(readTex);
		readTex = null;
	}

	//This function take screenshot at specific rect on screen, and read the pixels for display/preview
	IEnumerator ReadPixelInPickerRect()
	{
		if(Time.time > nextUpdateTime_DisplayReadPixel)
		{
			//Clear last picked texture
			_ClearPickedSampleTexture();

			rectPicker.gameObject.SetActive(false);

			//Pick image for calculate a color
			readTex = new Texture2D((int)pickRectSize, (int)pickRectSize, TextureFormat.RGB24, false);
			yield return new WaitForEndOfFrame();
			Rect rect = new Rect(rectPicker.position.x-pickRectSize/2f, rectPicker.position.y-pickRectSize/2f, pickRectSize, pickRectSize);
			//Debug.Log("rect: " + rect);
			readTex.ReadPixels(rect, 0, 0);
			readTex.Apply();

			yield return new WaitForEndOfFrame();

			//Set picked result
			_SetPickedSampleResult(readTex);

			//Flag
			enableRectPicker = false;

			//Reset picker rect
			pickerBtnMessage.gameObject.SetActive(true);
			rectPicker.position = originPickerPos;
			rectPicker.gameObject.SetActive(true);

			//Show
			pallettePanelGO.SetActive(true);
			_CursorVisible(true);
			btn_Close.gameObject.SetActive(true);


			btn_Save.transform.parent.gameObject.SetActive(true);
			btn_Save.enabled = true;
		}
		yield return null;
	}

	private void _SetPickedSampleResult(Texture2D tex)
	{
		if(tex == null) return;
		displayReadPixel.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
		SetColorWithTexture(tex);
	}

	//Call this at the PickerRect button is clicked
	public void ReadColorFromRectPicker()
	{
		if(!enableRectPicker){
			originPickerPos = rectPicker.position;

			//Hide
			pickerBtnMessage.gameObject.SetActive(false);
			pallettePanelGO.SetActive(false);
			_CursorVisible(false);
			btn_Close.gameObject.SetActive(false);

			//Flag
			enableRectPicker = true;
			float interval = 0.5f;
			nextUpdateTime_DisplayReadPixel = Time.time + interval;
		}

		StartCoroutine(ReadPixelInPickerRect());
	}

	//Pick color from a Texture2D
	//This function read pixel(s) from a Texture2D, and calculate an averaged color
	private void SetColorWithTexture(Texture2D tex)
	{
		bool onePointColor = false;
		_IsOnSilderValueChange = false;
		if(onePointColor)
		{
			Color getColor = tex.GetPixel((int)Screen.width/2, (int)Screen.height/2);
			_SetColor(getColor);
		}
		else
		{
			Color[] getColors = tex.GetPixels();
			float r = 0f, g = 0f, b = 0f;
			foreach(Color c in getColors)
			{
				r += c.r;
				g += c.g;
				b += c.b;
			}
			Color resultColor = new Color(r/(float)getColors.Length, g/(float)getColors.Length, b/(float)getColors.Length);
			_SetColor(resultColor);
		}
	}

	bool _IsSetColor = false;
	private void _SetColor(Color color)
	{
		if(_IsOnSilderValueChange)
		{
			_IsOnSilderValueChange = false;
			return;
		}

		labelR.SetText(Mathf.FloorToInt(255 * color.r).ToString());
		labelG.SetText(Mathf.FloorToInt(255 * color.g).ToString());
		labelB.SetText(Mathf.FloorToInt(255 * color.b).ToString());
		_IsSetColor = true;
		sliderR.value = color.r;
		_IsSetColor = true;
		sliderG.value = color.g;
		_IsSetColor = true;
		sliderB.value = color.b;
		selectedColorDisplay.color = new Color(color.r, color.g, color.b, selectedColorDisplay.color.a);
		_SetHexCode();
		_CursorVisible(true);
	}

	private void _SetAlpha(float a)
	{
		labelA.SetText((255 * a).ToString());
		sliderA.value = a;
		selectedColorDisplay.color = new Color(selectedColorDisplay.color.r, selectedColorDisplay.color.g, selectedColorDisplay.color.b, a);
		_SetHexCode();
		_CursorVisible(true);
	}

	bool _IsOnSilderValueChange = false;
	public void OnSliderValueChange(Slider slider)
	{
		if(_IsSetColor)
		{
			_IsSetColor = false;
			return;
		}
		_IsOnSilderValueChange = true;

		if(slider == sliderR)
		{
			labelR.SetText(((int)(255 * slider.value)).ToString());
		}
		if(slider == sliderG)
		{
			labelG.SetText(((int)(255 * slider.value)).ToString());
		}
		if(slider == sliderB)
		{
			labelB.SetText(((int)(255 * slider.value)).ToString());
		}
		if(slider == sliderA)
		{
			labelA.SetText(((int)(255 * slider.value)).ToString());
		}

		if(hasAlpha)
		{
			selectedColorDisplay.color = new Color((float)labelR.GetInt()/255f, (float)labelG.GetInt()/255f, (float)labelB.GetInt()/255f, (float)labelA.GetInt()/255f);
		}
		else
		{
			selectedColorDisplay.color = new Color((float)labelR.GetInt()/255f, (float)labelG.GetInt()/255f, (float)labelB.GetInt()/255f, 1f);
		}

		_SetHexCode();
		_CursorVisible(false);
	}

	private void _SetHexCode()
	{
		string hex = (hasAlpha)? _ColorToHex(selectedColorDisplay.color):_ColorToHexWithOutAlpha(selectedColorDisplay.color);
		labelHexCode.SetText(hex);
	}

	private void _CursorVisible(bool isVisible)
	{
		if(appCursor.gameObject.activeSelf != isVisible)
		{
			appCursor.gameObject.SetActive(isVisible);
		}
	}

	private Action _onCloseAction = null;

	public void Close()
	{
		_ClearPickedSampleTexture();
		if(_onCloseAction != null) _onCloseAction();

		Destroy(gameObject, 0.1f);
	}

	#region ----- Common -----
	private float _GetPowerOfTwoFor(float f)
	{
		return Mathf.Log(f, 2);
	}

	private string _ColorToHex(Color32 color)
	{
		string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
		return hex.ToLower();
	}

	private string _ColorToHexWithOutAlpha(Color32 color)
	{
		string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex.ToLower();
	}

	private Color _HexToColor(string hex)
	{
		Vector4 v = _HexToVector4(hex);
		return new Color(v.x/255f, v.y/255f, v.z/255f, v.w/255f);
	}

	private Vector4 _HexToVector4(string hex)
	{
		if (hex.StartsWith("#"))
			hex = hex.Substring(1);

		if (hex.Length < 6) throw new Exception("Color not valid");

		int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

		int a = 255;
		if (hex.Length >= 8)
		{
			a = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
		}

		return new Vector4(r, g, b, a);
	}

	private void _SaveImageToDataPath(Texture2D texture, string folderName, string fileName, DataPathType dataPathType = DataPathType.PersistentDataPath, bool isThumbnail = false)
	{
		string fullFolderPath = _GetDataPath(dataPathType) + "/" + folderName;
		_SaveImageToPath(texture, fullFolderPath, fileName, isThumbnail);
		Debug.Log("File Saved: " + fullFolderPath);
	}

	private void _SaveImageToPath(Texture2D texture, string fullFolderPath, string fileName, bool isThumbnail = false)
	{
		string fullFilePath = fullFolderPath + "/" + fileName;

		if(!Directory.Exists(fullFolderPath))
		{
			Directory.CreateDirectory(fullFolderPath) ;
		}

		string extensionName = ".jpg";
		byte[] toBytes = null;
		switch(saveFileFormat)
		{
		case SaveFileFormat.JPG:
			extensionName = ".jpg";
			toBytes = texture.EncodeToJPG(90);
			break;

		case SaveFileFormat.PNG:
			extensionName = ".png";
			toBytes = texture.EncodeToPNG();
			break;
		}

		//Encode & Save the image
		if(isThumbnail)
		{
			System.IO.File.WriteAllBytes(fullFilePath, toBytes); //no file extension name
		}
		else
		{
			System.IO.File.WriteAllBytes(fullFilePath + extensionName, toBytes);
		}
	}

	private string _GetDataPath(DataPathType dataPathType)
	{
		string getDataPath = string.Empty;
		switch(dataPathType)
		{
		case DataPathType.DataPath:
			getDataPath = Application.dataPath;
			break;
		case DataPathType.PersistentDataPath:
			getDataPath = Application.persistentDataPath;
			break;
		case DataPathType.StreamingAssetsPath:
			getDataPath = Application.streamingAssetsPath;
			break;
		case DataPathType.TemporaryCachePath:
			getDataPath = Application.temporaryCachePath;
			break;
		}
		return getDataPath;
	}

	public enum SaveFileFormat{
		JPG = 0,
		PNG
	}

	public enum DataPathType{
		PersistentDataPath = 0,
		TemporaryCachePath,
		StreamingAssetsPath,
		DataPath,
	}

	#endregion

}
