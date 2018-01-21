using System.Collections.Generic;
using System.Linq;
using Assets.IUnified.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class IUnifiedContainerSelectWindow : EditorWindow
{
    public static IUnifiedContainerSelectWindow ShowSelectWindow(string resultTypeName, bool selectingForProjectAsset, IUnifiedContainerPropertyDrawer.SerializedContainer serializedContainer, IEnumerable<IUnifiedContainerPropertyDrawer.SelectableObject> selectableObjects)
    {
        var window = (IUnifiedContainerSelectWindow) CreateInstance(typeof(IUnifiedContainerSelectWindow));
        window.Initialize(resultTypeName, selectingForProjectAsset, serializedContainer, selectableObjects);
        window.ShowUtility();
        return window;
    }

    #region Private Parts
    
    private string _resultTypeName;
    private bool _selectingForProjectAsset;
    private IUnifiedContainerPropertyDrawer.SerializedContainer _serializedContainer;
    private List<ObjectNode> _allObjects;
    private List<ObjectNode> _projectAssets;
    private List<ObjectNode> _sceneAssets;
    private Vector2 _scrollPos;
    private bool _close = true;
    private bool _sceneAssetsExist = true;
    private bool _projectAssetsExist = true;
    private bool _selectingProjectAssets;
    private bool _switchBoxStyle;
    private const string NoResultsMessage = "\nNo {0}assets found that implement or derive from {1}.\n";
    private const string IndentString = "    ";

    private bool IsValid
    {
        get { return _serializedContainer != null && _allObjects != null && !_close; }
    }
    
    private void Initialize(string resultTypeName, bool selectingForProjectAsset, IUnifiedContainerPropertyDrawer.SerializedContainer serializedContainer, IEnumerable<IUnifiedContainerPropertyDrawer.SelectableObject> selectableObject)
    {
        serializedContainer.SetSelecting(true);
        _resultTypeName = resultTypeName;
        _selectingForProjectAsset = selectingForProjectAsset;
        titleContent = new GUIContent(string.Format("Implementing {0} {1}", _resultTypeName, _selectingForProjectAsset ? "( project assets only )" : ""));

        _serializedContainer = serializedContainer;
        _allObjects = new SelectableObjectsHierarchyBuilder().BuildSelectableObjectsList(selectableObject, _serializedContainer.ObjectField, out _selectingProjectAssets);
        _projectAssets = _allObjects.Where(g => g.IsProjectAsset).ToList();
        _sceneAssets = _allObjects.Where(g => !g.IsProjectAsset).ToList();
        _projectAssetsExist = _projectAssets.Any();
        _sceneAssetsExist = _sceneAssets.Any();
        _selectingProjectAssets = (_selectingProjectAssets || _selectingForProjectAsset) || (_projectAssetsExist && !_sceneAssetsExist);
        _close = false;
    }

    private void OnGUI()
    {
        if(!IsValid)
        {
            return;
        }

        GUINullOption();

        if(!_allObjects.Any())
        {
            GUILayout.Space(10.0f);
            GUILayout.Label(string.Format(NoResultsMessage, _selectingForProjectAsset ? "project " : "", _resultTypeName), IUnifiedGUIHelper.SelectWindowStyles.DontPanic, GUILayout.ExpandWidth(true));
            return;
        }
        
        if(!_selectingForProjectAsset)
        {
            GUISelectObjectType();
        }
        
        _scrollPos = IUnifiedGUIHelper.ScrollViewBlock(_scrollPos, false, false, () =>
        {
            _switchBoxStyle = false;
            foreach(var selectableObject in (_selectingProjectAssets ? _projectAssets : _sceneAssets))
            {
                GUIObjectNode(selectableObject);
            }
        });
    }

    private void GUINullOption()
    {
        IUnifiedGUIHelper.HorizontalBlock(() =>
        {
            IUnifiedGUIHelper.EnabledBlock(() =>
            {
                GUI.enabled = _allObjects.Any();

                if(GUILayout.Button(new GUIContent("▼", "Expand All"), GUILayout.ExpandWidth(false)))
                {
                    FoldoutAll(_allObjects, true);
                }

                if(GUILayout.Button(new GUIContent("▲", "Collapse All"), GUILayout.ExpandWidth(false)))
                {
                    FoldoutAll(_allObjects, false);
                }
            });

            var style = _serializedContainer.ObjectField == null && string.IsNullOrEmpty(_serializedContainer.ResultType) ? IUnifiedGUIHelper.SelectWindowStyles.NullSelected : IUnifiedGUIHelper.SelectWindowStyles.NullOption;
            if(GUILayout.Button("NULL", style, GUILayout.ExpandWidth(true)))
            {
                _serializedContainer.ObjectField = null;
                _serializedContainer.ApplyModifiedProperties();
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            GUILayout.Space(5.0f);
        });
    }

    private void GUISelectObjectType()
    {
        IUnifiedGUIHelper.HorizontalBlock(() =>
        {
            IUnifiedGUIHelper.EnabledBlock(() =>
            {
                GUI.enabled = _sceneAssetsExist;
                if(GUILayout.Button("Scene Assets", _selectingProjectAssets ? GUI.skin.button : IUnifiedGUIHelper.SelectWindowStyles.SelectedButton))
                {
                    _selectingProjectAssets = false;
                }
            });

            IUnifiedGUIHelper.EnabledBlock(() =>
            {
                GUI.enabled = _projectAssetsExist;
                if(GUILayout.Button("Project Assets", _selectingProjectAssets ? IUnifiedGUIHelper.SelectWindowStyles.SelectedButton : GUI.skin.button))
                {
                    _selectingProjectAssets = true;
                }
            });
        });
    }

    private void GUIObjectNode(ObjectNode objectNode, int indentLevel = 0)
    {
        GUIObjectNodeHelper.ObjectNodeGUI(objectNode, _serializedContainer, GetNextStyle(), indentLevel);

        if(objectNode.Foldout)
        {
            foreach(var child in objectNode.Children)
            {
                GUIObjectNode(child, indentLevel + 1);
            }
        }
    }
    
    private GUIStyle GetNextStyle()
    {
        _switchBoxStyle = !_switchBoxStyle;
         return _switchBoxStyle ? IUnifiedGUIHelper.SelectWindowStyles.ObjectGroup : IUnifiedGUIHelper.SelectWindowStyles.ObjectGroupSwitch;
    }

    private void FoldoutAll(IEnumerable<ObjectNode> selectableObjects, bool foldout)
    {
        foreach(var selectableObject in selectableObjects)
        {
            FoldoutAll(selectableObject.Children, (selectableObject.Foldout = foldout));
        }
    }

    private void Update()
    {
        if(!IsValid)
        {
            Close();
        }
    }

    private void OnLostFocus()
    {
        _close = true;
    }

    private void OnDestroy()
    {
        if(_serializedContainer != null)
        {
            _serializedContainer.SetSelecting(false);
        }
    }

    #endregion

    public class ObjectNode
    {
        public Object Object;

        public ObjectNode Parent = null;
        public List<ObjectNode> Children = new List<ObjectNode>();

        public bool IsSelectable;
        public bool IsPingable;
        public bool IsProjectAsset;
        public bool Foldout = true;
        public string NodeName;

        public int TotalNodeCount
        {
            get
            {
                if(_totalNodeCount == null)
                {
                    _totalNodeCount = GetTotalNodeCount(this);
                }
                return _totalNodeCount.Value;
            }
        }
        public int? _totalNodeCount;

        private static int GetTotalNodeCount(ObjectNode node)
        {
            var totalCount = 1;
            foreach(var childNode in node.Children)
            {
                totalCount += childNode.TotalNodeCount;
            }
            return totalCount;
        }
    }

    private class SelectableObjectsHierarchyBuilder
    {
        private Dictionary<Object, ObjectNode> _parentNodes;
        private List<ObjectNode> _rootNodes;

        public List<ObjectNode> BuildSelectableObjectsList(IEnumerable<IUnifiedContainerPropertyDrawer.SelectableObject> selectableObjects, Object selectedObject, out bool selectingProjectAssets)
        {
            selectingProjectAssets = false;

            _rootNodes = new List<ObjectNode>();
            _parentNodes = new Dictionary<Object, ObjectNode>();
            
            foreach(var selectableObject in selectableObjects)
            {
                if(selectedObject == selectableObject.Object)
                {
                    selectingProjectAssets = selectableObject.IsProjectAsset;
                }

                var selectableObjectNode = new ObjectNode
                {
                    Object = selectableObject.Object,
                    NodeName = IUnifiedGUIHelper.GetObjectName(selectableObject.Object),

                    IsSelectable = true,
                    IsPingable = !selectableObject.IsComponent && IUnifiedGUIHelper.IsPingable(selectableObject.Object),
                    IsProjectAsset = selectableObject.IsProjectAsset,
                };

                if(selectableObject.IsComponent)
                {
                    selectableObjectNode.Parent = GetOrCreateParentNode(selectableObjectNode);
                }
                else
                {
                    _rootNodes.Add(selectableObjectNode);
                }
            }

            var missingParents = _parentNodes.Values.Where(n => n.Parent == null && !_rootNodes.Contains(n)).ToList();
            _rootNodes.AddRange(missingParents);
            var @return = SortedNodes(_rootNodes);

            _parentNodes.Clear();
            _rootNodes.Clear();

            return @return;
        }

        private static List<ObjectNode> SortedNodes(List<ObjectNode> nodes)
        {
            nodes = nodes.OrderBy(n => n.TotalNodeCount).ThenBy(n => n.NodeName).ToList();
            foreach(var node in nodes)
            {
                node.Children = SortedNodes(node.Children);
            }
            return nodes;
        }

        private ObjectNode GetOrCreateParentNode(ObjectNode childNode)
        {
            GameObject parent = null;

            var component = childNode.Object as Component;
            if(component != null)
            {
                parent = component.gameObject;
            }
            else
            {
                var gameobject = childNode.Object as GameObject;
                if(gameobject != null && gameobject.transform.parent != null)
                {
                    parent = gameobject.transform.parent.gameObject;
                }
            }

            if(parent != null)
            {
                ObjectNode parentNode;
                if(!_parentNodes.TryGetValue(parent, out parentNode))
                {
                    parentNode = new ObjectNode
                        {
                            Object = parent,
                            IsSelectable = false,
                            IsPingable = IUnifiedGUIHelper.IsPingable(parent),
                            IsProjectAsset = IUnifiedGUIHelper.IsProjectAsset(parent),
                            NodeName = IUnifiedGUIHelper.GetObjectName(parent)
                        };
                    _parentNodes.Add(parent, parentNode);
                    parentNode.Parent = GetOrCreateParentNode(parentNode);
                }
                parentNode.Children.Add(childNode);

                return parentNode;
            }

            return null;
        }
    }

    

    private class GUIObjectNodeHelper
    {
        public static void ObjectNodeGUI(ObjectNode objectNode, IUnifiedContainerPropertyDrawer.SerializedContainer serializedContainer, GUIStyle style, int indentLevel)
        {
            IUnifiedGUIHelper.HorizontalBlock(() =>
            {
                var helper = new GUIObjectNodeHelper(objectNode, serializedContainer);
                helper.DrawGUI(style, indentLevel);
            });
        }

        #region Private Parts

        private readonly ObjectNode _objectNode;
        private readonly IUnifiedContainerPropertyDrawer.SerializedContainer _serializedContainer;
        private readonly bool _displayFoldout;
        private bool _toggleFoldout = false;
        private bool _select = false;
        private bool _selectDown = false;
        private bool _ping = false;
        private bool _pingDown = false;

        private GUIObjectNodeHelper(ObjectNode objectNode, IUnifiedContainerPropertyDrawer.SerializedContainer serializedContainer)
        {
            _objectNode = objectNode;
            _serializedContainer = serializedContainer;
            _displayFoldout = _objectNode.Children.Any();
        }

        private void DrawGUI(GUIStyle style, int indentLevel)
        {
            GUIContentRect nodeGUI, foldoutGUI, iconGUI, labelGUI;
            CreateContentRects(style, indentLevel, out nodeGUI, out foldoutGUI, out iconGUI, out labelGUI);
            DetermineActions(nodeGUI, foldoutGUI, labelGUI);
            DrawControls(style, nodeGUI, foldoutGUI, iconGUI, labelGUI);
            ExecuteActions();
        }

        private void CreateContentRects(GUIStyle style, int indentLevel, out GUIContentRect nodeGUI, out GUIContentRect foldoutGUI, out GUIContentRect iconGUI, out GUIContentRect labelGUI)
        {
            GUIContent iconContent, labelContent;
            var foldoutContent = GetFoldoutContent(indentLevel);
            GetObjectNodeContents(_objectNode, out iconContent, out labelContent);

            nodeGUI = new GUIContentRect(GUIContent.none, GUILayoutUtility.GetRect(foldoutContent, style));
            foldoutGUI = new GUIContentRect(foldoutContent, nodeGUI);
            iconGUI = new GUIContentRect(iconContent, nodeGUI);
            labelGUI = new GUIContentRect(labelContent, nodeGUI);

            foldoutGUI.SetWidth(IUnifiedGUIHelper.GetMinWidth(foldoutContent, style) + 2.0f);

            iconGUI.MoveNextTo(foldoutGUI);
            iconGUI.SetWidth(iconContent == null ? 0.0f : (IUnifiedGUIHelper.GetScaledTextureWidth(iconContent.image, foldoutGUI.Rect.height, style) + 2.0f));

            labelGUI.MoveNextTo(iconGUI);
            labelGUI.SetWidth(IUnifiedGUIHelper.GetMinWidth(labelContent, style));
        }

        private void DetermineActions(Rect objectRect, Rect foldoutRect, Rect nameRect)
        {
            FoldoutButton(foldoutRect);

            if(_objectNode.IsSelectable)
            {
                var selectRect = new Rect(objectRect);
                if(_displayFoldout)
                {
                    selectRect.xMin = foldoutRect.xMax;
                }
                if(_objectNode.IsPingable)
                {
                    selectRect.xMax = nameRect.xMax;
                }

                SelectButton(selectRect);

                if(_objectNode.IsPingable)
                {
                    var pingRect = new Rect(objectRect);
                    pingRect.xMin = selectRect.xMax;
                    PingButton(pingRect);
                }
            }
            else
            {
                if(_objectNode.IsPingable)
                {
                    var pingRect = new Rect(objectRect);
                    pingRect.xMin = foldoutRect.xMax;
                    PingButton(pingRect);
                }
            }
        }

        private void DrawControls(GUIStyle style, GUIContentRect nodeGUI, GUIContentRect foldoutGUI, GUIContentRect iconGUI, GUIContentRect nameGUI)
        {
            style = DetermineObjectStyle(style, _objectNode, _serializedContainer.ObjectField);
            GUI.Label(nodeGUI, _displayFoldout ? foldoutGUI : nodeGUI, style);

            style.normal.background = null;
            if(iconGUI.Content != null)
            {
                GUI.Label(iconGUI, iconGUI, style);
            }
            GUI.Label(nameGUI, nameGUI, style);
        }

        private void ExecuteActions()
        {
            if(_toggleFoldout)
            {
                _objectNode.Foldout = !_objectNode.Foldout;
            }
            if(_select)
            {
                _serializedContainer.ObjectField = _objectNode.Object;
                _serializedContainer.ResultType = null;
                _serializedContainer.ApplyModifiedProperties();
            }
            if(_ping)
            {
                IUnifiedGUIHelper.PingObject(_objectNode.Object);
            }
        }

        private GUIContent GetFoldoutContent(int indentLevel)
        {
            var foldoutString = " ";
            for(var i = 0; i < indentLevel; ++i)
            {
                foldoutString += IndentString;
            }
            foldoutString += (_objectNode.Foldout ? "▼" : "►");
            return new GUIContent(foldoutString);
        }

        private static void GetObjectNodeContents(ObjectNode objectNode, out GUIContent iconContent, out GUIContent labelContent)
        {
            iconContent = null;
            labelContent = new GUIContent(objectNode.NodeName);

            Texture icon;
            if(objectNode.Object is GameObject)
            {
                if(objectNode.IsProjectAsset)
                {
                    icon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
                }
                else
                {
                    icon = EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image;
                }
            }
            else
            {
                icon = EditorGUIUtility.ObjectContent(objectNode.Object, null).image;
            }

            if(icon != null)
            {
                iconContent = new GUIContent(icon);
            }
        }

        private GUIStyle DetermineObjectStyle(GUIStyle defaultStyle, ObjectNode objectNode, Object selectedObject)
        {
            if(_ping || _pingDown)
            {
                return IUnifiedGUIHelper.SelectWindowStyles.Pinged;
            }

            if(selectedObject == objectNode.Object || _select || _selectDown)
            {
                return IUnifiedGUIHelper.SelectWindowStyles.SelectedObject;
            }

            return defaultStyle;
        }

        private void FoldoutButton(Rect rect)
        {
            if(_displayFoldout)
            {
                if(GUI.Button(rect, "", GUIStyle.none))
                {
                    _toggleFoldout = true;
                }
            }
        }

        private void SelectButton(Rect rect)
        {
            var buttonId = GUIUtility.GetControlID(FocusType.Passive) + 1;
            if(GUI.Button(rect, "", GUIStyle.none))
            {
                _select = true;
            }
            _selectDown = GUIUtility.hotControl == buttonId;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        }

        private void PingButton(Rect rect)
        {
            var buttonId = GUIUtility.GetControlID(FocusType.Passive) + 1;
            if(GUI.Button(rect, "", GUIStyle.none))
            {
                _ping = true;
            }
            _pingDown = GUIUtility.hotControl == buttonId;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Zoom);
        }

        #endregion
    }
}