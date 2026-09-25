using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace VisualScript.Runtime.Memos
{
    [AddComponentMenu("")]
    public partial class MemosManager : Singleton<MemosManager>
    {
        private readonly struct Subscription
        {
            [NonSerialized] public readonly PropertyName name;
            [NonSerialized] public readonly Action<Args> callback;
            [NonSerialized] public readonly Args args;

            public Subscription(PropertyName name, Action<Args> callback, Args args)
            {
                this.name = name;
                this.callback = callback;
                this.args = args;
            }
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private readonly Dictionary<PropertyName, IMemo> m_Memos = new Dictionary<PropertyName, IMemo>();

        [NonSerialized]
        private readonly Dictionary<Guid, Subscription> m_Subscriptions = new Dictionary<Guid, Subscription>();

        // EVENTS: --------------------------------------------------------------------------------

        [AutoStaticsCleanup] public static event Action EventChange;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static T Get<T>(PropertyName name)
        {
            return Instance.m_Memos.TryGetValue(name, out IMemo memo)
                ? memo is TMemo<T> memoTyped 
                    ? memoTyped.Value
                    : memo.Convert<T>()
                : default;
        }

        public static void Set<T>(PropertyName name, T value)
        {
            if (Instance.m_Memos.TryGetValue(name, out IMemo memo) && memo is TMemo<T> memoTyped)
            {
                memoTyped.Value = value;
                return;
            }

            memoTyped = new TMemo<T>(name, OnChange);
            Instance.m_Memos.Add(name, memoTyped);

            memoTyped.Value = value;
        }

        public static Guid Subscribe(PropertyName name, Action<Args> callback, Args args)
        {
            Guid subscriptionId = Guid.NewGuid();
            while (Instance.m_Subscriptions.ContainsKey(subscriptionId))
            {
                subscriptionId = Guid.NewGuid();
            }

            Subscription subscription = new Subscription(name, callback, args);
            Instance.m_Subscriptions.Add(subscriptionId, subscription);

            return subscriptionId;
        }

        public static void Unsubscribe(Guid subscriptionId)
        {
            Instance.m_Subscriptions.Remove(subscriptionId);
        }

        // EDITOR METHODS: ------------------------------------------------------------------------

        #if UNITY_EDITOR

        public static void EditorGetMemos(ref List<IMemo> memos)
        {
            memos.Clear();

            if (!Application.isPlaying) return;
            memos.AddRange(Instance.m_Memos.Values);
        }

        #endif

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void OnChange(PropertyName name)
        {
            EventChange?.Invoke();

            foreach (Subscription subscription in Instance.m_Subscriptions.Values)
            {
                if (subscription.name != name) continue;
                subscription.callback?.Invoke(subscription.args);
            }
        }
    }
}