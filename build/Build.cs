using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions("Net-Core-Automation-CI", GitHubActionsImage.UbuntuLatest, OnPushBranches = new[] {"'**'"},
    InvokedTargets = new[] {nameof(PublishPackages)})]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [PathExecutable] readonly Tool Sh;

    private static AbsolutePath SourceDirectory => RootDirectory / "src";

    private static AbsolutePath ResultsDirectory => SourceDirectory / "TestResults";

    private static AbsolutePath AllureCliDirectory => RootDirectory / "resources" / "allure-commandline" / "bin";

    private static AbsolutePath OutputDirectory => SourceDirectory / "Output";

    private static AbsolutePath Settings => RootDirectory / "build";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults", OutputDirectory).ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .EnableNoBuild()
                .SetResultsDirectory(ResultsDirectory)
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLogger($"trx;LogFileName={Solution.Name}.trx"));
        });

    Target PublishTestResults => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            Sh($"./allure generate {ResultsDirectory} --clean", AllureCliDirectory);
        });

    [GitVersion(Framework = "netcoreapp3.1")] readonly GitVersion GitVersion;

    Target Pack => _ => _
        .DependsOn(PublishTestResults)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution.GetProject("NetCoreAutomationUiCommon"))
                .SetOutputDirectory(OutputDirectory)
                .SetVersion(GitVersion.NuGetVersionV2));
        });

    IEnumerable<AbsolutePath> Packages => OutputDirectory.GlobFiles("*.nupkg");

    [Parameter] static string PackageApiSettings => File.ReadAllText(Settings / "appsettings.json");
    
    Target PublishPackages => _ => _
        .When(Configuration.Equals(Configuration.Release), pp => pp
            .DependsOn(Pack)
            .Executes(() =>
            {
                var myGet = JsonConvert.DeserializeObject<Dictionary<string, string>>(PackageApiSettings);
                DotNetNuGetPush(_ => _
                    .SetSource(myGet["my-get-uri"])
                    .SetApiKey(myGet["my-get-key"])
                    .CombineWith(Packages, (_, v) => _
                        .SetTargetPath(v)));
            }));

}