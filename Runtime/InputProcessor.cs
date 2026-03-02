namespace P3k.PlayerInputController
{
   using System.Collections.Generic;

   using UnityEngine.InputSystem;

   /// <summary>
   /// Manages a dynamic set of named <see cref="ButtonState"/> and <see cref="AxisState"/>
   /// instances. Handles registration, lookup, and batch enable / disable.
   /// </summary>
   public class InputProcessor
   {
      private readonly Dictionary<string, ButtonState> _buttons = new Dictionary<string, ButtonState>();

      private readonly Dictionary<string, AxisState> _axes = new Dictionary<string, AxisState>();

      public IReadOnlyDictionary<string, ButtonState> Buttons => _buttons;

      public IReadOnlyDictionary<string, AxisState> Axes => _axes;

      // =====================================================================
      // Registration
      // =====================================================================

      public ButtonState RegisterButton(string name, InputAction action)
      {
         UnregisterButton(name);

         var state = new ButtonState();
         state.Bind(action);
         _buttons[name] = state;
         return state;
      }

      public AxisState RegisterAxis(string name, InputAction action)
      {
         UnregisterAxis(name);

         var state = new AxisState();
         state.Bind(action);
         _axes[name] = state;
         return state;
      }

      /// <summary>
      /// Registers and immediately enables a button from an <see cref="InputActionReference"/>.
      /// Returns <c>null</c> when <paramref name="actionRef"/> is <c>null</c>.
      /// </summary>
      public ButtonState RegisterButton(string name, InputActionReference actionRef)
      {
         if (actionRef == null) return null;
         var state = RegisterButton(name, actionRef.action);
         state.Enable();
         return state;
      }

      /// <summary>
      /// Registers and immediately enables an axis from an <see cref="InputActionReference"/>.
      /// Returns <c>null</c> when <paramref name="actionRef"/> is <c>null</c>.
      /// </summary>
      public AxisState RegisterAxis(string name, InputActionReference actionRef)
      {
         if (actionRef == null) return null;
         var state = RegisterAxis(name, actionRef.action);
         state.Enable();
         return state;
      }

      /// <summary>
      /// Registers all bindings defined in the given profile asset.
      /// Skips entries with a <c>null</c> action reference or empty name.
      /// </summary>
      public void RegisterProfile(InputActionBindingProfile profile)
      {
         if (profile == null) return;

         foreach (var binding in profile.Bindings)
         {
            if (binding.ActionRef == null || string.IsNullOrEmpty(binding.Name)) continue;

            if (binding.Type == InputBindingType.Button)
               RegisterButton(binding.Name, binding.ActionRef.action);
            else
               RegisterAxis(binding.Name, binding.ActionRef.action);
         }
      }

      // =====================================================================
      // Unregistration
      // =====================================================================

      public void UnregisterButton(string name)
      {
         if (_buttons.TryGetValue(name, out var state))
         {
            state.Disable();
            state.Unbind();
            _buttons.Remove(name);
         }
      }

      public void UnregisterAxis(string name)
      {
         if (_axes.TryGetValue(name, out var state))
         {
            state.Disable();
            state.Unbind();
            _axes.Remove(name);
         }
      }

      public void UnregisterAll()
      {
         foreach (var kvp in _buttons)
         {
            kvp.Value.Disable();
            kvp.Value.Unbind();
         }

         _buttons.Clear();

         foreach (var kvp in _axes)
         {
            kvp.Value.Disable();
            kvp.Value.Unbind();
         }

         _axes.Clear();
      }

      /// <summary>
      /// Unregisters a previously registered button or axis by name.
      /// </summary>
      public void Unregister(string name)
      {
         UnregisterButton(name);
         UnregisterAxis(name);
      }

      // =====================================================================
      // Lookup
      // =====================================================================

      public ButtonState GetButton(string name)
      {
         _buttons.TryGetValue(name, out var state);
         return state;
      }

      public AxisState GetAxis(string name)
      {
         _axes.TryGetValue(name, out var state);
         return state;
      }

      // =====================================================================
      // Batch enable / disable
      // =====================================================================

      public void EnableAll()
      {
         foreach (var kvp in _buttons)
            kvp.Value.Enable();

         foreach (var kvp in _axes)
            kvp.Value.Enable();
      }

      public void DisableAll()
      {
         foreach (var kvp in _buttons)
            kvp.Value.Disable();

         foreach (var kvp in _axes)
            kvp.Value.Disable();
      }
   }
}
