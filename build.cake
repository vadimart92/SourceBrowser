var target = Argument("target", "BuildIndex");
var slnPath = Argument<string>("slnPath", "");
var destination = Argument("destination", "Index");
var configuration = Argument("configuration", "Release");
Task("BuidGenerator").Does(() => {
    MSBuild("./SourceBrowser.sln", (settings)=>{
        settings
        .WithRestore()
        .SetConfiguration(configuration);
    });
});

Task("BuildIndex")
.IsDependentOn("BuidGenerator")
.Does(() => {
  if (string.IsNullOrWhiteSpace(slnPath)){
      throw new Exception("[slnPath] parameter empty");
  }
  var generatorPath = new FilePath("./HtmlGenerator/HtmlGenerator.exe");
  StartProcess(generatorPath, new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append(new FilePath(slnPath).FullPath)
            .Append("/force")
            .Append("/out:{0}", destination)
        });
});

RunTarget(target);