using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Runtime
{
    public class PetrifierRuntime
    {
        private Dictionary<RuntimeModuleHandle, Task> tasks;

        public PetrifierRuntime()
        {
            tasks = new Dictionary<RuntimeModuleHandle, Task>();
        }

        public RuntimeModuleHandle Execute(RuntimeExecutionContext context)
        {
            RuntimeModuleHandle handle = new RuntimeModuleHandle
            {
                ExecutionContext = context
            };

            Task task = new Task(() => ExecuteThreaded(context));
            lock (tasks)
            {
                tasks.Add(handle, task);
            }
            task.Start();

            return handle;
        }

        public void ExecuteThreaded(RuntimeExecutionContext context)
        {
            var petriNet = context.PetriNetModule.PetriNet;

            Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();
            Dictionary<Type, object> objectCache = new Dictionary<Type, object>();

            bool suitableActionFound;
            do
            {
                suitableActionFound = false;
                var transitionsToFire = petriNet.Elements
                    .Where(element =>
                        element is ITokenConsumer
                        && ((ITokenConsumer)element).Input
                            .All(input => input.Marking > 0));

                foreach (var transition in transitionsToFire)
                {
                    // Consuming tokens from the input places.
                    if (transition is ITokenConsumer)
                    {
                        (transition as ITokenConsumer).Input.ForEach(input => input.Marking--);
                    }

                    // Executing the methods, if the transition is bound to a method.
                    if (transition is IMethodReference)
                    {
                        var methodReference = (IMethodReference)transition;
                        
                        // Loading type from assembly.
                        Type type;
                        if (!string.IsNullOrEmpty(methodReference.AssemblyFileName))
                        {
                            Assembly assembly;
                            if (!assemblyCache.TryGetValue(methodReference.AssemblyFileName, out assembly))
                            {
                                string fileName = Path.IsPathRooted(methodReference.AssemblyFileName)
                                    ? methodReference.AssemblyFileName
                                    : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), methodReference.AssemblyFileName);

                                assembly = Assembly.LoadFile(fileName);
                                assemblyCache.Add(methodReference.AssemblyFileName, assembly);
                            }
                            type = assembly.GetType(methodReference.TypeName);
                        }
                        else
                        {
                            type = Assembly.GetEntryAssembly().GetType(methodReference.TypeName);
                        }

                        if (type != null)
                        {
                            // Looking for the appropriate method in the type.
                            MethodInfo methodInfo = type.GetMethod(methodReference.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (methodInfo != null)
                            {
                                // Creating a target object (or pulling it from cache) and invoking the method on it.
                                object targetObject;
                                if (!objectCache.TryGetValue(type, out targetObject))
                                {
                                    targetObject = Activator.CreateInstance(type);
                                    objectCache.Add(type, targetObject);
                                }

                                methodInfo.Invoke(targetObject, null);
                                suitableActionFound = true;
                            }
                        }
                    }

                    // Placing tokens to the output places.
                    if (transition is ITokenDispatcher)
                    {
                        (transition as ITokenDispatcher).Output.ForEach(output => output.Marking++);
                    }
                }
            }
            while (suitableActionFound);

            objectCache.Clear();
            assemblyCache.Clear();
        }

        public void WaitForProcessEnd(RuntimeModuleHandle handle)
        {
            if (tasks.ContainsKey(handle))
            {
                tasks[handle].Wait();
            }
        }
    }
}
