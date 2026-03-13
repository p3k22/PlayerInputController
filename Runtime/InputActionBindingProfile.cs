namespace P3k.PlayerInputController
{
   using System.Collections.Generic;

   using UnityEngine;

   /// <summary>
   /// Reusable asset that defines a set of named input action bindings.
   /// Create via <b>Assets → Create → P3k → Input Action Binding Profile</b>.
   /// </summary>
   [CreateAssetMenu(
      fileName = "NewInputActionBindingProfile",
      menuName = "P3k/Input Action Binding Profile")]
   public class InputActionBindingProfile : ScriptableObject
   {
      [SerializeField]
      private List<InputActionBinding> _bindings = new List<InputActionBinding>();

      public IReadOnlyList<InputActionBinding> Bindings => _bindings;
   }
}
