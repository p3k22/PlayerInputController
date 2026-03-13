namespace P3k.PlayerInputController
{
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;

   /// <summary>
   /// Manages input action bindings and exposes events + polled state.
   /// Assign a <see cref="InputActionBindingProfile"/> asset in the Inspector,
   /// or register actions dynamically at runtime through <see cref="InputProcessor"/>.
   /// </summary>
   public class PlayerInputController : MonoBehaviour
   {
      [SerializeField]
      private InputActionBindingProfile _profile;

      public InputProcessor Processor { get; } = new InputProcessor();

      private void OnEnable() => EnableActions();

      private void OnDisable() => DisableActions();

      // =====================================================================
      // Dynamic API
      // =====================================================================

      /// <summary>
      /// Registers and immediately enables a button input at runtime.
      /// </summary>
      public ButtonState RegisterButton(int key, InputActionReference actionRef) =>
         Processor.RegisterButton(key, actionRef);

      /// <summary>
      /// Registers and immediately enables an axis input at runtime.
      /// </summary>
      public AxisState RegisterAxis(int key, InputActionReference actionRef) =>
         Processor.RegisterAxis(key, actionRef);

      /// <summary>
      /// Unregisters a previously registered button or axis.
      /// </summary>
      public void Unregister(int key) => Processor.Unregister(key);

      // =====================================================================
      // Action map binding
      // =====================================================================

      enum MyEnum { poo = 1 }

      private void EnableActions()
      {
         DisableActions();
         Processor.RegisterProfile(_profile);
         Processor.EnableAll();
      }

      private void DisableActions()
      {
         Processor.DisableAll();
         Processor.UnregisterAll();
      }
   }
}