using System;
using UnityEngine;

namespace VisualScript.Runtime.Memos
{
    public class TMemo<T> : IMemo
    {
        [NonSerialized] private readonly PropertyName m_Name;
        [NonSerialized] private T m_Value;
        [NonSerialized] private readonly Action<PropertyName> m_OnChange;

        #if UNITY_EDITOR

        Type IMemo.Type => typeof(T);

        object IMemo.Value => this.m_Value;

        string IMemo.EditorName => this.m_Name.ToString().Split(':')[0];

        #endif

        public T Value
        {
            get => m_Value;
            set
            {
                if (CompareUtils.Equals(this.m_Value, value)) return;

                this.m_Value = value;
                this.m_OnChange?.Invoke(this.m_Name);
            }
        }

        public TMemo(PropertyName name, T value, Action<PropertyName> onChange)
        {
            this.m_Name = name;
            this.m_Value = value;
            this.m_OnChange = onChange;
        }

        public U Convert<U>()
        {
            return ConvertUtils.To<U, T>(this.m_Value);
        }
    }
}