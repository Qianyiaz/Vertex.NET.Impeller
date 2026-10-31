using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RenderDemo.Core;

[SupportedOSPlatform("windows10.0.17763")]
public static unsafe partial class WindowsThemeHelper
{
    private const uint MsgActivate = 0x0086;
    private const uint MsgSettingChange = 0x001A;
    private const uint ImmersiveDarkSubclassId = 0x1001;

    private const string DwmApi = "dwmapi.dll";
    private const string ComCtl32 = "comctl32.dll";
    private const string User32 = "user32.dll";
    private const string UxTheme = "uxtheme.dll";

    private static readonly int DwmUseImmersiveDarkMode =
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041) ? 20 : 19;

    static WindowsThemeHelper() => SetPreferredAppMode(1); // Allow Dark

    public static bool IsSystemUseDarkMode => GetSystemIsUseDarkMode();

    public static void SyncWindowThemeMode(nint hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hwnd));

        var hr = DwmGetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, out var current, sizeof(int));
        if (hr == 0 && current == isDarkMode)
            return;

        ApplyWindowThemeMode(hwnd, isDarkMode);
    }

    public static void ApplyWindowThemeMode(nint hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hwnd));

        AllowDarkModeForWindow(hwnd, isDarkMode);
        DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref isDarkMode, sizeof(int));

        FlushMenuThemes();

        DefWindowProcW(hwnd, MsgActivate, IntPtr.Zero, IntPtr.Zero);
        DefWindowProcW(hwnd, MsgActivate, new IntPtr(1), IntPtr.Zero);
    }

    public sealed class SystemThemeWatcher : IDisposable
    {
        private static readonly nuint SubclassProcPtr =
            (nuint)(delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nint, nint>)&OnSubclass;

        private readonly nint _hwnd;

        public SystemThemeWatcher(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle", nameof(hwnd));

            _hwnd = hwnd;

            SyncWindowThemeMode(hwnd, IsSystemUseDarkMode);
            SetWindowSubclass(hwnd, SubclassProcPtr, ImmersiveDarkSubclassId, IntPtr.Zero);
        }

        public void Dispose() => RemoveWindowSubclass(_hwnd, SubclassProcPtr, ImmersiveDarkSubclassId);

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static nint OnSubclass(nint hwnd, uint msg, nint wParam, nint lParam, nuint uIdSubclass, nint dwRefData)
        {
            switch (msg)
            {
                case MsgSettingChange when Marshal.PtrToStringUni(lParam) is "ImmersiveColorSet":
                    SyncWindowThemeMode(hwnd, IsSystemUseDarkMode);
                    break;
            }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }
    }

    #region Imports

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(DwmApi)]
    private static partial void DwmSetWindowAttribute(nint hwnd, int attr,
        [MarshalAs(UnmanagedType.Bool)] ref bool attrValue, int attrSize);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(DwmApi)]
    private static partial int DwmGetWindowAttribute(nint hwnd, int dwAttribute,
        [MarshalAs(UnmanagedType.Bool)] out bool pvAttribute, int cbAttribute);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(User32)]
    private static partial void DefWindowProcW(nint hwnd, uint msg, nint wParam, nint lParam);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#133")]
    private static partial void AllowDarkModeForWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#135")]
    private static partial void SetPreferredAppMode(int appMode);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#136")]
    private static partial void FlushMenuThemes();

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport(UxTheme, EntryPoint = "#138")]
    private static partial bool GetSystemIsUseDarkMode();

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial void SetWindowSubclass(nint hwnd, nuint pfnSubclass, nuint uIdSubclass, nint dwRefData);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial void RemoveWindowSubclass(nint hwnd, nuint pfnSubclass, nuint uIdSubclass);

    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial nint DefSubclassProc(nint hwnd, uint uMsg, nint wParam, nint lParam);

    #endregion
}