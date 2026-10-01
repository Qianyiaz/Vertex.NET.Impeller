using System.Diagnostics;
using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public sealed class StarScene : IScene
{
    private readonly Stopwatch _st = Stopwatch.StartNew();

    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        const int points = 5;
        var outerR = MathF.Min(parameters.Width, parameters.Height) * 0.35f;
        var innerR = outerR * 0.4f;

        using var path = Impeller.PathBuilderNew();
        for (var i = 0; i < points * 2; i++)
        {
            var r = (i & 1) == 0 ? outerR : innerR;
            var a = i * MathF.PI / points - MathF.PI * 0.5f;
            var p = new ImpellerPoint(r * MathF.Cos(a), r * MathF.Sin(a));
            if (i == 0)
                path.MoveTo(p);
            else
                path.LineTo(p);
        }

        path.Close();
        using var star = path.TakePathNew(ImpellerFillType.FillTypeNonZero);

        var colors = stackalloc ImpellerColor[3]
        {
            new(1f, 0.4f, 0f, 1f),
            new(1f, 1f, 0f, 1f),
            new(1f, 0f, 0.5f, 1f)
        };
        var stops = stackalloc float[3] { 0f, 0.5f, 1f };
        using var gradient = Impeller.ColorSourceCreateLinearGradientNew(
            new ImpellerPoint(-outerR, -outerR),
            new ImpellerPoint(outerR, outerR),
            3, colors, stops, ImpellerTileMode.TileModeClamp, null);

        using var paint = Impeller.PaintNew();
        paint.SetColorSource(gradient);

        builder.Save();
        builder.Translate(parameters.Width * 0.5f, parameters.Height * 0.5f);
        builder.Rotate((float)(_st.Elapsed.TotalSeconds * 0.8 * (180.0 / Math.PI)));
        builder.DrawPath(star, paint);
        builder.Restore();
    }
}