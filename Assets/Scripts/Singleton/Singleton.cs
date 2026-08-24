using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T: MonoBehaviour{
    
    public static T Instance {  get; private set; }
    protected virtual void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(Instance);
            return;
        }
        Instance = this as T;
    }

    protected virtual void OnAplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }
}
