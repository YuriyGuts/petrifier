using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Persistence.Pnml
{
    /// <summary>
    /// Represents the Petri Net export utitity that works with PNML format.
    /// </summary>
    public class PnmlPersister : IPetriNetPersister
    {
        #region IPetriNetPersister implementation

        /// <summary>
        /// Exports the Petri Net to a PNML document.
        /// </summary>
        /// <param name="petriNet">An instance of Petri Net to export.</param>
        /// <returns>An instance of PnmlDocument representing the persisted Petri Net.</returns>
        public PersistentDocument Export(PetriNet petriNet)
        {
            var xmlDocument = BuildPetriNetXmlDocument(petriNet);
            var result = new PnmlDocument();
            result.Load(xmlDocument);
            return result;
        }

        /// <summary>
        /// Imports a PNML document and converts it to a PetriNet instance.
        /// </summary>
        /// <param name="document">An instance of PnmlDocument to import.</param>
        /// <returns>An instance of PetriNet that represents the Petri Net extracted from the document.</returns>
        public PetriNet Import(PersistentDocument document)
        {
            var petriNet = new PetriNet();

            // Loading PNML document content.
            var xDocument = GetXmlDocumentFromPnmlDocument(document);

            // Loading Places and Transitions.
            var nonArcElements = xDocument.Element("pnml").Element("net").Elements().Where(element => element.Name != "arc");
            foreach (var xPNElement in nonArcElements)
            {
                switch (xPNElement.Name.ToString())
                {
                    case "place":
                        var place = GetPlaceObjectFromPnmlElement(xPNElement);
                        if (place != null)
                        {
                            petriNet.Elements.Add(place);
                        }
                        break;

                    case "transition":
                        var transition = GetTransitionObjectFromPnmlElement(xPNElement);
                        if (transition != null)
                        {
                            petriNet.Elements.Add(transition);
                        }
                        break;
                }
            }

            // Loading Arcs.
            var arcElements = xDocument.Element("pnml").Element("net").Elements().Where(element => element.Name == "arc");
            foreach (var xArcElement in arcElements)
            {
                var arc = GetArcObjectFromPnmlElement(petriNet, xArcElement);
                if (arc != null)
                {
                    petriNet.Elements.Add(arc);
                }
            }

            petriNet.UpdateElementRelationships();
            return petriNet;
        }

        #endregion IPetriNetPersister implementation

        /// <summary>
        /// Converts the PnmlDocument to XDocument.
        /// </summary>
        /// <param name="document">An instance of PnmlDocument to convert to XML.</param>
        /// <returns>An instance of XDocument containing the PNML document.</returns>
        private static XDocument GetXmlDocumentFromPnmlDocument(PersistentDocument document)
        {
            XDocument xDocument;
            if (document is PnmlDocument)
            {
                xDocument = (document as PnmlDocument).XmlDocument;
            }
            else
            {
                using (StringReader reader = new StringReader(document.GetContent()))
                {
                    xDocument = XDocument.Load(reader);
                }
            }
            return xDocument;
        }

        private static Arc GetArcObjectFromPnmlElement(PetriNet petriNet, XElement pnmlElement)
        {
            Arc arc = new Arc();

            arc.ID = pnmlElement.Attribute("id").Value;
            string startElementID = pnmlElement.Attribute("source").Value;
            string endElementID = pnmlElement.Attribute("target").Value;
            arc.StartElement = petriNet.Elements.FirstOrDefault(element => element.ID == startElementID);
            arc.EndElement = petriNet.Elements.FirstOrDefault(element => element.ID == endElementID);

            if (pnmlElement.Element("graphics") != null)
            {
                if (pnmlElement.Element("graphics").Element("dimension") != null)
                {
                    arc.SetGraphicsMetadataItem("graphics.dimension.@x", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("x").Value));
                    arc.SetGraphicsMetadataItem("graphics.dimension.@y", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("y").Value));
                }
            }
            return arc;
        }

        private static Place GetPlaceObjectFromPnmlElement(XElement pnmlElement)
        {
            Place place = new Place();

            place.ID = pnmlElement.Attribute("id").Value;
            place.Label = pnmlElement.Element("name").Element("text").Value;

            if (pnmlElement.Element("initialMarking") != null)
            {
                place.Marking = int.Parse(pnmlElement.Element("initialMarking").Element("text").Value);
            }
            if (pnmlElement.Element("graphics") != null)
            {
                if (pnmlElement.Element("graphics").Element("position") != null)
                {
                    place.SetGraphicsMetadataItem("graphics.position.@x", int.Parse(pnmlElement.Element("graphics").Element("position").Attribute("x").Value));
                    place.SetGraphicsMetadataItem("graphics.position.@y", int.Parse(pnmlElement.Element("graphics").Element("position").Attribute("y").Value));
                }
                if (pnmlElement.Element("graphics").Element("dimension") != null)
                {
                    place.SetGraphicsMetadataItem("graphics.dimension.@x", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("x").Value));
                    place.SetGraphicsMetadataItem("graphics.dimension.@y", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("y").Value));
                }
            }
            return place;
        }

        private static PetriNetElement GetTransitionObjectFromPnmlElement(XElement pnmlElement)
        {
            PetriNetElement transition = null;
            var toolSpecificElements = pnmlElement.Elements("toolspecific").Where(element => element.Attribute("tool") != null);

            foreach (var toolSpecificElement in toolSpecificElements)
            {
                if (toolSpecificElement.Attribute("tool").Value == "Petrifier")
                {
                    var type = toolSpecificElement.Element("transitiontype").Value;
                    transition = CreateTransitionFromPnmlType(type);

                    if (transition is IMethodReference && toolSpecificElement.Element("methodReference") != null)
                    {
                        var methodReference = (IMethodReference)transition;
                        var methodReferenceElement = toolSpecificElement.Element("methodReference");

                        if (methodReferenceElement.Element("assembly") != null)
                        {
                            methodReference.AssemblyFileName = methodReferenceElement.Element("assembly").Value;
                        }
                        methodReference.TypeName = methodReferenceElement.Element("type").Value;
                        methodReference.MethodName = methodReferenceElement.Element("method").Value;
                    }
                }
            }

            if (transition == null)
            {
                transition = new Transition();
            }

            transition.ID = pnmlElement.Attribute("id").Value;
            if (transition is ILabeledElement)
            {
                ((ILabeledElement)transition).Label = pnmlElement.Element("name").Element("text").Value;
            }

            if (pnmlElement.Element("graphics") != null)
            {
                if (pnmlElement.Element("graphics").Element("position") != null)
                {
                    transition.SetGraphicsMetadataItem("graphics.position.@x", int.Parse(pnmlElement.Element("graphics").Element("position").Attribute("x").Value));
                    transition.SetGraphicsMetadataItem("graphics.position.@y", int.Parse(pnmlElement.Element("graphics").Element("position").Attribute("y").Value));
                }
                if (pnmlElement.Element("graphics").Element("dimension") != null)
                {
                    transition.SetGraphicsMetadataItem("graphics.dimension.@x", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("x").Value));
                    transition.SetGraphicsMetadataItem("graphics.dimension.@y", int.Parse(pnmlElement.Element("graphics").Element("dimension").Attribute("y").Value));
                }
            }

            return transition;
        }

        private static PetriNetElement CreateTransitionFromPnmlType(string type)
        {
            PetriNetElement transition;
            switch (type)
            {
                case "AndSplit":
                    transition = new AndSplit();
                    break;
                case "AndJoin":
                    transition = new AndJoin();
                    break;
                case "OrSplit":
                    transition = new OrSplit();
                    break;
                case "OrJoin":
                    transition = new OrJoin();
                    break;
                default:
                    transition = new Transition();
                    break;
            }
            return transition;
        }

        private XDocument BuildPetriNetXmlDocument(PetriNet petriNet)
        {
            XDocument xmlDocument = new XDocument
            (
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("pnml")
            );

            // Creating XML root elements.
            var netElement = new XElement("net");
            if (xmlDocument.Root != null)
            {
                xmlDocument.Root.Add(netElement);
            }

            // Adding Places.
            var places = petriNet.Elements.OfType<Place>();
            foreach (var place in places)
            {
                var placeElement = ConvertPlaceToPnmlElement(place);
                netElement.Add(placeElement);
            }

            // Adding Transitions.
            List<Type> transitionTypes = new List<Type> { typeof(Transition), typeof(AndSplit), typeof(AndJoin), typeof(OrSplit), typeof(OrJoin), typeof(Subprocess) };
            var transitions = petriNet.Elements.Where(element => transitionTypes.Contains(element.GetType()));
            foreach (var transition in transitions)
            {
                var transitionElement = ConvertTransitionToPnmlElement(transition);
                netElement.Add(transitionElement);
            }

            // Adding Arcs.
            var arcs = petriNet.Elements.OfType<Arc>();
            foreach (var arc in arcs)
            {
                var arcElement = ConvertArcToPnmlElement(arc);
                netElement.Add(arcElement);
            }

            // Adding Subprocesses.
            var subprocesses = petriNet.Elements.OfType<Subprocess>();
            foreach (var subprocess in subprocesses)
            {
                var subprocessElement = ConvertSubprocessToPnmlElement(subprocess);
                netElement.Add(subprocessElement);
            }

            return xmlDocument;
        }

        private XElement ConvertPlaceToPnmlElement(Place place)
        {
            var placeElement = new XElement("place");
            place.ID = place.ID ?? place.ID ?? Guid.NewGuid().ToString().Substring(0, 8);
            placeElement.SetAttributeValue("id", place.ID);

            var nameElement = new XElement("name");
            var textElement = new XElement("text", place.Label);

            nameElement.Add(textElement);
            placeElement.Add(nameElement);

            if (place.Marking > 0)
            {
                var markingElement = new XElement("initialMarking");
                markingElement.Add(new XElement("text", place.Marking));
                placeElement.Add(markingElement);
            }

            AppendMetadataToPnmlElement(placeElement, place.GraphicsMetadata);
            return placeElement;
        }

        private XElement ConvertTransitionToPnmlElement(PetriNetElement transition)
        {
            var transitionElement = new XElement("transition");
            transition.ID = transition.ID ?? transition.ID ?? Guid.NewGuid().ToString().Substring(0, 8);
            transitionElement.SetAttributeValue("id", transition.ID);

            if (transition is ILabeledElement)
            {
                var nameElement = new XElement("name");
                var textElement = new XElement("text", ((ILabeledElement)transition).Label);
                nameElement.Add(textElement);
                transitionElement.Add(nameElement);
            }

            AppendMetadataToPnmlElement(transitionElement, transition.GraphicsMetadata);

            var toolSpecificElement = new XElement("toolspecific");
            toolSpecificElement.SetAttributeValue("tool", "Petrifier");
            toolSpecificElement.SetAttributeValue("version", "0.1");
            toolSpecificElement.Add(new XElement("transitiontype", transition.GetType().Name));
            
            if (transition is IMethodReference)
            {
                var methodReference = (IMethodReference)transition;
                var implementorElement = new XElement("methodReference");

                if (!string.IsNullOrEmpty(methodReference.AssemblyFileName))
                {
                    implementorElement.Add(new XElement("assembly", methodReference.AssemblyFileName));
                }
                implementorElement.Add(new XElement("type", methodReference.TypeName));
                implementorElement.Add(new XElement("method", methodReference.MethodName));
                toolSpecificElement.Add(implementorElement);
            }

            transitionElement.Add(toolSpecificElement);
            return transitionElement;
        }

        private XElement ConvertSubprocessToPnmlElement(Subprocess subprocess)
        {
            var subprocessElement = new XElement("page");
            subprocess.ID = subprocess.ID ?? Guid.NewGuid().ToString().Substring(0, 8);
            subprocessElement.SetAttributeValue("id", subprocess.ID);

            var subnetElement = new XElement("net");
            subprocessElement.Add(subnetElement);
            return subprocessElement;
        }

        private XElement ConvertArcToPnmlElement(Arc arc)
        {
            var arcElement = new XElement("arc");
            arc.ID = arc.ID ?? Guid.NewGuid().ToString().Substring(0, 8);
            arcElement.SetAttributeValue("id", arc.ID);
            arcElement.SetAttributeValue("source", arc.StartElement.ID);
            arcElement.SetAttributeValue("target", arc.EndElement.ID);

            var inscriptionElement = new XElement("inscription");
            var textElement = new XElement("text", string.Empty);
            inscriptionElement.Add(textElement);
            arcElement.Add(inscriptionElement);

            AppendMetadataToPnmlElement(arcElement, arc.GraphicsMetadata);
            return arcElement;
        }

        /// <summary>
        /// Serializes the Petri Net element metadata into XML elements.
        /// </summary>
        /// <param name="xmlElement">Container element.</param>
        /// <param name="metadata">Element metadata dictionary.</param>
        private void AppendMetadataToPnmlElement(XElement xmlElement, Dictionary<string, object> metadata)
        {
            foreach (var item in metadata)
            {
                if (item.Key.StartsWith("@"))
                {
                    xmlElement.SetAttributeValue(item.Key.Substring(1), item.Value);
                    continue;
                }
                if (item.Value is Dictionary<string, object>)
                {
                    var subelement = new XElement(item.Key);
                    AppendMetadataToPnmlElement(subelement, (Dictionary<string, object>)item.Value);
                    xmlElement.Add(subelement);
                }
                else
                {
                    var subelement = new XElement(item.Key, item.Value.ToString());
                    xmlElement.Add(subelement);
                }
            }
        }
    }
}
