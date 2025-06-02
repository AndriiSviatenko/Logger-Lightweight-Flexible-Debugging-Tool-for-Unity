using UnityEngine;

public class LoggerExample : MonoBehaviour
{
    [Header("Drag your LoggerConfig here")]
    public LoggerConfig loggerConfig;

    private void Awake()
    {
        CustomLogger.Config = loggerConfig;

        // Крок 2: Приклади логування
        CustomLogger.Log("Це звичайний лог (System)", CustomLogger.LogCategory.System);
        CustomLogger.LogWarning("Це попередження (UI)", CustomLogger.LogCategory.UI);
        CustomLogger.LogError("Це помилка (Gameplay)", CustomLogger.LogCategory.Gameplay);

        // Крок 3: Лог об'єкта
        Vector3 position = transform.position;
        CustomLogger.Log(position, CustomLogger.LogCategory.Other);

        // Крок 4: Маркер логування
        CustomLogger.Mark("Start Game Flow", CustomLogger.LogCategory.Gameplay);
    }
}

