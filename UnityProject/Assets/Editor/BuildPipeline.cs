using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public static class GameBuildPipeline
{
    public static void BuildAll()
    {
        GraphicsSettings.defaultRenderPipeline = null;
        MatchSceneBuilder.BuildMatchScene();

        string[] scenes = new string[]
        {
            "Assets/Scenes/Match.unity"
        };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/fifaGame.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        UnityEditor.BuildPipeline.BuildPlayer(options);
    }
}