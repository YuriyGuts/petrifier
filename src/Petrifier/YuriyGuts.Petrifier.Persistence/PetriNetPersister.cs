using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Persistence
{
    /// <summary>
    /// Defines the methods that should be implemented by Petri Net export/import utilities.
    /// </summary>
    public interface IPetriNetPersister
    {
        /// <summary>
        /// Exports the Petri Net to a persistent document.
        /// </summary>
        /// <param name="petriNet">An instance of PetriNet to export.</param>
        /// <returns>An instance of PersistentDocument representing the Petri Net.</returns>
        PersistentDocument Export(PetriNet petriNet);

        /// <summary>
        /// Imports a persistent document and converts it to a PetriNet instance.
        /// </summary>
        /// <param name="document">An instance of PersistentDocument to import.</param>
        /// <returns>An instance of PetriNet that represents the Petri Net extracted from the document.</returns>
        PetriNet Import(PersistentDocument document);
    }
}
