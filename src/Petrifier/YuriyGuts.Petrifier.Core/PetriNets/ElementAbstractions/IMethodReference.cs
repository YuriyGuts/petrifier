namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public interface IMethodReference
    {
        /// <summary>
        /// Gets or sets the file name of assembly in which the transition method can be found.
        /// </summary>
        string AssemblyFileName { get; set; }

        /// <summary>
        /// Gets or sets the type in which the transition method can be found.
        /// </summary>
        string TypeName { get; set; }

        /// <summary>
        /// Gets or sets he name of the method containing the transition implementation.
        /// </summary>
        string MethodName { get; set; }
    }
}
