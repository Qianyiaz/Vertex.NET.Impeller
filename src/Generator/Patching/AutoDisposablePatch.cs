using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator.Patching;

internal class AutoDisposablePatch(string prefix) : MyPostPatch
{
    protected override bool ShouldProcess(string file) => file.Contains("Handles");

    protected override CompilationUnitSyntax ProcessRoot(CompilationUnitSyntax root)
    {
        var rewriter = new AutoDisposableRewriter(prefix);
        return rewriter.Visit(root) as CompilationUnitSyntax ?? root;
    }

    protected override CompilationUnitSyntax AddRequiredUsings(CompilationUnitSyntax root)
    {
        return HasUsing(root, "System")
            ? root
            : root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));
    }

    private class AutoDisposableRewriter(string prefix) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (node.BaseList?.Types.Any(t => t.ToString() == "IDisposable") == true || node.Members
                    .OfType<PropertyDeclarationSyntax>().All(p => p.Identifier.Text != "Handle"))
                return node;

            var shortName = node.Identifier.Text.StartsWith(prefix)
                ? node.Identifier.Text[prefix.Length..]
                : node.Identifier.Text;

            var releaseCall = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(prefix),
                    SyntaxFactory.IdentifierName($"{shortName}ReleaseNative")),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()))));

            var disposeMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Dispose")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(releaseCall))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(SyntaxFactory.Comment("/// <inheritdoc/>"), SyntaxFactory.CarriageReturnLineFeed);

            var baseList = node.BaseList ?? SyntaxFactory.BaseList();
            baseList = baseList.AddTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDisposable")));

            return node.WithBaseList(baseList).AddMembers(disposeMethod);
        }
    }
}