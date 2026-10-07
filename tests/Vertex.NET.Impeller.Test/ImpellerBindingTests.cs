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
    
     [Fact]
    public void ContextCreateMetalNew_ShouldSucceedOnMac()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var version = Impeller.GetVersion();
        using var context = Impeller.ContextCreateMetalNew(version);
        Assert.NotEqual(ImpellerContext.Null, context);
    }

    [Fact(Skip = "Requires Vulkan and valid instance")]
    public unsafe void ContextGetVulkanInfo_ShouldPopulateStructure()
    {
        var version = Impeller.GetVersion();
        var settings = new ImpellerContextVulkanSettings(null, null);
        using var context = Impeller.ContextCreateVulkanNew(version, settings);
        Assert.NotEqual(ImpellerContext.Null, context);

        ImpellerContextVulkanInfo info = default;
        var success = Impeller.ContextGetVulkanInfo(context, ref info);
        Assert.True(success);
        Assert.NotEqual(IntPtr.Zero, (IntPtr)info.VkInstance);
    }

    [Fact]
    public void PathBuilder_BuildPath_ShouldReturnValidPath()
    {
        using var builder = Impeller.PathBuilderNew();
        var point1 = new ImpellerPoint(10, 20);
        var point2 = new ImpellerPoint(30, 40);
        builder.MoveTo(point1);
        builder.LineTo(point2);
        var rect = new ImpellerRect(0, 0, 100, 100);
        builder.AddRect(rect);

        using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        Assert.NotEqual(ImpellerPath.Null, path);

        ImpellerRect bounds = default;
        Impeller.PathGetBounds(path, ref bounds);
        Assert.True(bounds.Width > 0);
    }

    [Theory]
    [InlineData(ImpellerDrawStyle.DrawStyleFill)]
    [InlineData(ImpellerDrawStyle.DrawStyleStroke)]
    [InlineData(ImpellerDrawStyle.DrawStyleStrokeAndFill)]
    public void Paint_SetDrawStyle_ShouldNotThrow(ImpellerDrawStyle style)
    {
        using var paint = Impeller.PaintNew();
        paint.SetDrawStyle(style);
    }

    [Fact]
    public void DisplayListBuilder_BasicOperations_ShouldNotThrow()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
        builder.Save();
        builder.Translate(10, 20);
        builder.Rotate(45);
        builder.Scale(1.5f, 1.5f);

        using var paint = Impeller.PaintNew();
        var rect = new ImpellerRect(0, 0, 100, 100);
        builder.DrawRect(rect, paint);
        builder.Restore();

        using var displayList = Impeller.DisplayListBuilderCreateDisplayListNew(builder);
        Assert.NotEqual(ImpellerDisplayList.Null, displayList);
    }

    [Fact]
    public void DisplayListBuilder_ClipOperations_ShouldNotThrow()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
        var rect = new ImpellerRect(0, 0, 200, 200);
        builder.ClipRect(rect, ImpellerClipOperation.ClipOperationIntersect);
        
        using var paint = Impeller.PaintNew();
        builder.DrawPaint(paint);
        builder.Restore();
    }
    
    [Fact]
    public unsafe void TextureCreateWithContents_ShouldSucceed()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var version = Impeller.GetVersion();
        using var context = Impeller.ContextCreateOpenGLESNew(version, (delegate*<byte*, void*, void*>)null, IntPtr.Zero);
        Assert.NotEqual(ImpellerContext.Null, context);

        var descriptor = new ImpellerTextureDescriptor
        {
            PixelFormat = ImpellerPixelFormat.PixelFormatRgba8888,
            Size = new ImpellerISize { Width = 64, Height = 64 },
            MipCount = 1
        };

        var data = new byte[64 * 64 * 4];
        fixed (byte* pData = data)
        {
            var mapping = new ImpellerMapping
            {
                Data = pData,
                Length = (ulong)data.Length,
                OnRelease = null
            };
            using var texture = Impeller.TextureCreateWithContentsNew(context, descriptor, mapping, IntPtr.Zero);
            Assert.NotEqual(ImpellerTexture.Null, texture);
        }
    }
    
    [Fact]
    public unsafe void ParagraphBuilder_BuildParagraph_ShouldReturnNonNull()
    {
        using var typoContext = Impeller.TypographyContextNew();
        Assert.NotEqual(ImpellerTypographyContext.Null, typoContext);

        using var paragraphBuilder = Impeller.ParagraphBuilderNew(typoContext);
        Assert.NotEqual(ImpellerParagraphBuilder.Null, paragraphBuilder);

        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(16);
        paragraphBuilder.PushStyle(style);
        var text = "Hello"u8;
        fixed (byte* pText = text)
            paragraphBuilder.AddText(pText, 5);
        
        paragraphBuilder.PopStyle();
        
        using var paragraph = Impeller.ParagraphBuilderBuildParagraphNew(paragraphBuilder, 200);
        Assert.NotEqual(ImpellerParagraph.Null, paragraph);

        var height = Impeller.ParagraphGetHeight(paragraph);
        Assert.True(height > 0);
    }

    [Theory]
    [InlineData(ImpellerPixelFormat.PixelFormatRgba8888, 0)]
    public void ImpellerPixelFormat_Values_ShouldMatch(ImpellerPixelFormat format, int expected)
        => Assert.Equal(expected, (int)format);

    [Theory]
    [InlineData(ImpellerTextAlignment.TextAlignmentLeft, 0)]
    [InlineData(ImpellerTextAlignment.TextAlignmentRight, 1)]
    [InlineData(ImpellerTextAlignment.TextAlignmentCenter, 2)]
    public void ImpellerTextAlignment_Values_ShouldMatch(ImpellerTextAlignment align, int expected)
        => Assert.Equal(expected, (int)align);

    [Fact]
    public void ManyPaints_ShouldNotLeakMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        const int count = 1000;
        for (var i = 0; i < count; i++)
        {
            using var paint = Impeller.PaintNew();
            paint.SetColor(new ImpellerColor { Red = 0.5f, Green = 0.5f, Blue = 0.5f, Alpha = 1 });
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);

        var diff = finalMemory - initialMemory;
        Assert.True(diff < 1024 * 1024, $"Memory grew by {diff} bytes");
    }

    [Theory]
    [InlineData(ImpellerBlendMode.BlendModeClear, 0)]
    [InlineData(ImpellerBlendMode.BlendModeSource, 1)]
    [InlineData(ImpellerBlendMode.BlendModeSourceOver, 3)]
    public void ImpellerBlendMode_Values_ShouldMatch(ImpellerBlendMode mode, int expected)
        => Assert.Equal(expected, (int)mode);

    [Fact]
    public void Paint_SetStrokeMiter_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        paint.SetStrokeMiter(4.0f);
    }

    [Fact]
    public void Paint_SetColorFilter_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        var color = new ImpellerColor { Red = 1f, Green = 0, Blue = 0, Alpha = 1f };
        using var filter = Impeller.ColorFilterCreateBlendNew(color, ImpellerBlendMode.BlendModeSourceOver);
        paint.SetColorFilter(filter);
    }

    [Fact]
    public void Paint_SetMaskFilter_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        using var filter = Impeller.MaskFilterCreateBlurNew(ImpellerBlurStyle.BlurStyleNormal, 2.0f);
        paint.SetMaskFilter(filter);
    }

    [Fact]
    public void Paint_SetImageFilter_ShouldNotThrow()
    {
        using var paint = Impeller.PaintNew();
        using var filter = Impeller.ImageFilterCreateBlurNew(1.0f, 1.0f, ImpellerTileMode.TileModeClamp);
        paint.SetImageFilter(filter);
    }
    
    [Fact]
    public void ColorFilterCreateColorMatrixNew_ShouldReturnNonNull()
    {
        var matrix = default(ImpellerColorMatrix);
        using var filter = Impeller.ColorFilterCreateColorMatrixNew(matrix);
        Assert.NotEqual(ImpellerColorFilter.Null, filter);
    }
    
    [Fact]
    public void ImageFilterCreateDilateNew_ShouldReturnNonNull()
    {
        using var filter = Impeller.ImageFilterCreateDilateNew(1.0f, 1.0f);
        Assert.NotEqual(ImpellerImageFilter.Null, filter);
    }

    [Fact]
    public void ImageFilterCreateErodeNew_ShouldReturnNonNull()
    {
        using var filter = Impeller.ImageFilterCreateErodeNew(1.0f, 1.0f);
        Assert.NotEqual(ImpellerImageFilter.Null, filter);
    }

    [Fact]
    public void ImageFilterCreateMatrixNew_ShouldReturnNonNull()
    {
        var matrix = default(ImpellerMatrix);
        const ImpellerTextureSampling sampling = default;
        using var filter = Impeller.ImageFilterCreateMatrixNew(matrix, sampling);
        Assert.NotEqual(ImpellerImageFilter.Null, filter);
    }

    [Fact]
    public void ImageFilterCreateComposeNew_ShouldReturnNonNull()
    {
        using var outer = Impeller.ImageFilterCreateBlurNew(1.0f, 1.0f, ImpellerTileMode.TileModeClamp);
        using var inner = Impeller.ImageFilterCreateDilateNew(1.0f, 1.0f);
        using var composed = Impeller.ImageFilterCreateComposeNew(outer, inner);
        Assert.NotEqual(ImpellerImageFilter.Null, composed);
    }
    
    [Theory]
    [InlineData(ImpellerBlurStyle.BlurStyleNormal)]
    [InlineData(ImpellerBlurStyle.BlurStyleSolid)]
    [InlineData(ImpellerBlurStyle.BlurStyleOuter)]
    [InlineData(ImpellerBlurStyle.BlurStyleInner)]
    public void MaskFilterCreateBlur_AllStyles_ShouldReturnNonNull(ImpellerBlurStyle style)
    {
        using var filter = Impeller.MaskFilterCreateBlurNew(style, 3.0f);
        Assert.NotEqual(ImpellerMaskFilter.Null, filter);
    }
    
    [Fact]
    public void PathBuilder_CubicAndQuad_ShouldNotThrow()
    {
        using var builder = Impeller.PathBuilderNew();
        builder.MoveTo(new ImpellerPoint(0, 0));
        builder.CubicCurveTo(new ImpellerPoint(10), new ImpellerPoint(20, 10), new ImpellerPoint(30, 30));
        builder.QuadraticCurveTo(new ImpellerPoint(40, 40), new ImpellerPoint(50, 50));
        builder.Close();

        using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        Assert.NotEqual(ImpellerPath.Null, path);
    }

    [Fact]
    public void PathBuilder_AddOval_ShouldReturnPathWithBounds()
    {
        using var builder = Impeller.PathBuilderNew();
        builder.AddOval(new ImpellerRect(0, 0, 100, 50));

        using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        Assert.NotEqual(ImpellerPath.Null, path);

        ImpellerRect bounds = default;
        Impeller.PathGetBounds(path, ref bounds);
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void PathBuilder_AddRoundedRect_ShouldReturnPathWithBounds()
    {
        using var builder = Impeller.PathBuilderNew();
        var rect = new ImpellerRect(0, 0, 100, 100);
        var radii = default(ImpellerRoundingRadii);
        builder.AddRoundedRect(rect, radii);

        using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        Assert.NotEqual(ImpellerPath.Null, path);

        ImpellerRect bounds = default;
        Impeller.PathGetBounds(path, ref bounds);
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void PathBuilder_AddArc_ShouldReturnPathWithBounds()
    {
        using var builder = Impeller.PathBuilderNew();
        builder.AddArc(new ImpellerRect(0, 0, 100, 100), 0, 180);

        using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        Assert.NotEqual(ImpellerPath.Null, path);
    }

    [Fact]
    public void DisplayListBuilder_DrawOval_ShouldNotThrow()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
        using var paint = Impeller.PaintNew();
        builder.DrawOval(new ImpellerRect(25, 25, 75, 75), paint);

        using var displayList = Impeller.DisplayListBuilderCreateDisplayListNew(builder);
        Assert.NotEqual(ImpellerDisplayList.Null, displayList);
    }

    [Fact]
    public void DisplayListBuilder_DrawPath_ShouldNotThrow()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
        using var paint = Impeller.PaintNew();

        using var pathBuilder = Impeller.PathBuilderNew();
        pathBuilder.AddRect(new ImpellerRect(0, 0, 100, 100));
        using var path = pathBuilder.CopyPathNew(ImpellerFillType.FillTypeNonZero);

        builder.DrawPath(path, paint);

        using var displayList = Impeller.DisplayListBuilderCreateDisplayListNew(builder);
        Assert.NotEqual(ImpellerDisplayList.Null, displayList);
    }
    
    [Fact]
    public void DisplayListBuilder_TranslateRotateScale_ShouldNotThrow()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
        builder.Translate(1, 2);
        builder.Rotate(90);
        builder.Scale(2, 2);
        builder.Save();
        builder.Restore();

        using var displayList = Impeller.DisplayListBuilderCreateDisplayListNew(builder);
        Assert.NotEqual(ImpellerDisplayList.Null, displayList);
    }

    [Fact]
    public void ParagraphStyle_SetFontWeightAndAlign_ShouldNotThrow()
    {
        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(14);
        style.SetTextAlignment(ImpellerTextAlignment.TextAlignmentCenter);
    }

    [Fact]
    public unsafe void ParagraphBuilder_MultipleTextRuns_ShouldBuildParagraph()
    {
        using var typoContext = Impeller.TypographyContextNew();
        using var paragraphBuilder = Impeller.ParagraphBuilderNew(typoContext);

        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(16);
        paragraphBuilder.PushStyle(style);

        var t1 = "Hello "u8;
        var t2 = "World"u8;
        fixed (byte* p1 = t1) paragraphBuilder.AddText(p1, 6);
        fixed (byte* p2 = t2) paragraphBuilder.AddText(p2, 5);

        paragraphBuilder.PopStyle();

        using var paragraph = Impeller.ParagraphBuilderBuildParagraphNew(paragraphBuilder, 300);
        Assert.NotEqual(ImpellerParagraph.Null, paragraph);

        var height = Impeller.ParagraphGetHeight(paragraph);
        Assert.True(height > 0);
    }

    [Fact]
    public unsafe void Paragraph_GetMaxWidth_ShouldBePositiveAfterBuild()
    {
        using var typoContext = Impeller.TypographyContextNew();
        using var paragraphBuilder = Impeller.ParagraphBuilderNew(typoContext);

        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(12);
        paragraphBuilder.PushStyle(style);

        var text = "Sample"u8;
        fixed (byte* pText = text)
            paragraphBuilder.AddText(pText, 6);
        paragraphBuilder.PopStyle();

        using var paragraph = Impeller.ParagraphBuilderBuildParagraphNew(paragraphBuilder, 200);
        var width = Impeller.ParagraphGetMaxWidth(paragraph);
        Assert.True(width >= 0);
    }

    [Theory]
    [InlineData(ImpellerBlurStyle.BlurStyleNormal, 0)]
    [InlineData(ImpellerBlurStyle.BlurStyleSolid, 1)]
    [InlineData(ImpellerBlurStyle.BlurStyleOuter, 2)]
    [InlineData(ImpellerBlurStyle.BlurStyleInner, 3)]
    public void ImpellerBlurStyle_Values_ShouldMatch(ImpellerBlurStyle style, int expected)
        => Assert.Equal(expected, (int)style);

    [Theory]
    [InlineData(ImpellerStrokeCap.StrokeCapButt, 0)]
    [InlineData(ImpellerStrokeCap.StrokeCapRound, 1)]
    [InlineData(ImpellerStrokeCap.StrokeCapSquare, 2)]
    public void ImpellerStrokeCap_Values_ShouldMatch(ImpellerStrokeCap cap, int expected)
        => Assert.Equal(expected, (int)cap);

    [Theory]
    [InlineData(ImpellerStrokeJoin.StrokeJoinMiter, 0)]
    [InlineData(ImpellerStrokeJoin.StrokeJoinRound, 1)]
    [InlineData(ImpellerStrokeJoin.StrokeJoinBevel, 2)]
    public void ImpellerStrokeJoin_Values_ShouldMatch(ImpellerStrokeJoin join, int expected)
        => Assert.Equal(expected, (int)join);

    [Theory]
    [InlineData(ImpellerFillType.FillTypeNonZero, 0)]
    [InlineData(ImpellerFillType.FillTypeOdd, 1)]
    public void ImpellerFillType_Values_ShouldMatch(ImpellerFillType fillType, int expected)
        => Assert.Equal(expected, (int)fillType);

    [Theory]
    [InlineData(ImpellerClipOperation.ClipOperationDifference, 0)]
    [InlineData(ImpellerClipOperation.ClipOperationIntersect, 1)]
    public void ImpellerClipOperation_Values_ShouldMatch(ImpellerClipOperation op, int expected)
        => Assert.Equal(expected, (int)op);

    [Theory]
    [InlineData(ImpellerDrawStyle.DrawStyleFill, 0)]
    [InlineData(ImpellerDrawStyle.DrawStyleStroke, 1)]
    [InlineData(ImpellerDrawStyle.DrawStyleStrokeAndFill, 2)]
    public void ImpellerDrawStyle_Values_ShouldMatch(ImpellerDrawStyle style, int expected)
        => Assert.Equal(expected, (int)style);
    
    [Fact]
    public void Context_Dispose_IsIdempotent()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var version = Impeller.GetVersion();
        using var context = Impeller.ContextCreateMetalNew(version);
    }
    
    [Fact]
    public void Paint_Dispose_IsIdempotent()
    {
        using var paint = Impeller.PaintNew();
    }

    [Fact]
    public void PathBuilder_Dispose_IsIdempotent()
    {
        using var builder = Impeller.PathBuilderNew();
    }

    [Fact]
    public void DisplayListBuilder_Dispose_IsIdempotent()
    {
        using var builder = Impeller.DisplayListBuilderNew(null);
    }
    
    [Fact]
    public void ManyPaths_ShouldNotLeakMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        const int count = 500;
        for (var i = 0; i < count; i++)
        {
            using var builder = Impeller.PathBuilderNew();
            builder.AddRect(new ImpellerRect(0, 0, 10, 10));
            using var path = builder.CopyPathNew(ImpellerFillType.FillTypeNonZero);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);

        var diff = finalMemory - initialMemory;
        Assert.True(diff < 2 * 1024 * 1024, $"Memory grew by {diff} bytes");
    }

    [Fact]
    public void ManyColorFilters_ShouldNotLeakMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        var color = new ImpellerColor { Red = 1f, Green = 0, Blue = 0, Alpha = 1f };
        for (var i = 0; i < 500; i++)
        {
            using var filter = Impeller.ColorFilterCreateBlendNew(color, ImpellerBlendMode.BlendModeSourceOver);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);

        var diff = finalMemory - initialMemory;
        Assert.True(diff < 2 * 1024 * 1024, $"Memory grew by {diff} bytes");
    }
}