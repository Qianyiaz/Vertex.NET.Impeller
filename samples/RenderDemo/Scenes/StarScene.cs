using System.Diagnostics;
using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public class StarScene : IScene
{
    private readonly Stopwatch _st = Stopwatch.StartNew();

    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        var time = _st.Elapsed.TotalSeconds;

        using var bgPaint = Impeller.PaintNew();
        bgPaint.SetColor(new ImpellerColor { Red = 0.05f, Green = 0.05f, Blue = 0.10f, Alpha = 1 });
        builder.DrawPaint(bgPaint);

        var centerX = parameters.Width / 2f;
        var centerY = parameters.Height / 2f;
        var outerRadius = MathF.Min(parameters.Width, parameters.Height) * 0.35f;
        var innerRadius = outerRadius * 0.4f;
        const int points = 5;

        using var path = Impeller.PathBuilderNew();
        for (var i = 0; i < points * 2; i++)
        {
            var radius = i % 2 == 0 ? outerRadius : innerRadius;

            var angle = (float)(i * Math.PI / points) - (float)(Math.PI / 2) + (float)time * 0.8f;
            var x = centerX + radius * MathF.Cos(angle);
            var y = centerY + radius * MathF.Sin(angle);
            if (i == 0)
                path.MoveTo(new ImpellerPoint { X = x, Y = y });
            else
                path.LineTo(new ImpellerPoint { X = x, Y = y });
        }

        path.Close();
        using var star = path.TakePathNew(ImpellerFillType.FillTypeNonZero);

        var colors = stackalloc ImpellerColor[3]
        {
            new ImpellerColor { Red = 1.0f, Green = 0.4f, Blue = 0.0f, Alpha = 1 }, // Orange
            new ImpellerColor { Red = 1.0f, Green = 1.0f, Blue = 0.0f, Alpha = 1 }, // Yellow
            new ImpellerColor { Red = 1.0f, Green = 0.0f, Blue = 0.5f, Alpha = 1 } // Pink-Purple
        };
        var stops = stackalloc float[3] { 0.0f, 0.5f, 1.0f };
        using var gradient = Impeller.ColorSourceCreateLinearGradientNew(
            new ImpellerPoint { X = centerX - outerRadius, Y = centerY - outerRadius },
            new ImpellerPoint { X = centerX + outerRadius, Y = centerY + outerRadius },
            3, colors, stops, ImpellerTileMode.TileModeClamp, null
        );

        using var paint = Impeller.PaintNew();
        paint.SetColorSource(gradient);

        builder.DrawPath(star, paint);
    }
}