using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }
    
    public void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool GetState(string id) => PlayerPrefs.GetInt(id, 0) == 1;

    public void SetState(string id, bool value)
    {
        PlayerPrefs.SetInt(id, value ? 1 :0);
        PlayerPrefs.Save();
    }

    public void ClearAll() => PlayerPrefs.DeleteAll();
}
