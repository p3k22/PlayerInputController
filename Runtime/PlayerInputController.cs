namespace P3k.PlayerInputController
{
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
      public ButtonState RegisterButton(string name, InputActionReference actionRef) =>
         Processor.RegisterButton(name, actionRef);

      /// <summary>
      /// Registers and immediately enables an axis input at runtime.
      /// </summary>
      public AxisState RegisterAxis(string name, InputActionReference actionRef) =>
         Processor.RegisterAxis(name, actionRef);

      /// <summary>
      /// Unregisters a previously registered button or axis by name.
      /// </summary>
      public void Unregister(string name) => Processor.Unregister(name);

      // =====================================================================
      // Action map binding
      // =====================================================================

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