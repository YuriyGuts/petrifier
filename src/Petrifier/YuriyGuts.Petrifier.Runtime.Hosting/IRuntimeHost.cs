using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Runtime.Hosting
{
    public interface IRuntimeHost
    {
        RuntimeModuleHandle ExecutePetriNetModule(PetriNetModule module);

        RuntimeModuleSnapshot TakeSnapshot(RuntimeModuleHandle handle);

        void WaitForEnd(RuntimeModuleHandle handle);
    }
}
