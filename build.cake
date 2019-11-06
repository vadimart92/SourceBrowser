var target = Argument("target", "BuildIndex");
var slnPath = Argument<string>("slnPath", "");
var destination = Argument("destination", "Index");
Task("BuidGenerator").Does(() => {
    MSBuild("./SourceBrowser.sln", (settings)=>{
        settings
        .SetConfiguration("Release");
    });
});

Task("EnsureGeneratorExists").Does(()=>{
    if (!new FileInfo(GetGeneratorPath().FullPath).Exists){
        RunTarget("BuidGenerator");
    }
});

Task("BuildSolution").Does(()=>{
    var path = GetSlnPath();
    MSBuild(path, (settings)=>{
        settings
        .EnableBinaryLogger("BuildSolution.binlog")
        .SetNoConsoleLogger(true)
        .WithProperty("StyleCopEnabled", "false")
        .SetConfiguration("Debug");
    });
});

Task("BuildIndex")
.IsDependentOn("EnsureGeneratorExists")
.IsDependentOn("BuildSolution")
.Does(() => {
  var path = GetSlnPath();
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

private string GetSlnPath(){
    if (string.IsNullOrWhiteSpace(slnPath)){
         throw new Exception("[slnPath] parameter empty");
    }
    return new FilePath(slnPath).MakeAbsolute(Context.Environment).FullPath;
}
