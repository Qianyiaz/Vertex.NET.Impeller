using HexaGen;

var generator = new CsCodeGenerator(CsCodeGeneratorConfig.Load("config.json"));
generator.Generate("include/impeller.h", "../../../../Vertex.NET.Impeller/Generated");