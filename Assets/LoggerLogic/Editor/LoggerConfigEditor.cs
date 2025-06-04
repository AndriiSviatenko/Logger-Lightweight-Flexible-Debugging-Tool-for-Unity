#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Azen.Logger
{
    [CustomEditor(typeof(LoggerConfig))]
    public class LoggerConfigEditor : Editor
    {
        private SerializedProperty categoriesProp;
        private SerializedProperty enableAllProp;
        private SerializedProperty editorOnlyProp;

        private Vector2 scrollPosition;
        private string searchFilter = "";

        private void OnEnable()
        {
            enableAllProp = serializedObject.FindProperty("EnableAllLogs");
            editorOnlyProp = serializedObject.FindProperty("EditorOnly");
            categoriesProp = serializedObject.FindProperty("categories");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGlobalSettings();
            DrawCategorySettings();
            DrawAddCategoryButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAddCategoryButton()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Category", GUILayout.Height(30)))
            {
                ShowAddCategoryMenu();
            }
            EditorGUILayout.Space();
        }

        private void ShowAddCategoryMenu()
        {
            GenericMenu menu = new GenericMenu();
            var existingCategories = GetExistingCategories();

            foreach (CustomLogger.LogCategory category in Enum.GetValues(typeof(CustomLogger.LogCategory)))
            {
                if (!existingCategories.Contains(category))
                {
                    menu.AddItem(new GUIContent(category.ToString()),
                        false,
                        () => AddNewCategory(category));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent($"{category} (Already exists)"));
                }
            }

            menu.ShowAsContext();
        }

        private HashSet<CustomLogger.LogCategory> GetExistingCategories()
        {
            var categories = new HashSet<CustomLogger.LogCategory>();
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var prop = categoriesProp.GetArrayElementAtIndex(i);
                categories.Add((CustomLogger.LogCategory)prop.FindPropertyRelative("Category").enumValueIndex);
            }
            return categories;
        }

        private void AddNewCategory(CustomLogger.LogCategory category)
        {
            categoriesProp.arraySize++;
            var newElement = categoriesProp.GetArrayElementAtIndex(categoriesProp.arraySize - 1);
            newElement.FindPropertyRelative("Category").enumValueIndex = (int)category;
            newElement.FindPropertyRelative("Enabled").boolValue = true;
            newElement.FindPropertyRelative("TagColor").colorValue = Color.white;
            newElement.FindPropertyRelative("Emoji").stringValue = "❓";

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGlobalSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableAllProp);
            EditorGUILayout.PropertyField(editorOnlyProp);
            EditorGUILayout.Space();
        }

        private void DrawCategorySettings()
        {
            EditorGUILayout.LabelField("Category Configuration", EditorStyles.boldLabel);
            DrawSearchField();

            if (categoriesProp.arraySize == 0)
            {
                DrawEmptyState();
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                var filtered = GetFilteredCategories();
                if (filtered.Count == 0)
                {
                    EditorGUILayout.HelpBox("No matching categories found", MessageType.Info);
                }
                else
                {
                    DrawCategoryTable(filtered);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
                searchFilter = EditorGUILayout.TextField(searchFilter);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            EditorGUILayout.HelpBox("No categories configured", MessageType.Info);
            if (GUILayout.Button("Initialize Default Categories"))
            {
                InitializeDefaultCategories();
            }
        }

        private List<SerializedProperty> GetFilteredCategories()
        {
            var filtered = new List<SerializedProperty>();
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var prop = categoriesProp.GetArrayElementAtIndex(i);
                var category = (CustomLogger.LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;
                if (category.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                {
                    filtered.Add(prop);
                }
            }
            return filtered;
        }

        private void DrawCategoryTable(List<SerializedProperty> categories)
        {
            const float categoryWidth = 120f;
            const float toggleWidth = 60f;
            const float colorWidth = 80f;
            const float emojiWidth = 60f;

            DrawTableHeader(categoryWidth, toggleWidth, colorWidth, emojiWidth);
            DrawCategoryRows(categories, categoryWidth, toggleWidth, colorWidth, emojiWidth);
        }

        private void DrawTableHeader(float categoryWidth, float toggleWidth, float colorWidth, float emojiWidth)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Category", GUILayout.Width(categoryWidth));
                EditorGUILayout.LabelField("Enabled", GUILayout.Width(toggleWidth));
                EditorGUILayout.LabelField("Color", GUILayout.Width(colorWidth));
                EditorGUILayout.LabelField("Emoji", GUILayout.Width(emojiWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryRows(List<SerializedProperty> categories, float categoryWidth,
        float toggleWidth, float colorWidth, float emojiWidth)
        {
            var duplicateTracker = new HashSet<CustomLogger.LogCategory>();
            var duplicates = new HashSet<CustomLogger.LogCategory>();

            // Перший прохід: виявлення дублікатів
            foreach (var prop in categories)
            {
                var category = (CustomLogger.LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;
                if (!duplicateTracker.Add(category))
                {
                    duplicates.Add(category);
                }
            }

            // Другий прохід: візуалізація
            for (int i = 0; i < categories.Count; i++)
            {
                var prop = categories[i];
                int originalIndex = categoriesProp.IndexOf(prop);
                bool elementRemoved = false;

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    var categoryProp = prop.FindPropertyRelative("Category");
                    var currentCategory = (CustomLogger.LogCategory)categoryProp.enumValueIndex;
                    bool isDuplicate = duplicates.Contains(currentCategory);

                    EditorGUILayout.BeginHorizontal();
                    {
                        // Додаємо поля для редагування категорії
                        DrawCategoryField(categoryProp, categoryWidth, isDuplicate);
                        DrawToggleField(prop.FindPropertyRelative("Enabled"), toggleWidth);
                        DrawColorField(prop.FindPropertyRelative("TagColor"), colorWidth);
                        DrawEmojiField(prop.FindPropertyRelative("Emoji"), emojiWidth);

                        // Кнопки переміщення
                        EditorGUI.BeginDisabledGroup(originalIndex == 0);
                        if (GUILayout.Button("↑", GUILayout.Width(20)))
                        {
                            MoveCategory(originalIndex, -1);
                            elementRemoved = true;
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(originalIndex == categoriesProp.arraySize - 1);
                        if (GUILayout.Button("↓", GUILayout.Width(20)))
                        {
                            MoveCategory(originalIndex, 1);
                            elementRemoved = true;
                        }
                        EditorGUI.EndDisabledGroup();

                        // Кнопка видалення
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            DeleteCategory(prop);
                            elementRemoved = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (isDuplicate)
                    {
                        EditorGUILayout.HelpBox(
                            $"Category '{currentCategory}' already exists!",
                            MessageType.Error
                        );
                    }
                }
                EditorGUILayout.EndVertical();

                if (elementRemoved)
                {
                    break;
                }
            }
        }

        private void MoveCategory(int currentIndex, int direction)
        {
            if (currentIndex >= 0 && currentIndex < categoriesProp.arraySize)
            {
                serializedObject.Update();
                categoriesProp.MoveArrayElement(currentIndex, currentIndex + direction);
                serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawCategoryField(SerializedProperty property, float width, bool isDuplicate)
        {
            var originalColor = GUI.color;
            if (isDuplicate) GUI.color = Color.red;

            var currentCategory = (CustomLogger.LogCategory)property.enumValueIndex;
            var newCategory = (CustomLogger.LogCategory)EditorGUILayout.EnumPopup(
                currentCategory,
                GUILayout.Width(width)
            );

            if (newCategory != currentCategory)
            {
                if (IsCategoryUnique(newCategory, property))
                {
                    property.enumValueIndex = (int)newCategory;
                }
                else
                {
                    Debug.LogWarning($"Category '{newCategory}' already exists!");
                }
            }

            GUI.color = originalColor;
        }

        private bool IsCategoryUnique(CustomLogger.LogCategory category, SerializedProperty currentProperty)
        {
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var prop = categoriesProp.GetArrayElementAtIndex(i);
                if (SerializedProperty.EqualContents(prop, currentProperty)) continue;

                var existingCategory = (CustomLogger.LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;
                if (existingCategory == category)
                {
                    return false;
                }
            }
            return true;
        }

        private void DeleteCategory(SerializedProperty categoryToDelete)
        {
            int index = categoriesProp.IndexOf(categoryToDelete);
            if (index >= 0)
            {
                categoriesProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawToggleField(SerializedProperty property, float width)
        {
            bool value = property.boolValue;
            bool newValue = EditorGUILayout.Toggle(value, GUILayout.Width(width));
            if (newValue != value)
            {
                property.boolValue = newValue;
            }
        }

        private void DrawColorField(SerializedProperty property, float width)
        {
            Color color = property.colorValue;
            Color newColor = EditorGUILayout.ColorField(color, GUILayout.Width(width));
            if (newColor != color)
            {
                property.colorValue = newColor;
            }
        }

        private void DrawEmojiField(SerializedProperty property, float width)
        {
            string emoji = property.stringValue;
            string newEmoji = EditorGUILayout.TextField(emoji, GUILayout.Width(width));

            if (newEmoji != emoji)
            {
                // Обмеження до 2 символів
                property.stringValue = newEmoji.Length > 2 ?
                    newEmoji.Substring(0, 2) :
                    newEmoji;
            }
        }

        private void InitializeDefaultCategories()
        {
            var defaultCategories = new[]
            {
            NewSetting(CustomLogger.LogCategory.System, "⚙️", Color.cyan),
            NewSetting(CustomLogger.LogCategory.UI, "🖥️", Color.green),
            NewSetting(CustomLogger.LogCategory.Network, "🌐", Color.yellow),
            NewSetting(CustomLogger.LogCategory.Gameplay, "🎮", new Color(1f, 0.5f, 0f)),
            NewSetting(CustomLogger.LogCategory.Audio, "🔊", Color.magenta),
            NewSetting(CustomLogger.LogCategory.Error, "❌", Color.red),
            NewSetting(CustomLogger.LogCategory.Warning, "⚠️", new Color(1f, 0.8f, 0f)),
            NewSetting(CustomLogger.LogCategory.Other, "📝", Color.gray)
        };

            var config = (LoggerConfig)target;
            config.categories = defaultCategories;
            EditorUtility.SetDirty(config);
        }

        private static LoggerConfig.CategorySetting NewSetting(
            CustomLogger.LogCategory category,
            string emoji,
            Color color)
        {
            return new LoggerConfig.CategorySetting
            {
                Category = category,
                Emoji = emoji,
                TagColor = color,
                Enabled = true
            };
        }
    }
}
#endif