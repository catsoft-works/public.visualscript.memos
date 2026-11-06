using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VisualScript.Icons;
using VisualScript.Runtime;
using VisualScript.Runtime.Memos;

namespace VisualScript.Editor.Memos
{
    [CustomPropertyDrawer(typeof(MemosRepository))]
    public class MemosRepositoryDrawer : PropertyDrawer
    {
        [NoAutoStaticsCleanup] private static readonly IIcon ICON_INFO = new IconInfoOutline(ColorTheme.Type.TextLight);

        [NoAutoStaticsCleanup] private static List<IMemo> Memos { get; } = new List<IMemo>();

        [NoAutoStaticsCleanup] private static VisualElement Container { get; set; }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            Container = new VisualElement();

            root.Add(new Title("Memos"));
            root.Add(Space.CreateSmaller());
            root.Add(Container);

            root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            RefreshMemos();

            return root;
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        private static void OnAttachToPanel(AttachToPanelEvent panelEvent)
        {
            EditorApplication.playModeStateChanged += OnChangePlayMode;
            ListenMemos(EditorApplication.isPlaying);
        }

        private static void OnDetachFromPanel(DetachFromPanelEvent panelEvent)
        {
            EditorApplication.playModeStateChanged -= OnChangePlayMode;
            ListenMemos(false);
        }

        private static void OnChangePlayMode(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    ListenMemos(true);
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    ListenMemos(false);
                    break;

                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingEditMode:
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            RefreshMemos();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void ListenMemos(bool listen)
        {
            MemosManager.EventChange -= RefreshMemos;
            if (listen) MemosManager.EventChange += RefreshMemos;
        }

        private static void RefreshMemos()
        {
            if (Container == null) return;
            Container.Clear();

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Container.Add(new Message("Memos are only available in Play Mode", ICON_INFO.Texture));
                return;
            }

            List<IMemo> memos = Memos;
            MemosManager.EditorGetMemos(ref memos);

            if (memos.Count == 0)
            {
                Container.Add(new Message("Empty Memos"));
                return;
            }

            foreach (IMemo memo in memos)
            {
                VisualElement element;

                if (memo.Type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    element = new ObjectField
                    {
                        value = memo.Value as UnityEngine.Object,
                        allowSceneObjects = true,
                        label = memo.EditorName,
                        objectType = memo.Type
                    };

                    element.SetEnabled(false);
                    element.AddToClassList(ObjectField.alignedFieldUssClassName);
                }
                else
                {
                    element = new TextField
                    {
                        value = memo.Value?.ToString() ?? string.Empty,
                        label = memo.EditorName,
                        isReadOnly = true
                    };

                    element.SetEnabled(false);
                    element.AddToClassList(TextField.alignedFieldUssClassName);
                }

                Container.Add(element);
            }
        }
    }
}