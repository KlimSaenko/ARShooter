using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance = null;

    private bool alive = true;

    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                var managers = FindObjectsOfType<T>();

                if (managers != null)
                {
                    if (managers.Length == 1)
                    {
                        instance = managers[0];
                        DontDestroyOnLoad(instance);
                        return instance;
                    }
                    else if (managers.Length > 1)
                    {
                        foreach (var manager in managers)
                        {
                            Destroy(manager.gameObject);
                        }
                    }
                }
                
                var go = new GameObject(typeof(T).Name, typeof(T));
                instance = go.GetComponent<T>();
                instance.Init();
                DontDestroyOnLoad(instance.gameObject);

                return instance;
            }
        }

        set
        {
            instance = value;
        }
    }

    /// <summary>
    /// Check flag if need work from OnDestroy or OnApplicationExit
    /// </summary>
    public static bool IsAlive
    {
        get
        {
            if (instance == null)
                return false;
            return instance.alive;
        }
    }

    private protected void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this as T;
            Init();
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    private protected void OnDestroy() { alive = false; }

    private protected void OnApplicationQuit() { alive = false; }

    private protected virtual void Init() { }
}
