using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
public class LoggerConfig : ScriptableObject
{
    public bool EnableAllLogs = true;
    public bool EditorOnly = true;

    [SerializeField] public CategorySetting[] categories;

    public bool IsCategoryEnabled(CustomLogger.LogCategory category)
    {
        return GetSetting(category)?.Enabled ?? false;
    }

    public string GetEmoji(CustomLogger.LogCategory category)
    {
        return GetSetting(category)?.Emoji;
    }

    public string GetHexColor(CustomLogger.LogCategory category)
    {
        var color = GetSetting(category)?.TagColor ?? Color.white;
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

    private CategorySetting GetSetting(CustomLogger.LogCategory category)
    {
        return categories?.FirstOrDefault(c => c.Category == category);
    }

    [System.Serializable]
    public class CategorySetting
    {
        public CustomLogger.LogCategory Category;
        [Tooltip("Enable logging for this category")]
        public bool Enabled = true;
        [Tooltip("Display color for log headers")]
        public Color TagColor = Color.white;
        [Tooltip("Emoji prefix (1-2 characters)")]
        public string Emoji = "📝";
    }
}