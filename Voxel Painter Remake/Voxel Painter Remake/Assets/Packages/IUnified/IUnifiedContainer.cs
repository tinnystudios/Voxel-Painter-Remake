using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Derivatives should be decorated with [System.Serializable] attribute.
/// </summary>
[Serializable]
public abstract class IUnifiedContainer<TResult> : IUnifiedContainerBase.IUnifiedContainerBase
    where TResult : class
{
    public TResult Result
    {
        //Using the null coalescing operator will break web player execution
        get
        {
            #if UNITY_EDITOR
                if(ObjectField == null && string.IsNullOrEmpty(ResultType))
                {
                    return _result = null;
                }
                if(string.IsNullOrEmpty(ResultType))
                {
                    _result = null;
                }
            #endif

            return _result != null ? _result : (_result = ObjectField as TResult);
        }
        set
        {
            _result = value;
            ObjectField = _result as Object;

            #if UNITY_EDITOR
                if(!Application.isPlaying)
                {
                    if(_result != null && ObjectField == null)
                    {
                        Debug.LogWarning("IUnifiedContainer: Cannot set Result property to non UnityEngine.Object derived types while application is not running.");
                        _result = null;
                    }
                }
                ResultType = _result != null ? ConstructResolvedName(_result.GetType()) : "";
            #endif
        }
    }

    public Object Object
    {
        //Using the null coalescing operator will break web player execution
        get { return ObjectField != null ? ObjectField : (ObjectField = _result as Object); }
    }
    
    private TResult _result;
}

namespace IUnifiedContainerBase
{
    /// <summary>
    /// Used to enable a single CustomPropertyDrawer for all derivatives.
    /// Do not derive from this class, use the generic IUnifiedContainer&lt;TResult&gt; class instead.
    /// </summary>
    [Serializable]
    public abstract class IUnifiedContainerBase
    {
        [SerializeField]
        [HideInInspector]
        protected Object ObjectField;

        //#if UNITY_EDITOR - Excluding this from the build seems to freak the serializer out and somehow result in prefab references coming through null - nonprefabs seem to continue working though.
        //Used internally to display properly in drawer.
#pragma warning disable 414
        [SerializeField]
        [HideInInspector]
        protected string ResultType;
#pragma warning restore 414
        //#endif

#if UNITY_EDITOR
        protected static Func<Type, string> _constructResolvedName;
        protected static string ConstructResolvedName(Type type)
        {
            if(type == null)
            {
                return "";
            }

            return _constructResolvedName == null ? type.Name : _constructResolvedName(type);
        }
#endif
    }
}