using System;

namespace YuriyGuts.Petrifier.Runtime.Hosting
{
    public class RemoteRuntimeHost : RuntimeHostBase
    {
        public RemoteRuntimeHost(Type runtimeType)
            : base(runtimeType)
        {
        }
    }
}
