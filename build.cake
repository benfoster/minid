// Install .NET Core Global tools.
#tool "dotnet:?package=dotnet-reportgenerator-globaltool&version=5.0.0"
#tool "dotnet:?package=coveralls.net&version=3.0.0"
#tool "dotnet:?package=dotnet-sonarscanner&version=5.4.0"

// Install addins 
#addin nuget:?package=Cake.Coverlet&version=2.5.4
#addin nuget:?package=Cake.Sonar&version=1.1.26
#addin nuget:?package=Cake.Git&version=1.0.0

 #r "System.Text.Json"
 #r "System.IO"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactsPath = "./artifacts";
var coveragePath = "./artifacts/coverage"; 
var packFiles = "./src/**/*.csproj";
var testFiles = "./test/**/*.csproj";
var packages = "./artifacts/*.nupkg";
DirectoryPath sitePath = "./artifacts/docs";
var docFxConfig = "./docs/docfx.json";

var coverallsToken = EnvironmentVariable("COVERALLS_TOKEN");
var sonarToken = EnvironmentVariable("SONAR_TOKEN");
GitBranch currentBranch = GitBranchCurrent("./");

uint coverageThreshold = 50;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
   BuildContext.Initialize(Context);
   Information($"Building Minid with configuration {configuration} on branch {currentBranch.FriendlyName}");
});

Teardown(ctx =>
{
   if (DirectoryExists(coveragePath))
   {
        DeleteDirectory(coveragePath, new DeleteDirectorySettings 
        {
            Recursive = true,
            Force = true
        });
   }
   
   Information("Finished running build");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => 
    {
        CleanDirectories(artifactsPath);
    });

Task("SonarBegin")
    .WithCriteria(!string.IsNullOrEmpty(sonarToken))
    .Does(() => 
    {
        SonarBegin(new SonarBeginSettings 
        {
            Key = "benfoster_minid",
            Organization = "benfoster",
            Url = "https://sonarcloud.io",
            Exclusions = "test/**",
            OpenCoverReportsPath = $"{coveragePath}/*.xml",
            Login = sonarToken,
            VsTestReportsPath = $"{artifactsPath}/*.TestResults.xml",
        });
    });

Task("Build")
    .Does(() => 
    {
        DotNetBuild("minid.sln", new DotNetBuildSettings 
        {
            Configuration = configuration
        });
    });

Task("Test")
   .Does(() => 
   {
        foreach (var project in GetFiles(testFiles))
        {
            var projectName = project.GetFilenameWithoutExtension();
            
            var testSettings = new DotNetCoreTestSettings 
            {
                NoBuild = true,
                Configuration = configuration,
                Loggers = { $"trx;LogFileName={projectName}.TestResults.xml" },
                ResultsDirectory = artifactsPath
            };
            
            // https://github.com/Romanx/Cake.Coverlet
            var coverletSettings = new CoverletSettings 
            {
                CollectCoverage = true,
                CoverletOutputFormat = CoverletOutputFormat.opencover,
                CoverletOutputDirectory = coveragePath,
                CoverletOutputName = $"{projectName}.opencover.xml",
                Threshold = coverageThreshold
            };
            
            DotNetCoreTest(project.ToString(), testSettings, coverletSettings);
        }
   });


Task("Pack")
    .Does(() => 
    {
        var settings = new DotNetPackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsPath,
            NoBuild = true
        };

        foreach (var file in GetFiles(packFiles))
        {
            DotNetPack(file.ToString(), settings);
        }
    });

Task("GenerateReports")
    .Does(() => 
    {
        ReportGenerator(GetFiles($"{coveragePath}/*.xml"), artifactsPath, new ReportGeneratorSettings
        {
            ArgumentCustomization = args => args.Append("-reporttypes:lcov;HTMLSummary;TextSummary;")
        });
    });

Task("UploadCoverage")
    .WithCriteria(!string.IsNullOrEmpty(coverallsToken) && BuildSystem.IsRunningOnGitHubActions)
    .Does(() => 
    {
        var workflow = BuildSystem.GitHubActions.Environment.Workflow;

        Dictionary<string, object> @event = default;
        if (workflow.EventName == "pull_request")
        {
            //string eventJson = System.IO.File.ReadAllText(workflow.EventPath); 
            //@event = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eventJson);
        }

        var args = new ProcessArgumentBuilder()
                    .Append($"--repoToken {coverallsToken}")
                    .Append("--lcov")
                    .Append("--useRelativePaths")
                    .Append("-i ./artifacts/lcov.info")
                    .Append($"--commitId {workflow.Sha}") 
                    .Append($"--commitBranch {workflow.Ref}")
                    .Append($"--serviceNumber {workflow.RunNumber}")
                    .Append($"--jobId {workflow.RunId}");
                    //.Append("--serviceName github")
                    //.Append("--dryrun");

        if (BuildSystem.IsPullRequest)
        {
            args.Append($"--pullRequest {@event["number"].ToString()}");
        }

        var settings = new ProcessSettings { Arguments = args };

        // We have to start the process manually since the cake addin forces us to provide 
        // a format enum which currently doesn't include lcov
        if (StartProcess(
            Context.Tools.Resolve("csmacnz.Coveralls")
                ?? Context.Tools.Resolve("csmacnz.coveralls.exe")
                ?? throw new Exception("Failed to resolve Coveralls shim."),
            settings
            ) != 0)
        {
            throw new Exception("Failed to execute Coveralls.");
        }
    });

Task("PublishPackages")
    .WithCriteria(() => BuildContext.ShouldPublishToNuget)
    .Does(() => 
    {
        foreach(var package in GetFiles(packages))
        {
            DotNetNuGetPush(package.ToString(), new DotNetNuGetPushSettings {
                ApiKey = BuildContext.NugetApiKey,
                Source = BuildContext.NugetApiUrl,
                SkipDuplicate = true
            });
        }
    });

Task("SonarEnd")
    .WithCriteria(!string.IsNullOrEmpty(sonarToken))
    .Does(() => 
    {
        SonarEnd(new SonarEndSettings
        {
            Login = sonarToken
        });
    });

Task("Dump").Does(() => BuildContext.PrintParameters(Context));

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("GenerateReports");

Task("CI")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("Default")
    .IsDependentOn("UploadCoverage")
    .IsDependentOn("SonarEnd");

Task("Publish")
    .IsDependentOn("CI")
    .IsDependentOn("PublishPackages");

RunTarget(target);


public static class BuildContext
{
    public static bool IsTag { get; private set; }
    public static string NugetApiUrl { get; private set; }
    public static string NugetApiKey { get; private set; }

    public static bool ShouldPublishToNuget
        => !string.IsNullOrWhiteSpace(BuildContext.NugetApiUrl) && !string.IsNullOrWhiteSpace(BuildContext.NugetApiKey);
        
    public static void Initialize(ICakeContext context)
    {
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            // https://github.com/cake-contrib/Cake.Recipe/blob/3ee5725b1cc0621f90205904848407515a2b62fd/Cake.Recipe/Content/github-actions.cake
            var tempName = context.BuildSystem().GitHubActions.Environment.Workflow.Ref;
            if (!string.IsNullOrEmpty(tempName) && tempName.IndexOf("tags/") >= 0)
            {
                IsTag = true;
                //Name = tempName.Substring(tempName.LastIndexOf('/') + 1);
            }
        }

        if (BuildContext.IsTag)
        {
            NugetApiUrl = context.EnvironmentVariable("NUGET_API_URL");
            NugetApiKey = context.EnvironmentVariable("NUGET_API_KEY");
        }
        else
        {
            NugetApiUrl = context.EnvironmentVariable("NUGET_PRE_API_URL");
            NugetApiKey = context.EnvironmentVariable("NUGET_PRE_API_KEY");
        }
    }

    public static void PrintParameters(ICakeContext context)
    {
        context.Information("Printing Build Parameters...");
        context.Information("IsTag: {0}", IsTag);
        context.Information("NugetApiUrl: {0}", NugetApiUrl);
        context.Information("NugetApiKey: {0}", NugetApiKey);
        context.Information("ShouldPublishToNuget: {0}", ShouldPublishToNuget);
    }
}
