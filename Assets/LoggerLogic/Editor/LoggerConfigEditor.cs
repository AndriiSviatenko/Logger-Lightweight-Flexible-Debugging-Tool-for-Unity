#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Azen.Logger
{
    [CustomEditor(typeof(LoggerConfig))]
    public class LoggerConfigEditor : Editor
    {
        private SerializedProperty _categoriesProp;
        private SerializedProperty _enableAllProp;
        private SerializedProperty _editorOnlyProp;

        private Vector2 _scrollPosition;
        private Vector2 _availableScrollPosition;
        private string _searchFilter = "";
        private string _newCategoryName = "";

        private bool _showAvailableCategories = false;

        private void OnEnable()
        {
            _enableAllProp = serializedObject.FindProperty("EnableAllLogs");
            _editorOnlyProp = serializedObject.FindProperty("EditorOnly");
            _categoriesProp = serializedObject.FindProperty("categories");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGlobalSettings();
            DrawAddCategorySection();
            DrawCategorySettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGlobalSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableAllProp);
            EditorGUILayout.PropertyField(_editorOnlyProp);
            EditorGUILayout.Space();
        }

        private void DrawAddCategorySection()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Add New Category", EditorStyles.boldLabel);

                EditorGUILayout.HelpBox(
                    "Введи назву нової категорії. Після додавання enum LogCategory буде автоматично згенеровано.",
                    MessageType.Info
                );

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Category Name:", GUILayout.Width(110));
                    _newCategoryName = EditorGUILayout.TextField(_newCategoryName);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                    if (GUILayout.Button("➕ Add New Category", GUILayout.Height(30)))
                    {
                        AddNewCategory();
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
                    if (GUILayout.Button("🔄 Add Default Categories", GUILayout.Height(30)))
                    {
                        InitializeDefaultCategories();
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("Add Existing Categories", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_showAvailableCategories ? "🔽 Hide Available Categories" : "▶️ Show Available Categories", GUILayout.Height(25)))
                    {
                        _showAvailableCategories = !_showAvailableCategories;
                    }

                    if (GUILayout.Button("➕ Add All Missing", GUILayout.Height(25)))
                    {
                        AddMissingEnumCategories();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_showAvailableCategories)
                {
                    DrawAvailableCategoriesList();
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("🔧 Regenerate Enum", GUILayout.Height(25)))
                    {
                        RegenerateEnum();
                    }

                    if (GUILayout.Button("📂 Open Category Manager", GUILayout.Height(25)))
                    {
                        LoggerCategoriesWindow.ShowWindow();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        private void DrawAvailableCategoriesList()
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Available Categories (Click to Add):", EditorStyles.miniLabel);

                var config = (LoggerConfig)target;
                var availableCategories = GetAvailableCategories();

                if (availableCategories.Count == 0)
                {
                    EditorGUILayout.HelpBox("All enum categories are already added!", MessageType.Info);
                }
                else
                {
                    _availableScrollPosition = EditorGUILayout.BeginScrollView(_availableScrollPosition, GUILayout.MaxHeight(150));
                    {
                        foreach (var category in availableCategories)
                        {
                            EditorGUILayout.BeginHorizontal(GUI.skin.box);
                            {
                                EditorGUILayout.LabelField(category.ToString(), GUILayout.Width(150));

                                GUILayout.FlexibleSpace();

                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                                if (GUILayout.Button("➕ Add", GUILayout.Width(60)))
                                {
                                    AddCategoryToConfig(category, "📝", GetRandomColor());
                                    EditorUtility.SetDirty(config);
                                    Repaint();
                                }
                                GUI.backgroundColor = Color.white;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private List<LogCategory> GetAvailableCategories()
        {
            var config = (LoggerConfig)target;
            var existingInConfig = new HashSet<LogCategory>();

            if (config.categories != null)
            {
                foreach (var cat in config.categories)
                {
                    existingInConfig.Add(cat.Category);
                }
            }

            var availableCategories = new List<LogCategory>();
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                if (!existingInConfig.Contains(category))
                {
                    availableCategories.Add(category);
                }
            }

            return availableCategories;
        }

        private void AddNewCategory()
        {
            if (string.IsNullOrWhiteSpace(_newCategoryName))
            {
                EditorUtility.DisplayDialog("Invalid Name",
                    "Category name cannot be empty!", "OK");
                return;
            }

            var config = (LoggerConfig)target;

            if (System.Enum.IsDefined(typeof(LogCategory), _newCategoryName))
            {
                EditorUtility.DisplayDialog("Category Exists",
                    $"Category '{_newCategoryName}' already exists in enum!", "OK");
                return;
            }

            LogCategoryGenerator.AddNewCategory(config, _newCategoryName);

            _newCategoryName = "";
            GUI.FocusControl(null);

            EditorUtility.DisplayDialog("Success",
                "Category added! Unity will recompile...", "OK");
        }

        private void RegenerateEnum()
        {
            var config = (LoggerConfig)target;
            LogCategoryGenerator.GenerateCategoriesEnum(config);

            EditorUtility.DisplayDialog("Success",
                "LogCategory enum regenerated!", "OK");
        }

        private void AddMissingEnumCategories()
        {
            var config = (LoggerConfig)target;
            var existingInConfig = new HashSet<LogCategory>();

            if (config.categories != null)
            {
                foreach (var cat in config.categories)
                {
                    existingInConfig.Add(cat.Category);
                }
            }

            var missingCategories = new List<LogCategory>();
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                if (!existingInConfig.Contains(category))
                {
                    missingCategories.Add(category);
                }
            }

            if (missingCategories.Count == 0)
            {
                EditorUtility.DisplayDialog("Info",
                    "All enum categories are already in config!", "OK");
                return;
            }

            foreach (var category in missingCategories)
            {
                AddCategoryToConfig(category, "📝", GetRandomColor());
            }

            EditorUtility.SetDirty(config);

            EditorUtility.DisplayDialog("Success",
                $"Added {missingCategories.Count} missing categories to config!", "OK");
        }

        private Color GetRandomColor()
        {
            var colors = new[]
            {
                Color.cyan, Color.green, Color.yellow, Color.magenta,
                new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f),
                new Color(0f, 1f, 1f), new Color(1f, 0f, 0.5f)
            };
            return colors[UnityEngine.Random.Range(0, colors.Length)];
        }

        private void DrawCategorySettings()
        {
            EditorGUILayout.LabelField("Category Configuration", EditorStyles.boldLabel);
            DrawSearchField();

            if (_categoriesProp.arraySize == 0)
            {
                DrawEmptyState();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(400));
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
                _searchFilter = EditorGUILayout.TextField(_searchFilter);

                if (GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    _searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            EditorGUILayout.HelpBox("No categories configured. Add categories above or initialize defaults.", MessageType.Info);
        }

        private List<SerializedProperty> GetFilteredCategories()
        {
            var filtered = new List<SerializedProperty>();
            for (int i = 0; i < _categoriesProp.arraySize; i++)
            {
                var prop = _categoriesProp.GetArrayElementAtIndex(i);
                var category = (LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;

                if (string.IsNullOrEmpty(_searchFilter) ||
                    category.ToString().Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
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
                EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(categoryWidth));
                EditorGUILayout.LabelField("Enabled", EditorStyles.boldLabel, GUILayout.Width(toggleWidth));
                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel, GUILayout.Width(colorWidth));
                EditorGUILayout.LabelField("Emoji", EditorStyles.boldLabel, GUILayout.Width(emojiWidth));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("🗑️=Config 💀=Enum", EditorStyles.miniLabel, GUILayout.Width(120));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryRows(List<SerializedProperty> categories, float categoryWidth,
            float toggleWidth, float colorWidth, float emojiWidth)
        {
            var duplicateTracker = new HashSet<LogCategory>();
            var duplicates = new HashSet<LogCategory>();

            foreach (var prop in categories)
            {
                var category = (LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;
                if (!duplicateTracker.Add(category))
                {
                    duplicates.Add(category);
                }
            }

            for (int i = 0; i < categories.Count; i++)
            {
                var prop = categories[i];
                int originalIndex = _categoriesProp.IndexOf(prop);
                bool elementRemoved = false;

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    var categoryProp = prop.FindPropertyRelative("Category");
                    var currentCategory = (LogCategory)categoryProp.enumValueIndex;
                    bool isDuplicate = duplicates.Contains(currentCategory);

                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawCategoryField(categoryProp, categoryWidth, isDuplicate);
                        DrawToggleField(prop.FindPropertyRelative("Enabled"), toggleWidth);
                        DrawColorField(prop.FindPropertyRelative("TagColor"), colorWidth);
                        DrawEmojiField(prop.FindPropertyRelative("Emoji"), emojiWidth);

                        EditorGUI.BeginDisabledGroup(originalIndex == 0);
                        if (GUILayout.Button("↑", GUILayout.Width(20)))
                        {
                            MoveCategory(originalIndex, -1);
                            elementRemoved = true;
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(originalIndex == _categoriesProp.arraySize - 1);
                        if (GUILayout.Button("↓", GUILayout.Width(20)))
                        {
                            MoveCategory(originalIndex, 1);
                            elementRemoved = true;
                        }
                        EditorGUI.EndDisabledGroup();

                        GUI.backgroundColor = new Color(1f, 0.8f, 0.5f);
                        if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                        {
                            DeleteCategoryFromConfig(prop);
                            elementRemoved = true;
                        }
                        GUI.backgroundColor = Color.white;

                        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                        if (GUILayout.Button("💀", GUILayout.Width(25)))
                        {
                            DeleteCategoryFromEnum(prop);
                            elementRemoved = true;
                        }
                        GUI.backgroundColor = Color.white;
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
            if (currentIndex >= 0 && currentIndex < _categoriesProp.arraySize)
            {
                serializedObject.Update();
                _categoriesProp.MoveArrayElement(currentIndex, currentIndex + direction);
                serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawCategoryField(SerializedProperty property, float width, bool isDuplicate)
        {
            var originalColor = GUI.color;
            if (isDuplicate) GUI.color = Color.red;

            var currentCategory = (LogCategory)property.enumValueIndex;
            var newCategory = (LogCategory)EditorGUILayout.EnumPopup(
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

        private bool IsCategoryUnique(LogCategory category, SerializedProperty currentProperty)
        {
            for (int i = 0; i < _categoriesProp.arraySize; i++)
            {
                var prop = _categoriesProp.GetArrayElementAtIndex(i);
                if (SerializedProperty.EqualContents(prop, currentProperty)) continue;

                var existingCategory = (LogCategory)prop.FindPropertyRelative("Category").enumValueIndex;
                if (existingCategory == category)
                {
                    return false;
                }
            }
            return true;
        }

        private void DeleteCategoryFromConfig(SerializedProperty categoryToDelete)
        {
            int index = _categoriesProp.IndexOf(categoryToDelete);
            if (index >= 0)
            {
                var categoryName = ((LogCategory)categoryToDelete.FindPropertyRelative("Category").enumValueIndex).ToString();

                if (EditorUtility.DisplayDialog("Remove from Config",
                    $"Remove '{categoryName}' from config?\n\n⚠️ Category залишиться в enum.\n" +
                    "Ти зможеш додати її назад через 'Show Available Categories'.",
                    "Remove", "Cancel"))
                {
                    _categoriesProp.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DeleteCategoryFromEnum(SerializedProperty categoryToDelete)
        {
            var category = (LogCategory)categoryToDelete.FindPropertyRelative("Category").enumValueIndex;
            var categoryName = category.ToString();

            if (category == LogCategory.None || category == LogCategory.System ||
                category == LogCategory.Error || category == LogCategory.Warning)
            {
                EditorUtility.DisplayDialog("Cannot Delete",
                    $"'{categoryName}' є базовою категорією і не може бути видалена!",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Delete from Enum",
                $"⚠️ ВИДАЛИТИ '{categoryName}' ПОВНІСТЮ?\n\n" +
                "Це видалить категорію з:\n" +
                "• LoggerConfig\n" +
                "• Enum LogCategory\n\n" +
                "⚠️ Unity перекомпілюється!\n" +
                "⚠️ Код що використовує цю категорію перестане компілюватись!\n\n" +
                "Продовжити?",
                "Yes, Delete Completely", "Cancel"))
            {
                return;
            }

            var config = (LoggerConfig)target;

            int index = _categoriesProp.IndexOf(categoryToDelete);
            if (index >= 0)
            {
                _categoriesProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
            }

            LogCategoryGenerator.DeleteCategory(config, categoryName);

            EditorUtility.DisplayDialog("Deleted",
                $"Category '{categoryName}' видалена з enum!\n\n" +
                "Unity перекомпілюється...",
                "OK");
        }

        private void DrawToggleField(SerializedProperty property, float width)
        {
            property.boolValue = EditorGUILayout.Toggle(property.boolValue, GUILayout.Width(width));
        }

        private void DrawColorField(SerializedProperty property, float width)
        {
            property.colorValue = EditorGUILayout.ColorField(property.colorValue, GUILayout.Width(width));
        }

        private void DrawEmojiField(SerializedProperty property, float width)
        {
            string emoji = property.stringValue;
            string newEmoji = EditorGUILayout.TextField(emoji, GUILayout.Width(width));

            if (newEmoji != emoji)
            {
                property.stringValue = newEmoji.Length > 2 ? newEmoji.Substring(0, 2) : newEmoji;
            }
        }

        private void InitializeDefaultCategories()
        {
            if (EditorUtility.DisplayDialog("Initialize Default Categories",
                "This will add default categories and regenerate the enum. Continue?", "Yes", "No"))
            {
                var defaultCategories = new[]
                {
                    ("System", "⚙️", Color.cyan),
                    ("UI", "🖥️", Color.green),
                    ("Network", "🌐", Color.yellow),
                    ("Gameplay", "🎮", new Color(1f, 0.5f, 0f)),
                    ("Audio", "🔊", Color.magenta),
                    ("Error", "❌", Color.red),
                    ("Warning", "⚠️", new Color(1f, 0.8f, 0f)),
                    ("Performance", "⚡", new Color(0f, 1f, 1f)),
                    ("Analytics", "📊", new Color(0.5f, 0.5f, 1f)),
                    ("Input", "🎯", new Color(1f, 0.5f, 1f)),
                    ("Other", "📝", Color.gray)
                };

                var config = (LoggerConfig)target;
                var categoriesToAdd = new List<string>();

                foreach (var (name, emoji, color) in defaultCategories)
                {
                    if (!System.Enum.IsDefined(typeof(LogCategory), name))
                    {
                        categoriesToAdd.Add(name);
                    }
                }

                if (categoriesToAdd.Count > 0)
                {
                    var allCategories = System.Enum.GetNames(typeof(LogCategory))
                        .Concat(categoriesToAdd)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var enumCode = GenerateEnumCode(allCategories);
                    var savePath = GetEnumPath();
                    System.IO.File.WriteAllText(savePath, enumCode);
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Success",
                        $"Added {categoriesToAdd.Count} categories! Unity will recompile...\n\n" +
                        "After compilation, press 'Add Default Categories' again to add them to config.",
                        "OK");
                }
                else
                {
                    foreach (var (name, emoji, color) in defaultCategories)
                    {
                        if (System.Enum.TryParse<LogCategory>(name, out var category))
                        {
                            if (!config.CategoryExists(category))
                            {
                                AddCategoryToConfig(category, emoji, color);
                            }
                        }
                    }

                    EditorUtility.SetDirty(config);
                    EditorUtility.DisplayDialog("Success", "Default categories added to config!", "OK");
                }
            }
        }

        private void AddCategoryToConfig(LogCategory category, string emoji, Color color)
        {
            _categoriesProp.arraySize++;
            var newElement = _categoriesProp.GetArrayElementAtIndex(_categoriesProp.arraySize - 1);
            newElement.FindPropertyRelative("Category").enumValueIndex = (int)category;
            newElement.FindPropertyRelative("Enabled").boolValue = true;
            newElement.FindPropertyRelative("TagColor").colorValue = color;
            newElement.FindPropertyRelative("Emoji").stringValue = emoji;
            serializedObject.ApplyModifiedProperties();
        }

        private string GenerateEnumCode(List<string> categories)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("// AUTO-GENERATED FILE - DO NOT EDIT MANUALLY");
            sb.AppendLine("// This file is generated by LogCategoryGenerator");
            sb.AppendLine("// To add new categories, use Tools -> Logger -> Category Manager");
            sb.AppendLine();
            sb.AppendLine("namespace Azen.Logger");
            sb.AppendLine("{");
            sb.AppendLine("    public enum LogCategory");
            sb.AppendLine("    {");

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i].Replace(" ", "");
                sb.AppendLine($"        {category} = {i},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetEnumPath()
        {
            var guids = AssetDatabase.FindAssets("LogCategory t:MonoScript");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return "Assets/Scripts/Logger/LogCategory.cs";
        }
    }
}
#endif