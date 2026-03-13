namespace P3k.PlayerInputController.Editor
{
   using System.Collections.Generic;
   using System.Linq;

   using UnityEditor;

   using UnityEngine;

   /// <summary>
   /// Custom property drawer for <see cref="InputActionBinding"/>.
   /// Shows the <c>Key</c> field as a dropdown populated from the
   /// JSON key definitions, and auto-sets the <c>IsAxis</c> toggle
   /// based on the selected key.
   /// </summary>
   [CustomPropertyDrawer(typeof(InputActionBinding))]
   public class InputActionBindingDrawer : PropertyDrawer
   {
      private const float Spacing = 2f;

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         // Key dropdown + ActionRef field + IsAxis (read-only indicator)
         return EditorGUIUtility.singleLineHeight * 3 + Spacing * 2;
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         EditorGUI.BeginProperty(position, label, property);

         var keyProp = property.FindPropertyRelative("Key");
         var isAxisProp = property.FindPropertyRelative("IsAxis");
         var actionRefProp = property.FindPropertyRelative("ActionRef");

         var lineHeight = EditorGUIUtility.singleLineHeight;
         var y = position.y;

         // --- Key dropdown ---
         var keyNames = InputKeyJsonHelper.LoadKeyNames();
         var axisFlags = InputKeyJsonHelper.LoadAxisFlags();

         // Build parallel arrays for the popup.
         var keys = keyNames.Keys.OrderBy(k => k).ToArray();
         var names = keys.Select(k => keyNames[k]).ToArray();

         var currentIndex = System.Array.IndexOf(keys, keyProp.intValue);
         if (currentIndex < 0) currentIndex = 0;

         var keyRect = new Rect(position.x, y, position.width, lineHeight);
         var newIndex = EditorGUI.Popup(keyRect, "Key", currentIndex, names);

         if (newIndex != currentIndex)
         {
            keyProp.intValue = keys[newIndex];

            // Auto-set IsAxis from the JSON definition.
            if (axisFlags.TryGetValue(keys[newIndex], out var isAxis))
               isAxisProp.boolValue = isAxis;
         }

         y += lineHeight + Spacing;

         // --- ActionRef ---
         var refRect = new Rect(position.x, y, position.width, lineHeight);
         EditorGUI.PropertyField(refRect, actionRefProp);

         y += lineHeight + Spacing;


         // --- IsAxis (disabled, auto-derived) ---
         var axisRect = new Rect(position.x, y, position.width, lineHeight);
         using (new EditorGUI.DisabledScope(true))
         {
            EditorGUI.PropertyField(axisRect, isAxisProp);
         }

         y += lineHeight + Spacing;



         EditorGUI.EndProperty();
      }
   }
}
