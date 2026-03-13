namespace P3k.PlayerInputController
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    [Serializable]
    public class InputKeyDefinition
    {
        public string Name = "";
        public bool IsAxis;
    }

    /// <summary>
    /// Stores the set of semantic input-key definitions used by both the editor
    /// tooling and the <c>InputKey</c> enum code generator.
    /// Create via <b>Assets → Create → P3k → Input Key Definitions</b>.
    /// </summary>
    [CreateAssetMenu(
       fileName = "InputKeyDefinitions",
       menuName = "P3k/Input Key Definitions")]
    public class InputKeyDefinitionAsset : ScriptableObject
    {
        [SerializeField]
        private List<InputKeyDefinition> _definitions = new List<InputKeyDefinition>();

        public List<InputKeyDefinition> Definitions => _definitions;
    }
}
