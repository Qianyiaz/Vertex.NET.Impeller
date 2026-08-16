using System.Text;
using Example.Core;
using Vertex.NET.Impeller;

namespace Example.Scenes;

public class ParagraphScene : IScene
{
    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        using var bgPaint = Impeller.PaintNew();
        bgPaint.SetColor(new ImpellerColor { Red = 1, Green = 1, Blue = 1, Alpha = 1 });
        builder.DrawPaint(bgPaint);

        using var typographyContext = Impeller.TypographyContextNew();
        using var fontPaint = Impeller.PaintNew();
        fontPaint.SetColor(new ImpellerColor { Alpha = 1 });

        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(30);
        style.SetForeground(fontPaint);
        style.SetTextAlignment(ImpellerTextAlignment.TextAlignmentLeft);
        style.SetTextDirection(ImpellerTextDirection.TextDirectionLtr);

        using var paragraphBuilder = Impeller.ParagraphBuilderNew(typographyContext);
        paragraphBuilder.PushStyle(style);

        const string text =
            "Hello, Vertex.NET.Impeller!\nThis is a paragraph with multiple lines.\nYou can render text!";
        var textBytes = Encoding.UTF8.GetBytes(text);
        fixed (byte* pText = textBytes)
            paragraphBuilder.AddText(pText, (uint)textBytes.Length);
        paragraphBuilder.PopStyle();

        float maxWidth = parameters.Width - 40;
        using var paragraph = paragraphBuilder.BuildParagraphNew(maxWidth);

        builder.DrawParagraph(paragraph, new ImpellerPoint { X = 20, Y = 20 });
    }
}