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

Task("EnsureGeneratorExists").Does(()=>{
    if (!new FileInfo(GetGeneratorPath().FullPath).Exists){
        RunTarget("BuidGenerator");
    }
});

Task("BuildIndex")
.IsDependentOn("EnsureGeneratorExists")
.Does(() => {
  if (string.IsNullOrWhiteSpace(slnPath)){
      throw new Exception("[slnPath] parameter empty");
  }
  var path = new FilePath(slnPath).MakeAbsolute(Context.Environment).FullPath;
  Information($"Solution file path: {path}");
  StartProcess(GetGeneratorPath(), new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append(path)
            .Append("/force")
            .Append("/out:{0}", destination)
        });
});

RunTarget(target);

private FilePath GetGeneratorPath(){
    return new FilePath("./HtmlGenerator/HtmlGenerator.exe");
}