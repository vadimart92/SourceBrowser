var target = Argument("target", "BuildIndex");
var slnPath = Argument<string>("slnPath", "");
var destination = Argument("destination", "Index");
var toolPath = Argument<string>("toolPath", null);
Task("BuidGenerator").Does(() => {
    DotNetCoreBuild("./SourceBrowser.sln", new DotNetCoreBuildSettings {
		Configuration = "Release"
	});
});

Task("EnsureGeneratorExists").Does(()=>{
    if (!new FileInfo(GetGeneratorPath().FullPath).Exists){
        RunTarget("BuidGenerator");
    }
});

Task("BuildSolution").Does(()=>{
    var path = GetSlnPath();
    var settings = new DotNetCoreMSBuildSettings();
		settings.EnableBinaryLogger("BuildSolution.binlog");
        settings.DisableConsoleLogger = true;
        settings.WithProperty("StyleCopEnabled", "false")
        .SetConfiguration("Debug")
		.SetMaxCpuCount(0)
		.WithProperty("BuildProjectReferences", "true");
    DotNetCoreMSBuild(path, settings);
});

Task("BuildIndexInternal")
.IsDependentOn("EnsureGeneratorExists")
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

Task("BuildIndex")
.IsDependentOn("BuildSolution")
.IsDependentOn("BuildIndexInternal");


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
