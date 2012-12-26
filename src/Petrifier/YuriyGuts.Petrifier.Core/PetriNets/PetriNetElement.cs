using System.Collections.Generic;
using System.ComponentModel;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class PetriNetElement
    {
        public PetriNetElement()
        {
            GraphicsMetadata = new Dictionary<string, object>();
        }

        public PetriNetElement(string id) : this()
        {
            ID = id;
        }

        [Category("Design")]
        [Description("Internal identifier of the element.")]
        public string ID { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, object> GraphicsMetadata { get; set; }

        public object GetGraphicsMetadataItem(string key, object defaultValue)
        {
            string[] keyParts = key.Split('.');

            Dictionary<string, object> bottomLevelDictionary = GraphicsMetadata;
            for (int i = 0; i < keyParts.Length - 1; i++)
            {
                if (bottomLevelDictionary.ContainsKey(keyParts[i]))
                {
                    bottomLevelDictionary = (Dictionary<string, object>)bottomLevelDictionary[keyParts[i]];
                }
                else
                {
                    return defaultValue;
                }
            }

            string lastKeyPart = keyParts[keyParts.Length - 1];
            if (bottomLevelDictionary.ContainsKey(lastKeyPart))
            {
                return bottomLevelDictionary[lastKeyPart];
            }
            return defaultValue;
        }

        public void SetGraphicsMetadataItem(string key, object value)
        {
            string[] keyParts = key.Split('.');

            Dictionary<string, object> bottomLevelDictionary = GraphicsMetadata;
            for (int i = 0; i < keyParts.Length - 1; i++)
            {
                if (!bottomLevelDictionary.ContainsKey(keyParts[i]))
                {
                    bottomLevelDictionary.Add(keyParts[i], new Dictionary<string, object>());
                }
                bottomLevelDictionary = (Dictionary<string, object>)bottomLevelDictionary[keyParts[i]];
            }

            string lastKeyPart = keyParts[keyParts.Length - 1];
            if (!bottomLevelDictionary.ContainsKey(lastKeyPart))
            {
                bottomLevelDictionary.Add(lastKeyPart, value);
            }
            else
            {
                bottomLevelDictionary[lastKeyPart] = value;
            }
        }
    }
}
