using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager> {

    public delegate void InputDelegate();
    public static event InputDelegate OnClickDown = delegate { };

    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            if (OnClickDown != null)
                OnClickDown.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            if(Input.GetKey(KeyCode.LeftShift))
                HistoryManager.Instance.Redo();
            else
                HistoryManager.Instance.Undo();
        }

        if (Input.GetKeyUp(KeyCode.C))
            ColorManager.Instance.ToggleColorInfoMenu();
        

    }
}
