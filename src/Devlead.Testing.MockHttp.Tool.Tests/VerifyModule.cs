using System.Runtime.CompilerServices;

internal static class VerifyModule
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.IgnoreMember<HttpResponseMessage>(x => x.RequestMessage);
    }
}
