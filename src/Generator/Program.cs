using Generator.Patching;
using HexaGen;
using HexaGen.Patching;

BatchGenerator batch = new();
batch.Start()
    .Setup<CsCodeGenerator>("config.json")
    .AddPrePatch(new NamingPatch(["Impeller"], NamingPatchOptions.None))
    .AddPrePatch(new EnumItemPrefixStripPatch(["KImpeller"]))
    .AddPostPatch(new AutoDisposablePatch("Impeller"))
    .AddPostPatch(new RemoveExceptExtensionsPatch(["Release"]))
    .AddPostPatch(new SuppressGcTransitionPatch())
    .AddPostPatch(new RemoveInvalidCommentsPatch())
    .Generate("include/impeller.h", "../../../../Vertex.NET.Impeller/Generated", [.. Directory.GetFiles("include")])
    .Finish();