namespace P3k.PlayerInputController.Editor
{
   using System.Collections.Generic;
   using System.IO;
   using System.Linq;
   using System.Text;
   using System.Text.RegularExpressions;

   using UnityEditor;

   using UnityEngine;

   /// <summary>
   ///    Editor window for defining semantic input keys and generating the
   ///    <c>InputKey</c> enum. Open via <b>Window → P3k's Input Key Editor</b>.
   ///    Definitions are persisted to an <see cref="InputKeyDefinitionAsset"/>
   ///    ScriptableObject that travels with the package.
   /// </summary>
   public class InputKeyEditorWindow : EditorWindow
   {
      // ------------------------------------------------------------------
      // Paths
      // ------------------------------------------------------------------

      private const string DefaultEnumDir = "Assets";

      private const string DefaultEnumFile = "InputKey.generated.cs";

      // ------------------------------------------------------------------
      // State
      // ------------------------------------------------------------------

      private static readonly Regex ValidIdentifier = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

      private bool _dirty;

      private InputKeyDefinitionAsset _asset;

      private List<InputKeyDefinition> _defs = new();

      /// <summary>Last chosen save path, remembered for the session.</summary>
      private string _lastEnumPath;

      private Vector2 _scroll;

      // ------------------------------------------------------------------
      // Lifecycle
      // ------------------------------------------------------------------

      private void OnEnable()
      {
         Load();
      }

      private void OnDisable()
      {
         if (_dirty)
         {
            Save();
         }
      }

      // ------------------------------------------------------------------
      // GUI
      // ------------------------------------------------------------------

      private void OnGUI()
      {
         EditorGUILayout.Space(4);
         EditorGUILayout.LabelField("Input Key Definitions", EditorStyles.boldLabel);
         EditorGUILayout.Space(2);

         EditorGUI.BeginChangeCheck();
         _asset = (InputKeyDefinitionAsset)EditorGUILayout.ObjectField(
            "Definitions Asset", _asset, typeof(InputKeyDefinitionAsset), false);
         if (EditorGUI.EndChangeCheck())
         {
            Load();
         }

         if (_asset == null)
         {
            EditorGUILayout.HelpBox(
               "No InputKeyDefinitionAsset found. Create one via Assets \u2192 Create \u2192 P3k \u2192 Input Key Definitions.",
               MessageType.Warning);
         }

         EditorGUILayout.Space(2);

         _scroll = EditorGUILayout.BeginScrollView(_scroll);

         var removeIndex = -1;

         for (var i = 0; i < _defs.Count; i++)
         {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            _defs[i].Name = EditorGUILayout.TextField(_defs[i].Name);
            _defs[i].IsAxis = EditorGUILayout.ToggleLeft("Axis", _defs[i].IsAxis, GUILayout.Width(48));

            if (EditorGUI.EndChangeCheck())
            {
               _dirty = true;
            }

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
               removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
         }

         if (removeIndex >= 0)
         {
            _defs.RemoveAt(removeIndex);
            _dirty = true;
         }

         EditorGUILayout.EndScrollView();

         // ----- Buttons -----
         EditorGUILayout.Space(4);

         if (GUILayout.Button("+ Add Key"))
         {
            _defs.Add(new InputKeyDefinition());
            _dirty = true;
         }

         // ----- Validation -----
         var hasErrors = false;
         var seen = new HashSet<string>();

         foreach (var def in _defs)
         {
            var n = def.Name?.Trim();
            if (string.IsNullOrEmpty(n))
            {
               continue;
            }

            if (n == "None")
            {
               EditorGUILayout.HelpBox("\"None\" is reserved and added automatically.", MessageType.Warning);
               continue;
            }

            if (!ValidIdentifier.IsMatch(n))
            {
               EditorGUILayout.HelpBox($"\"{n}\" is not a valid C# identifier.", MessageType.Error);
               hasErrors = true;
               continue;
            }

            if (!seen.Add(n))
            {
               EditorGUILayout.HelpBox($"Duplicate name \"{n}\".", MessageType.Error);
               hasErrors = true;
            }
         }

         EditorGUILayout.Space(4);

         using (new EditorGUI.DisabledScope(hasErrors))
         {
            if (GUILayout.Button("Generate Enum", GUILayout.Height(28)))
            {
               Save();
               GenerateEnum();
            }
         }

         EditorGUILayout.Space(4);
      }

      // ------------------------------------------------------------------
      // Menu item
      // ------------------------------------------------------------------

      [MenuItem("Window/P3k's Input Key Editor")]
      private static void Open()
      {
         var w = GetWindow<InputKeyEditorWindow>("Input Key Editor");
         w.minSize = new Vector2(340, 260);
         w.Show();
      }

      // ------------------------------------------------------------------
      // Code generation
      // ------------------------------------------------------------------

      private void GenerateEnum()
      {
         var entries = new List<(string name, bool isAxis)>();
         var seen = new HashSet<string>();

         foreach (var def in _defs)
         {
            var name = def.Name?.Trim();
            if (string.IsNullOrEmpty(name) || name == "None")
            {
               continue;
            }

            if (!ValidIdentifier.IsMatch(name))
            {
               continue;
            }

            if (!seen.Add(name))
            {
               continue;
            }

            entries.Add((name, def.IsAxis));
         }

         var sb = new StringBuilder();
         sb.AppendLine("// <auto-generated>");
         sb.AppendLine("//   Generated by InputKeyEditorWindow. Do not edit manually.");
         sb.AppendLine("//   Re-generate from P3k → Input Key Editor.");
         sb.AppendLine("// </auto-generated>");
         sb.AppendLine();
         sb.AppendLine("namespace P3k.PlayerInputController");
         sb.AppendLine("{");
         sb.AppendLine("   public enum InputKey");
         sb.AppendLine("   {");
         sb.AppendLine("      None = 0,");

         for (var i = 0; i < entries.Count; i++)
         {
            sb.AppendLine($"      {entries[i].name} = {i + 1},");
         }

         sb.AppendLine("   }");
         sb.AppendLine();

         var axisNames = entries.Where(e => e.isAxis).Select(e => e.name).ToList();

         sb.AppendLine("   public static class InputKeyExtensions");
         sb.AppendLine("   {");

         if (axisNames.Count == 0)
         {
            sb.AppendLine("      public static bool IsAxis(this InputKey key) => false;");
         }
         else
         {
            sb.AppendLine("      public static bool IsAxis(this InputKey key)");
            sb.AppendLine("      {");
            sb.AppendLine("         switch (key)");
            sb.AppendLine("         {");
            foreach (var axis in axisNames)
            {
               sb.AppendLine($"            case InputKey.{axis}:");
            }

            sb.AppendLine("               return true;");
            sb.AppendLine("            default:");
            sb.AppendLine("               return false;");
            sb.AppendLine("         }");
            sb.AppendLine("      }");
         }

         sb.AppendLine("   }");
         sb.AppendLine("}");

         // =================================================================
         // Typed extension methods so user code stays clean:
         //   processor.GetButton(InputKey.Jump)
         // instead of:
         //   processor.GetButton((int)InputKey.Jump)
         // =================================================================
         sb.AppendLine();
         sb.AppendLine("namespace P3k.PlayerInputController");
         sb.AppendLine("{");
         sb.AppendLine("   using UnityEngine.InputSystem;");
         sb.AppendLine();
         sb.AppendLine("   /// <summary>");
         sb.AppendLine("   /// Typed <see cref=\"InputKey\"/> overloads for <see cref=\"InputProcessor\"/>.");
         sb.AppendLine("   /// </summary>");
         sb.AppendLine("   public static class InputProcessorInputKeyExtensions");
         sb.AppendLine("   {");
         sb.AppendLine(
         "      public static ButtonState GetButton(this InputProcessor p, InputKey key) => p?.GetButton((int)key);");
         sb.AppendLine(
         "      public static AxisState GetAxis(this InputProcessor p, InputKey key) => p?.GetAxis((int)key);");
         sb.AppendLine(
         "      public static ButtonState RegisterButton(this InputProcessor p, InputKey key, InputAction action) => p?.RegisterButton((int)key, action);");
         sb.AppendLine(
         "      public static AxisState RegisterAxis(this InputProcessor p, InputKey key, InputAction action) => p?.RegisterAxis((int)key, action);");
         sb.AppendLine(
         "      public static ButtonState RegisterButton(this InputProcessor p, InputKey key, InputActionReference actionRef) => p?.RegisterButton((int)key, actionRef);");
         sb.AppendLine(
         "      public static AxisState RegisterAxis(this InputProcessor p, InputKey key, InputActionReference actionRef) => p?.RegisterAxis((int)key, actionRef);");
         sb.AppendLine(
         "      public static void UnregisterButton(this InputProcessor p, InputKey key) => p?.UnregisterButton((int)key);");
         sb.AppendLine(
         "      public static void UnregisterAxis(this InputProcessor p, InputKey key) => p?.UnregisterAxis((int)key);");
         sb.AppendLine(
         "      public static void Unregister(this InputProcessor p, InputKey key) => p?.Unregister((int)key);");
         sb.AppendLine("   }");
         sb.AppendLine("}");

         // Ask the user where to save.
         var defaultDir = string.IsNullOrEmpty(_lastEnumPath) ?
                             Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultEnumDir)) :
                             Path.GetDirectoryName(_lastEnumPath);

         var defaultFile = string.IsNullOrEmpty(_lastEnumPath) ? DefaultEnumFile : Path.GetFileName(_lastEnumPath);

         var fullPath = EditorUtility.SaveFilePanel("Save InputKey Enum", defaultDir, defaultFile, "cs");

         if (string.IsNullOrEmpty(fullPath))
         {
            return; // user cancelled
         }

         _lastEnumPath = fullPath;

         var dir = Path.GetDirectoryName(fullPath);
         if (!Directory.Exists(dir))
         {
            Directory.CreateDirectory(dir);
         }

         File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);

         // If inside the Assets folder, import so Unity picks it up.
         var dataPath = Path.GetFullPath(Application.dataPath);
         if (fullPath.StartsWith(dataPath))
         {
            var relative = "Assets" + fullPath.Substring(dataPath.Length).Replace('\\', '/');
            AssetDatabase.ImportAsset(relative, ImportAssetOptions.ForceUpdate);
         }

         AssetDatabase.Refresh();

         Debug.Log($"[InputKey] Generated {entries.Count} key(s) → {fullPath}");
      }

      // ------------------------------------------------------------------
      // Persistence (ScriptableObject)
      // ------------------------------------------------------------------

      private void Load()
      {
         if (_asset == null)
            _asset = InputKeyJsonHelper.FindAsset();

         if (_asset == null)
         {
            _defs = new List<InputKeyDefinition>();
            return;
         }

         // Deep copy so the SO is not modified until Save().
         _defs = new List<InputKeyDefinition>();
         foreach (var def in _asset.Definitions)
         {
            _defs.Add(new InputKeyDefinition { Name = def.Name, IsAxis = def.IsAxis });
         }

         _dirty = false;
      }

      private void Save()
      {
         if (_asset == null)
         {
            Debug.LogWarning("[InputKey] No InputKeyDefinitionAsset assigned. Cannot save.");
            return;
         }

         Undo.RecordObject(_asset, "Update Input Key Definitions");

         _asset.Definitions.Clear();
         foreach (var def in _defs)
         {
            _asset.Definitions.Add(new InputKeyDefinition { Name = def.Name, IsAxis = def.IsAxis });
         }

         EditorUtility.SetDirty(_asset);
         AssetDatabase.SaveAssetIfDirty(_asset);
         _dirty = false;
      }
   }
}