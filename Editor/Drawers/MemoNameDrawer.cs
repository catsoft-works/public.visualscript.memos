using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualScript.Icons;
using VisualScript.Runtime;
using VisualScript.Runtime.Memos;

namespace VisualScript.Editor.Memos
{
    [CustomPropertyDrawer(typeof(MemoName))]
    public class MemoNameDrawer : PropertyDrawer
    {
        [Serializable]
        private class Favorites
        {
            public string[] values = Array.Empty<string>();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // CONSTANTS: -----------------------------------------------------------------------------

        private const string USS_PATH = "Packages/works.catsoft.visualscript-memos/Editor/StyleSheets/MemoName.uss";

        private const string NAME_FAVORITE = "VS-MemoName-Favorite";
        private const string NAME_DROPDOWN = "VS-MemoName-Dropdown";

        private const string TIP_FAVORITE = "Toggle as favorite";
        private const string TIP_DROPDOWN = "Choose from the favorite list";

        [NoAutoStaticsCleanup] private static readonly IIcon ICON_FAVORITE_ON = new IconStarSolid(ColorTheme.Type.Yellow);
        [NoAutoStaticsCleanup] private static readonly IIcon ICON_FAVORITE_OFF = new IconStarOutline(ColorTheme.Type.TextLight);
        [NoAutoStaticsCleanup] private static readonly IIcon ICON_DROPDOWN = new IconDropdown(ColorTheme.Type.TextLight);

        private const string KEY_MEMO_NAMES_FAVORITES = "vs:memos:favorites";

        // PAINT METHODS: -------------------------------------------------------------------------

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.LoadStyleSheet(USS_PATH);

            SerializedProperty propertyString = property.FindPropertyRelative("m_Name");
            TextField textField = new TextField
            {
                label = " ",
                bindingPath = propertyString.propertyPath
            };

            textField.AddToClassList(TextField.alignedFieldUssClassName);

            Image favorite = new Image
            {
                image = IsFavorite(propertyString.stringValue)
                    ? ICON_FAVORITE_ON.Texture
                    : ICON_FAVORITE_OFF.Texture,
                name = NAME_FAVORITE,
                tooltip = TIP_FAVORITE,
                focusable = true
            };

            favorite.SetEnabled(!string.IsNullOrEmpty(propertyString.stringValue));
            textField.RegisterValueChangedCallback(changeEvent =>
            {
                favorite.SetEnabled(!string.IsNullOrEmpty(changeEvent.newValue));
                favorite.image = IsFavorite(changeEvent.newValue)
                    ? ICON_FAVORITE_ON.Texture
                    : ICON_FAVORITE_OFF.Texture;
            });

            favorite.RegisterCallback<ClickEvent>(_ =>
            {
                string text = propertyString.stringValue;
                if (string.IsNullOrEmpty(text)) return;

                if (IsFavorite(text))
                {
                    RemoveFavorite(text);
                    favorite.image = ICON_FAVORITE_OFF.Texture;
                }
                else
                {
                    AddFavorite(text);
                    favorite.image = ICON_FAVORITE_ON.Texture;
                }
            });

            Image dropdown = new Image
            {
                image = ICON_DROPDOWN.Texture,
                name = NAME_DROPDOWN,
                tooltip = TIP_DROPDOWN,
                focusable = true
            };

            dropdown.AddManipulator(new ButtonMenuManipulator(context =>
            {
                string[] favoriteNames = GetFavoriteNames();
                if (favoriteNames.Length == 0)
                {
                    context.menu.AppendAction(
                        "No favorite Memo names found", null,
                        DropdownMenuAction.Status.Disabled
                    );

                    return;
                }

                foreach (string favoriteName in favoriteNames)
                {
                    context.menu.AppendAction(
                        favoriteName,
                        menuAction =>
                        {
                            propertyString.serializedObject.Update();
                            propertyString.stringValue = menuAction.name;

                            propertyString.serializedObject.ApplyModifiedProperties();
                            propertyString.serializedObject.Update();
                        },
                        menuAction => menuAction.name != propertyString.stringValue
                            ? DropdownMenuAction.Status.Normal
                            : DropdownMenuAction.Status.Checked);
                }
            }));

            textField.contentContainer.contentContainer.Add(favorite);
            textField.contentContainer.Add(dropdown);

            root.Add(textField);
            root.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);

            return root;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static string[] GetFavoriteNames()
        {
            string json = EditorPrefs.GetString(KeyFavorites, string.Empty);
            if (string.IsNullOrEmpty(json)) return Array.Empty<string>();

            Favorites favorites = JsonUtility.FromJson<Favorites>(json);
            return favorites != null ? favorites.values : Array.Empty<string>();
        }

        private static bool IsFavorite(string text)
        {
            string[] favorites = GetFavoriteNames();
            foreach (string favorite in favorites)
            {
                if (favorite == text) return true;
            }

            return false;
        }

        private static void AddFavorite(string text)
        {
            List<string> values = new List<string>(GetFavoriteNames()) { text };
            values.Sort();

            Favorites favorites = new Favorites
            {
                values = values.ToArray()
            };

            string json = JsonUtility.ToJson(favorites);
            EditorPrefs.SetString(KeyFavorites, json);
        }

        private static void RemoveFavorite(string text)
        {
            List<string> values = new List<string>(GetFavoriteNames());
            values.Remove(text);

            Favorites favorites = new Favorites
            {
                values = values.ToArray()
            };

            string json = JsonUtility.ToJson(favorites);
            EditorPrefs.SetString(KeyFavorites, json);
        }

        private static string KeyFavorites
        {
            get
            {
                int hashProductName = Application.productName.GetHashCode();
                return $"{KEY_MEMO_NAMES_FAVORITES}:{hashProductName}";
            }
        }
    }
}