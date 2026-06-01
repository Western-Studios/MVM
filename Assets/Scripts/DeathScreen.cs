using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the death screen overlay.
/// Wire PlayerHealth.onReadyToRespawn → DeathScreen.Show in the Inspector.
/// Assign the panel, playerHealth reference, and both buttons in the Inspector.
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject   panel;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Button       respawnButton;
    [SerializeField] private Button       mainMenuButton;
    [Tooltip("Name of the main menu scene. Must match exactly what's in Build Settings.")]
    [SerializeField] private string       mainMenuSceneName = "MainMenu";

    private Vector3 startPosition;

    private void Awake()
    {
        panel.SetActive(false);

        if (playerHealth != null)
            startPosition = playerHealth.transform.position;

        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    // Called by PlayerHealth.onReadyToRespawn (wire in Inspector).
    public void Show()
    {
        panel.SetActive(true);
    }

    private void OnRespawnClicked()
    {
        panel.SetActive(false);

        Vector3 spawnPos = CheckpointManager.Instance != null
            ? CheckpointManager.Instance.GetRespawnPosition(startPosition)
            : startPosition;

        playerHealth?.Respawn(spawnPos);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f; // safety reset in case anything paused the game
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
