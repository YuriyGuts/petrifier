using System.Collections.Generic;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public interface ITokenDispatcher
    {
        /// <summary>
        /// Gets or sets the list of output elements that will contains the tokens after the element is executed.
        /// </summary>
        List<ITokenContainer> Output { get; set; }
    }
}
