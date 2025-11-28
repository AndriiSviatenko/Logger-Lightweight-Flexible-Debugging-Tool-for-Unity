# 📘 Logger — Lightweight & Flexible Debugging Tool for Unity

> Professional logging system with enum categories, autocomplete and optimized performance

## 🚀 Quick Start

### 1. Setup

```
1. Copy the Logger folder to Assets/
2. Right Click → Create → Logging → Logger Config
3. Open LoggerConfig → "Add Default Categories"
4. Assign LoggerConfig to CustomLogger.Config
```

### 2. Usage

```csharp
using Azen.Logger;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LoggerConfig loggerConfig;
    
    private void Awake()
    {
        CustomLogger.Config = loggerConfig;
        
        CustomLogger.Log("Game started", LogCategory.System);
        CustomLogger.LogWarning("Low FPS", LogCategory.Performance);
        CustomLogger.LogError("Connection failed", LogCategory.Network);
    }
}
```

**Done!** 🎉

---

## 📁 Project Structure

```
Assets/
└── LoggerLogic/
    ├── AutoGenerate/
    │   └── LogCategory.cs          # Auto-generated enum (don't edit manually!)
    ├── config/
    │   ├── LoggerConfig.cs         # ScriptableObject config
    │   └── LoggerConfig.asset      # Config asset
    ├── Core/
    │   └── Logger/
    │       ├── CustomLogger.cs     # Main logger
    │       ├── LoggerAdapter.cs    # Dependency Injection adapter
    │       └── ILogger.cs          # Interface
    ├── Editor/
    │   ├── LoggerCategoriesWindow.cs    # Category manager window
    │   ├── LoggerConfigEditor.cs        # Custom inspector
    │   ├── LogCategoryGenerator.cs      # Enum generator
    │   └── SerializedPropertyExtensions.cs
    └── Example/
        └── LoggerExample.cs        # Usage examples
```

---

## 📖 API Reference

### Basic Logging

```csharp
// Information logs
CustomLogger.Log(string message, LogCategory category);
CustomLogger.Log(object obj, LogCategory category);

// Warnings
CustomLogger.LogWarning(string message, LogCategory category);

// Errors
CustomLogger.LogError(string message, LogCategory category);

// Markers (for tracking flow)
CustomLogger.Mark(string message, LogCategory category);
```

### Examples

```csharp
// Simple message
CustomLogger.Log("Player spawned", LogCategory.Gameplay);

// Object logging
Vector3 pos = transform.position;
CustomLogger.Log(pos, LogCategory.Gameplay);

// Formatting
CustomLogger.Log($"Score: {score}", LogCategory.UI);

// Error handling
try {
    LoadData();
} catch (Exception e) {
    CustomLogger.LogError($"Failed to load: {e.Message}", LogCategory.System);
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

// Usage
public class GameService
{
    private readonly ILogger logger;
    
    public GameService(ILogger logger)
    {
        this.logger = logger;
    }
    
    public void DoWork()
    {
        logger.Log("Working...", LogCategory.System);
    }
}

// Initialize
ILogger logger = new LoggerAdapter(loggerConfig);
var service = new GameService(logger);
```

---

## 🎨 Category Management

### Add New Category

```
1. Open LoggerConfig Inspector
2. Enter name: "PlayerInventory"
3. Click "Add New Category"
4. Wait for compilation (10-30 sec)
5. Use: CustomLogger.Log("Item added", LogCategory.PlayerInventory);
```

### Add Existing Categories

```
1. Open LoggerConfig
2. Click "Show Available Categories"
3. Click "Add" next to needed category
```

### Delete Category

#### 🗑️ Remove from Config (soft delete)
- Category stays in enum
- Can be added back via "Show Available Categories"
- Code continues to compile

```
Click 🗑️ next to category
```

#### 💀 Delete from Enum (hard delete)
- ⚠️ Deleted from both enum and config
- ⚠️ Unity will recompile
- ⚠️ Code using this category will fail to compile
- ⚠️ Base categories (None, System, Error, Warning) are protected

```
Click 💀 next to category
```

---

## ⚙️ Configuration

### Global Settings

| Parameter | Description |
|-----------|-------------|
| `Enable All Logs` | Enable/disable all logs |
| `Editor Only` | Logs only in editor (disable for builds) |

### Category Settings

| Parameter | Description |
|-----------|-------------|
| `Category` | Enum category |
| `Enabled` | Enabled/disabled |
| `Tag Color` | Console color |
| `Emoji` | Prefix (1-2 characters) |

---

## 🛠️ Common Issues

### ❗ "Category disappeared from enum"

```
1. LoggerConfig → "Regenerate Enum"
2. If doesn't help → "Add Default Categories"
3. Then → "Add All Missing"
```

### ❗ "Category in enum but not in config"

```
LoggerConfig → "Add All Missing"
```

### ❗ "Unity doesn't compile after adding category"

```
1. Wait for compilation (may take 30+ sec)
2. Check Console for errors
3. Click "Regenerate Enum"
```

### ❗ "Logs not showing"

```
1. Check "Enable All Logs" is ON
2. Check category "Enabled" is ON
3. Check "Editor Only" setting for builds
```

---

## 📄 Log Files

Saved to:
```
Application.persistentDataPath/Logs/
DebugLog_YYYY-MM-DD_HH-mm-ss.txt
```

**Paths:**
- Windows: `%USERPROFILE%\AppData\LocalLow\CompanyName\GameName\Logs\`
- Android: `/storage/emulated/0/Android/data/com.company.game/files/Logs/`
- macOS: `~/Library/Application Support/CompanyName/GameName/Logs/`

---

## ✅ Best Practices

### ✅ Do

```csharp
// Use enums
CustomLogger.Log("Test", LogCategory.System);

// Group by logic
public class NetworkManager
{
    void Connect() { 
        CustomLogger.Log("Connecting...", LogCategory.Network); 
    }
}

// Use in catch blocks
try {
    LoadData();
} catch (Exception e) {
    CustomLogger.LogError(e.Message, LogCategory.Error);
}
```

### ❌ Don't

```csharp
// Don't log in Update without conditions
void Update() {
    CustomLogger.Log("Frame", LogCategory.System); // ❌ Lag!
}

// Don't edit LogCategory.cs manually
// Use the Editor instead!

// Don't use too many categories
// Keep it simple and organized
```

---

## 🆘 FAQ

**Q: Why aren't logs showing?**  
A: Check `Enable All Logs` and category `Enabled`

**Q: How to disable logs in builds?**  
A: Set `Editor Only = true` in LoggerConfig

**Q: Category disappeared after adding new one?**  
A: Click "Regenerate Enum" then "Add All Missing"

**Q: Can I use this in production?**  
A: Yes! Set `Editor Only = true` to disable logs in builds

---

## 📄 License

MIT License - use as you wish!

---

<p align="center">
  <b>🎉 Happy Logging! 🎉</b>
</p>

<p align="center">
  Made with ❤️ for Unity Developers
</p>