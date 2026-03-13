namespace P3k.PlayerInputController
{
   using System;

   using UnityEngine;
   using UnityEngine.InputSystem;

   /// <summary>
   /// Inspector-friendly mapping of an integer key to an <see cref="InputActionReference"/>.
   /// The integer value corresponds to a generated <c>InputKey</c> enum member.
   /// </summary>
   [Serializable]
   public class InputActionBinding
   {
      [Tooltip("Integer key that maps to a generated InputKey enum value.")]
      public int Key;

      [Tooltip("The input action to trigger for this key.")]
      public InputActionReference ActionRef;

      [Tooltip("True for two-axis inputs (sticks / WASD). False for buttons.")]
      public bool IsAxis;


   }
}
