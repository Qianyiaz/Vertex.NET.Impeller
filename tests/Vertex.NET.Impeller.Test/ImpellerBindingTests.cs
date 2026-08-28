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

    [Fact]
    public void ImpellerConstants_VersionParts_ShouldBeReasonable()
    {
        // These constants are generated from the native headers.
        Assert.True(Impeller.IMPELLER_VERSION_MAJOR >= 1);
        Assert.True(Impeller.IMPELLER_VERSION_MINOR >= 0);
        Assert.True(Impeller.IMPELLER_VERSION_PATCH >= 0);
    }

    [Fact]
    public void GetLibraryName_ShouldContainImpeller()
    {
        var name = Impeller.GetLibraryName();
        Assert.False(string.IsNullOrWhiteSpace(name));
        Assert.Contains("impeller", name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ImpellerColor_Defaults_ShouldBeZero()
    {
        var c = new ImpellerColor();
        Assert.Equal(0f, c.Red);
        Assert.Equal(0f, c.Green);
        Assert.Equal(0f, c.Blue);
        Assert.Equal(0f, c.Alpha);
    }

    [Fact]
    public void PathBuilderNew_ShouldReturnNonNull()
    {
        using var builder = Impeller.PathBuilderNew();
        Assert.False(builder.IsNull);
    }

    [Fact]
    public void Paint_SetStrokeProperties_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        paint.SetStrokeWidth(5.0f);
        paint.SetStrokeCap(ImpellerStrokeCap.StrokeCapRound);
        paint.SetStrokeJoin(ImpellerStrokeJoin.StrokeJoinBevel);
        // No native assertion here; just ensure calls don't throw.
    }

    [Theory]
    [InlineData(ImpellerTileMode.TileModeClamp, 0)]
    [InlineData(ImpellerTileMode.TileModeRepeat, 1)]
    [InlineData(ImpellerTileMode.TileModeMirror, 2)]
    [InlineData(ImpellerTileMode.TileModeDecal, 3)]
    public void ImpellerTileMode_Values_ShouldMatch(ImpellerTileMode mode, int expected)
        => Assert.Equal(expected, (int)mode);

    [Fact]
    public void ImageFilterCreateBlur_LargeRadius_ShouldReturnNonNull()
    {
        using var filter = Impeller.ImageFilterCreateBlurNew(50.0f, 50.0f, ImpellerTileMode.TileModeClamp);
        Assert.NotEqual(ImpellerImageFilter.Null, filter);
    }

    [Theory]
    [InlineData(ImpellerBlendMode.BlendModeClear, 0)]
    [InlineData(ImpellerBlendMode.BlendModeSource, 1)]
    [InlineData(ImpellerBlendMode.BlendModeSourceOver, 3)]
    public void ImpellerBlendMode_Values_ShouldMatch(ImpellerBlendMode mode, int expected)
        => Assert.Equal(expected, (int)mode);
}