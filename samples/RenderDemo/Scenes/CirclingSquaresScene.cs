using System.Diagnostics;
using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public sealed class CirclingSquaresScene : IScene
{
    private readonly Stopwatch _st = Stopwatch.StartNew();

    public void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        using var paint = Impeller.PaintNew();
        paint.SetColor(new ImpellerColor(1, alpha: 1));

        var centerX = parameters.Width * 0.5f;
        var centerY = parameters.Height * 0.5f;
        var orbitRadius = MathF.Min(parameters.Width, parameters.Height) * 0.3f;
        var squareSize = MathF.Min(parameters.Width, parameters.Height) * 0.12f;

        const int count = 5;
        for (var i = 0; i < count; i++)
        {
            var orbitAngle = (float)(_st.Elapsed.TotalSeconds * 60.0 + i * 360.0 / count);
            var rad = orbitAngle * MathF.PI / 180.0f;
            var squareCenterX = centerX + orbitRadius * MathF.Cos(rad);
            var squareCenterY = centerY + orbitRadius * MathF.Sin(rad);

            var rect = new ImpellerRect
            {
                X = -squareSize * 0.5f,
                Y = -squareSize * 0.5f,
                Width = squareSize,
                Height = squareSize
            };

            builder.Save();
            builder.Translate(squareCenterX, squareCenterY);
            builder.Rotate((float)(_st.Elapsed.TotalSeconds * 90.0 + i * 20.0));
            builder.DrawRect(rect, paint);
            builder.Restore();
        }
    }
}