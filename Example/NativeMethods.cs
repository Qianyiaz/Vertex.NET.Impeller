using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
///     Native methods for interacting with Windows APIs related to window theming and dark mode.
///     API support is limited to Windows 10 version 17763 and later.
/// </summary>
[UnsupportedOSPlatform("windows")]
[SupportedOSPlatform("windows10.0.17763")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static partial class NativeMethods
{
    static NativeMethods() => SetPreferredAppMode(PreferredAppMode.AllowDark); // AllowDark

    #region Public Methods

    // ReSharper disable once MemberCanBePrivate.Global
    public static void SyncWindowThemeMode(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd is 0) throw new ArgumentNullException(nameof(hwnd));

        DwmGetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, out var isAppDark, 4);
        if (isDarkMode == isAppDark)
            return;

        // Window
        AllowDarkModeForWindow(hwnd, isDarkMode);
        DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref isDarkMode, 4);

        // Menu
        FlushMenuThemes();

        // Apply Dark Window Style Form (https://github.com/godotengine/godot/issues/65492#issuecomment-1347391733)
        DefWindowProcW(hwnd, WM_NCACTIVATE, false, 0);
        DefWindowProcW(hwnd, WM_NCACTIVATE, true, 0);
    }

    public static class SystemThemeWatcher
    {
        private static readonly SubclassProcDelegate _subclassProc = OnSubclass;

        public static void Watch(IntPtr hwnd)
        {
            if (hwnd is 0) throw new ArgumentException("Invalid window handle", nameof(hwnd));

            SyncWindowThemeMode(hwnd, GetSystemIsUseDarkMode());
            SetWindowSubclass(hwnd, _subclassProc, ImmersiveDarkSubclassId, 0);
        }

        private static IntPtr OnSubclass(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint uIdSubclass,
            IntPtr dwRefData)
        {
            switch (msg)
            {
                case WM_SETTINGCHANGE when Marshal.PtrToStringUni(lParam) is "ImmersiveColorSet":
                    SyncWindowThemeMode(hwnd, GetSystemIsUseDarkMode());
                    break;

                case WM_NCDESTROY:
                    RemoveWindowSubclass(hwnd, _subclassProc, uIdSubclass);
                    break;
            }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }
    }

    #endregion

    #region Windows API Imports

    // Dll Names
    private const string s_dwmapi = "dwmapi.dll";
    private const string s_comctl32 = "comctl32.dll";
    private const string s_user32 = "user32.dll";
    private const string s_uxtheme = "uxtheme.dll";

    // Window messages
    private const uint WM_NCACTIVATE = 0x0086;
    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint WM_NCDESTROY = 0x0082;
    private const uint ImmersiveDarkSubclassId = 0x1001;

    private static readonly int DwmwaUseImmersiveDarkMode =
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041) ? 20 : 19; // Possibly OK.

    public enum PreferredAppMode
    {
        // Default = 0,
        AllowDark = 1
        /*
        ForceDark = 2,
        ForceLight = 3,
        Max = 4
        */
    } // Just need one.

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_dwmapi, StringMarshalling = StringMarshalling.Utf16)]
    private static partial void DwmSetWindowAttribute(IntPtr hwnd, int attr,
        [MarshalAs(UnmanagedType.Bool)] ref bool attrValue, int attrSize);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_dwmapi, StringMarshalling = StringMarshalling.Utf16)]
    private static partial void DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute,
        [MarshalAs(UnmanagedType.Bool)] out bool pvAttribute, int cbAttribute);

    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_user32, StringMarshalling = StringMarshalling.Utf16)]
    private static partial void DefWindowProcW(IntPtr hwnd, uint msg, [MarshalAs(UnmanagedType.Bool)] bool wParam,
        IntPtr lParam);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_uxtheme, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "#133")]
    private static partial void AllowDarkModeForWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_uxtheme, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "#135")]
    private static partial void SetPreferredAppMode(PreferredAppMode appMode);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_uxtheme, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "#136")]
    private static partial void FlushMenuThemes();

    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.Bool)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_uxtheme, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "#138")]
    private static partial bool GetSystemIsUseDarkMode();

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_comctl32, StringMarshalling = StringMarshalling.Utf16)]
    private static partial void SetWindowSubclass(IntPtr hwnd, SubclassProcDelegate pfnSubclass, uint uIdSubclass,
        IntPtr dwRefData);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_comctl32, StringMarshalling = StringMarshalling.Utf16)]
    private static partial void RemoveWindowSubclass(IntPtr hwnd, SubclassProcDelegate pfnSubclass, uint uIdSubclass);

    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(s_comctl32, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr DefSubclassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr SubclassProcDelegate(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass,
        IntPtr dwRefData);

    #endregion
}