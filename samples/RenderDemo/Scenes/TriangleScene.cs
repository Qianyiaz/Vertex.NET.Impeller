using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public sealed class TriangleScene : IScene
{
    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        using var paint = Impeller.PaintNew();
        paint.SetColor(new ImpellerColor { Red = 1, Green = 1, Blue = 1, Alpha = 1 });
        builder.DrawPaint(paint);

        var size = MathF.Min(parameters.Width, parameters.Height) * 0.5f;
        using var path = Impeller.PathBuilderNew();
        path.MoveTo(new ImpellerPoint(y: -size));
        path.LineTo(new ImpellerPoint(-size, size / 2));
        path.LineTo(new ImpellerPoint(size, size / 2));
        path.Close();

        using var triangle = path.TakePathNew(ImpellerFillType.FillTypeNonZero);

        var colors = stackalloc ImpellerColor[2]
        {
            new() { Red = 1, Alpha = 1 },
            new() { Blue = 1, Alpha = 1 }
        };
        var stops = stackalloc float[2] { 0.0f, 1.0f };
        using var gradient = Impeller.ColorSourceCreateLinearGradientNew(
            new ImpellerPoint(y: -size),
            new ImpellerPoint(y: size / 2),
            2, colors, stops, ImpellerTileMode.TileModeClamp, null
        );
        paint.SetColorSource(gradient);

        builder.Save();
        builder.Translate(parameters.Width / 2f, parameters.Height / 2f);
        builder.DrawPath(triangle, paint);
        builder.Restore();
    }
}