#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Azen.Logger
{
    /// <summary>
    /// Окреме вікно для керування категоріями логера
    /// Відкрити: Tools -> Logger -> Category Manager
    /// </summary>
    public class LoggerCategoriesWindow : EditorWindow
    {
        private LoggerConfig _selectedConfig;
        private Vector2 _scrollPosition;
        private Vector2 _availableScrollPosition;
        private string _newCategoryName = "";
        private string _searchFilter = "";

        private Color _newCategoryColor = Color.white;
        private string _newCategoryEmoji = "📝";
        private bool _newCategoryEnabled = true;

        private bool _showAvailableCategories = false;

        [MenuItem("Tools/Logger/Category Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoggerCategoriesWindow>("Logger Categories");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();

            if (_selectedConfig == null)
            {
                DrawConfigSelection();
                return;
            }

            DrawConfigInfo();
            DrawAddCategorySection();
            DrawCategoriesList();
            DrawFooter();
        }

        #region Header
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Logger Category Manager", titleStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("📖 Documentation", GUILayout.Width(150)))
                {
                    Application.OpenURL("https://github.com/yourrepo/logger-docs");
                }
                if (GUILayout.Button("🔄 Refresh", GUILayout.Width(100)))
                {
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            DrawSeparator();
        }
        #endregion

        #region Config Selection
        private void DrawConfigSelection()
        {
            EditorGUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "Select a LoggerConfig to manage its categories.\n" +
                "Create one via: Right Click → Create → Logging → Logger Config",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Select Config:", GUILayout.Width(100));
                _selectedConfig = (LoggerConfig)EditorGUILayout.ObjectField(
                    _selectedConfig,
                    typeof(LoggerConfig),
                    false
                );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Find LoggerConfigs in Project", GUILayout.Height(30)))
            {
                FindAndSelectConfig();
            }
        }

        private void FindAndSelectConfig()
        {
            var guids = AssetDatabase.FindAssets("t:LoggerConfig");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Configs Found",
                    "No LoggerConfig assets found in the project.\n" +
                    "Create one via: Right Click → Create → Logging → Logger Config",
                    "OK"
                );
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _selectedConfig = AssetDatabase.LoadAssetAtPath<LoggerConfig>(path);
        }
        #endregion

        #region Config Info
        private void DrawConfigInfo()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Current Config:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    _selectedConfig = (LoggerConfig)EditorGUILayout.ObjectField(
                        _selectedConfig,
                        typeof(LoggerConfig),
                        false
                    );

                    if (GUILayout.Button("Clear", GUILayout.Width(60)))
                    {
                        _selectedConfig = null;
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();

                var so = new SerializedObject(_selectedConfig);
                var enableAll = so.FindProperty("EnableAllLogs");
                var editorOnly = so.FindProperty("EditorOnly");

                EditorGUILayout.PropertyField(enableAll);
                EditorGUILayout.PropertyField(editorOnly);

                so.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
        }
        #endregion

        #region Add Category Section
        private void DrawAddCategorySection()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Add New Category", EditorStyles.boldLabel);

                EditorGUILayout.HelpBox(
                    "Введи назву нової категорії. Після додавання enum LogCategory буде автоматично згенеровано.",
                    MessageType.Info
                );

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Name:", GUILayout.Width(80));
                    _newCategoryName = EditorGUILayout.TextField(_newCategoryName);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Emoji:", GUILayout.Width(80));
                    _newCategoryEmoji = EditorGUILayout.TextField(_newCategoryEmoji, GUILayout.Width(60));

                    EditorGUILayout.LabelField("Color:", GUILayout.Width(60));
                    _newCategoryColor = EditorGUILayout.ColorField(_newCategoryColor, GUILayout.Width(60));

                    EditorGUILayout.LabelField("Enabled:", GUILayout.Width(60));
                    _newCategoryEnabled = EditorGUILayout.Toggle(_newCategoryEnabled, GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                    if (GUILayout.Button("➕ Add Category", GUILayout.Height(30)))
                    {
                        AddCategory();
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
                    if (GUILayout.Button("🔄 Add Defaults", GUILayout.Height(30)))
                    {
                        AddDefaultCategories();
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("🔧 Regenerate Enum", GUILayout.Height(25)))
                    {
                        RegenerateEnum();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
        }

        private void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(_newCategoryName))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Category name cannot be empty!", "OK");
                return;
            }

            if (Enum.IsDefined(typeof(LogCategory), _newCategoryName))
            {
                EditorUtility.DisplayDialog("Category Exists",
                    $"Category '{_newCategoryName}' already exists in enum!", "OK");
                return;
            }

            LogCategoryGenerator.AddNewCategory(_selectedConfig, _newCategoryName);

            _newCategoryName = "";
            _newCategoryEmoji = "📝";
            _newCategoryColor = Color.white;
            _newCategoryEnabled = true;
            GUI.FocusControl(null);

            EditorUtility.DisplayDialog("Success",
                "Category added! Unity will recompile...", "OK");
        }

        private void RegenerateEnum()
        {
            LogCategoryGenerator.GenerateCategoriesEnum(_selectedConfig);
            EditorUtility.DisplayDialog("Success", "LogCategory enum regenerated!", "OK");
        }

        private void AddDefaultCategories()
        {
            if (!EditorUtility.DisplayDialog(
                "Add Default Categories",
                "This will add standard categories and regenerate enum. Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            var defaults = new[]
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

            var categoriesToAdd = new List<string>();

            foreach (var (name, emoji, color) in defaults)
            {
                if (!Enum.IsDefined(typeof(LogCategory), name))
                {
                    categoriesToAdd.Add(name);
                }
            }

            if (categoriesToAdd.Count > 0)
            {
                var allCategories = Enum.GetNames(typeof(LogCategory))
                    .Concat(categoriesToAdd)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                LogCategoryGenerator.GenerateCategoriesEnum(_selectedConfig);

                EditorUtility.DisplayDialog("Success",
                    $"Adding {categoriesToAdd.Count} categories! Unity will recompile...\n\n" +
                    "After compilation, open LoggerConfig to add them to config.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info",
                    "All default categories already exist in enum!\n" +
                    "Open LoggerConfig Inspector to add them to config.",
                    "OK");
            }

            Repaint();
        }
        #endregion

        #region Categories List
        private void DrawCategoriesList()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Categories", EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
                    _searchFilter = EditorGUILayout.TextField(_searchFilter, GUILayout.Width(150));

                    if (GUILayout.Button("✕", GUILayout.Width(25)))
                    {
                        _searchFilter = "";
                        GUI.FocusControl(null);
                    }
                }
                EditorGUILayout.EndHorizontal();

                DrawSeparator();

                var so = new SerializedObject(_selectedConfig);
                var categories = so.FindProperty("categories");

                if (categories.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("No categories configured. Add categories above!", MessageType.Info);
                }
                else
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
                    {
                        DrawCategoryTable(so, categories);
                    }
                    EditorGUILayout.EndScrollView();
                }

                so.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCategoryTable(SerializedObject so, SerializedProperty categories)
        {
            var toDelete = -1;
            var toDeleteFromEnum = -1;

            for (int i = 0; i < categories.arraySize; i++)
            {
                var category = categories.GetArrayElementAtIndex(i);
                var categoryEnum = (LogCategory)category.FindPropertyRelative("Category").enumValueIndex;
                var name = categoryEnum.ToString();

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    var categoryProp = category.FindPropertyRelative("Category");
                    categoryProp.enumValueIndex = (int)(LogCategory)EditorGUILayout.EnumPopup(
                        categoryEnum,
                        GUILayout.Width(120)
                    );

                    var emoji = category.FindPropertyRelative("Emoji");
                    emoji.stringValue = EditorGUILayout.TextField(
                        emoji.stringValue,
                        GUILayout.Width(40)
                    );

                    var color = category.FindPropertyRelative("TagColor");
                    color.colorValue = EditorGUILayout.ColorField(
                        GUIContent.none,
                        color.colorValue,
                        false, false, false,
                        GUILayout.Width(40)
                    );

                    var enabled = category.FindPropertyRelative("Enabled");
                    enabled.boolValue = EditorGUILayout.Toggle(
                        enabled.boolValue,
                        GUILayout.Width(20)
                    );

                    GUILayout.FlexibleSpace();

                    GUI.backgroundColor = new Color(1f, 0.8f, 0.5f);
                    if (GUILayout.Button("🗑️", GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Remove from Config",
                            $"Remove '{name}' from config?\n\n⚠️ Category залишиться в enum.",
                            "Remove", "Cancel"))
                        {
                            toDelete = i;
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                    if (GUILayout.Button("💀", GUILayout.Width(30)))
                    {
                        toDeleteFromEnum = i;
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (toDelete >= 0)
            {
                categories.DeleteArrayElementAtIndex(toDelete);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedConfig);
            }

            if (toDeleteFromEnum >= 0)
            {
                DeleteCategoryFromEnum(categories.GetArrayElementAtIndex(toDeleteFromEnum));
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

            var so = new SerializedObject(_selectedConfig);
            var categories = so.FindProperty("categories");

            for (int i = 0; i < categories.arraySize; i++)
            {
                var cat = categories.GetArrayElementAtIndex(i);
                if ((LogCategory)cat.FindPropertyRelative("Category").enumValueIndex == category)
                {
                    categories.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_selectedConfig);

            LogCategoryGenerator.DeleteCategory(_selectedConfig, categoryName);

            EditorUtility.DisplayDialog("Deleted",
                $"Category '{categoryName}' видалена з enum!\n\nUnity перекомпілюється...",
                "OK");

            Close();
        }
        #endregion

        #region Footer
        private void DrawFooter()
        {
            EditorGUILayout.Space(5);
            DrawSeparator();

            var so = new SerializedObject(_selectedConfig);
            var categories = so.FindProperty("categories");

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"Total Categories: {categories.arraySize}", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Azen Logger v2.0 (Enum)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space(2);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUILayout.Space(2);
        }
        #endregion
    }
}
#endif