# Dependency Injection Framework

A lightweight, reflection-based dependency injection container for .NET applications with support for multiple service lifetimes and attribute-based injection.

## Features

-   🚀 **Multiple Registration Methods**: Instance, factory, and type-based registration
-   🔄 **Service Lifetimes**: Transient, Singleton, and Scoped support
-   💉 **Attribute-Based Injection**: Automatic dependency injection using `[Inject]` attribute
-   🔍 **Auto-Discovery**: Automatic detection and injection of classes with injectable members
-   🎯 **Constructor Injection**: Automatic resolution of constructor dependencies
-   📦 **Unity Compatible**: Designed to work seamlessly with Unity Package Manager

## Installation

### Unity Package Manager

Add to your `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.yourname.dependencyinjection": "https://github.com/yourusername/dependency-injection.git"
    }
}
```

### Manual Installation

Copy the Runtime folder contents to your project.

## Quick Start

### 1. Basic Service Registration and Resolution

```csharp
using YourFramework.DI;

// Create container
var container = new ServiceContainer();

// Register services
container.Register<ILogger, ConsoleLogger>(ServiceLifetime.Singleton);
container.RegisterInstance<IConfig>(new AppConfig());

// Resolve services
var logger = container.Resolve<ILogger>();
var config = container.Resolve<IConfig>();
```

### 2. Attribute-Based Injection

```csharp
public class PlayerService
{
    [Inject] public ILogger Logger { get; set; }
    [Inject] private IDatabase _database;

    public void SavePlayer(Player player)
    {
        Logger.Log("Saving player...");
        _database.Save(player);
    }
}

// Setup and inject
var container = new ServiceContainer();
container.Register<ILogger, ConsoleLogger>();
container.Register<IDatabase, SqlDatabase>();

var injector = new Injector(container);
var playerService = new PlayerService();
injector.Inject(playerService); // Dependencies automatically injected
```

## API Reference

### ServiceContainer

#### Registration Methods

```csharp
// Type-based registration
container.Register<TInterface, TImplementation>(ServiceLifetime lifetime = Transient);

// Instance registration (always singleton)
container.RegisterInstance<T>(T instance);

// Factory registration
container.RegisterFactory<T>(Func<T> factory, ServiceLifetime lifetime = Transient);
```

#### Resolution Methods

```csharp
// Generic resolution
T service = container.Resolve<T>();

// Non-generic resolution
object service = container.Resolve(Type serviceType);

// Safe resolution
bool success = container.TryResolve<T>(out T service);
bool success = container.TryResolve(Type serviceType, out object service);
```

### Service Lifetimes

| Lifetime    | Description                         | Use Cases                                |
| ----------- | ----------------------------------- | ---------------------------------------- |
| `Transient` | New instance every time             | Stateless services, lightweight objects  |
| `Singleton` | One instance for entire application | Expensive objects, caches, configuration |
| `Scoped`    | One instance per scope              | Per-request state, per-session data      |

### Fluent Extensions

```csharp
// Extension methods for cleaner syntax
container.AddTransient<IService, ServiceImpl>();
container.AddSingleton<IService, ServiceImpl>();
container.AddSingleton<IService>(new ServiceImpl());
container.AddScoped<IService, ServiceImpl>();
```

### Injector

```csharp
var injector = new Injector(container);

// Inject dependencies into existing object
injector.Inject(targetObject);
```

### Auto-Discovery

```csharp
var autoWirer = new AutoWirer(container);

// Automatically discover and inject all classes with [Inject] attributes
autoWirer.AutoWire(); // Uses calling assembly
autoWirer.AutoWire(specificAssembly);
```

## Examples

### Basic Usage

```csharp
public interface IWeaponSystem
{
    void Attack(ITarget target);
}

public class WeaponSystem : IWeaponSystem
{
    private readonly ILogger _logger;

    public WeaponSystem(ILogger logger) // Constructor injection
    {
        _logger = logger;
    }

    public void Attack(ITarget target)
    {
        _logger.Log($"Attacking {target.Name}");
    }
}

// Setup
var container = new ServiceContainer();
container.AddSingleton<ILogger, ConsoleLogger>();
container.AddTransient<IWeaponSystem, WeaponSystem>();

// Usage
var weaponSystem = container.Resolve<IWeaponSystem>();
```

### Game Service Example

```csharp
public class GameManager
{
    [Inject] public IPlayerService PlayerService { get; set; }
    [Inject] public IInventorySystem InventorySystem { get; set; }
    [Inject] private ILogger _logger;

    public void StartGame()
    {
        _logger.Log("Game starting...");
        var player = PlayerService.CreatePlayer("Hero");
        InventorySystem.GiveStartingItems(player);
    }
}

// Bootstrap
public class GameBootstrap
{
    public static void Initialize()
    {
        var container = new ServiceContainer();

        // Register services
        container.AddSingleton<ILogger, UnityLogger>();
        container.AddSingleton<IPlayerService, PlayerService>();
        container.AddTransient<IInventorySystem, InventorySystem>();

        // Auto-wire all game objects
        var autoWirer = new AutoWirer(container);
        autoWirer.AutoWire();
    }
}
```

### Factory Registration

```csharp
// Complex object creation
container.RegisterFactory<IDatabase>(() =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
    return new SqlDatabase(connectionString);
}, ServiceLifetime.Singleton);

// Conditional registration
container.RegisterFactory<ILogger>(() =>
{
    return Application.isEditor
        ? new DebugLogger()
        : new FileLogger("game.log");
}, ServiceLifetime.Singleton);
```

### Behavior Tree Integration

```csharp
public class ActionAttack : ActionNode
{
    [Inject] private IWeaponSystem _weaponSystem;
    [Inject] private ITargetingSystem _targeting;

    public ActionAttack()
    {
        // Injection happens automatically when added to tree
    }

    public override NodeStatus Execute()
    {
        var target = _targeting.GetNearestEnemy();
        if (target == null) return NodeStatus.Failure;

        _weaponSystem.Attack(target);
        return NodeStatus.Success;
    }
}
```

## Best Practices

### 1. Registration Order

Register dependencies before dependents:

```csharp
// ✅ Good
container.AddSingleton<ILogger, ConsoleLogger>();
container.AddTransient<IService, Service>(); // Service depends on ILogger

// ❌ Bad - will throw when resolving IService
container.AddTransient<IService, Service>();
container.AddSingleton<ILogger, ConsoleLogger>();
```

### 2. Lifetime Selection

-   Use `Transient` for stateless, lightweight services
-   Use `Singleton` for expensive, stateful, or configuration services
-   Use `Scoped` for per-request/per-session state

### 3. Interface Segregation

```csharp
// ✅ Good - specific interfaces
container.AddSingleton<ILogger, Logger>();
container.AddSingleton<IConfigReader, ConfigService>();

// ❌ Avoid - overly broad interfaces
container.AddSingleton<IEverything, GodObject>();
```

### 4. Avoid Circular Dependencies

```csharp
// ❌ Bad - A depends on B, B depends on A
public class ServiceA
{
    public ServiceA(ServiceB b) { }
}

public class ServiceB
{
    public ServiceB(ServiceA a) { }
}
```

## Error Handling

Common exceptions and solutions:

| Exception                                             | Cause                    | Solution                                  |
| ----------------------------------------------------- | ------------------------ | ----------------------------------------- |
| `InvalidOperationException: Service X not registered` | Service not registered   | Register the service before resolving     |
| `StackOverflowException`                              | Circular dependency      | Review and break circular dependencies    |
| `ArgumentException: Cannot inject into member`        | Invalid injection target | Ensure `[Inject]` is on property or field |

## Performance Considerations

-   **Singleton Resolution**: O(1) after first resolution
-   **Transient Resolution**: O(n) where n = dependency depth
-   **Reflection Overhead**: Minimal after first injection per type
-   **Memory**: Singletons held for application lifetime

## Thread Safety

-   ✅ **ServiceContainer**: Thread-safe for registration and resolution
-   ✅ **Injector**: Thread-safe for injection operations
-   ⚠️ **Registered Services**: Thread safety depends on implementation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Changelog

### v1.0.0

-   Initial release
-   Basic DI container with lifetime management
-   Attribute-based injection
-   Auto-discovery functionality
-   Unity Package Manager support
