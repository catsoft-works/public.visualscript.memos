using System;
using UnityEngine;

namespace VisualScript.Runtime.Memos
{
    [Serializable]
    public struct MemoName : ISerializationCallbackReceiver
    {
        [SerializeField] private string m_Name;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PropertyName Value => this.m_Name;

        // STRING: --------------------------------------------------------------------------------

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.m_Name)
                ? $"${this.m_Name}"
                : TextUtils.TEXT_EMPTY;
        }

        // SERIALIZATION: -------------------------------------------------------------------------

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (AssemblyUtils.IsReloading) return;
            this.m_Name = TextUtils.MakeId(this.m_Name);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        { }
    }
}