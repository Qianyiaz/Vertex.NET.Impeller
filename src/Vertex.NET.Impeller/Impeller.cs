using System.Runtime.InteropServices;

namespace Vertex.NET.Impeller;

public partial class Impeller
{
    static Impeller() => InitApi();

    public static string GetLibraryName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "impeller" : "libimpeller";
}