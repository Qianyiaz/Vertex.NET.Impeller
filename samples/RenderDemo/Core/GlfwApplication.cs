using System.Diagnostics.CodeAnalysis;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace RenderDemo.Core;

public class GlfwApplication
{
    private readonly bool _isVulkanSupported = GLFW.VulkanSupported() == 1;
    private readonly GLFWwindowPtr _window;

    public GlfwApplication(int width, int height, string title = "Window")
    {
        if (GLFW.Init() == 0)
            throw new Exception("Failed to create GLFW window");

        if (_isVulkanSupported)
        {
            GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
        }
        else
        {
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 1);
            GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_OPENGL_ES_API);
        }

        _window = GLFW.CreateWindow(width, height, title, null, null);
        if (_window.IsNull)
        {
            GLFW.Terminate();
            throw new Exception("Failed to create GLFW window");
        }

        if (_isVulkanSupported) return;

        GLFW.MakeContextCurrent(_window);
        GLFW.SwapInterval(1); // V-sync On
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public unsafe void Run(IScene scene, bool isEventDriven = true)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene), "Scene is not set");

        using var context = _isVulkanSupported
            ? Impeller.ContextCreateVulkanNew(
                Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(null, &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(
                Impeller.GetVersion(),
                GLProcAddressCallback,
                null);

        ImpellerVulkanSwapchain swapChain = default;
        if (_isVulkanSupported)
        {
            ImpellerContextVulkanInfo vkInfo = default;
            if (!Impeller.ContextGetVulkanInfo(context, ref vkInfo))
                throw new Exception("Failed to get Vulkan info");

            VkSurfaceKHR surfaceKhr = default;
            GLFW.CreateWindowSurface(new VkInstance((nint)vkInfo.VkInstance), _window, 0, ref surfaceKhr);
            swapChain = Impeller.VulkanSwapchainCreateNew(context, surfaceKhr.Handle);
        }

        int width = 0, height = 0;
        GLFW.GetFramebufferSize(_window, ref width, ref height);
        var surface = _isVulkanSupported
            ? swapChain.AcquireNextSurfaceNew()
            : context.SurfaceCreateWrappedFboNew(
                0u, ImpellerPixelFormat.PixelFormatRgba8888, new ImpellerISize { Width = width, Height = height });

        var parameters = new SceneParameters(width, height);
        GLFW.SetFramebufferSizeCallback(_window, (_, w, h) =>
        {
            parameters = new SceneParameters(w, h);

            surface.Dispose();
            surface = _isVulkanSupported
                ? swapChain.AcquireNextSurfaceNew()
                : context.SurfaceCreateWrappedFboNew(
                    0u, ImpellerPixelFormat.PixelFormatRgba8888, new ImpellerISize { Width = w, Height = h });

            RenderFrame(surface, scene, parameters);
        });

        if (isEventDriven)
        {
            RenderFrame(surface, scene, parameters);
            while (GLFW.WindowShouldClose(_window) == 0) GLFW.WaitEvents();
        }
        else
        {
            while (GLFW.WindowShouldClose(_window) == 0)
            {
                GLFW.PollEvents();
                RenderFrame(surface, scene, parameters);
            }
        }

        swapChain.Dispose();
        surface.Dispose();
        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
    }

    private void RenderFrame(ImpellerSurface surface, IScene scene, SceneParameters parameters)
    {
        using var builder = Impeller.DisplayListBuilderNew(new ImpellerRect
            { Width = parameters.Width, Height = parameters.Height });

        scene.Render(builder, parameters);

        using var displayList = builder.CreateDisplayListNew();
        surface.DrawDisplayList(displayList);

        if (_isVulkanSupported)
            surface.Present();
        else
            GLFW.SwapBuffers(_window);
    }

    // ReSharper disable once InconsistentNaming
    private static unsafe void* GLProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetInstanceProcAddress(VkInstance.Null, procName);
}