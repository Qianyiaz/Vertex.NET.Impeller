using HexaGen.Metadata;
using HexaGen.Patching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator.Patching;

internal abstract class MyPostPatch : PostPatch
{
    public sealed override void Apply(PatchContext context, CsCodeGeneratorMetadata metadata, List<string> files)
    {
        foreach (var file in files.Where(ShouldProcess))
        {
            var root = ParseAndGetRoot(file);
            var newRoot = ProcessRoot(root);
            if (newRoot == root) continue;
            
            SaveFile(file, newRoot);
        }
    }

    protected virtual bool ShouldProcess(string file) => true;

    private static CompilationUnitSyntax ParseAndGetRoot(string file)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
        return (tree.GetRoot() as CompilationUnitSyntax)!;
    }

    protected abstract CompilationUnitSyntax ProcessRoot(CompilationUnitSyntax root);

    private static void SaveFile(string file, CompilationUnitSyntax root) =>
        File.WriteAllText(file, root.NormalizeWhitespace().ToFullString());
}