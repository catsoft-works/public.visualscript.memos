using System;
using System.Collections.Generic;
using UnityEngine;
using VisualScript.Icons;

namespace VisualScript.Runtime.Memos
{
    [Get, Set]

    [Title("Memo")]
    [Category("Variables/Memo")]

    [Description("Access to a temporal variable to read and write values")]
    [Image(typeof(IconMemo), ColorTheme.Type.TextLight)]

    [Keywords("Temp", "Temporal", "Variable", "Memo", "Note", "Sticky")]

    [Serializable]
    public class TypeCollectionGameObjectsMemo : TypeCollectionGameObjects
    {
        [SerializeField] private MemoName m_Name;

        public override void Get(Args args, List<GameObject> values)
        {
            values.Clear();
            List<GameObject> list = MemosManager.Get<List<GameObject>>(this.m_Name.Value);

            int count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                values.Add(list[i]);
            }
        }

        public override void Set(List<GameObject> value, Args args)
        {
            List<GameObject> list = new List<GameObject>(value);
            MemosManager.Set(this.m_Name.Value, list);
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