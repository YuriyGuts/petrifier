using System.Collections.Generic;
using System.ComponentModel;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class OrJoin : PetriNetElement, ILabeledElement, ITokenConsumer, ITokenDispatcher
    {
        /// <summary>
        /// Gets or sets the text that should be displayed next to the element.
        /// </summary>
        [Category("Appearance")]
        [Description("The text that should be displayed next to the element.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the list of input elements that contain tokens.
        /// </summary>
        [Browsable(false)]
        public List<ITokenContainer> Input { get; set; }

        /// <summary>
        /// Gets or sets the list of output elements that will contains the tokens after the element is executed.
        /// </summary>
        [Browsable(false)]
        public List<ITokenContainer> Output { get; set; }

        public OrJoin()
        {
            Input = new List<ITokenContainer>();
            Output = new List<ITokenContainer>();
        }

        public OrJoin(string id)
            : base(id)
        {
            Input = new List<ITokenContainer>();
            Output = new List<ITokenContainer>();
        }
    }
}
