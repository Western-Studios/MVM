using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Player SFX")]
    [SerializeField] private AudioClip arcaneBlastSFX;
    [SerializeField] private AudioClip fireballSFX;
    [SerializeField] private AudioClip iceBoltSFX;
    [SerializeField] private AudioClip teleportSFX;
    [SerializeField] private AudioClip playerHurtSFX;
    [SerializeField] private AudioClip playerDeathSFX;

    [Header("Boss SFX")]
    [SerializeField] private AudioClip bossAttackSFX;
    [SerializeField] private AudioClip bossHurtSFX;
    [SerializeField] private AudioClip bossDeathSFX;
    [SerializeField] private AudioClip bossPhase2SFX;

    [Header("Environment SFX")]
    [SerializeField] private AudioClip trapDropSFX;
    [SerializeField] private AudioClip trapImpactSFX;
    [SerializeField] private AudioClip checkpointSFX;
    [SerializeField] private AudioClip abilityPickupSFX;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource                    = gameObject.AddComponent<AudioSource>();
        musicSource.loop               = true;
        musicSource.outputAudioMixerGroup = musicGroup;

        sfxSource                      = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup   = sfxGroup;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        => PlayMusicForScene(scene.name);

    private void PlayMusicForScene(string sceneName)
        => PlayMusic(sceneName == "MainMenu" ? menuMusic : gameMusic);

    // ── Music ─────────────────────────────────────────────────────────────
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayBossMusic() => PlayMusic(bossMusic);

    // ── Player ────────────────────────────────────────────────────────────
    public void PlayArcaneBlast()   => PlaySFX(arcaneBlastSFX);
    public void PlayFireball()      => PlaySFX(fireballSFX);
    public void PlayIceBolt()       => PlaySFX(iceBoltSFX);
    public void PlayTeleport()      => PlaySFX(teleportSFX);
    public void PlayPlayerHurt()    => PlaySFX(playerHurtSFX);
    public void PlayPlayerDeath()   => PlaySFX(playerDeathSFX);

    // ── Boss ──────────────────────────────────────────────────────────────
    public void PlayBossAttack()    => PlaySFX(bossAttackSFX);
    public void PlayBossHurt()      => PlaySFX(bossHurtSFX);
    public void PlayBossDeath()     => PlaySFX(bossDeathSFX);
    public void PlayBossPhase2()    => PlaySFX(bossPhase2SFX);

    // ── Environment ───────────────────────────────────────────────────────
    public void PlayTrapDrop()      => PlaySFX(trapDropSFX);
    public void PlayTrapImpact()    => PlaySFX(trapImpactSFX);
    public void PlayCheckpoint()    => PlaySFX(checkpointSFX);
    public void PlayAbilityPickup() => PlaySFX(abilityPickupSFX);

    // ── UI ────────────────────────────────────────────────────────────────
    public void PlayButtonClick()   => PlaySFX(buttonClickSFX);

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}
