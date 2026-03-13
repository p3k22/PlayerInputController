namespace P3k.PlayerInputController.Editor
{
    using System.Collections.Generic;

    using UnityEditor;

    /// <summary>
    /// Reads key definitions from an <see cref="InputKeyDefinitionAsset"/> in the project.
    /// Shared by the property drawer and profile editor so they can display
    /// friendly key names for the raw <c>int</c> key values.
    /// </summary>
    public static class InputKeyJsonHelper
    {
        /// <summary>
        /// Finds the first <see cref="InputKeyDefinitionAsset"/> in the project.
        /// </summary>
        public static InputKeyDefinitionAsset FindAsset()
        {
            var guids = AssetDatabase.FindAssets("t:InputKeyDefinitionAsset");
            if (guids.Length == 0) return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<InputKeyDefinitionAsset>(path);
        }

        /// <summary>
        /// Returns a dictionary mapping int key values → display names,
        /// including <c>0 → "None"</c>. Key values start at 1, matching
        /// the generated enum.
        /// </summary>
        public static Dictionary<int, string> LoadKeyNames()
        {
            var result = new Dictionary<int, string> { { 0, "None" } };

            var asset = FindAsset();
            if (asset == null) return result;

            var index = 1;
            var seen = new HashSet<string>();

            foreach (var def in asset.Definitions)
            {
                var name = def.Name?.Trim();
                if (string.IsNullOrEmpty(name) || name == "None") continue;
                if (!seen.Add(name)) continue;

                result[index] = name;
                index++;
            }

            return result;
        }

        /// <summary>
        /// Returns a dictionary mapping int key values → IsAxis flag.
        /// </summary>
        public static Dictionary<int, bool> LoadAxisFlags()
        {
            var result = new Dictionary<int, bool> { { 0, false } };

            var asset = FindAsset();
            if (asset == null) return result;

            var index = 1;
            var seen = new HashSet<string>();

            foreach (var def in asset.Definitions)
            {
                var name = def.Name?.Trim();
                if (string.IsNullOrEmpty(name) || name == "None") continue;
                if (!seen.Add(name)) continue;

                result[index] = def.IsAxis;
                index++;
            }

            return result;
        }
    }
}
