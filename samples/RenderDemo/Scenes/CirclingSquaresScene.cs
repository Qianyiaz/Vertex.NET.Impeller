using System.Diagnostics;
using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public class CirclingSquaresScene : IScene
{
    private readonly Stopwatch _st = Stopwatch.StartNew();

    public void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        var time = _st.Elapsed.TotalSeconds;
        using var paint = Impeller.PaintNew();
        paint.SetColor(new ImpellerColor { Red = 1, Alpha = 1 });

        var centerX = parameters.Width * 0.5f;
        var centerY = parameters.Height * 0.5f;
        var orbitRadius = MathF.Min(parameters.Width, parameters.Height) * 0.3f;
        var squareSize = MathF.Min(parameters.Width, parameters.Height) * 0.12f;

        for (var i = 0; i < 5; i++)
        {
            var orbitAngle = (float)(time * 60.0 + i * 72.0);
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
            builder.Rotate((float)(time * 90.0 + i * 20.0));
            builder.DrawRect(rect, paint);
            builder.Restore();
        }
    }
}