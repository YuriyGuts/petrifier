using System;
using System.IO;
using YuriyGuts.Petrifier.Core.PetriNets;
using YuriyGuts.Petrifier.Persistence.Pnml;
using YuriyGuts.Petrifier.Runtime;
using YuriyGuts.Petrifier.Runtime.Hosting;

namespace PetriNetRunner
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                try
                {
                    ExecutePetriNet(args[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 2;
                }
            }
            else
            {
                Console.WriteLine("Error in parameter syntax! Usage: PetriNetRunner <pnmlFile>");
                return 1;
            }
            return 0;
        }

        public static void ExecutePetriNet(string fileName)
        {
            var document = new PnmlDocument();
            document.Load(File.ReadAllText(fileName));
            var petriNet = new PnmlPersister().Import(document);

            var runtimeHost = new EmbeddedRuntimeHost(typeof(PetrifierRuntime));
            var handle = runtimeHost.ExecutePetriNetModule(new PetriNetModule(petriNet));
            runtimeHost.WaitForEnd(handle);
        }
    }
}
