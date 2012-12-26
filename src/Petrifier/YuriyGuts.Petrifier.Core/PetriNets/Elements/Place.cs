using System.ComponentModel;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class Place : PetriNetElement, ILabeledElement, ITokenContainer
    {
        /// <summary>
        /// Gets or sets the text that should be displayed next to the element.
        /// </summary>
        [Category("Appearance")]
        [Description("The text that should be displayed next to the element.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the marking of the place.
        /// </summary>
        [Category("Petri Net Specific")]
        [Description("Initial marking of the place.")]
        public int Marking { get; set; }
    }
}
