using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace Example;

internal static class Program
{
    [STAThread]
    public static unsafe void Main()
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

        var window = GLFW.CreateWindow(800, 450, "Vertex.NET.Impeller Example", null, null);
        if (window.IsNull)
        {
            GLFW.Terminate();
            throw new Exception("Failed to create GLFW window");
        }

        int width = 0, height = 0;
        GLFW.GetFramebufferSize(window, ref width, ref height);
        GLFW.MakeContextCurrent(window);
        GLFW.WindowHint(GLFW.GLFW_RESIZABLE, 1);
        GLFW.SwapInterval(1); // V-sync On

        var context = isSupportVulkan
            ? Impeller.ContextCreateVulkanNew(Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(procAddressCallback: &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(Impeller.GetVersion(),
                (procName, _) => GLFW.GetProcAddress(procName),
                null);

        if (context.IsNull) throw new Exception("Failed to create Impeller context");

        var surface = context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
            new ImpellerISize { Width = width, Height = height });
        var displayList = BuildDisplayList(width, height);

        GLFW.SetFramebufferSizeCallback(window, (_, w, h) =>
        {
            displayList.Release();
            surface.Release();

            surface = context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
                new ImpellerISize { Width = w, Height = h });
            displayList = BuildDisplayList(w, h);
        });
        GLFW.SetWindowRefreshCallback(window, _ =>
        {
            surface.DrawDisplayList(displayList);
            GLFW.SwapBuffers(window);
        });

        while (GLFW.WindowShouldClose(window) == 0)
            GLFW.WaitEvents();

        displayList.Release();
        surface.Release();
        context.Release();

        GLFW.DestroyWindow(window);
        GLFW.Terminate();
    }

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetProcAddress(procName);

    private static unsafe ImpellerDisplayList BuildDisplayList(int width, int height)
    {
        var builder = Impeller.DisplayListBuilderNew(null);
        var paint = Impeller.PaintNew();

        paint.SetColor(new ImpellerColor { Red = 1, Green = 1, Blue = 1, Alpha = 1 });
        builder.DrawPaint(paint);

        var path = Impeller.PathBuilderNew();
        const float size = 200f;
        path.MoveTo(new ImpellerPoint { Y = -size });
        path.LineTo(new ImpellerPoint { X = -size, Y = size / 2f });
        path.LineTo(new ImpellerPoint { X = size, Y = size / 2f });
        path.Close();

        var trianglePath = path.TakePathNew(ImpellerFillType.FillTypeNonZero);
        path.Release();

        var colors = stackalloc ImpellerColor[2]
        {
            new ImpellerColor { Red = 1, Alpha = 1 },
            new ImpellerColor { Blue = 1, Alpha = 1 }
        };
        var stops = stackalloc float[2] { 0.0f, 1.0f };

        var gradient = Impeller.ColorSourceCreateLinearGradientNew(new ImpellerPoint { Y = -size },
            new ImpellerPoint { Y = size / 2f }, 2, colors, stops, ImpellerTileMode.TileModeClamp, null);
        paint.SetColorSource(gradient);

        builder.Translate(width / 2f, height / 2f);
        builder.DrawPath(trianglePath, paint);

        var displayListNew = builder.CreateDisplayListNew();

        builder.Release();
        paint.Release();
        trianglePath.Release();
        gradient.Release();

        return displayListNew;
    }
}