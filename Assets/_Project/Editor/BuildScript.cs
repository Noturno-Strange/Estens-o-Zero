using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ExtensaoZero.EditorTools
{
    /// <summary>
    /// Build Linux x86_64 em Mono. Executável em batch mode. Não usa IL2CPP.
    /// </summary>
    public static class BuildScript
    {
        private const string OutputDirectory = "Builds/Linux";
        private const string OutputName = "ExtensaoZero.x86_64";

        [MenuItem("Extensao Zero/Build Linux")]
        public static void BuildLinux()
        {
            Directory.CreateDirectory(OutputDirectory);
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneLinux64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Bootstrap.unity" },
                locationPathName = Path.Combine(OutputDirectory, OutputName),
                target = BuildTarget.StandaloneLinux64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[EZ] Build {summary.result}: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime})");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Build Linux falhou: {summary.result} ({summary.totalErrors} erros).");
            }
        }

        [MenuItem("Extensao Zero/Build Linux (Development)")]
        public static void BuildLinuxDevelopment()
        {
            Directory.CreateDirectory(OutputDirectory);
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneLinux64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Bootstrap.unity" },
                locationPathName = Path.Combine(OutputDirectory, "ExtensaoZero-dev.x86_64"),
                target = BuildTarget.StandaloneLinux64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
            });

            Debug.Log($"[EZ] Build dev {report.summary.result}: {report.summary.outputPath}");
        }
    }
}
