#if !V1_29_1
using UnityEngine;

public class SharedCoroutineStarter : MonoBehaviour
{
    static MonoBehaviour _instance = null;

    public static MonoBehaviour instance => _instance ??= new GameObject().AddComponent<SharedCoroutineStarter>();

    void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}
#endif