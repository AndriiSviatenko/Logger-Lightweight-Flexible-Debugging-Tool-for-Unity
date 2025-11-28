using System.Runtime.CompilerServices;
using UnityEngine;

namespace Azen.Logger
{
    public class LoggerExample : MonoBehaviour
    {
        [Header("Drag your LoggerConfig here")]
        public LoggerConfig loggerConfig;

        private void Awake()
        {
            CustomLogger.Config = loggerConfig;

            CustomLogger.Log("System initialized", LogCategory.System);

            CustomLogger.LogWarning("Low memory warning", LogCategory.Warning);

            CustomLogger.LogError("Failed to connect", LogCategory.Network);

            CustomLogger.Log("Player spawned", LogCategory.Gameplay);
            CustomLogger.Log("UI element created", LogCategory.UI);

            Vector3 position = transform.position;
            CustomLogger.Log(position, LogCategory.Other);

            CustomLogger.Mark("Start Game Flow", LogCategory.Analytics);
            CustomLogger.Mark("Low memory", LogCategory.Analytics);
            CustomLogger.Mark("Player", LogCategory.Analytics);
            CustomLogger.Mark("UI element", LogCategory.Analytics);
        }
    }
}

