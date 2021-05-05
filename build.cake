#addin nuget:?package=Cake.Git&version=1.0.1

using System.Text.RegularExpressions;
using System.Linq;

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

var solution = "WpfExtensions.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
});

Task("Build")
    .Description("Bump version to next.")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var targetDir = Environment.CurrentDirectory;
    var latestTag = GitTags(targetDir).LastOrDefault();
    // var logs = GitLogTag(targetDir, latestTag.FriendlyName);
    var logs = GitLogTag(targetDir, "1.0.0");

    var prevAuthor = logs.Last().Author;
    var nextVersion = GetBumpVersion(latestTag.FriendlyName, IsBumpMinor(logs));
    var releaseNotes = GetReleaseNotes(logs);

    var releaseNotesPath = System.IO.Path.Combine(targetDir, "Release Notes.txt");
    var releaseNotesText = $"{nextVersion}{Environment.NewLine}{releaseNotes}{Environment.NewLine}{Environment.NewLine}";
    if (FileExists(releaseNotesPath)) {
        var prevText = System.IO.File.ReadAllText(releaseNotesPath);
        System.IO.File.WriteAllText(releaseNotesPath, $"{releaseNotesText}{prevText}");
    } else {
        System.IO.File.WriteAllText(releaseNotesPath, releaseNotesText);
    }

    GitAddAll(targetDir);
    GitCommit(targetDir, prevAuthor.Name, prevAuthor.Email, $"Bump version to {nextVersion}");
    GitTag(targetDir, nextVersion);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// FUNCTIONS
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

private static string GetBumpVersion(string version, bool bumpMinor = false, string pattern = null) {
    var regex = new Regex(pattern ?? @"v?(\d+?)\.(\d+?)\.(\d+?)", RegexOptions.Compiled);
    var match = regex.Match(version);

    if (match.Success) {
        var majar = int.Parse(match.Groups[1].Value);
        var minor = int.Parse(match.Groups[2].Value);
        var patch = int.Parse(match.Groups[3].Value);

        if (bumpMinor) {
            minor++;
        } else {
            patch++;
        }

        return $"{majar}.{minor}.{patch}";
    }

    return "0.0.0";
}
