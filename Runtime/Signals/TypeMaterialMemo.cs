using System;
using UnityEngine;
using VisualScript.Icons;

namespace VisualScript.Runtime.Memos
{
    [Get, Set]

    [Title("Memo")]
    [Category("Variable/Memo")]

    [Description("Access to a temporal variable to read and write values")]
    [Image(typeof(IconMemo), ColorTheme.Type.TextLight)]

    [Keywords("Temp", "Temporal", "Variable", "Memo", "Note", "Sticky")]

    [Serializable]
    public class TypeMaterialMemo : TypeMaterial
    {
        [SerializeField] private MemoName m_Name;

        public override Material Get(Args args)
        {
            return MemosManager.Get<Material>(this.m_Name.Value);
        }

        public override void Set(Material value, Args args)
        {
            MemosManager.Set(this.m_Name.Value, value);
        }

        public override string ToString()
        {
            return this.m_Name.ToString();
        }

        protected override SubscriptionMode Subscription => SubscriptionMode.Custom;

        protected override Guid OnSubscribe(Action<Args> callback, Args args)
        {
            return MemosManager.Subscribe(this.m_Name.Value, callback, args);
        }

        protected override void OnUnsubscribe(Guid subscriptionId)
        {
            MemosManager.Unsubscribe(subscriptionId);
        }
    }
}