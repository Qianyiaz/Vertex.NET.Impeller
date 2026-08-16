using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator.Patching;

internal class RemoveExceptExtensionsPatch(string[] names) : MyPostPatch
{
    protected override bool ShouldProcess(string file) => file.Contains("Extensions.");

    protected override CompilationUnitSyntax ProcessRoot(CompilationUnitSyntax root)
    {
        var rewriter = new RemoveExtensionsRewriter(names);
        return rewriter.Visit(root) as CompilationUnitSyntax ?? root;
    }

    private class RemoveExtensionsRewriter(string[] names) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!node.Identifier.Text.Contains("Extensions", StringComparison.OrdinalIgnoreCase))
                return node;

            var members = node.Members.Where(m =>
                !(m is MethodDeclarationSyntax { ParameterList.Parameters.Count: >= 1 } method &&
                  method.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword) &&
                  names.Contains(method.Identifier.Text))).ToArray();

            return node.WithMembers(SyntaxFactory.List(members));
        }
    }
}