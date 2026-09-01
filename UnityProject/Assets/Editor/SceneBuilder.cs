using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SceneBuilder
{
    public static void CreateScenes()
    {
        // Create Boot scene
        Scene boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(boot, "Assets/Scenes/Boot.unity");

        // Create Menu scene
        Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(menu, "Assets/Scenes/Menu.unity");

        // Create Match scene
        Scene match = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(match, "Assets/Scenes/Match.unity");
    }

    public static void PerformBuild()
    {
        CreateScenes();

        string[] scenes = new string[]
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Match.unity"
        };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/fifaGame.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(options);
    }
}