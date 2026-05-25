using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

#if UNITY_EDITOR
    [Tooltip("Wipe all saved pickups every time you hit Play in the editor. " +
             "Turn this OFF when you want to test that persistence actually works.")]
    [SerializeField] private bool clearSavesOnPlay = true;
#endif

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        if (clearSavesOnPlay)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[PersistenceManager] Saves cleared for editor play session.");
        }
#endif
    }

    public bool GetState(string id) => PlayerPrefs.GetInt(id, 0) == 1;

    public void SetState(string id, bool value)
    {
        PlayerPrefs.SetInt(id, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ClearAll() => PlayerPrefs.DeleteAll();
}
