#addin nuget:?package=Cake.Git&version=1.0.1
#addin nuget:?package=Cake.FileHelpers&version=4.0.1

using System.Text.RegularExpressions;
using System.Linq;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// CUSTOM CONFIGURES
//////////////////////////////////////////////////////////////////////

/// <summary>
/// The relative path of the build directory, which is the work dirctory of task actions.
/// </summary>
var buildDir = "Build";

/// <summary>
/// The relative path of the *.sln file.
/// </summary>
var solutionFile = "WpfExtensions.sln";

/// <summary>
/// The regex pattern of the version in tags.
/// </summary>
var versionPattern = @"[Vv]?(?<majar>\d+?)\.(?<minor>\d+?)\.(?<patch>\d+?)";

/// <summary>
/// The root dirctory of the workspace.
/// </summary>
var rootDir = Environment.CurrentDirectory;

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    // Executed BEFORE the first task.

    // Arguments validation
    if(configuration.ToLower() != "release" &&
        configuration.ToLower() != "enterprise")
    {
        throw new Exception("Unknown configuration, it should be 'Relaese' or 'Enterprise'");
    }
});

//////////////////////////////////////////////////////////////////////
// TEARDOWN
//////////////////////////////////////////////////////////////////////

Teardown(context =>
{
    // Executed AFTER the last task.

    //Cleanup
    var tempDirs = new string[]
    {
        // TODO
    };

    foreach(var tempDir in tempDirs)
    {   
        if(DirectoryExists(tempDir))
        {
            DeleteDirectory(tempDir, new DeleteDirectorySettings()
            {
                Recursive = true,
                Force = true
            });
        }
    };
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("BumpVersion")
    .Does(() =>
{
    // Get git info.
    var latestTag = GitTags(rootDir).LastOrDefault();
    var logs = GitLogTag(rootDir, latestTag.FriendlyName);
    var nextVersion = GetBumpVersion(latestTag.FriendlyName, versionPattern, IsBumpMinor(logs));

    // Writes the release notes file.
    var releaseNotesPath = System.IO.Path.Combine(rootDir, "Release Notes.txt");
    var releaseNotesText = $"{nextVersion}{Environment.NewLine}{GetReleaseNotes(logs)}{Environment.NewLine}{Environment.NewLine}";

    if (FileExists(releaseNotesPath)) {
        var prevText = FileReadText(releaseNotesPath);
        FileWriteText(releaseNotesPath, $"{releaseNotesText}{prevText}");
    } else {
        FileWriteText(releaseNotesPath, releaseNotesText);
    }

    // Git Commit and push.
    var prevAuthor = logs.Last().Author;

    GitAddAll(rootDir);
    GitCommit(rootDir, prevAuthor.Name, prevAuthor.Email, $"Bump version to {nextVersion}");
    GitTag(rootDir, nextVersion);
    // TODO: Push
});

Task("RestorePackages")
    .Description("Restores packages from nuget in the legacy style *.proj file.")
    .Does(() =>
{
    // The package.config file is not support the `msbuild -restore` target.
    var legacyProjectFiles = new string[] {
        // TODO
    };
    var settings = new NuGetRestoreSettings {
        PackagesDirectory = Directory ("./packages")
    };

    NuGetRestore (legacyProjectFiles.Select(item => new FilePath(item)), settings);
});

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    var settings = new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        Configuration = configuration,
        ToolVersion = MSBuildToolVersion.VS2019,
        PlatformTarget = PlatformTarget.x64,
        // Using `msbuild -restore` target for SDK-style `*.csproj` files.
        Restore = true,
    };

    MSBuild(solutionFile, settings);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("BumpVersion")
    .IsDependentOn("Build");

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELP FUNCTIONS
//////////////////////////////////////////////////////////////////////

private static bool IsBumpMinor(IEnumerable<GitCommit> commits) {
    return commits
        .Select(item => item.Message)
        .Any(item => string.Equals(item, "[feature]", StringComparison.OrdinalIgnoreCase));
}

private static string GetReleaseNotes(IEnumerable<GitCommit> commits) {
    var regex = new Regex(@"^\[?(feature|bugfix)\]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    return string.Join(
        Environment.NewLine,
        commits
            .Where(item => regex.IsMatch(item.Message))
            .Select(item => $"{item.Author.When:yyyy-MM-dd} {item.Author.Name} {item.Message.TrimEnd('\r', '\n')}"));
}

private static string GetBumpVersion(string version, string pattern, bool bumpMinor = false) {
    var match = Regex.Match(version, pattern);

    if (match.Success) {
        var majar = int.Parse(match.Groups["majar"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);

        if (bumpMinor) {
            minor++;
        } else {
            patch++;
        }

        return $"{majar}.{minor}.{patch}";
    }

    return "0.0.1";
}
