# PlayerInputController

Lightweight wrapper around Unity's Input System that maps integer keys to polled `ButtonState` and `AxisState` objects. An editor window lets you define semantic key names and generate a type-safe `InputKey` enum — but the core package uses raw `int` keys, so it compiles standalone with **zero generated code required**.

---

## Quick Start

### 1. Define your input keys

The package ships with a default **InputKeyDefinitionAsset** (`DefaultInputKeyDefinitions.asset` in the package's `Runtime/` folder) pre-populated with common keys (Move, Look, Interact01, Interact02, Run, Jump, Crouch).

To customise, create your own via **Assets → Create → P3k → Input Key Definitions**, or edit the existing one directly.

Open **Window → P3k's Input Key Editor**, assign your definitions asset in the object field, add/remove entries and tick **Axis** for stick/WASD-style inputs. Click **Generate Enum** and choose where to save the file (typically somewhere in your `Assets/` folder).

This produces an `InputKey` enum + typed extension methods:

```csharp
InputKey.Interact   // = 1
InputKey.Jump       // = 2
InputKey.Move       // = 3 (axis)
```

### 2. Create a binding profile

**Assets → Create → P3k → Input Action Binding Profile**

Each entry maps a key (shown as a dropdown from your definitions) to an `InputActionReference`. The `IsAxis` toggle is set automatically based on the key definition. The inspector warns on duplicate keys and has an **Open Input Key Editor** button.

### 3. Use the MonoBehaviour

Add `PlayerInputController` to a GameObject, assign the profile in the Inspector. On enable it registers everything and enables all actions automatically.

```csharp
var controller = GetComponent<PlayerInputController>();
InputProcessor processor = controller.Processor;

// Buttons  (typed via generated extension methods)
ButtonState jump = processor.GetButton(InputKey.Jump);
if (jump.WasPressed) { /* single-frame press */ }
if (jump.IsHeld)     { /* held down */ }

// Axes
AxisState move = processor.GetAxis(InputKey.Move);
Vector2 dir = move.Value;
```

### 3b. Or use `InputProcessor` standalone (no MonoBehaviour)

```csharp
var processor = new InputProcessor();
processor.RegisterProfile(myProfile);
processor.EnableAll();

// ... later
processor.DisableAll();
processor.UnregisterAll();
```

---

## Architecture

```
InputKeyDefinitionAsset  (ScriptableObject — ships with the package)
  └─ Stores key names + IsAxis flags
     Created via Assets → Create → P3k → Input Key Definitions

InputKey  (auto-generated enum — lives in YOUR project, not the package)
  └─ Generated via Window → P3k's Input Key Editor
     Reads definitions from the InputKeyDefinitionAsset

InputActionBindingProfile  (ScriptableObject)
  └─ Maps int key  →  InputActionReference  +  IsAxis flag

InputProcessor  (plain C# class — uses int keys internally)
  ├── Dictionary<int, ButtonState>   press / held tracking + events
  └── Dictionary<int, AxisState>     continuous Vector2 value + events

PlayerInputController  (MonoBehaviour)
  └── Owns an InputProcessor, wires OnEnable / OnDisable

InputProcessorInputKeyExtensions  (auto-generated — typed overloads)
  └── processor.GetButton(InputKey.Jump) → processor.GetButton((int)InputKey.Jump)
```

---

## Dynamic Registration

Register actions at runtime without a profile:

```csharp
// Via the generated typed extensions
processor.RegisterButton(InputKey.Interact, interactActionRef);
processor.RegisterAxis(InputKey.Move, moveActionRef);
processor.Unregister(InputKey.Interact);

// Or with raw ints (no enum needed)
processor.RegisterButton(1, interactActionRef);
```

---

## Events

```csharp
// ButtonState
jump.OnPressed  += () => Debug.Log("pressed");
jump.OnReleased += () => Debug.Log("released");

// AxisState
move.OnChanged  += v => Debug.Log($"move: {v}");
```

---

## API Reference

### Core API (always available — `int` keys)

#### `InputProcessor`

| Member | Description |
|---|---|
| `Buttons` | `IReadOnlyDictionary<int, ButtonState>` |
| `Axes` | `IReadOnlyDictionary<int, AxisState>` |
| `RegisterButton(int, InputAction)` | Bind a `ButtonState` (caller must enable). |
| `RegisterButton(int, InputActionReference)` | Bind + auto-enable. Returns `null` if ref is `null`. |
| `RegisterAxis(int, InputAction)` | Bind an `AxisState` (caller must enable). |
| `RegisterAxis(int, InputActionReference)` | Bind + auto-enable. Returns `null` if ref is `null`. |
| `RegisterProfile(InputActionBindingProfile)` | Register all entries from a profile. |
| `GetButton(int)` | Lookup a `ButtonState` or `null`. |
| `GetAxis(int)` | Lookup an `AxisState` or `null`. |
| `UnregisterButton(int)` | Disable, unbind, remove a button. |
| `UnregisterAxis(int)` | Disable, unbind, remove an axis. |
| `Unregister(int)` | Remove by key (button or axis). |
| `UnregisterAll()` | Clear everything. |
| `EnableAll()` / `DisableAll()` | Batch toggle all registered actions. |

#### `PlayerInputController` — MonoBehaviour

| Member | Description |
|---|---|
| `Processor` | The underlying `InputProcessor` instance. |
| `RegisterButton(int, InputActionReference)` | Register + enable a button at runtime. |
| `RegisterAxis(int, InputActionReference)` | Register + enable an axis at runtime. |
| `Unregister(int)` | Unregister a button or axis. |

#### `InputActionBinding`

| Field | Description |
|---|---|
| `Key` | `int` — corresponds to a generated `InputKey` enum value. |
| `IsAxis` | `bool` — auto-set from key definition; determines button vs axis registration. |
| `ActionRef` | `InputActionReference` from your Input Actions asset. |

#### `InputActionBindingProfile` — ScriptableObject

| Member | Description |
|---|---|
| `Bindings` | `IReadOnlyList<InputActionBinding>` |

### Generated API (after running Generate Enum)

#### `InputKey` — enum

Always includes `None = 0`. Values match the order in the editor window.

#### `InputKeyExtensions`

| Method | Description |
|---|---|
| `bool IsAxis(this InputKey)` | Returns `true` for keys marked as axes. |

#### `InputProcessorInputKeyExtensions`

Typed overloads so you can write `processor.GetButton(InputKey.Jump)` instead of `processor.GetButton((int)InputKey.Jump)`. Covers `GetButton`, `GetAxis`, `RegisterButton`, `RegisterAxis`, `Unregister`, `UnregisterButton`, `UnregisterAxis`.

### `ButtonState`

| Member | Description |
|---|---|
| `WasPressed` | `true` only on the frame the button was first pressed. |
| `IsHeld` | `true` while held down. |
| `OnPressed` / `OnReleased` | `Action` callbacks. |
| `Bind(InputAction)` / `Unbind()` | Subscribe / unsubscribe. |
| `Enable()` / `Disable()` | Toggle the underlying `InputAction`. |

### `AxisState`

| Member | Description |
|---|---|
| `Value` | Current `Vector2`. |
| `OnChanged` | `Action<Vector2>` callback. |
| `Bind(InputAction)` / `Unbind()` | Subscribe / unsubscribe. |
| `Enable()` / `Disable()` | Toggle the underlying `InputAction`. |

---

## Git Package Compatibility

The package compiles out of the box with **no generated code**. The core uses `int` keys so there is no dependency on a generated `InputKey` enum.

- Key definitions are stored in an `InputKeyDefinitionAsset` ScriptableObject that ships inside the package folder, so they travel with the package when imported into any project.
- A default asset with common keys is included at `Runtime/DefaultInputKeyDefinitions.asset`.
- The editor window auto-discovers the first `InputKeyDefinitionAsset` in the project, or you can assign one manually.
- The generated `InputKey` enum is saved to a location you choose (your `Assets/` folder), outside the package.
- The profile inspector reads key names from the JSON to show a dropdown — no enum needed in the editor.

---

## Requirements

| Dependency | Minimum Version |
|---|---|
| Unity | 2021.3+ |
| Input System | `com.unity.inputsystem` |

---

## License

See repository for license details.
