// GameAutoSetup.cs
// Automatically runs the Iron Bastion bootstrap the first time Unity
// finishes compiling after this file is imported.

using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class GameAutoSetup
{
    private const string DoneKey = "IronBastion_BootstrapDone";
    private const string ScenePath = "Assets/_Project/Scenes/Production/MainGame.unity";

    static GameAutoSetup()
    {
        // Only run if the scene doesn't exist yet and we haven't run before this session
        if (!SessionState.GetBool(DoneKey, false) && !System.IO.File.Exists(ScenePath))
        {
            SessionState.SetBool(DoneKey, true);
            EditorApplication.delayCall += RunBootstrap;
        }
    }

    private static void RunBootstrap()
    {
        UnityEngine.Debug.Log("[GameAutoSetup] Detected missing scene — running bootstrap...");
        IronBastionBootstrap.BuildCompleteGame();
    }
}
