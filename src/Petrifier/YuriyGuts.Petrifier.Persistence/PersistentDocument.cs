namespace YuriyGuts.Petrifier.Persistence
{
    /// <summary>
    /// Represents a document that can be persisted to a storage media.
    /// </summary>
    public abstract class PersistentDocument
    {
        /// <summary>
        /// Retrieves the content of the document.
        /// </summary>
        /// <returns>String representation of document content.</returns>
        public abstract string GetContent();
    }
}
