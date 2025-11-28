using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Azen.Logger
{
    [CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
    public class LoggerConfig : ScriptableObject
    {
        public bool EnableAllLogs = true;
        public bool EditorOnly = true;

        [SerializeField] public CategorySetting[] categories;

        public bool IsCategoryEnabled(LogCategory category)
        {
            return GetSetting(category)?.Enabled ?? false;
        }

        public string GetEmoji(LogCategory category)
        {
            return GetSetting(category)?.Emoji;
        }

        public string GetHexColor(LogCategory category)
        {
            var color = GetSetting(category)?.TagColor ?? Color.white;
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        private CategorySetting GetSetting(LogCategory category)
        {
            return categories?.FirstOrDefault(c => c.Category == category);
        }

        public bool CategoryExists(LogCategory category)
        {
            return categories?.Any(c => c.Category == category) ?? false;
        }

        public LogCategory[] GetAllCategories()
        {
            if (categories == null) return new LogCategory[0];
            return categories.Select(c => c.Category).ToArray();
        }

        [System.Serializable]
        public class CategorySetting
        {
            [Tooltip("Категорія логу")]
            public LogCategory Category;

            [Tooltip("Enable logging for this category")]
            public bool Enabled = true;

            [Tooltip("Display color for log headers")]
            public Color TagColor = Color.white;

            [Tooltip("Emoji prefix (1-2 characters)")]
            public string Emoji = "📝";
        }
    }
}