using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAndroid
{
    private const string OutputDir = "Build";
    private const string ApkName = "SeerGo.apk";

    public static void Build()
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.qy.seer2");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
        PlayerSettings.stripEngineCode = true;

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;

        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        if (!Directory.Exists(OutputDir))
            Directory.CreateDirectory(OutputDir);
        string outputPath = Path.Combine(OutputDir, ApkName);

        BuildReport report = BuildPipeline.BuildPlayer(
            scenes.ToArray(), outputPath, BuildTarget.Android, BuildOptions.None);

        BuildSummary summary = report.summary;
        Debug.Log($"[BuildAndroid] result={summary.result} errors={summary.totalErrors} size={summary.totalSize} time={summary.totalTime} output={outputPath}");

        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("[BuildAndroid] Build failed");
            EditorApplication.Exit(1);
        }
    }
}
