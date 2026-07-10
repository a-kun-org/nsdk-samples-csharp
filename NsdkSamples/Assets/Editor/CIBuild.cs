using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

/// <summary>
/// CI entry points for building the NSDK samples as Android APKs.
/// Invoked via -executeMethod from GitHub Actions (see .github/workflows).
///
///   CIBuild.BuildAndroid : phone AR flavor (Nsdk AR Core Loader)
///   CIBuild.BuildQuest   : Meta Quest flavor (OpenXR loader + Meta Quest Support)
///
/// Output path is taken from -customBuildPath &lt;path.apk&gt;.
/// Set NSDK_DEVELOPMENT_BUILD=true for a development build.
/// </summary>
public static class CIBuild
{
    private const string ArCoreLoaderAsset = "Assets/XR/Loaders/Nsdk AR Core Loader.asset";
    private const string OpenXRLoaderAsset = "Assets/XR/Loaders/OpenXRLoader.asset";

    public static void BuildAndroid()
    {
        ConfigureAndroidLoaders(new[] { ArCoreLoaderAsset });
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        Build("Android");
    }

    public static void BuildQuest()
    {
        ConfigureAndroidLoaders(new[] { OpenXRLoaderAsset });
        EnableOpenXRFeatures(
            "com.unity.openxr.feature.metaquest",
            "com.unity.openxr.feature.input.oculustouchcontroller");
        // Quest devices are ARM64 only.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        Build("Quest");
    }

    private static void Build(string flavor)
    {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle = false;

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = GetOutputPath(flavor),
            target = BuildTarget.Android,
            options = IsDevelopmentBuild() ? BuildOptions.Development : BuildOptions.None,
        };

        Debug.Log($"[CIBuild] Building {flavor} -> {options.locationPathName} " +
                  $"({options.scenes.Length} scenes, dev={IsDevelopmentBuild()})");

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception(
                $"[CIBuild] {flavor} build failed: {report.summary.result} " +
                $"({report.summary.totalErrors} errors)");
        }

        Debug.Log($"[CIBuild] {flavor} build succeeded: {report.summary.totalSize} bytes");
    }

    /// <summary>
    /// Replaces the XR Plug-in Management loader list for the Android target so
    /// each flavor initializes the right XR stack (ARCore for phones, OpenXR for Quest).
    /// </summary>
    private static void ConfigureAndroidLoaders(IEnumerable<string> loaderAssetPaths)
    {
        if (!EditorBuildSettings.TryGetConfigObject(
                XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget perTarget))
        {
            throw new Exception("[CIBuild] XRGeneralSettingsPerBuildTarget not found in EditorBuildSettings.");
        }

        var androidSettings = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
        if (androidSettings == null || androidSettings.Manager == null)
        {
            throw new Exception("[CIBuild] XR general settings for Android not found.");
        }

        var loaders = new List<XRLoader>();
        foreach (var path in loaderAssetPaths)
        {
            var loader = AssetDatabase.LoadAssetAtPath<XRLoader>(path);
            if (loader == null)
            {
                throw new Exception($"[CIBuild] XR loader asset not found: {path}");
            }
            loaders.Add(loader);
        }

        if (!androidSettings.Manager.TrySetLoaders(loaders))
        {
            throw new Exception("[CIBuild] Failed to assign XR loaders for Android.");
        }

        EditorUtility.SetDirty(androidSettings.Manager);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CIBuild] Android XR loaders set to: {string.Join(", ", loaderAssetPaths)}");
    }

    private static void EnableOpenXRFeatures(params string[] featureIds)
    {
        FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
        foreach (var id in featureIds)
        {
            var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, id);
            if (feature == null)
            {
                throw new Exception($"[CIBuild] OpenXR feature not found: {id}");
            }
            feature.enabled = true;
            EditorUtility.SetDirty(feature);
            Debug.Log($"[CIBuild] OpenXR feature enabled: {id}");
        }
        AssetDatabase.SaveAssets();
    }

    private static string GetOutputPath(string flavor)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-customBuildPath")
            {
                return args[i + 1];
            }
        }
        return $"Builds/{flavor}/NsdkSamples-{flavor}.apk";
    }

    private static bool IsDevelopmentBuild()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("NSDK_DEVELOPMENT_BUILD"),
            "true", StringComparison.OrdinalIgnoreCase);
    }
}
