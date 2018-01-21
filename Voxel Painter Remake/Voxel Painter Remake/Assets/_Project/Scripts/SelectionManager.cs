using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Action = core.Action;

//To optimize this, make it only find block with marquee.
public class SelectionManager : Singleton<SelectionManager> {
    public List<GameObject> selectedGameObjects;
    public List<Block> blocks;

    public HashSet<ISelectable> hashSelectable = new HashSet<ISelectable>();
    public Transform pivot;

    RaycastHit lastHit;
    ISelectable selectableHovered;
    public core.Action clearAction;
     
    void Awake() {
        //InputManager.OnClickDown += OnClickDown;
    }

    void OnDestroy() {
        //InputManager.OnClickDown -= OnClickDown;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        ISelectable iSelectable;
        TryFindSelectableFromRay(out iSelectable, out lastHit);

        //Pointed to nothing
        if (iSelectable == null)
        {
            //Deselect when you hovered to nothing
            if (selectableHovered != null)
            {
                if (!selectableHovered.IsSelected)
                    selectableHovered.HoverExit();
                selectableHovered = null;
            }

            return;
        }

        //Pointer changed target
        if (selectableHovered != iSelectable) {
            
            if (selectableHovered != null)
            {
                if (!selectableHovered.IsSelected)
                    selectableHovered.HoverExit();

                selectableHovered = null;
            }
        }

        //Assigning
        selectableHovered = iSelectable;

        if (selectableHovered != null)
        {
            if(!selectableHovered.IsSelected)
                selectableHovered.Hover();
        }
    }

    public bool TryFindSelectableFromRay(out ISelectable iSelectable, out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit)) {
            iSelectable = null;
            return false;
        }
         
        iSelectable = hit.transform.GetComponent<ISelectable>();
        if (iSelectable == null) return false;
        return true;
    }

    public ISelectable TryFindSelectableFromRay()
    {
        ISelectable iSelectable = null;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit))
        {
            iSelectable = null;
            return null;
        }

        iSelectable = hit.transform.GetComponent<ISelectable>();
        if (iSelectable == null) return null;

        return iSelectable;
    }

    //This shoulod be on SelectAction
    public ISelectable OnClickDown()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //If you hit nothing.
        if (!Physics.Raycast(ray, out hit))
        {
            Clear();
            return null;
        }

        ISelectable iSelectable = hit.transform.GetComponent<ISelectable>();

        if (iSelectable == null)
        {
            Clear();
            return null;
        }

        if (hashSelectable.Contains(iSelectable))
            Deselect(iSelectable);
        else
            Select(iSelectable);

        return iSelectable;
    }

    public void Select(ISelectable iSelectable) {
        if (hashSelectable.Contains(iSelectable))
            return;

        //This increase marquee performance by ten folds
        //Select the whole object. Keep in mind this may break select action
        /*if (Input.GetKey(KeyCode.LeftControl))
        {
            Block block = iSelectable.gameObject.GetComponent<Face>().transform.parent.GetComponent<Block>();
            foreach (Face face in block.faces)
            {
                if (face.gameObject != iSelectable.gameObject) {
                    selectedGameObjects.Add(face.gameObject);
                    hashSelectable.Add(face);
                    face.Select();
                }
            }
        }
        */

        selectedGameObjects.Add(iSelectable.gameObject);
        hashSelectable.Add(iSelectable);
        iSelectable.Select();
        AddBlock(iSelectable);
        CheckPivot();
    }

    public void AddBlock(ISelectable iSelectable) {
        Face face = iSelectable.gameObject.GetComponent<Face>();

        if (face != null)
        {
            Block b = face.transform.parent.GetComponent<Block>();
            if (!blocks.Contains(b))
                blocks.Add(b);
        }
    }

    public void Deselect(ISelectable iSelectable) {
        selectedGameObjects.Remove(iSelectable.gameObject);
        hashSelectable.Remove(iSelectable);
        iSelectable.Deselect();

        RemoveBlock(iSelectable);
        CheckPivot();
    }

    public void RemoveBlock(ISelectable iSelectable) {
        Face face = iSelectable.gameObject.GetComponent<Face>();
        if (face != null)
        {
            Block block = face.transform.parent.GetComponent<Block>();
            bool canRemove = true;
            //block needs to check if a face it has exists in the selected
            for (int i = 0; i < block.faces.Length; i++)
            {
                if (hashSelectable.Contains(block.faces[i]))
                {
                    //don't remove
                    canRemove = false;
                    break;
                }
            }

            if (canRemove)
            {
                blocks.Remove(block);
                block.transform.SetParent(null);
            }
        }
    }

    public void DeselectAll() {

    }

    public void SelectAll() {

    }
    public void Clear() {

        if (hashSelectable.Count <= 0)
            return;

        clearAction.Result.Use();
        HistoryManager.Instance.AddAction(clearAction);

        foreach (ISelectable iSelectable in hashSelectable) {
            iSelectable.Deselect();
        }

        hashSelectable.Clear();
        selectedGameObjects.Clear();
        foreach (Block block in blocks)
            block.transform.SetParent(null);
        blocks.Clear();
    }

    public void CheckPivot() {

        foreach (Block b in blocks) {
            b.transform.SetParent(null);
        }

        float xMin = 0, xMax = 0, yMin = 0, yMax = 0, zMin = 0, zMax = 0;

        Vector3 minPosition = new Vector3(xMin, yMin, zMin);
        Vector3 maxPosition = new Vector3(xMax, yMax, zMax);

        for (int i = 0; i < selectedGameObjects.Count; i++) {
            Vector3 pos = selectedGameObjects[i].transform.position;

            if (i == 0) {
                minPosition = pos;
                maxPosition = pos;
            }

            if (pos.x < minPosition.x) minPosition.x = pos.x;
            if (pos.x > maxPosition.x) maxPosition.x = pos.x;

            if (pos.y < minPosition.y) minPosition.y = pos.y;
            if (pos.y > maxPosition.y) maxPosition.y = pos.y;

            if (pos.z < minPosition.z) minPosition.z = pos.z;
            if (pos.z > maxPosition.z) maxPosition.z = pos.z;
        }
        
        Vector3 dir = maxPosition - minPosition;
        float dist = Vector3.Distance(maxPosition, minPosition);
        dir.Normalize();
        Vector3 pivotPosition = minPosition + (dir * dist/2);
        pivot.position = pivotPosition;

        foreach (Block b in blocks)
        {
            b.transform.SetParent(pivot);
        }

    }

    public void AssignBlocksInGroup() {

    }

    public Block SelectBlockByFace(ISelectable iSelectable) {
        Block block = iSelectable.gameObject.GetComponent<Face>().transform.parent.GetComponent<Block>();
        foreach (Face face in block.faces)
        {
            if (face.gameObject != iSelectable.gameObject)
            {
                SelectionManager.Instance.Select(face);
                //selectables.Add(face);
            }
        }
        return block;
    }
}
