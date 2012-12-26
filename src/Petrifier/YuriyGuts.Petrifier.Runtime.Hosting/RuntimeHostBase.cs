using System;
using System.Diagnostics;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Runtime.Hosting
{
    public class RuntimeHostBase : IRuntimeHost
    {
        private PetrifierRuntime runtime;

        public RuntimeHostBase(Type runtimeType)
        {
            Trace.WriteLine(GetType().Name + " constructor");
            if (!typeof(PetrifierRuntime).IsAssignableFrom(runtimeType))
            {
                throw new ArgumentException("Runtime Type must be PetrifierRuntime or a type derived from it.", "runtimeType");
            }
            runtime = (PetrifierRuntime)Activator.CreateInstance(runtimeType);
        }

        #region IRuntimeHost Members

        public virtual RuntimeModuleHandle ExecutePetriNetModule(PetriNetModule module)
        {
            Trace.WriteLine(GetType().Name + ".ExecutePetriNetModule()");
            RuntimeExecutionContext context = new RuntimeExecutionContext
            {
                PetriNetModule = module
            };
            return runtime.Execute(context);
        }

        public virtual RuntimeModuleSnapshot TakeSnapshot(RuntimeModuleHandle handle)
        {
            return new RuntimeModuleSnapshot
            {
                PetriNet = handle.ExecutionContext.PetriNetModule.PetriNet
            };
        }

        public virtual void WaitForEnd(RuntimeModuleHandle handle)
        {
            runtime.WaitForProcessEnd(handle);
        }

        #endregion
    }
}
