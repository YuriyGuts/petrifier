namespace YuriyGuts.Petrifier.Diagnostics.Verification
{
    public abstract class PetriNetVerifier
    {
        public abstract PetriNetVerificationResult Verify(PetriNetVerificationContext context);
    }
}
