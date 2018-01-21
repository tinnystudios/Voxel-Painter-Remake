using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Assets.IUnified.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(IUnifiedContainerBase.IUnifiedContainerBase), true)]
public class IUnifiedContainerPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var containerType = GetContainerType(property);
        if(containerType == null)
        {
            position.width -= 4;
            position.x += 2;
            position.height -= 2;
            position.y += 1;
            GUI.Label(position, string.Format("Cannot draw '{0}'.", property.name), IUnifiedGUIHelper.InspectorStyles.Error);
            return;
        }

        var resultType = containerType.GetProperty("Result").PropertyType;
        var drawContainerMethod = GetDrawMethod(resultType);
        
        label = EditorGUI.BeginProperty(position, label, property);
        drawContainerMethod(position, label, new SerializedContainer(HashCode, property));
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + ButtonSpace + ButtonSpace;
    }

    #region Private Parts
    private static IUnifiedContainerSelectWindow _selectWindow;
    private static Event _currentEvent;

    private const float ButtonSpace = 1.0f;
    private const float ButtonWidth = 20.0f;

    private int HashCode
    {
        get { return (_hashCode != null ? _hashCode : (_hashCode = GetHashCode())).Value; }
    }
    private int? _hashCode;

    private static void DrawIUnifiedContainer<TResult>(Rect position, GUIContent label, SerializedContainer serializedContainer)
        where TResult : class
    {
        _currentEvent = Event.current;
        var resultTypeName = IUnifiedGUIHelper.ConstructResolvedName(CachedType<TResult>.Type);

        label.tooltip = resultTypeName;
        var labelRect = new GUIContentRect(label, position);
        labelRect.SetWidth(EditorGUIUtility.labelWidth);

        var resultRect = new GUIContentRect(null, position);
        resultRect.MoveNextTo(labelRect);
        resultRect.Rect.xMax -= (ButtonSpace + ButtonWidth) * 2;

        var nullButonRect = new GUIContentRect(null, position);
        nullButonRect.MoveNextTo(resultRect, ButtonSpace);
        nullButonRect.SetWidth(ButtonWidth);

        var listButtonRect = new GUIContentRect(null, position);
        listButtonRect.MoveNextTo(nullButonRect, ButtonSpace);
        listButtonRect.SetWidth(ButtonWidth);
        
        var isProjectAsset = serializedContainer.IsProjectAsset;
        var pingable = !serializedContainer.ObjectFieldProperty.hasMultipleDifferentValues && IUnifiedGUIHelper.IsPingable(serializedContainer.ObjectField);
        var dragDropResult = GetDragAndDropResult<TResult>(resultRect, isProjectAsset, serializedContainer);

        EditorGUI.LabelField(labelRect, label);
        IUnifiedGUIHelper.EnabledBlock(() =>
        {
            GUI.enabled = pingable;

            IUnifiedGUIHelper.ColorBlock(() =>
            {
                if(serializedContainer.Selecting || serializedContainer.Dropping)
                {
                    GUI.color = new Color(1, 1, 1, 2);
                }
                else
                {
                    GUI.color = pingable ? new Color(1, 1, 1, 2) : Color.white;
                }

                DrawField(serializedContainer, resultRect, pingable);
            });
        });
        
        if(dragDropResult != null)
        {
            serializedContainer.ObjectField = dragDropResult as Object;
            GUI.changed = true;
        }

        if(GUI.Button(nullButonRect, new GUIContent("○", "Set to null"), IUnifiedGUIHelper.InspectorStyles.NullOutButton))
        {
            serializedContainer.ObjectField = null;
        }

        IUnifiedGUIHelper.EnabledBlock(() =>
        {
            if(GUI.Button(listButtonRect, new GUIContent("◉", "Select from list"), IUnifiedGUIHelper.InspectorStyles.SelectFromListButton))
            {
                _selectWindow = IUnifiedContainerSelectWindow.ShowSelectWindow(resultTypeName, isProjectAsset, serializedContainer, GetSelectable<TResult>(isProjectAsset));
            }
        });
    }

    private static void DrawField(SerializedContainer serializedContainer, Rect rect, bool pingable)
    {
        var buttonId = GUIUtility.GetControlID(FocusType.Passive) + 1;
        if(GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            if(pingable)
            {
                IUnifiedGUIHelper.PingObject(serializedContainer.ObjectField);
            }
        }
        var pinging = GUIUtility.hotControl == buttonId && pingable;

        GUIStyle style;
        if(serializedContainer.Dropping)
        {
            style = IUnifiedGUIHelper.InspectorStyles.DropBox;
        }
        else if(pinging)
        {
            style = IUnifiedGUIHelper.InspectorStyles.Pinging;
        }
        else if(serializedContainer.Selecting)
        {
            style = IUnifiedGUIHelper.InspectorStyles.Selecting;
        }
        else
        {
            style = IUnifiedGUIHelper.InspectorStyles.Result;
        }

        if(serializedContainer.ObjectFieldProperty.hasMultipleDifferentValues || serializedContainer.ObjectField == null)
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.imagePosition = ImagePosition.TextOnly;
        }
        else
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.imagePosition = ImagePosition.ImageLeft;
        }

        GUI.Label(rect, GUIContent.none, style);

        GUIContentRect icon, label;
        serializedContainer.GetContainerContents(rect, serializedContainer.Dropping, out icon, out label);

        style.normal.background = null;

        if(icon.Content != null)
        {
            icon.SetWidth(IUnifiedGUIHelper.GetScaledTextureWidth(icon.Content.image, rect.height + 2.0f));
            label.MoveNextTo(icon, -3.0f);
            GUI.Label(icon, icon, style);
        }
        GUI.Label(label, label, style);

        if(!SerializedContainer.AnyDropping && pingable)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Zoom);
        }
    }

    private static TResult GetDragAndDropResult<TResult>(Rect dropArea, bool selectingForProjectAsset, SerializedContainer serializedContainer)
        where TResult : class
    {
        TResult result = null;
        bool? dropping = null;

        switch(_currentEvent.rawType)
        {
            case EventType.DragExited:
            case EventType.DragUpdated:
            case EventType.DragPerform:
                dropping = false;
                if(!serializedContainer.MouseInRects(dropArea, _currentEvent.mousePosition))
                {
                    break;
                }

                var single = DragAndDrop.objectReferences.SelectMany(o => GetObjectImplementationsOf<TResult>(o)).FirstOrDefault();
                if(single != null)
                {
                    var singleObject = single as Object;
                    if(singleObject != null && (!selectingForProjectAsset || IUnifiedGUIHelper.IsProjectAsset(singleObject)))
                    {
                        dropping = true;
                    }
                }
                DragAndDrop.visualMode = SerializedContainer.AnyDropping ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

                if(_currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    _currentEvent.Use();
                    dropping = false;
                    result = single;
                }
                break;
        }

        if(dropping != null)
        {
            serializedContainer.SetDropping(dropping.Value);
        }

        return result;
    }

    private static List<TResult> GetObjectImplementationsOf<TResult>(Object @object)
        where TResult : class
    {
        var implementations = new List<TResult>();
        var implementation = @object as TResult;
        if(implementation != null)
        {
            implementations.Add(implementation);
        }

        var gameObject = @object as GameObject;
        if(gameObject != null)
        {
            implementations.AddRange(gameObject.GetComponents<Component>().OfType<TResult>());
        }

        var transform = @object as Transform;
        if(transform != null)
        {
            implementations.AddRange(transform.GetComponents<Component>().OfType<TResult>());
        }

        return implementations.Distinct().ToList();
    }

    private static string BuildEditorResultString(string resultType, Object @object)
    {
        if(@object != null)
        {
            var component = @object as Component;
            if(component != null)
            {
                return string.Format("{0} ( {1} )", component.gameObject.name, IUnifiedGUIHelper.ConstructResolvedName(@object.GetType()));
            }

            return IUnifiedGUIHelper.GetObjectName(@object);
        }

        if(!string.IsNullOrEmpty(resultType))
        {
            return resultType;
        }

        return null;
    }

    private static IEnumerable<SelectableObject> GetSelectable<TResult>(bool projectAssetsOnly)
        where TResult : class
    {
        var objects = IUnifiedGUIHelper.EnumerateSavedObjects().Concat(projectAssetsOnly ? new Object[0] : Object.FindObjectsOfType<Object>());
        var implementations = new HashSet<TResult>();
        foreach(var implementation in objects.SelectMany(o => GetObjectImplementationsOf<TResult>(o)))
        {
            implementations.Add(implementation);
        }
        return implementations.Select(i => SelectableObject.GetSelectableObject(i));
    }

    /// <summary>
    /// Given a SerializedProperty of a field, List, or array of IUnifiedContainer&lt;T&gt; derivative(s), will initialize null references and return the container derivative type.
    /// </summary>
    /// <returns>The type deriving from IUnifiedContainer&lt;T&gt;, if any - null otherwise.</returns>
    private static readonly Dictionary<Type, Dictionary<string, Type>> ContainerTypesByPaths = new Dictionary<Type, Dictionary<string, Type>>();
    private static readonly Regex ArrayMatch = new Regex(@"\.Array\.data\[\d+\]", RegexOptions.Compiled);
    private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
    private static Type GetContainerType(SerializedProperty property)
    {
        var type = property.serializedObject.targetObject.GetType();
        Dictionary<string, Type> containerTypes;
        if(!ContainerTypesByPaths.TryGetValue(type, out containerTypes))
        {
            ContainerTypesByPaths.Add(type, (containerTypes = new Dictionary<string, Type>()));
        }

        var cleanPath = ArrayMatch.Replace(property.propertyPath, "");
        Type containerType;
        if(containerTypes.TryGetValue(cleanPath, out containerType))
        {
            return containerType;
        }

        List<Type> genericArguments;
        foreach(var field in cleanPath.Split('.'))
        {
            var fieldInfo = GetFieldInChain(type, field);
            if(fieldInfo == null)
            {
                return null;
            }

            type = fieldInfo.FieldType;
            if(type.IsArray)
            {
                type = type.GetElementType();
            }
            else if(IsSubclassOfRawGeneric(type, typeof(List<>), out genericArguments))
            {
                type = genericArguments[0];
            }
        }

        if(IsSubclassOfRawGeneric(type, typeof(IUnifiedContainer<>), out genericArguments))
        {
            containerTypes.Add(cleanPath, type);
            return type;
        }

        return null;
    }

    private static FieldInfo GetFieldInChain(Type type, string fieldName)
    {
        if(type == null)
        {
            return null;
        }

        var field = type.GetField(fieldName, FieldBindingFlags);
        if(field == null)
        {
            return GetFieldInChain(type.BaseType, fieldName);
        }

        return field;
    }

    /// <summary>
    /// Credit to JaredPar: http://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
    /// </summary>
    public static bool IsSubclassOfRawGeneric(Type toCheck, Type generic, out List<Type> genericTypeArguments)
    {
        genericTypeArguments = null;

        while(toCheck != null && toCheck != typeof(object))
        {
            Type checkGeneric;
            if(toCheck.IsGenericType)
            {
                checkGeneric = toCheck.GetGenericTypeDefinition();
            }
            else
            {
                checkGeneric = toCheck;
            }

            if(generic == checkGeneric)
            {
                genericTypeArguments = toCheck.GetGenericArguments().ToList();
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    private static readonly MethodInfo DrawMethodInfo = CachedType<IUnifiedContainerPropertyDrawer>.Type.GetMethod("DrawIUnifiedContainer", BindingFlags.Static | BindingFlags.NonPublic);
    private static readonly Dictionary<Type, DrawMethod> DrawMethods = new Dictionary<Type, DrawMethod>();

    private delegate void DrawMethod(Rect position, GUIContent label, SerializedContainer serializedContainer);
    private static DrawMethod GetDrawMethod(Type type)
    {
        DrawMethod drawMethod;
        if(!DrawMethods.TryGetValue(type, out drawMethod))
        {
            DrawMethods.Add(type, (drawMethod = (DrawMethod)Delegate.CreateDelegate(CachedType<DrawMethod>.Type, DrawMethodInfo.MakeGenericMethod(new[] { type }))));
        }
        return drawMethod;
    }

    #endregion

    /// <summary>
    /// Handy class wrapping around an IUnifiedContainer-derived serialized property.
    /// </summary>
    public class SerializedContainer
    {
        public SerializedProperty ObjectFieldProperty
        {
            get { return _objectFieldProperty != null ? _objectFieldProperty : (_objectFieldProperty = _containerProperty.FindPropertyRelative("ObjectField")); }
        }

        public SerializedProperty ResultTypeProperty
        {
            get { return _resultTypeProperty != null ? _resultTypeProperty : (_resultTypeProperty = _containerProperty.FindPropertyRelative("ResultType")); }
        }

        public Object ObjectField
        {
            get { return ObjectFieldProperty.objectReferenceValue; }
            set
            {
                ResultType = "";
                ObjectFieldProperty.objectReferenceValue = value;
            }
        }

        public string ResultType
        {
            get { return ResultTypeProperty.stringValue; }
            set { ResultTypeProperty.stringValue = value; }
        }

        public bool IsProjectAsset
        {
            get { return IUnifiedGUIHelper.IsProjectAsset(_containerProperty.serializedObject.targetObject); }
        }

        public void ApplyModifiedProperties()
        {
            _containerProperty.serializedObject.ApplyModifiedProperties();
        }

        public bool Dropping
        {
            get { return _droppingHashcode == _containerHashcode; }
        }

        public static bool AnyDropping
        {
            get { return _droppingHashcode != null; }
        }

        public bool Selecting
        {
            get { return _selectWindow != null && _selectingHashcode == _containerHashcode; }
        } 

        public SerializedContainer(int propertyDrawerHash, SerializedProperty containerProperty)
        {
            if(containerProperty == null) {  throw new ArgumentNullException("containerProperty"); }
            _containerProperty = containerProperty;

            unchecked
            {
                var propertyPath = ObjectFieldProperty.propertyPath;
                _containerHashcode = (propertyDrawerHash * 397) ^ (propertyPath == null ? 0 : propertyPath.GetHashCode());
            }
        }

        public void GetContainerContents(Rect rect, bool droppingFor, out GUIContentRect iconContent, out GUIContentRect labelContent)
        {
            Texture icon;
            string resultString, resultTip;
            if(ObjectFieldProperty.hasMultipleDifferentValues)
            {
                icon = null;
                resultString = "-";
                resultTip = null;
            }
            else
            {
                var @object = ObjectField;
                if(IUnifiedGUIHelper.IsProjectAsset(@object) && (@object is Component || @object is GameObject))
                {
                    icon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
                }
                else
                {
                    icon = EditorGUIUtility.ObjectContent(@object, null).image;
                }

                resultString = resultTip = BuildEditorResultString(ResultType, ObjectField);
                if(resultString == null)
                {
                    resultString = "null";
                }
            }

            iconContent = icon == null ? new GUIContentRect(null, rect) : new GUIContentRect(new GUIContent(icon, resultTip), rect);
            labelContent = new GUIContentRect(new GUIContent(resultString, !droppingFor ? resultTip : null), rect);
        }

        public void SetDropping(bool dropping)
        {
            if(Dropping != dropping)
            {
                _droppingHashcode = dropping ? (int?)_containerHashcode : null;
                EditorUtility.SetDirty(_containerProperty.serializedObject.targetObject);
            }
        }

        public void SetSelecting(bool selecting)
        {
            if(Selecting != selecting)
            {
                _selectingHashcode = selecting ? (int?)_containerHashcode : null;
                EditorUtility.SetDirty(_containerProperty.serializedObject.targetObject);
            }
        }

        public bool MouseInRects(Rect rect, Vector2 mousePosition)
        {
            return TimedRect.MouseInRects(_containerHashcode, rect, mousePosition);
        }
        
        private readonly SerializedProperty _containerProperty;
        private SerializedProperty _objectFieldProperty;
        private SerializedProperty _resultTypeProperty;
        private readonly int _containerHashcode;

        private static int? _droppingHashcode = null;
        private static int? _selectingHashcode = null;

        /// <summary>
        /// Created to address a very minor and super-rare graphical inconsistency.
        /// Definitely overkill, but... well there it is. And hey, it works!
        /// </summary>
        private class TimedRect
        {
            private Rect _rect;
            private readonly Stopwatch _stopwatch;

            private TimedRect(Rect rect)
            {
                _rect = rect;
                _stopwatch = Stopwatch.StartNew();
            }

            public static bool MouseInRects(int containerHash, Rect rect, Vector2 mousePosition)
            {
                _lastAccess = Stopwatch.StartNew();

                List<TimedRect> rects;
                if(!TimestampedRects.TryGetValue(containerHash, out rects))
                {
                    TimestampedRects.Add(containerHash, (rects = new List<TimedRect>()));
                }
                rects.Add(new TimedRect(rect));

                var mouseInRect = false;
                rects.RemoveAll(r =>
                {
                    if(r._stopwatch.ElapsedMilliseconds > 100)
                    {
                        return true;
                    }
                    if(r._rect.Contains(mousePosition))
                    {
                        mouseInRect = true;
                    }
                    return false;
                });
                return mouseInRect;
            }

            private static Stopwatch _lastAccess;
            private static readonly Dictionary<int, List<TimedRect>> TimestampedRects = new Dictionary<int, List<TimedRect>>();

            static TimedRect()
            {
                EditorApplication.update += MaintainRects;
            }

            private static void MaintainRects()
            {
                if(_lastAccess != null && _lastAccess.ElapsedMilliseconds > 5000)
                {
                    TimestampedRects.Clear();
                    _lastAccess.Stop();
                    _lastAccess = null;
                }
            }
        }
    }

    public class SelectableObject
    {
        public readonly Object Object;
        public readonly bool IsProjectAsset;
        public readonly bool IsComponent;

        private SelectableObject(Object @object)
        {
            Object = @object;
            IsProjectAsset = IUnifiedGUIHelper.IsProjectAsset(Object);
            IsComponent = Object is Component;
        }

        public static SelectableObject GetSelectableObject<TResult>(TResult result)
        {
            var @object = result as Object;
            return @object == null ? null : new SelectableObject(@object);
        }
    }
}