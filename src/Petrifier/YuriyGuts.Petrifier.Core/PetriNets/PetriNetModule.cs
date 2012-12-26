namespace YuriyGuts.Petrifier.Core.PetriNets
{
    public class PetriNetModule
    {
        public PetriNet PetriNet { get; set; }

        public PetriNetModule(PetriNet petriNet)
        {
            PetriNet = petriNet;
        }
    }
}
