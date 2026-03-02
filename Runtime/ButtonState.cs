namespace P3k.PlayerInputController
{
   using System;

   using UnityEngine;
   using UnityEngine.InputSystem;

   /// <summary>
   /// Tracks press/held state for a single button <see cref="InputAction"/>.
   /// </summary>
   [Serializable]
   public class ButtonState
   {
      // Optional callbacks for forwarding to external events
      public Action OnPressed;

      public Action OnReleased;

      private InputAction _action;

      private int _pressedFrame = -1;

      public bool IsHeld { get; private set; }

      /// <summary>
      /// <c>true</c> only during the frame the button was first pressed.
      /// Resets automatically on the next frame — no manual clearing required.
      /// </summary>
      public bool WasPressed => _pressedFrame == Time.frameCount;

      public void Bind(InputAction action)
      {
         if (action == null) return;
         _action = action;
         _action.started += HandleStarted;
         _action.canceled += HandleCanceled;
      }

      public void Enable() => _action?.Enable();

      public void Disable() => _action?.Disable();

      public void Unbind()
      {
         if (_action == null) return;
         _action.started -= HandleStarted;
         _action.canceled -= HandleCanceled;
         _action = null;
      }

      private void HandleCanceled(InputAction.CallbackContext ctx)
      {
         IsHeld = false;
         OnReleased?.Invoke();
      }

      private void HandleStarted(InputAction.CallbackContext ctx)
      {
         _pressedFrame = Time.frameCount;
         IsHeld = true;
         OnPressed?.Invoke();
      }
   }
}
