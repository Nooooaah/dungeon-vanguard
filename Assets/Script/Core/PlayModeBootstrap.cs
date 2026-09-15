using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures the intro video plays first when entering Play Mode from MainMenu in the Editor.
/// For builds, scene 0 (IntroVideo) already plays first automatically.
/// </summary>
public static class PlayModeBootstrap
{
    private static bool _introPlayed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RedirectToIntro()
    {
        var scene = SceneManager.GetActiveScene();
        if (!_introPlayed && scene.name == "MainMenu")
        {
            _introPlayed = true;
            SceneManager.LoadScene("IntroVideo");
        }
    }
}
