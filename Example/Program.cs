using System.Runtime.InteropServices;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

unsafe
{
    GLFW.Init();

    var isSupportVulkan = GLFW.VulkanSupported() == 1;
    if (isSupportVulkan)
    {
        GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
    }
    else
    {
        GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 1);
        GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_OPENGL_ES_API);
    }

    GLFW.WindowHint(GLFW.GLFW_RESIZABLE, 1);
    GLFW.SwapInterval(1); // V-sync On

    var window = GLFW.CreateWindow(800, 450, "Vertex.NET.Impeller Example", null, null);
    if (window.IsNull)
    {
        GLFW.Terminate();
        throw new Exception("Failed to create GLFW window");
    }

    int fbWidth = 0, fbHeight = 0;
    GLFW.GetFramebufferSize(window, ref fbWidth, ref fbHeight);
    GLFW.MakeContextCurrent(window);

    var context = isSupportVulkan
        ? Impeller.ContextCreateVulkanNew(
            Impeller.GetVersion(),
            new ImpellerContextVulkanSettings(procAddressCallback: &VulkanProcCallback))
        : Impeller.ContextCreateOpenGLESNew(
            Impeller.GetVersion(),
            GLProcCallback,
            null);
    if (context.IsNull) throw new Exception("Failed to create Impeller context");

    var surface = context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888, new ImpellerISize
    {
        Width = fbWidth,
        Height = fbHeight
    });

    var displayList = BuildDisplayList();
    while (GLFW.WindowShouldClose(window) == 0)
    {
        GLFW.WaitEvents();
        Impeller.SurfaceDrawDisplayList(surface, displayList);
        GLFW.SwapBuffers(window);
    }

    Impeller.DisplayListRelease(displayList);
    Impeller.SurfaceRelease(surface);
    Impeller.ContextRelease(context);

    GLFW.DestroyWindow(window);
    GLFW.Terminate();
    return;

    // ReSharper disable once InconsistentNaming
    static void* GLProcCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(Marshal.PtrToStringAnsi((IntPtr)procName));

    static void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetProcAddress(Marshal.PtrToStringAnsi((IntPtr)procName));

    ImpellerDisplayList BuildDisplayList()
    {
        var builder = Impeller.DisplayListBuilderNew(null);
        var paint = Impeller.PaintNew();

        Impeller.PaintSetColor(paint, new ImpellerColor { Alpha = 1 });
        Impeller.DisplayListBuilderDrawPaint(builder, paint);

        var pathBuilder = Impeller.PathBuilderNew();
        const float cx = 400f;
        const float cy = 225f;
        const float size = 200f;
        Impeller.PathBuilderMoveTo(pathBuilder, new ImpellerPoint { X = cx, Y = cy - size });
        Impeller.PathBuilderLineTo(pathBuilder, new ImpellerPoint { X = cx - size * 0.866f, Y = cy + size * 0.5f });
        Impeller.PathBuilderLineTo(pathBuilder, new ImpellerPoint { X = cx + size * 0.866f, Y = cy + size * 0.5f });
        Impeller.PathBuilderClose(pathBuilder);
        var trianglePath = Impeller.PathBuilderTakePathNew(pathBuilder, ImpellerFillType.FillTypeNonZero);

        var start = new ImpellerPoint { X = cx, Y = cy - size };
        var end = new ImpellerPoint { X = cx, Y = cy + size * 0.5f };
        var colors = stackalloc ImpellerColor[2]
        {
            new ImpellerColor { Red = 1, Alpha = 1 },
            new ImpellerColor { Blue = 1, Alpha = 1 }
        };
        var stops = stackalloc float[2] { 0.0f, 1.0f };
        var gradient = Impeller.ColorSourceCreateLinearGradientNew(
            start, end, 2, colors, stops, ImpellerTileMode.TileModeClamp, null);

        Impeller.PaintSetColorSource(paint, gradient);
        Impeller.DisplayListBuilderDrawPath(builder, trianglePath, paint);

        var displayListNew = Impeller.DisplayListBuilderCreateDisplayListNew(builder);
        Impeller.DisplayListBuilderRelease(builder);
        Impeller.PaintRelease(paint);
        Impeller.PathBuilderRelease(pathBuilder);
        Impeller.PathRelease(trianglePath);
        Impeller.ColorSourceRelease(gradient);
        return displayListNew;
    }
}