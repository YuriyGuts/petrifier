using System;

namespace YuriyGuts.Petrifier.Runtime.Hosting
{
    public class EmbeddedRuntimeHost : RuntimeHostBase
    {
        public EmbeddedRuntimeHost(Type runtimeType)
            : base(runtimeType)
        {
        }
    }
}
