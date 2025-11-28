#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Azen.Logger
{
    /// <summary>
    /// Генерує enum LogCategory на основі категорій в LoggerConfig
    /// </summary>
    public static class LogCategoryGenerator
    {
        private const string EnumFileName = "LogCategory.cs";
        private const string DefaultPath = "Assets/Scripts/Logger";

        [MenuItem("Tools/Logger/Generate Categories Enum")]
        public static void GenerateFromMenu()
        {
            var configs = FindAllLoggerConfigs();
            if (configs.Length == 0)
            {
                EditorUtility.DisplayDialog("No Config Found",
                    "No LoggerConfig found in project!", "OK");
                return;
            }

            var config = configs[0];
            GenerateCategoriesEnum(config);
        }

        public static void GenerateCategoriesEnum(LoggerConfig config)
        {
            var categories = ExtractCategories(config);
            var enumCode = GenerateEnumCode(categories);
            var savePath = GetOrCreateEnumPath();

            File.WriteAllText(savePath, enumCode);
            AssetDatabase.Refresh();

            Debug.Log($"[Logger] Generated LogCategory enum with {categories.Count} categories at: {savePath}");
        }

        public static void GenerateCategoriesEnumFromList(List<string> categories)
        {
            var enumCode = GenerateEnumCode(categories);
            var savePath = GetOrCreateEnumPath();

            File.WriteAllText(savePath, enumCode);
            AssetDatabase.Refresh();

            Debug.Log($"[Logger] Generated LogCategory enum with {categories.Count} categories at: {savePath}");
        }

        private static List<string> ExtractCategories(LoggerConfig config)
        {
            var categories = new HashSet<string> { "None" }; // Завжди додаємо None

            try
            {
                var existingEnumValues = Enum.GetNames(typeof(LogCategory));
                foreach (var enumValue in existingEnumValues)
                {
                    categories.Add(enumValue);
                }
            }
            catch
            {
            }

            if (config.categories != null)
            {
                foreach (var category in config.categories)
                {
                    categories.Add(category.Category.ToString());
                }
            }

            return categories.OrderBy(c => c == "None" ? "" : c).ToList();
        }


        public static List<string> ExtractCategoriesFromConfigOnly(LoggerConfig config)
        {
            var categories = new HashSet<string> { "None" }; // Завжди додаємо None

            if (config.categories != null)
            {
                foreach (var category in config.categories)
                {
                    categories.Add(category.Category.ToString());
                }
            }

            return categories.OrderBy(c => c == "None" ? "" : c).ToList();
        }

        public static void AddNewCategory(LoggerConfig config, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                Debug.LogError("[Logger] Category name cannot be empty!");
                return;
            }

            var existingCategories = ExtractCategories(config);
            if (existingCategories.Contains(categoryName))
            {
                Debug.LogWarning($"[Logger] Category '{categoryName}' already exists!");
                return;
            }

            existingCategories.Add(categoryName);

            var enumCode = GenerateEnumCode(existingCategories);
            var savePath = GetOrCreateEnumPath();
            File.WriteAllText(savePath, enumCode);

            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                AddCategoryToConfig(config, categoryName);
            };
        }

        public static void DeleteCategory(LoggerConfig config, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                Debug.LogError("[Logger] Category name cannot be empty!");
                return;
            }

            var categories = ExtractCategoriesFromConfigOnly(config);

            categories.Remove(categoryName);

            GenerateCategoriesEnumFromList(categories);

            Debug.Log($"[Logger] Deleted category '{categoryName}' from enum");
        }

        private static void AddCategoryToConfig(LoggerConfig config, string categoryName)
        {
            var so = new SerializedObject(config);
            var categories = so.FindProperty("categories");

            var enumType = typeof(LogCategory);
            if (!System.Enum.TryParse(enumType, categoryName, out var enumValue))
            {
                Debug.LogError($"[Logger] Failed to parse enum value for: {categoryName}");
                return;
            }

            categories.arraySize++;
            var newElement = categories.GetArrayElementAtIndex(categories.arraySize - 1);
            newElement.FindPropertyRelative("Category").enumValueIndex = (int)enumValue;
            newElement.FindPropertyRelative("Enabled").boolValue = true;
            newElement.FindPropertyRelative("TagColor").colorValue = GetRandomColor();
            newElement.FindPropertyRelative("Emoji").stringValue = "📝";

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);

            Debug.Log($"[Logger] Added category '{categoryName}' to config");
        }

        private static string GenerateEnumCode(List<string> categories)
        {
            var sb = new StringBuilder();

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
                var category = SanitizeName(categories[i]);
                sb.AppendLine($"        {category} = {i},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string SanitizeName(string name)
        {
            name = name.Replace(" ", "");
            name = name.Replace("-", "");
            name = name.Replace("_", "");

            if (char.IsLower(name[0]))
            {
                name = char.ToUpper(name[0]) + name.Substring(1);
            }

            return name;
        }

        private static string GetOrCreateEnumPath()
        {
            var guids = AssetDatabase.FindAssets($"t:MonoScript {EnumFileName.Replace(".cs", "")}");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return path;
            }

            if (!Directory.Exists(DefaultPath))
            {
                Directory.CreateDirectory(DefaultPath);
            }

            return Path.Combine(DefaultPath, EnumFileName);
        }

        private static LoggerConfig[] FindAllLoggerConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:LoggerConfig");
            var configs = new LoggerConfig[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                configs[i] = AssetDatabase.LoadAssetAtPath<LoggerConfig>(path);
            }

            return configs;
        }

        private static Color GetRandomColor()
        {
            var colors = new[]
            {
                Color.cyan, Color.green, Color.yellow, Color.magenta,
                new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f),
                new Color(0f, 1f, 1f), new Color(1f, 0f, 0.5f)
            };
            return colors[UnityEngine.Random.Range(0, colors.Length)];
        }
    }
}
#endif