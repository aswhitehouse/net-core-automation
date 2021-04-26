using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions("Net-Core-Automation-CI", GitHubActionsImage.UbuntuLatest, OnPushBranches = new[] {"'**'"},
    InvokedTargets = new[] {nameof(Pack)})]
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
    [GitRepository] readonly GitRepository GitRepository;
    [PathExecutable] readonly Tool sh;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ResultsDirectory => SourceDirectory / "TestResults";
    AbsolutePath AllureCliDirectory => RootDirectory / "resources" / "allure-commandline" / "bin";
    AbsolutePath OutputDirectory => SourceDirectory / "Output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(DeleteDirectory);
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
            sh($"./Allure generate {ResultsDirectory} --clean", AllureCliDirectory);
        });

    Target Pack => _ => _
        .DependsOn(PublishTestResults)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution.GetProject("NetCoreAutomationUiCommon"))
                .SetOutputDirectory(OutputDirectory));
        });
}