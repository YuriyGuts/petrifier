namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class Arc : PetriNetElement
    {
        public Arc()
        {
        }

        public Arc(PetriNetElement startElement, PetriNetElement endElement)
        {
            StartElement = startElement;
            EndElement = endElement;
        }

        public PetriNetElement StartElement { get; set; }

        public PetriNetElement EndElement { get; set; }
    }
}
