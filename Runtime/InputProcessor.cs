namespace P3k.PlayerInputController
{
   using System.Collections.Generic;

   using UnityEngine.InputSystem;

   /// <summary>
   /// Manages a dynamic set of <see cref="ButtonState"/> and <see cref="AxisState"/>
   /// instances keyed by <c>int</c>.
   /// The integer keys correspond to values from a generated <c>InputKey</c> enum.
   /// Handles registration, lookup, and batch enable / disable.
   /// </summary>
   public class InputProcessor
   {
      private readonly Dictionary<int, ButtonState> _buttons =
         new Dictionary<int, ButtonState>();

      private readonly Dictionary<int, AxisState> _axes =
         new Dictionary<int, AxisState>();

      public IReadOnlyDictionary<int, ButtonState> Buttons => _buttons;

      public IReadOnlyDictionary<int, AxisState> Axes => _axes;

      // =====================================================================
      // Registration
      // =====================================================================

      public ButtonState RegisterButton(int key, InputAction action)
      {
         UnregisterButton(key);

         var state = new ButtonState();
         state.Bind(action);
         _buttons[key] = state;
         return state;
      }

      public AxisState RegisterAxis(int key, InputAction action)
      {
         UnregisterAxis(key);

         var state = new AxisState();
         state.Bind(action);
         _axes[key] = state;
         return state;
      }

      /// <summary>
      /// Registers and immediately enables a button from an <see cref="InputActionReference"/>.
      /// Returns <c>null</c> when <paramref name="actionRef"/> is <c>null</c>.
      /// </summary>
      public ButtonState RegisterButton(int key, InputActionReference actionRef)
      {
         if (actionRef == null) return null;
         var state = RegisterButton(key, actionRef.action);
         state.Enable();
         return state;
      }

      /// <summary>
      /// Registers and immediately enables an axis from an <see cref="InputActionReference"/>.
      /// Returns <c>null</c> when <paramref name="actionRef"/> is <c>null</c>.
      /// </summary>
      public AxisState RegisterAxis(int key, InputActionReference actionRef)
      {
         if (actionRef == null) return null;
         var state = RegisterAxis(key, actionRef.action);
         state.Enable();
         return state;
      }

      /// <summary>
      /// Registers all bindings defined in the given profile asset.
      /// Skips entries with a <c>null</c> action reference or a key of <c>0</c> (None).
      /// </summary>
      public void RegisterProfile(InputActionBindingProfile profile)
      {
         if (profile == null) return;

         foreach (var binding in profile.Bindings)
         {
            if (binding.ActionRef == null || binding.Key == 0) continue;

            if (binding.IsAxis)
               RegisterAxis(binding.Key, binding.ActionRef.action);
            else
               RegisterButton(binding.Key, binding.ActionRef.action);
         }
      }

      // =====================================================================
      // Unregistration
      // =====================================================================

      public void UnregisterButton(int key)
      {
         if (_buttons.TryGetValue(key, out var state))
         {
            state.Disable();
            state.Unbind();
            _buttons.Remove(key);
         }
      }

      public void UnregisterAxis(int key)
      {
         if (_axes.TryGetValue(key, out var state))
         {
            state.Disable();
            state.Unbind();
            _axes.Remove(key);
         }
      }

      public void Unregister(int key)
      {
         UnregisterButton(key);
         UnregisterAxis(key);
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

      // =====================================================================
      // Lookup
      // =====================================================================

      public ButtonState GetButton(int key)
      {
         _buttons.TryGetValue(key, out var state);
         return state;
      }

      public AxisState GetAxis(int key)
      {
         _axes.TryGetValue(key, out var state);
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
