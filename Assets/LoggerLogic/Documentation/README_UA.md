# 📘 Logger — Lightweight & Flexible Debugging Tool for Unity

> Професійна система логування з enum категоріями, автокомплітом та оптимізованою продуктивністю


## 🚀 Швидкий старт

### 1. Встановлення

```
1. Скопіюй папку Logger в Assets/
2. Right Click → Create → Logging → Logger Config
3. Відкрий LoggerConfig → "Add Default Categories"
4. Призначь LoggerConfig в CustomLogger.Config
```

### 2. Використання

```csharp
using Azen.Logger;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LoggerConfig loggerConfig;
    
    private void Awake()
    {
        CustomLogger.Config = loggerConfig;
        
        CustomLogger.Log("Гра запущена", LogCategory.System);
        CustomLogger.LogWarning("Низький FPS", LogCategory.Performance);
        CustomLogger.LogError("Помилка з'єднання", LogCategory.Network);
    }
}
```

**Готово!** 🎉

---

## 📁 Структура проекту

```
Assets/
└── LoggerLogic/
    ├── AutoGenerate/
    │   └── LogCategory.cs          # Автогенерований enum (не редагуй вручну!)
    ├── config/
    │   ├── LoggerConfig.cs         # ScriptableObject конфіг
    │   └── LoggerConfig.asset      # Файл конфігу
    ├── Core/
    │   └── Logger/
    │       ├── CustomLogger.cs     # Головний логер
    │       ├── LoggerAdapter.cs    # Адаптер для Dependency Injection
    │       └── ILogger.cs          # Інтерфейс
    ├── Editor/
    │   ├── LoggerCategoriesWindow.cs    # Вікно керування категоріями
    │   ├── LoggerConfigEditor.cs        # Кастомний інспектор
    │   ├── LogCategoryGenerator.cs      # Генератор enum
    │   └── SerializedPropertyExtensions.cs
    └── Example/
        └── LoggerExample.cs        # Приклади використання
```

---

## 📖 API довідка

### Базове логування

```csharp
// Інформаційні логи
CustomLogger.Log(string message, LogCategory category);
CustomLogger.Log(object obj, LogCategory category);

// Попередження
CustomLogger.LogWarning(string message, LogCategory category);

// Помилки
CustomLogger.LogError(string message, LogCategory category);

// Маркери (для відстеження флоу)
CustomLogger.Mark(string message, LogCategory category);
```

### Приклади

```csharp
// Просте повідомлення
CustomLogger.Log("Гравець створений", LogCategory.Gameplay);

// Логування об'єкта
Vector3 pos = transform.position;
CustomLogger.Log(pos, LogCategory.Gameplay);

// Форматування
CustomLogger.Log($"Рахунок: {score}", LogCategory.UI);

// Обробка помилок
try {
    LoadData();
} catch (Exception e) {
    CustomLogger.LogError($"Помилка завантаження: {e.Message}", LogCategory.System);
}
```

### Dependency Injection

```csharp
public interface ILogger
{
    void Log(string message, LogCategory category);
    void LogWarning(string message, LogCategory category);
    void LogError(string message, LogCategory category);
}

// Використання
public class GameService
{
    private readonly ILogger logger;
    
    public GameService(ILogger logger)
    {
        this.logger = logger;
    }
    
    public void DoWork()
    {
        logger.Log("Працюю...", LogCategory.System);
    }
}

// Ініціалізація
ILogger logger = new LoggerAdapter(loggerConfig);
var service = new GameService(logger);
```

---

## 🎨 Керування категоріями

### Додати нову категорію

```
1. Відкрий LoggerConfig Inspector
2. Введи назву: "PlayerInventory"
3. Натисни "Add New Category"
4. Дочекайся компіляції (10-30 сек)
5. Використовуй: CustomLogger.Log("Предмет додано", LogCategory.PlayerInventory);
```

### Додати існуючі категорії

```
1. Відкрий LoggerConfig
2. Натисни "Show Available Categories"
3. Клікни "Add" біля потрібної категорії
```

### Видалити категорію

#### 🗑️ Видалити з Config (м'яке видалення)
- Категорія залишається в enum
- Можна додати назад через "Show Available Categories"
- Код продовжує компілюватись

```
Натисни 🗑️ біля категорії
```

#### 💀 Видалити з Enum (повне видалення)
- ⚠️ Видаляється з enum та config
- ⚠️ Unity перекомпілюється
- ⚠️ Код що використовує цю категорію перестане компілюватись
- ⚠️ Базові категорії (None, System, Error, Warning) захищені

```
Натисни 💀 біля категорії
```

---

## ⚙️ Налаштування

### Глобальні налаштування

| Параметр | Опис |
|----------|------|
| `Enable All Logs` | Увімкнути/вимкнути всі логи |
| `Editor Only` | Логи тільки в редакторі (вимкни для білдів) |

### Налаштування категорії

| Параметр | Опис |
|----------|------|
| `Category` | Enum категорія |
| `Enabled` | Увімкнена/вимкнена |
| `Tag Color` | Колір в консолі |
| `Emoji` | Префікс (1-2 символи) |

---

## 🛠️ Типові помилки

### ❗ "Категорія зникла з enum"

```
1. LoggerConfig → "Regenerate Enum"
2. Якщо не допомогло → "Add Default Categories"
3. Потім → "Add All Missing"
```

### ❗ "Категорія є в enum але не в config"

```
LoggerConfig → "Add All Missing"
```

### ❗ "Unity не компілює після додавання категорії"

```
1. Дочекайся компіляції (може зайняти 30+ сек)
2. Перевір Console на помилки
3. Натисни "Regenerate Enum"
```

### ❗ "Логи не показуються"

```
1. Перевір що "Enable All Logs" увімкнено
2. Перевір що "Enabled" для категорії увімкнено
3. Перевір "Editor Only" для білдів
```

---

## 📄 Файли логів

Зберігаються в:
```
Application.persistentDataPath/Logs/
DebugLog_YYYY-MM-DD_HH-mm-ss.txt
```

**Шляхи:**
- Windows: `%USERPROFILE%\AppData\LocalLow\CompanyName\GameName\Logs\`
- Android: `/storage/emulated/0/Android/data/com.company.game/files/Logs/`
- macOS: `~/Library/Application Support/CompanyName/GameName/Logs/`

---

## ✅ Кращі практики

### ✅ Робити

```csharp
// Використовуй enum
CustomLogger.Log("Тест", LogCategory.System);

// Групуй за логікою
public class NetworkManager
{
    void Connect() { 
        CustomLogger.Log("З'єднання...", LogCategory.Network); 
    }
}

// Використовуй в catch блоках
try {
    LoadData();
} catch (Exception e) {
    CustomLogger.LogError(e.Message, LogCategory.Error);
}
```

### ❌ Не робити

```csharp
// Не логуй в Update без умов
void Update() {
    CustomLogger.Log("Кадр", LogCategory.System); // ❌ Лаг!
}

// Не редагуй LogCategory.cs вручну
// Використовуй Editor!

// Не створюй занадто багато категорій
// Тримай все просто та організовано
```

---

## 🆘 FAQ

**Q: Чому логи не показуються?**  
A: Перевір `Enable All Logs` та `Enabled` для категорії

**Q: Як вимкнути логи в білді?**  
A: Встанови `Editor Only = true` в LoggerConfig

**Q: Категорія зникла після додавання нової?**  
A: Натисни "Regenerate Enum" потім "Add All Missing"

**Q: Чи можна використовувати в продакшені?**  
A: Так! Встанови `Editor Only = true` щоб вимкнути логи в білдах

---

## 📄 Ліцензія

MIT License - використовуй як завгодно!

---

<p align="center">
  <b>🎉 Happy Logging! 🎉</b>
</p>

<p align="center">
  Made with ❤️ for Unity Developers
</p>