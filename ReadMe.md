# PlayerInputController

Lightweight wrapper around Unity's Input System that maps named actions to polled `ButtonState` and `AxisState` objects. Define bindings in a ScriptableObject profile or register them dynamically at runtime.

---

## Architecture

```
InputProcessor  (plain C# class — all logic lives here)
  ├── Dictionary<string, ButtonState>   — press / held tracking + events
  └── Dictionary<string, AxisState>     — continuous Vector2 value + events

PlayerInputController  (MonoBehaviour — thin Unity shell)
  └── Owns an InputProcessor, wires LateUpdate / OnEnable / OnDisable
```

There are two ways to use the system:

| Approach | When to use |
|---|---|
| **`PlayerInputController`** (MonoBehaviour) | Drop-in component: Inspector-configured profile, automatic lifecycle. |
| **`InputProcessor`** (standalone) | No MonoBehaviour needed: create the processor yourself. No per-frame calls required. |

---

## Requirements

| Dependency | Minimum Version |
|---|---|
| **Unity** | 2020.1+ |
| **Input System** | Any version (`com.unity.inputsystem`) |

---

## Usage — MonoBehaviour

### 1. Create an Input Action Binding Profile

**Assets → Create → P3k → Input Action Binding Profile**

Each entry in the profile maps a **name** and **type** (`Button` or `Axis`) to an `InputActionReference` from your Input Actions asset.

### 2. Add `PlayerInputController` to a GameObject

Drag the component onto any GameObject and assign the profile asset in the Inspector:

| Field | What it does |
|---|---|
| **Profile** | The `InputActionBindingProfile` asset that defines which actions to register. |

On enable the controller loads the profile via `Processor.RegisterProfile(...)` and enables all actions.

### 3. Access the service from other scripts

```csharp
var controller = GetComponent<PlayerInputController>();

// Access the processor directly
InputProcessor processor = controller.Processor;

// Or use the convenience methods on the component
ButtonState jump = controller.RegisterButton("Jump", jumpActionRef);
AxisState   move = controller.RegisterAxis("Move", moveActionRef);
controller.Unregister("Jump");
```

---

## Usage — Standalone (no MonoBehaviour)

Create an `InputProcessor` and drive it from any update loop — no GameObject required.

```csharp
// Create
var processor = new InputProcessor();

// --- Register from a profile asset ---
processor.RegisterProfile(myProfile);
processor.EnableAll();

// --- Or register individual actions ---
processor.RegisterButton("Jump", jumpActionRef);   // InputActionReference overload (auto-enables)
processor.RegisterAxis("Move", moveAction);         // raw InputAction overload

// --- Tear down ---
processor.DisableAll();
processor.UnregisterAll();
```

---

## Polling State

```csharp
var processor = /* from controller.Processor or new InputProcessor() */;

// Buttons
ButtonState jump = processor.GetButton("Jump");
if (jump.WasPressed) { /* single-frame press */ }
if (jump.IsHeld)     { /* held down */ }

// Axes
AxisState move = processor.GetAxis("Move");
Vector2 dir = move.Value;
```

---

## Events

Both state types expose optional callbacks:

```csharp
// ButtonState
jump.OnPressed  += () => Debug.Log("Jump pressed");
jump.OnReleased += () => Debug.Log("Jump released");

// AxisState
move.OnChanged  += v  => Debug.Log($"Move: {v}");
```

---

## API Reference

### `PlayerInputController` — MonoBehaviour

| Member | Description |
|---|---|
| `Processor` | The `InputProcessor` instance driven by this component. |
| `RegisterButton(string, InputActionReference)` | Convenience pass-through to `Processor.RegisterButton`. |
| `RegisterAxis(string, InputActionReference)` | Convenience pass-through to `Processor.RegisterAxis`. |
| `Unregister(string)` | Convenience pass-through to `Processor.Unregister`. |

### `InputProcessor`

| Member | Description |
|---|---|
| `Buttons` | `IReadOnlyDictionary<string, ButtonState>` of all registered buttons. |
| `Axes` | `IReadOnlyDictionary<string, AxisState>` of all registered axes. |
| `RegisterButton(string, InputAction)` | Creates and binds a `ButtonState`. |
| `RegisterAxis(string, InputAction)` | Creates and binds an `AxisState`. |
| `RegisterButton(string, InputActionReference)` | Creates, binds, and enables a `ButtonState`. Returns `null` if ref is `null`. |
| `RegisterAxis(string, InputActionReference)` | Creates, binds, and enables an `AxisState`. Returns `null` if ref is `null`. |
| `RegisterProfile(InputActionBindingProfile)` | Registers all bindings from a profile asset. |
| `UnregisterButton(string)` | Disables, unbinds, and removes a button. |
| `UnregisterAxis(string)` | Disables, unbinds, and removes an axis. |
| `Unregister(string)` | Unregisters a button or axis by name. |
| `UnregisterAll()` | Clears all buttons and axes. |
| `GetButton(string)` | Returns the `ButtonState` for the given name, or `null`. |
| `GetAxis(string)` | Returns the `AxisState` for the given name, or `null`. |
| `EnableAll()` / `DisableAll()` | Batch enable or disable every registered action. |

### `ButtonState`

| Member | Description |
|---|---|
| `WasPressed` | `true` only during the frame the button was first pressed. Resets automatically. |
| `IsHeld` | `true` while the button is held down. |
| `OnPressed` | `Action` callback fired on press. |
| `OnReleased` | `Action` callback fired on release. |
| `Bind(InputAction)` / `Unbind()` | Subscribe or unsubscribe from the `InputAction`. |
| `Enable()` / `Disable()` | Enable or disable the underlying `InputAction`. |

### `AxisState`

| Member | Description |
|---|---|
| `Value` | Current `Vector2` value of the axis. |
| `OnChanged` | `Action<Vector2>` callback fired when the value changes. |
| `Bind(InputAction)` / `Unbind()` | Subscribe or unsubscribe from the `InputAction`. |
| `Enable()` / `Disable()` | Enable or disable the underlying `InputAction`. |

### `InputActionBinding`

| Member | Description |
|---|---|
| `Name` | Friendly name used as the dictionary key. |
| `Type` | `InputBindingType.Button` or `InputBindingType.Axis`. |
| `ActionRef` | `InputActionReference` pointing to the Input Actions asset. |

### `InputActionBindingProfile` — ScriptableObject

| Member | Description |
|---|---|
| `Bindings` | `IReadOnlyList<InputActionBinding>` defined in the asset. |

---

## License

See repository for license details.
