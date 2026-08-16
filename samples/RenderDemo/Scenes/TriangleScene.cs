using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public class TriangleScene : IScene
{
    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        builder.Translate(parameters.Width / 2f, parameters.Height / 2f);

        using var paint = Impeller.PaintNew();
        paint.SetColor(new ImpellerColor { Red = 1, Green = 1, Blue = 1, Alpha = 1 });
        builder.DrawPaint(paint);

        using var path = Impeller.PathBuilderNew();
        const float size = 200f;
        path.MoveTo(new ImpellerPoint { Y = -size });
        path.LineTo(new ImpellerPoint { X = -size, Y = size / 2f });
        path.LineTo(new ImpellerPoint { X = size, Y = size / 2f });
        using var triangle = path.TakePathNew(ImpellerFillType.FillTypeNonZero);

        var colors = stackalloc ImpellerColor[2]
        {
            new ImpellerColor { Red = 1, Alpha = 1 },
            new ImpellerColor { Blue = 1, Alpha = 1 }
        };
        var stops = stackalloc float[2] { 0.0f, 1.0f };
        using var gradient = Impeller.ColorSourceCreateLinearGradientNew(new ImpellerPoint { Y = -size },
            new ImpellerPoint { Y = size / 2f }, 2, colors, stops, ImpellerTileMode.TileModeClamp, null);
        paint.SetColorSource(gradient);

        builder.DrawPath(triangle, paint);
    }
}