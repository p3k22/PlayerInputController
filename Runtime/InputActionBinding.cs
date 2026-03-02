namespace P3k.PlayerInputController
{
   using System;

   using UnityEngine.InputSystem;

   public enum InputBindingType
   {
      Button,
      Axis
   }

   /// <summary>
   /// Inspector-friendly mapping of a name + type to an <see cref="InputActionReference"/>.
   /// </summary>
   [Serializable]
   public class InputActionBinding
   {
      public string Name;

      public InputBindingType Type;

      public InputActionReference ActionRef;
   }
}
