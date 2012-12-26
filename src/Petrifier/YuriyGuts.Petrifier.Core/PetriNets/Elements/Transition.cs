using System.Collections.Generic;
using System.ComponentModel;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class Transition : PetriNetElement, ILabeledElement, ITokenConsumer, ITokenDispatcher, IMethodReference
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

        /// <summary>
        /// Gets or sets the file name of assembly in which the transition method can be found.
        /// </summary>
        [Category("Implementation")]
        [Description("File name of the assembly in which the transition method can be found.")]
        public string AssemblyFileName { get; set; }

        /// <summary>
        /// Gets or sets the type in which the transition method can be found.
        /// </summary>
        [Category("Implementation")]
        [Description("The name of the type in which the transition method can be found.")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets he name of the method containing the transition implementation.
        /// </summary>
        [Category("Implementation")]
        [Description("The name of the method containing the transition implementation.")]
        public string MethodName { get; set; }

        public Transition()
        {
            Input = new List<ITokenContainer>();
            Output = new List<ITokenContainer>();
        }

        public Transition(string id)
            : base(id)
        {
            Input = new List<ITokenContainer>();
            Output = new List<ITokenContainer>();
        }
    }
}
