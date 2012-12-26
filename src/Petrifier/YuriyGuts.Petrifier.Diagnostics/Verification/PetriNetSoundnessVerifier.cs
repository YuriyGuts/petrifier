using System.Diagnostics;

namespace YuriyGuts.Petrifier.Diagnostics.Verification
{
    public class PetriNetSoundnessVerifier : PetriNetVerifier
    {
        public override PetriNetVerificationResult Verify(PetriNetVerificationContext context)
        {
            Trace.WriteLine(GetType().Name + ".Verify()");
            var result = new PetriNetVerificationResult
            {
                IsSuccessful = true,
            };
            return result;
        }
    }
}
