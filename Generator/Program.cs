using HexaGen;
using HexaGen.Patching;

BatchGenerator batch = new();
batch.Start()
    .Setup<CsCodeGenerator>("config.json")
    .AddPrePatch(new NamingPatch(["Impeller"], NamingPatchOptions.None))
    .AddPrePatch(new EnumItemPrefixStripPatch(["KImpeller"]))
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