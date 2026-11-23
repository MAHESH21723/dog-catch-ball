using UnityEditor;
using UnityEngine;

/// Builds the browser version into docs/ so GitHub Pages can host it from the main branch.
/// Headless: Unity.exe -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod WebBuild.Build
public static class WebBuild
{
    [MenuItem("Play Fetch/Build WebGL to docs")]
    public static void Build()
    {
        PlayerSettings.productName = "Play Fetch";
        PlayerSettings.companyName = "Maheshwaran";
        // GitHub Pages cannot send Content-Encoding headers, so let the loader decompress in the browser.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.SplashScreen.show = false;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Challenge 2/Challenge 2.unity" },
            target = BuildTarget.WebGL,
            locationPathName = "docs",
            options = BuildOptions.None
        });
        Debug.Log("FETCH_BUILD " + report.summary.result + " " + report.summary.totalSize);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(2);
    }
}
