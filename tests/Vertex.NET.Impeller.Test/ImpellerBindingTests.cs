using System.Runtime.InteropServices;

namespace Vertex.NET.Impeller.Test;

public class ImpellerBindingTests
{
    [Fact]
    public void GetVersion_ShouldReturnPositiveVersion()
    {
        var version = Impeller.GetVersion();
        Assert.True(version > 0, "Version should be greater than 0");
    }

    [Fact(Skip = "This machine does not support Vulkan")]
    public unsafe void ContextCreateVulkanNew_AndDispose_ShouldNotThrow()
    {
        var settings = new ImpellerContextVulkanSettings(null, &Callback);
        using var context = Impeller.ContextCreateVulkanNew(Impeller.GetVersion(), settings);

        Assert.NotEqual(ImpellerContext.Null, context);
        return;

        static void* Callback(void* userData, byte* procName, void* reserved) => null;
    }

    [Fact]
    public unsafe void ContextCreateOpenGLESNew_WithDelegateCallback_ShouldSucceed()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var version = Impeller.GetVersion();
        using var context = Impeller.ContextCreateOpenGLESNew(version, Callback, IntPtr.Zero);

        Assert.NotEqual(ImpellerContext.Null, context);
        return;

        static void* Callback(byte* name, void* userData) => null;
    }

    [Fact]
    public void ColorFilterCreateBlendNew_ShouldReturnNonNull()
    {
        var color = new ImpellerColor { Red = 1.0f, Green = 0, Blue = 0, Alpha = 1.0f };
        using var filter = Impeller.ColorFilterCreateBlendNew(color, ImpellerBlendMode.BlendModeSourceOver);
        Assert.NotEqual(ImpellerColorFilter.Null, filter);
    }

    [Fact]
    public void PaintNew_AndDispose_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        Assert.False(paint.IsNull);
    }

    [Fact]
    public void Paint_SetColorAndBlendMode_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        var color = new ImpellerColor { Red = 1, Green = 0, Blue = 0, Alpha = 1 };
        paint.SetColor(color);
        paint.SetBlendMode(ImpellerBlendMode.BlendModeMultiply);
    }

    [Fact]
    public void MaskFilterCreateBlur_ShouldReturnNonNull()
    {
        using var filter = Impeller.MaskFilterCreateBlurNew(ImpellerBlurStyle.BlurStyleNormal, 2.0f);
        Assert.NotEqual(ImpellerMaskFilter.Null, filter);
    }

    [Fact]
    public void ImageFilterCreateBlur_ShouldReturnNonNull()
    {
        using var filter = Impeller.ImageFilterCreateBlurNew(1.0f, 1.0f, ImpellerTileMode.TileModeClamp);
        Assert.NotEqual(ImpellerImageFilter.Null, filter);
    }

    [Theory]
    [InlineData(ImpellerBlendMode.BlendModeClear, 0)]
    [InlineData(ImpellerBlendMode.BlendModeSource, 1)]
    [InlineData(ImpellerBlendMode.BlendModeSourceOver, 3)]
    public void ImpellerBlendMode_Values_ShouldMatch(ImpellerBlendMode mode, int expected)
        => Assert.Equal(expected, (int)mode);
}