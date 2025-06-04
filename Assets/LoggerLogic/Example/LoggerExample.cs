using UnityEngine;

namespace Azen.Logger
{
    public class LoggerExample : MonoBehaviour
    {
        [Header("Drag your LoggerConfig here")]
        public LoggerConfig loggerConfig;

        private void Awake()
        {
            // Init Cfg
            CustomLogger.Config = loggerConfig;

            // Example Log
            CustomLogger.Log("This is a system log", CustomLogger.LogCategory.System);
            CustomLogger.LogWarning("UI warning", CustomLogger.LogCategory.UI);
            CustomLogger.LogError("Gameplay error", CustomLogger.LogCategory.Gameplay);

            Vector3 position = transform.position;
            CustomLogger.Log(position, CustomLogger.LogCategory.Other);

            // Mark Log
            CustomLogger.Mark("Start Game Flow", CustomLogger.LogCategory.Gameplay);
        }
    }
}


