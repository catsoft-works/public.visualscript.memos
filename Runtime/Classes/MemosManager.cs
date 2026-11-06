using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace VisualScript.Runtime.Memos
{
    [AddComponentMenu("")]
    public partial class MemosManager : Singleton<MemosManager>
    {
        private readonly struct Subscription : IDisposable
        {
            [NonSerialized] public readonly PropertyName name;
            [NonSerialized] private readonly Action<Args> callback;
            [NonSerialized] private readonly Args args;

            public Subscription(PropertyName name, Action<Args> callback, Args args)
            {
                this.name = name;
                this.callback = callback;
                this.args = Args.Get(args);
            }

            public void Invoke()
            {
                using Args newArgs = Args.Get(this.args);
                this.callback?.Invoke(newArgs);
            }

            public void Dispose() => this.args?.Dispose();
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

            Instance.m_Memos[name] = new TMemo<T>(name, value, OnChange);
            OnChange(name);
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
            Instance.m_Subscriptions.Remove(subscriptionId, out Subscription subscription);
            subscription.Dispose();
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

            Dictionary<Guid, Subscription> subscriptions = Instance.m_Subscriptions;
            List<Guid> ids = ListPool<Guid>.Get();

            try
            {
                foreach (KeyValuePair<Guid, Subscription> entry in subscriptions)
                {
                    if (entry.Value.name == name) ids.Add(entry.Key);
                }

                foreach (Guid id in ids)
                {
                    if (subscriptions.TryGetValue(id, out Subscription subscription))
                    {
                        subscription.Invoke();
                    }
                }
            }
            finally
            {
                ListPool<Guid>.Release(ids);
            }
        }
    }
}