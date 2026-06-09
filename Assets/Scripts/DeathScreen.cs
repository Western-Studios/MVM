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

    [Tooltip("The room the player starts in — used as camera fallback when no checkpoint is set.")]
    [SerializeField] private Room startRoom;

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
        Time.timeScale = 0f;
    }

    private void OnRespawnClicked()
    {
        Time.timeScale = 1f;
        panel.SetActive(false);

        Vector3 spawnPos = CheckpointManager.Instance != null
            ? CheckpointManager.Instance.GetRespawnPosition(startPosition)
            : startPosition;

        playerHealth?.Respawn(spawnPos);

        Room spawnRoom = CheckpointManager.Instance?.GetRespawnRoom() ?? startRoom;
        spawnRoom?.Activate();
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f; // safety reset in case anything paused the game
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
