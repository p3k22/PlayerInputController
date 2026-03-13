namespace P3k.PlayerInputController
{
   using System;

   using UnityEngine;
   using UnityEngine.InputSystem;

   /// <summary>
   /// Tracks a continuous <see cref="Vector2"/> value for an axis <see cref="InputAction"/>.
   /// </summary>
   [Serializable]
   public class AxisState
   {
      public Action<Vector2> OnChanged;

      private InputAction _action;

      public Vector2 Value { get; private set; }

      public void Bind(InputAction action)
      {
         if (action == null) return;
         _action = action;
         _action.performed += HandlePerformed;
         _action.canceled += HandleCanceled;
      }

      public void Enable() => _action?.Enable();

      public void Disable() => _action?.Disable();

      public void Unbind()
      {
         if (_action == null) return;
         _action.performed -= HandlePerformed;
         _action.canceled -= HandleCanceled;
         _action = null;
         Value = Vector2.zero;
      }

      private void HandleCanceled(InputAction.CallbackContext ctx)
      {
         Value = Vector2.zero;
         OnChanged?.Invoke(Value);
      }

      private void HandlePerformed(InputAction.CallbackContext ctx)
      {
         Value = ctx.ReadValue<Vector2>();
         OnChanged?.Invoke(Value);
      }
   }
}
