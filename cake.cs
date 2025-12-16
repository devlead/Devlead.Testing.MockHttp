#:sdk Cake.Sdk@6.0.0
#:property IncludeAdditionalFiles=./build/*.cs


/*****************************
 * Setup
 *****************************/
Setup(
    static context => {
        InstallTool("dotnet:https://api.nuget.org/v3/index.json?package=DPI&version=2025.11.25.337");
        InstallTool("dotnet:https://api.nuget.org/v3/index.json?package=GitVersion.Tool&version=6.5.1");
         var assertedVersions = context.GitVersion(new GitVersionSettings
            {
                OutputType = GitVersionOutput.Json
            });

        var branchName = assertedVersions.BranchName;
        var isMainBranch = StringComparer.OrdinalIgnoreCase.Equals("main", branchName);
        
        var buildDate = DateTime.UtcNow;
        var runNumber = GitHubActions.IsRunningOnGitHubActions
                            ? GitHubActions.Environment.Workflow.RunNumber
                            : 0;

        var suffix = runNumber == 0 
                       ? $"-{(short)((buildDate - buildDate.Date).TotalSeconds/3)}"
                       : string.Empty;

        var version = FormattableString
                          .Invariant($"{buildDate:yyyy.M.d}.{runNumber}{suffix}");

        context.Information("Building version {0} (Branch: {1}, IsMain: {2})",
            version,
            branchName,
            isMainBranch);

        var artifactsPath = context
                            .MakeAbsolute(context.Directory("./artifacts"));

        var projectRoot =  context
                            .MakeAbsolute(context.Directory("./src"));

        var projectPath = projectRoot.CombineWithFilePath("Devlead.Testing.MockHttp/Devlead.Testing.MockHttp.csproj");

        return new BuildData(
            version,
            isMainBranch,
            !context.IsRunningOnWindows(),
            BuildSystem.IsLocalBuild,
            projectRoot,
            projectPath,
            new DotNetMSBuildSettings()
                .SetConfiguration("Release")
                .SetVersion(version)
                .WithProperty("Copyright", $"Mattias Karlsson © {DateTime.UtcNow.Year}")
                .WithProperty("Authors", "devlead")
                .WithProperty("Company", "devlead")
                .WithProperty("PackageLicenseExpression", "MIT")
                .WithProperty("PackageTags", "testing;http")
                .WithProperty("PackageDescription", ".NET Library for mocking HTTP client requests")
                .WithProperty("RepositoryUrl", "https://github.com/devlead/Devlead.Testing.MockHttp.git")
                .WithProperty("ContinuousIntegrationBuild", GitHubActions.IsRunningOnGitHubActions ? "true" : "false")
                .WithProperty("EmbedUntrackedSources", "true"),
            artifactsPath,
            artifactsPath.Combine(version)
            );
    }
);

/*****************************
 * Tasks
 *****************************/
Task("Clean")
    .Does<BuildData>(
        static (context, data) => context.CleanDirectories(data.DirectoryPathsToClean)
    )
.Then("Restore")
    .Does<BuildData>(
        static (context, data) => context.DotNetRestore(
            data.ProjectRoot.FullPath,
            new DotNetRestoreSettings {
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("DPI")
    .Does<BuildData>(
        static (context, data) => Command(
                ["dpi", "dpi.exe"],
                new ProcessArgumentBuilder()
                    .Append("nuget")
                    .Append("--silent")
                    .AppendSwitchQuoted("--output", "table")
                    .Append(
                        (
                            !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_SharedKey"))
                            &&
                            !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_WorkspaceId"))
                        )
                            ? "report"
                            : "analyze"
                        )
                    .AppendSwitchQuoted("--buildversion", data.Version)
                
            )
    )
.Then("Build")
    .Does<BuildData>(
        static (context, data) => context.DotNetBuild(
            data.ProjectRoot.FullPath,
            new DotNetBuildSettings {
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Test")
    .Does<BuildData>(
        static (context, data) => context.DotNetTest(
            data.ProjectRoot.FullPath,
            new DotNetTestSettings {
                NoBuild = true,
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Pack")
    .Does<BuildData>(
        static (context, data) => context.DotNetPack(
            data.ProjectPath.FullPath,
            new DotNetPackSettings {
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = data.NuGetOutputPath,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Upload-Artifacts")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushGitHubPackages())
    .Does<BuildData>(
        static (context, data) => GitHubActions
            .Commands
            .UploadArtifact(data.ArtifactsPath, "artifacts")
    )
.Then("Prepare-Integration-Test")
    .Does<BuildData>(
        static (context, data) => {
            context.CopyDirectory(data.ProjectRoot.Combine("Devlead.Testing.MockHttp.Tests"), data.IntegrationTestPath);
            context.CopyFile(data.ProjectRoot.CombineWithFilePath("Directory.Packages.props"), data.IntegrationTestPath.CombineWithFilePath("Directory.Packages.props"));
            context.CopyFile("nuget.config", data.IntegrationTestPath.CombineWithFilePath("nuget.config"));
            context.DotNetAddPackage(
                "Devlead.Testing.MockHttp",
                new DotNetPackageAddSettings {
                    EnvironmentVariables = { { "Configuration", "IntegrationTest" } },
                    WorkingDirectory = data.IntegrationTestPath,
                    Version = data.Version,
                    Source = data.NuGetOutputPath.FullPath
                }
            );
        }
    )
.Then("Integration-Test")
    .Does<BuildData>(
        static (context, data) => {
            context.DotNetTest(
                data.IntegrationTestPath.FullPath,
                new DotNetTestSettings {
                   Configuration = "IntegrationTest"
                }
            );
        }
    )
    .Default()
.Then("Push-GitHub-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushGitHubPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
            new DotNetNuGetPushSettings
            {
                Source = data.GitHubNuGetSource,
                ApiKey = data.GitHubNuGetApiKey
            }
        )
    )
.Then("Push-NuGet-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
                new DotNetNuGetPushSettings
                {
                    Source = data.NuGetSource,
                    ApiKey = data.NuGetApiKey
                }
        )
    )
.Then("Create-GitHub-Release")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .Does<BuildData>(
        static (context, data) => context
            .Command(
                new CommandSettings {
                    ToolName = "GitHub CLI",
                    ToolExecutableNames = new []{ "gh.exe", "gh" },
                    EnvironmentVariables = { { "GH_TOKEN", data.GitHubNuGetApiKey } }
                },
                new ProcessArgumentBuilder()
                    .Append("release")
                    .Append("create")
                    .Append(data.Version)
                    .AppendSwitchQuoted("--title", data.Version)
                    .Append("--generate-notes")
                    .Append(string.Join(
                        ' ',
                        context
                            .GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg")
                            .Select(path => path.FullPath.Quote())
                        ))

            )
    )
.Then("GitHub-Actions")
.Run();

