using System.Runtime.InteropServices;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

unsafe
{
    if (GLFW.Init() == 0) throw new Exception("Failed to initialize GLFW");
    if (GLFW.GetPlatform() == GLFW.GLFW_PLATFORM_COCOA)
    {
        GLFW.Terminate();
        throw new Exception("OpenGL(ES) is not available on macOS. Please use Metal or Vulkan instead.");
    }

    GLFW.WindowHint(GLFW.GLFW_RESIZABLE, 1);
    GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
    GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 1);
    GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_OPENGL_ES_API);
    GLFW.SwapInterval(1); // V-sync On

    var window = GLFW.CreateWindow(800, 450, "Vertex.NET.Impeller Example (OpenGL)", null, null);
    if (window.IsNull)
    {
        GLFW.Terminate();
        throw new Exception("Failed to create GLFW window");
    }

    int fbWidth = 0, fbHeight = 0;
    GLFW.GetFramebufferSize(window, ref fbWidth, ref fbHeight);
    GLFW.MakeContextCurrent(window);

    var context = Impeller.ImpellerContextCreateOpenGLESNew(Impeller.ImpellerGetVersion(), ProcCallback, null);
    if (context.IsNull) throw new Exception("Failed to create Impeller context");

    var surface = Impeller.ImpellerSurfaceCreateWrappedFBONew(context, 0u,
        ImpellerPixelFormat.KImpellerPixelFormatRgba8888, new ImpellerISize
        {
            Width = fbWidth,
            Height = fbHeight
        });

    var displayList = BuildDisplayList();
    while (GLFW.WindowShouldClose(window) == 0)
    {
        GLFW.WaitEvents();
        Impeller.ImpellerSurfaceDrawDisplayList(surface, displayList);
        GLFW.SwapBuffers(window);
    }

    Impeller.ImpellerDisplayListRelease(displayList);
    Impeller.ImpellerSurfaceRelease(surface);
    Impeller.ImpellerContextRelease(context);

    GLFW.DestroyWindow(window);
    GLFW.Terminate();
    return;

    void* ProcCallback(byte* procName, void* _) => GLFW.GetProcAddress(Marshal.PtrToStringAnsi((IntPtr)procName));

    ImpellerDisplayList BuildDisplayList()
    {
        var builder = Impeller.ImpellerDisplayListBuilderNew(null);
        var paint = Impeller.ImpellerPaintNew();
        Impeller.ImpellerPaintSetColor(paint, new ImpellerColor { Alpha = 1 });
        Impeller.ImpellerDisplayListBuilderDrawPaint(builder, paint);

        var pathBuilder = Impeller.ImpellerPathBuilderNew();
        var top = new ImpellerPoint { X = 60, Y = 10 };
        Impeller.ImpellerPathBuilderMoveTo(pathBuilder, top);
        Impeller.ImpellerPathBuilderLineTo(pathBuilder, new ImpellerPoint { X = 110, Y = 110 });
        Impeller.ImpellerPathBuilderLineTo(pathBuilder, new ImpellerPoint { X = 10, Y = 110 });
        Impeller.ImpellerPathBuilderClose(pathBuilder);

        var trianglePath =
            Impeller.ImpellerPathBuilderTakePathNew(pathBuilder, ImpellerFillType.KImpellerFillTypeNonZero);

        var colors = stackalloc ImpellerColor[2]
        {
            new ImpellerColor { Red = 1, Alpha = 1 },
            new ImpellerColor { Blue = 1, Alpha = 1 }
        };
        var stops = stackalloc float[2] { 0.0f, 1.0f };

        var gradient = Impeller.ImpellerColorSourceCreateLinearGradientNew(top, new ImpellerPoint { X = 60, Y = 110 },
            2, colors, stops, ImpellerTileMode.KImpellerTileModeClamp, null);

        Impeller.ImpellerPaintSetColorSource(paint, gradient);
        Impeller.ImpellerDisplayListBuilderDrawPath(builder, trianglePath, paint);

        var displayListNew = Impeller.ImpellerDisplayListBuilderCreateDisplayListNew(builder);

        Impeller.ImpellerDisplayListBuilderRelease(builder);
        Impeller.ImpellerPaintRelease(paint);
        Impeller.ImpellerPathBuilderRelease(pathBuilder);
        Impeller.ImpellerPathRelease(trianglePath);
        Impeller.ImpellerColorSourceRelease(gradient);

        return displayListNew;
    }
}