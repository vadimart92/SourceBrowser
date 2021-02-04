#addin nuget:?package=Cake.FileHelpers&version=3.3.0

var target = Argument("target", "BuildIndex");
var slnPath = Argument<string>("slnPath", "");
var destination = Argument("destination", "Index");
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

Task("SetSdkSettings").Does(() => {
	var settings = @"{""sdk"":{""version"": ""3.1.201"",""rollForward"": ""latestFeature""}}";
	var path = GetSlnPath();
	var settingsPath = File(path).Path.GetDirectory().CombineWithFilePath(File("global.json"));
	Information($"settingsPath: {settingsPath}");
	FileWriteText(settingsPath, settings);
});

Task("BuildSolution")
.IsDependentOn("SetSdkSettings")
.Does(()=>{
	var path = GetSlnPath();
	DotNetCoreRestore(path);
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
		},
		EnvironmentVariables = new Dictionary<string, string> {
			{ "MSBuildSDKsPath", @"C:\Program Files\dotnet\sdk\3.1.201\Sdks" }
		}
	);
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
