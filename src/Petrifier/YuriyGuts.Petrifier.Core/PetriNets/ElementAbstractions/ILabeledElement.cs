namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public interface ILabeledElement
    {
        /// <summary>
        /// Gets or sets the text that should be displayed next to the element.
        /// </summary>
        string Label { get; set; }
    }
}
