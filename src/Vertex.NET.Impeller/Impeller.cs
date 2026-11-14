using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vertex.NET.Impeller;

public partial class Impeller
{
    static Impeller() => NativeLibrary.SetDllImportResolver(typeof(Impeller).Assembly, ResolveDllImport);

    private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) =>
        NativeLibrary.TryLoad(GetLibraryPath(libraryName), out var handle) ? handle : IntPtr.Zero;

    private static string GetLibraryPath(string libraryName) =>
        Path.Combine(AppContext.BaseDirectory, "runtimes", Rid, "native",
            OperatingSystem.IsWindows() ? $"{libraryName}.dll"
            : OperatingSystem.IsMacOS() ? $"lib{libraryName}.dylib"
            : $"lib{libraryName}.so");

    /// <summary>
    ///     Normalized runtime identifier, computed once per process.
    ///     https://github.com/dotnet/runtime/issues/114156#issuecomment-2773234611
    /// </summary>
    private static readonly string Rid = GetNormalizedRuntimeIdentifier();

    private static string GetNormalizedRuntimeIdentifier()
    {
        var rid = RuntimeInformation.RuntimeIdentifier;

        // Already in the expected format
        if (rid is "win-x64" or "win-arm64"
            or "linux-x64" or "linux-arm64"
            or "linux-musl-x64" or "linux-musl-arm64"
            or "osx-x64" or "osx-arm64")
            return rid;

        // Extract architecture (defaults to x64 if not present)
        var dash = rid.LastIndexOf('-');
        var arch = dash >= 0 ? rid[(dash + 1)..] : "x64";

        if (OperatingSystem.IsWindows()) return $"win-{arch}";
        if (OperatingSystem.IsMacOS()) return $"osx-{arch}";
        if (OperatingSystem.IsLinux())
            return rid.Contains("alpine", StringComparison.Ordinal) || IsAlpineLinux()
                ? $"linux-musl-{arch}"
                : $"linux-{arch}";

        // Fallback to the original RID if we can't normalize it
        return rid;
    }

    /// <summary>Checks if the current Linux distribution is Alpine (musl-based).</summary>
    private static bool IsAlpineLinux()
    {
        if (File.Exists("/etc/alpine-release"))
            return true;

        try
        {
            var content = File.ReadAllText("/etc/os-release");
            return content.Contains("ID=\"alpine\"", StringComparison.Ordinal) ||
                   content.Contains("ID=alpine", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}