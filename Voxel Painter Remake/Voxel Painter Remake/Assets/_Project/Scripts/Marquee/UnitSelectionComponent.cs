using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

public class UnitSelectionComponent : MonoBehaviour, IAction
{
    bool isSelecting = false;
    Vector3 mousePosition1;
    bool canUse = false;

    public HistoryTracker<List<ISelectable>> history = new HistoryTracker<List<ISelectable>>();
    public core.Action myAction;

    public void Undo()
    {
        var element = history.Undo();

        //See you're deselecting everything, but hey, some objects may have been selected!
        foreach (var iSelectable in element.mInitial)
            SelectionManager.Instance.Deselect(iSelectable);
    }

    public void Redo()
    {
        var element = history.Redo();

        foreach (var iSelectable in element.mInitial)
            SelectionManager.Instance.Select(iSelectable);
    }

    //IAction, need an ActionUpdate function. 
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (Input.GetKey(KeyCode.LeftAlt))
            return;

        if (!canUse)
            return;

        // If we press the left mouse button, begin selection and remember the location of the mouse
        if( Input.GetMouseButtonDown( 0 ) )
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;

            /*
            var iSelectables = InterfaceHelper.FindObjects<ISelectable>();

            foreach ( var selectableObject in iSelectables )
                SelectionManager.Instance.Deselect(selectableObject);
            */

            if(!Input.GetKey(KeyCode.LeftShift))    
                SelectionManager.Instance.Clear();
        }

        // If we let go of the left mouse button, end selection
        if( Input.GetMouseButtonUp( 0 ) )
        {
            var iSelectables = InterfaceHelper.FindObjects<ISelectable>();
            var selectedObjects = new List<ISelectable>();
            
            foreach( var selectableObject in iSelectables)
            {
                if (!SelectionManager.Instance.hashSelectable.Contains(selectableObject))
                {
                    if (IsWithinSelectionBounds(selectableObject.gameObject))
                    {
                        /*
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            Block block = selectableObject.gameObject.transform.parent.GetComponent<Block>();

                            foreach (Face face in block.faces)
                            {
                                if (face.gameObject != selectableObject.gameObject)
                                {
                                    SelectionManager.Instance.Select(face);
                                    selectedObjects.Add(face);
                                }
                            }
                        }
                        */

                        if (!SelectionManager.Instance.hashSelectable.Contains(selectableObject))
                        {
                            SelectionManager.Instance.Select(selectableObject);
                            selectedObjects.Add(selectableObject);
                        }
                    }
                }
            }

            if (selectedObjects.Count > 0)
            {
                var instance = history.NewInstance<List<ISelectable>>();
                instance.mInitial = selectedObjects;
                history.undoList.Add(instance);
                HistoryManager.Instance.AddAction(myAction);
            }

            isSelecting = false;
        }

    }

    public bool IsWithinSelectionBounds( GameObject gameObject )
    {
        if( !isSelecting )
            return false;

        var camera = Camera.main;
        var viewportBounds = Utils.GetViewportBounds( camera, mousePosition1, Input.mousePosition );
        return viewportBounds.Contains( camera.WorldToViewportPoint( gameObject.transform.position ) );
    }

    void OnGUI()
    {
        if( isSelecting )
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect( mousePosition1, Input.mousePosition );
            Utils.DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            Utils.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
        }
    }

    //Means I will not be listed in the history
    public bool Use()
    {
        return false;
    }

    public void Reset()
    {

    }

    public void Select()
    {
        canUse = true;
    }

    public void Deselect()
    {
        canUse = false;
    }


}