using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBlockAction : MonoBehaviour, IAction
{
    public GameObject block;
    public float size = 1.0F;

    public List<GameObject> undoList = new List<GameObject>();
    public List<GameObject> redoList = new List<GameObject>();

    public void Undo()
    {
        GameObject go = undoList[undoList.Count - 1];
        go.SetActive(false);
        undoList.Remove(go);
        redoList.Add(go);
    }

    public void Redo()
    {
        GameObject go = redoList[redoList.Count - 1];
        go.SetActive(true);
        undoList.Add(go);
        redoList.Remove(go);
    }

    public void Deselect()
    {

    }

    public void Reset()
    {

    }

    public void Select()
    {

    }



    public bool Use()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 scale = Vector3.one * size;
            Vector3 dir = hit.transform.forward;

            float hitSize = hit.transform.parent.localScale.x;
            float gap = hitSize - size;

            Vector3 blockPosition = hit.transform.parent.position;
            blockPosition += dir * (size + gap / 2);

            GameObject go = Instantiate(block);

            go.GetComponent<Block>().SetColor(ColorManager.Instance.primaryColor);

            go.transform.localScale = scale;
            go.transform.position = blockPosition;
            go.transform.rotation = hit.transform.parent.rotation;

            undoList.Add(go);
            redoList.Clear();
            return true;
        }

        return false;

    }


}
