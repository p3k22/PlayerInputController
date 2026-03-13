namespace P3k.PlayerInputController.Editor
{
   using System.Collections.Generic;
   using System.Linq;

   using UnityEditor;

   using UnityEngine;

   /// <summary>
   ///    Custom inspector for <see cref="InputActionBindingProfile" /> that draws
   ///    a warning box when two or more bindings share the same non-zero key.
   /// </summary>
   [CustomEditor(typeof(InputActionBindingProfile))]
   public class InputActionBindingProfileEditor : Editor
   {
      private SerializedProperty _bindingsProp;

      private void OnEnable()
      {
         _bindingsProp = serializedObject.FindProperty("_bindings");
      }

      public override void OnInspectorGUI()
      {
         serializedObject.Update();

         // Draw the default list so users keep the normal editing experience.
         DrawDefaultInspector();

         // -----------------------------------------------------------------
         // Duplicate key detection
         // -----------------------------------------------------------------
         var keyNames = InputKeyJsonHelper.LoadKeyNames();
         var seen = new Dictionary<int, List<string>>();

         for (var i = 0; i < _bindingsProp.arraySize; i++)
         {
            var element = _bindingsProp.GetArrayElementAtIndex(i);
            var keyProp = element.FindPropertyRelative("Key");

            var key = keyProp.intValue;
            if (key == 0)
            {
               continue;
            }

            var label = keyNames.TryGetValue(key, out var n) ? n : key.ToString();

            if (!seen.TryGetValue(key, out var list))
            {
               list = new List<string>();
               seen[key] = list;
            }

            list.Add(label);
         }

         foreach (var kvp in seen)
         {
            if (kvp.Value.Count > 1)
            {
               var label = keyNames.TryGetValue(kvp.Key, out var n) ? n : kvp.Key.ToString();
               EditorGUILayout.HelpBox(
               $"Duplicate key \"{label}\" assigned to {kvp.Value.Count} bindings. "
               + "Each key should be unique — only the last binding will be kept at runtime.",
               MessageType.Warning);
            }
         }

         // -----------------------------------------------------------------
         // Quick-access button
         // -----------------------------------------------------------------
         EditorGUILayout.Space(4);

         if (GUILayout.Button("Open Input Key Editor", GUILayout.Height(24)))
         {
            EditorApplication.ExecuteMenuItem("Window/P3k's Input Key Editor");
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
