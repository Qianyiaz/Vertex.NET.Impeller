using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator.Patching;

internal partial class RemoveInvalidCommentsPatch : MyPostPatch
{
    protected override bool ShouldProcess(string file) => file.Contains("Functions.") || file.Contains("Extensions.");

    protected override CompilationUnitSyntax ProcessRoot(CompilationUnitSyntax root)
    {
        var rewriter = new RemoveInvalidCommentsRewriter();
        return rewriter.Visit(root) as CompilationUnitSyntax ?? root;
    }

    private sealed partial class RemoveInvalidCommentsRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            token = base.VisitToken(token);

            if (!token.HasLeadingTrivia)
                return token;

            var newTrivia = SyntaxFactory.TriviaList(
                token.LeadingTrivia.Where(t => !IsInvalidDocumentationComment(t)));

            return token.WithLeadingTrivia(newTrivia);
        }

        private static bool IsInvalidDocumentationComment(SyntaxTrivia trivia)
        {
            if (!trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                return false;

            var text = trivia.ToFullString();

            var cleaned = MyRegex().Replace(text, string.Empty);
            return cleaned.Length == 0;
        }

        [GeneratedRegex(@"(///|/\*\*|\*/|<summary>|</summary>|<br\s*/?>|&nbsp;|-|\s)+")]
        private static partial Regex MyRegex();
    }
}