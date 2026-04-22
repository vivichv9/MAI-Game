using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MAIGame.EditorTools
{
    /// <summary>
    /// Provides repeatable Windows builds for milestone verification.
    /// </summary>
    public static class BuildPipelineRunner
    {
        private const string BuildDirectory = "Builds/Windows";
        private const string BuildExecutable = "EchoPuzzleGame.exe";

        [MenuItem("MAI Game/Build/Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory(BuildDirectory);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(BuildDirectory, BuildExecutable),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Windows build failed: {report.summary.result}");
            }
        }
    }
}
