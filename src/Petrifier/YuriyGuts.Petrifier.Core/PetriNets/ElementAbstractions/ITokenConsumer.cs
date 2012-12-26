using System.Collections.Generic;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public interface ITokenConsumer
    {
        /// <summary>
        /// Gets or sets the list of input elements that contain tokens.
        /// </summary>
        List<ITokenContainer> Input { get; set; }
    }
}
