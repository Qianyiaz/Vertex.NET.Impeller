using HexaGen;
using HexaGen.Metadata;
using HexaGen.Patching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

BatchGenerator batch = new();
batch.Start()
    .Setup<CsCodeGenerator>("config.json")
    .AddPrePatch(new NamingPatch(["Impeller"], NamingPatchOptions.None))
    .AddPrePatch(new EnumItemPrefixStripPatch(["KImpeller"]))
    .AddPostPatch(new AutoDisposablePatch("Impeller"))
    .AddPostPatch(new RemoveExceptExtensionsPatch(["Release"]))
    .Generate("include/impeller.h", "../../../../Vertex.NET.Impeller/Generated", [.. Directory.GetFiles("include")])
    .Finish();


internal class EnumItemPrefixStripPatch(string[] prefixes) : PrePatch
{
    protected override void PatchCompilation(CsCodeGeneratorConfig config, ParseResult result)
    {
        base.PatchCompilation(config, result);

        config.CustomEnumItemMapper = (_, _, _, csEnumItem) =>
            csEnumItem.Name = prefixes.Aggregate(csEnumItem.Name,
                (current, prefix) => current!.Replace(prefix, string.Empty));
    }
}

internal class AutoDisposablePatch(string prefix) : PostPatch
{
    public override void Apply(PatchContext context, CsCodeGeneratorMetadata metadata, List<string> files)
    {
        foreach (var file in files)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);

            if (tree.GetRoot() is not CompilationUnitSyntax root) continue;
            var newRoot = root;

            foreach (var structDecl in root.DescendantNodes().OfType<StructDeclarationSyntax>())
            {
                var hasDisposable = structDecl.BaseList?.Types.Any(t => t.ToString() == "IDisposable") ?? false;
                if (hasDisposable) continue;

                var hasHandle = structDecl.Members.OfType<PropertyDeclarationSyntax>()
                    .Any(p => p.Identifier.Text == "Handle");
                if (!hasHandle) continue;

                var structName = structDecl.Identifier.Text;

                var shortName = structName.StartsWith(prefix) ? structName[prefix.Length..] : structName;

                var releaseMethod = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(prefix),
                    SyntaxFactory.IdentifierName(shortName + "Release" + "Native"));

                var disposeInvocation = SyntaxFactory.InvocationExpression(releaseMethod,
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()))));

                // Generate: public void Dispose() => Impeller.ContextRelease(this);
                var disposeMethod = SyntaxFactory
                    .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                        "Dispose")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(disposeInvocation))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                var baseList = structDecl.BaseList ?? SyntaxFactory.BaseList();
                baseList = baseList.AddTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDisposable")));

                var newStruct = structDecl.WithBaseList(baseList).AddMembers(disposeMethod);

                newRoot = newRoot.ReplaceNode(structDecl, newStruct);
            }

            if (newRoot.Usings.All(u => u.Name?.ToString() != "System"))
                newRoot = newRoot.AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            if (newRoot == root) continue;
            File.WriteAllText(file, newRoot.NormalizeWhitespace().ToFullString());
        }
    }
}

internal class RemoveExceptExtensionsPatch(string[] names) : PostPatch
{
    public override void Apply(PatchContext context, CsCodeGeneratorMetadata metadata, List<string> files)
    {
        foreach (var file in files)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            if (tree.GetRoot() is not CompilationUnitSyntax root) continue;

            var newRoot = root;

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (!classDecl.Identifier.Text.Contains("Extensions", StringComparison.OrdinalIgnoreCase))
                    continue;

                var newMembers = new SyntaxList<MemberDeclarationSyntax>();
                foreach (var member in classDecl.Members)
                {
                    if (member is MethodDeclarationSyntax { ParameterList.Parameters.Count: >= 1 } method &&
                        method.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword) &&
                        names.Contains(method.Identifier.Text))
                        continue;

                    newMembers = newMembers.Add(member);
                }

                newRoot = newRoot.ReplaceNode(classDecl, classDecl.WithMembers(newMembers));
            }

            if (newRoot == root) continue;
            File.WriteAllText(file, newRoot.NormalizeWhitespace().ToFullString());
        }
    }
}