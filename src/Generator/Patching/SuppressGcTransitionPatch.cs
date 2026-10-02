using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator.Patching;

internal class SuppressGcTransitionPatch : MyPostPatch
{
    protected override bool ShouldProcess(string file) => file.Contains("Functions.");

    protected override CompilationUnitSyntax ProcessRoot(CompilationUnitSyntax root)
    {
        var rewriter = new SuppressGcTransitionRewriter();
        return rewriter.Visit(root) as CompilationUnitSyntax ?? root;
    }

    private class SuppressGcTransitionRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.InternalKeyword) ||
                !node.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                !node.Identifier.Text.EndsWith("Native") || node.AttributeLists.SelectMany(a => a.Attributes)
                    .Any(a => a.Name.ToString() == "SuppressGCTransition"))
                return base.VisitMethodDeclaration(node)!;

            var name = GetEntryPoint(node) ?? node.Identifier.Text;
            
            if (!name.StartsWith("ImpellerPathBuilder") &&
                !name.EndsWith("Retain") &&
                !name.EndsWith("Release") &&
                !name.Contains("Set") &&
                !name.Contains("Get") &&
                !name.StartsWith("ImpellerLineMetricsIs") &&
                !name.StartsWith("ImpellerGlyphInfoIs"))
                return base.VisitMethodDeclaration(node)!;

            var attr = SyntaxFactory.Attribute(SyntaxFactory.ParseName("SuppressGCTransition"));
            node = node.AddAttributeLists(SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(attr)));
            return base.VisitMethodDeclaration(node)!;
        }

        private static string? GetEntryPoint(MethodDeclarationSyntax node)
        {
            foreach (var arg in node.AttributeLists.SelectMany(list => from attr in list.Attributes from arg in attr.ArgumentList?.Arguments ?? default where arg.NameEquals?.Name.Identifier.Text == "EntryPoint" select arg))
                if (arg.Expression is LiteralExpressionSyntax { Token.Value: string s })
                    return s;
            return null;
        }
    }
}