using HexaGen;
using HexaGen.Patching;

namespace Generator.Patching;

internal class EnumItemPrefixStripPatch(string[] prefixes) : PrePatch
{
    protected override void PatchCompilation(CsCodeGeneratorConfig config, ParseResult result)
    {
        config.CustomEnumItemMapper = (_, _, _, csEnumItem) =>
            csEnumItem.Name = prefixes.Aggregate(csEnumItem.Name, (current, p) => current!.Replace(p, string.Empty));
    }
}