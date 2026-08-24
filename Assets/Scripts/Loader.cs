using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader {

    public enum Scene {
        BootScene,
        DuelScene,
        TestingConnectionScene,
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene) {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene) {
        Loader.targetScene = targetScene;

        NetworkManager.Singleton.SceneManager.LoadScene(Loader.targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoadCallback() {
        SceneManager.LoadScene(targetScene.ToString());
    }

}
