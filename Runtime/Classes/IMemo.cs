using System;

namespace VisualScript.Runtime.Memos
{
    public interface IMemo
    {
        #if UNITY_EDITOR

        Type Type { get; }
        object Value { get; }

        string EditorName { get; }

        #endif

        T Convert<T>();
    }
}