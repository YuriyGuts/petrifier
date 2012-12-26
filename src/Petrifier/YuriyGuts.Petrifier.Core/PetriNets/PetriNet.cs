using System.Collections.Generic;
using System.Linq;

namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class PetriNet
    {
        public PetriNet()
        {
            Elements = new List<PetriNetElement>();
        }

        public PetriNet(List<PetriNetElement> elements)
        {
            Elements = elements;
        }

        public List<PetriNetElement> Elements { get; set; }

        public void UpdateElementRelationships()
        {
            foreach (var element in Elements)
            {
                if (element is ITokenConsumer)
                {
                    ((ITokenConsumer)element).Input.Clear();
                }
                if (element is ITokenDispatcher)
                {
                    ((ITokenDispatcher)element).Output.Clear();
                }
            }

            var arcs = Elements.Where(element => element is Arc).Cast<Arc>();
            foreach (var arc in arcs)
            {
                if (arc.StartElement != null && arc.StartElement is ITokenContainer && arc.EndElement is ITokenConsumer)
                {
                    ((ITokenConsumer)arc.EndElement).Input.Add((ITokenContainer)arc.StartElement);
                }
                if (arc.StartElement != null && arc.StartElement is ITokenDispatcher && arc.EndElement is ITokenContainer)
                {
                    ((ITokenDispatcher)arc.StartElement).Output.Add((ITokenContainer)arc.EndElement);
                }
            }
        }
    }
}
