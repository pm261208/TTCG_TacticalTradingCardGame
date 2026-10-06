using UnityEngine;

public class BootMenu : MonoBehaviour{
    void Start(){
#if !UNITY_SERVER && !UNITY_STANDALONE_SERVER
        Loader.Load(Loader.Scene.MainMenuScene);
#endif
    }

}
