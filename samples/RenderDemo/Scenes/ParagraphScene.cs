using RenderDemo.Core;
using Vertex.NET.Impeller;

namespace RenderDemo.Scenes;

public class ParagraphScene : IScene
{
    public unsafe void Render(ImpellerDisplayListBuilder builder, SceneParameters parameters)
    {
        using var bgPaint = Impeller.PaintNew();
        bgPaint.SetColor(new ImpellerColor { Red = 1, Green = 1, Blue = 1, Alpha = 1 });
        builder.DrawPaint(bgPaint);

        using var fontPaint = Impeller.PaintNew();
        fontPaint.SetColor(new ImpellerColor { Alpha = 1 });

        using var style = Impeller.ParagraphStyleNew();
        style.SetFontSize(18);
        style.SetForeground(fontPaint);
        style.SetTextAlignment(ImpellerTextAlignment.TextAlignmentLeft);
        style.SetTextDirection(ImpellerTextDirection.TextDirectionLtr);

        using var context = Impeller.TypographyContextNew();
        using var paragraphBuilder = Impeller.ParagraphBuilderNew(context);
        paragraphBuilder.PushStyle(style);

        var text =
            """
            Hello👋, Vertex.NET.Impeller!
            This is a paragraph with multiple lines.
            You can render text😋, emoji🎉, and complex scripts.

            🌍 Internationalization (i18n) is fully supported:
            - 简体中文：你好，中国！这里有一段很长的中文文本，用来测试换行和字体回退。Impeller 可以完美渲染。
            - 日本語：こんにちは、世界！これは日本語のテキストです。長い文章も大丈夫です。
            - 한국어：안녕하세요, 세계! 이것은 한국어 텍스트입니다.
            - Русский：Привет, мир! Это текст на русском языке. 
            - العربية：مرحبا بالعالم! هذا نص باللغة العربية.
            - Deutsch：Hallo Welt! Das ist ein deutscher Text.
            - Français：Bonjour le monde ! Ceci est un texte en français.

            ✨ Special characters: ♥ ★ ♦ © ® ™ ℃ € ¥ £ § ¶

            🔢 Numbers and symbols: 1234567890 !@#$%^&*()_+-=[]{}|;:'",.<>?/~`
            """u8;
        fixed (byte* pText = text)
            paragraphBuilder.AddText(pText, (uint)text.Length);
        paragraphBuilder.PopStyle();

        float maxWidth = parameters.Width - 40;
        using var paragraph = paragraphBuilder.BuildParagraphNew(maxWidth);

        builder.Save();
        builder.DrawParagraph(paragraph, new ImpellerPoint { X = 20, Y = 20 });
        builder.Restore();
    }
}