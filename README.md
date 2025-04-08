# EasyCoroutine - Godot Coroutine Management Plugin

[![Godot 4.x](https://img.shields.io/badge/Godot-4.0%2B-%23478cbf)](https://godotengine.org)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)

## 📦 Installation Guide
1. Copy the `EasyCoroutine` folder to your project's `addons/` directory
2. In Godot Editor, navigate to `Project Settings -> AutoLoad`
3. Add `CoroutineManager.cs` as an autoload script (recommended name: `CoroutineManager`)
4. Ensure the node exists in scene tree for automatic coroutine system initialization

## 🛠️ Basic Usage
### Starting Coroutines
```csharp
// Start coroutine in any node
Coroutine.StartCoroutine(TestCoroutine());

IEnumerator<ulong> TestCoroutine()
{
    GD.Print("Coroutine launched");
    yield return Coroutine.WaitForSeconds(1.5f);
    GD.Print("Executed after 1.5 seconds");
}
```

### Wait Types Reference
| Wait Method                 | Description                |
|-----------------------------|----------------------------|
| `WaitForSeconds`            | Wait specified seconds     |
| `WaitForTimeSpan`           | Wait custom time interval  |
| `WaitForPhysicsFrame`       | Wait next physics frame    |
| `WaitForTween`              | Wait Tween animation completion |
| `WaitForUnlock`             | Wait custom event lock (multi-agent support) |
| `WaitForOtherCoroutine`     | Wait other coroutine completion |

## 🔗 Examples
### Event Lock Mechanism
```csharp
var attackLock = Coroutine.CreateLock();

IEnumerator<ulong> AttackSequence()
{
    GD.Print("Charging started");
    yield return Coroutine.WaitForSeconds(2f);
    attackLock.Unlock();
}

IEnumerator<ulong> WaitAttack()
{
    GD.Print("Waiting for attack completion");
    yield return Coroutine.WaitForUnlock(attackLock);
    GD.Print("Attack released");
}
```

### Tween Coordination
```csharp
IEnumerator<ulong> MoveCharacter()
{
    var tween = CreateTween();
    tween.TweenProperty(GetNode<Sprite2D>("Sprite"), "position", new Vector2(300, 200), 1.5f);
    
    yield return Coroutine.WaitForTween(tween);
    GD.Print("Movement completed");
}
```

### Coroutine Dependency
```csharp
IEnumerator<ulong> CoroutineA()
{
    GD.Print("Coroutine A started");
    yield return Coroutine.WaitForSeconds(2f);
    GD.Print("Coroutine A completed");
}

IEnumerator<ulong> CoroutineB()
{
    GD.Print("Coroutine B started");
    yield return Coroutine.WaitForOtherCoroutine(Coroutine.StartCoroutine(CoroutineA()));
    GD.Print("Coroutine B completed");
}
```