using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Central audio for CombatZone: BGM, gunshot, win/lose stingers, celebrate fireworks.
/// Assign clips in the Inspector, or run menu <b>Combat Zone ▸ Bake audio into CombatZone scene</b> once.
/// </summary>
[DefaultExecutionOrder(-500)]
public class CombatZoneAudioManager : MonoBehaviour
{
    public static CombatZoneAudioManager Instance;

    [Header("Clips (optional if assigned via Editor menu)")]
    public AudioClip backgroundMusic;
    public AudioClip gunshot;
    public AudioClip sadTrombone;
    public AudioClip victory;
    public AudioClip fireworks;

    [Range(0f, 1f)] public float musicVolume = 0.55f;
    [Range(0f, 1f)] public float sfxVolume = 0.95f;

    AudioSource _music;
    AudioSource _sfx;
    bool _bgmRoutineRunning;
#if UNITY_EDITOR
    bool _diskBgmLoadStarted;
#endif

    void Awake()
    {
        Instance = this;
        EnsureSources();
#if UNITY_EDITOR
        TryLoadClipsFromDefaultPathsEditor();
#else
        TryLoadClipsFromResources();
#endif
    }

    void Start()
    {
#if UNITY_EDITOR
        TryLoadClipsFromDefaultPathsEditor();
#else
        TryLoadClipsFromResources();
#endif
        if (backgroundMusic == null)
            Debug.LogWarning(
                "[CombatZoneAudio] backgroundMusic is not assigned. In the Editor, use menu " +
                "<b>Combat Zone ▸ Bake audio into CombatZone scene</b> so builds and the scene file reference the clips.");
        var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (listeners.Length == 0)
            Debug.LogWarning("[CombatZoneAudio] No AudioListener in the scene. Add one on the active camera (XR rig usually includes it).");
        // Ambient BGM: start with play mode, not after UI setup.
        RequestStartAmbientMusic();
    }

    /// <summary>
    /// Starts looping ambient BGM as soon as possible. Safe to call multiple times (e.g. from <see cref="UIManager"/>).
    /// </summary>
    public void BeginBackgroundMusicAfterUiReady() => RequestStartAmbientMusic();

    void RequestStartAmbientMusic()
    {
#if UNITY_EDITOR
        TryLoadClipsFromDefaultPathsEditor();
#else
        TryLoadClipsFromResources();
#endif
        if (backgroundMusic != null)
        {
            if (_bgmRoutineRunning) return;
            if (_music != null && _music.isPlaying) return;
            _bgmRoutineRunning = true;
            StartCoroutine(StartBackgroundMusicWhenReady());
            return;
        }

#if UNITY_EDITOR
        if (!_diskBgmLoadStarted)
        {
            _diskBgmLoadStarted = true;
            StartCoroutine(LoadBackgroundMusicFromDiskThenPlay());
        }
#else
        LogBgmMissingOnce();
#endif
    }

    /// <summary>
    /// Waits one frame after UI init, then ensures clip data is ready before <see cref="PlayBackgroundMusic"/>.
    /// </summary>
    IEnumerator StartBackgroundMusicWhenReady()
    {
        if (_music == null || backgroundMusic == null)
        {
            _bgmRoutineRunning = false;
            yield break;
        }

        yield return null;

        // Streaming: never call LoadAudioData / wait on Loading — Unity can keep Loading until Play(),
        // or LoadAudioData can fail and leave Failed with no playback.
        if (backgroundMusic.loadType == AudioClipLoadType.Streaming)
        {
            // Long streaming clips (especially WAV + loadInBackground) may not begin until decode is ready;
            // a single Play() can leave the source silent until we retry after a short wait.
            for (var attempt = 0; attempt < 4; attempt++)
            {
                PlayBackgroundMusic();
                var waited = 0f;
                while (_music != null && !_music.isPlaying && waited < 2.5f)
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (_music != null && _music.isPlaying)
                    break;
            }

            if (_music != null && !_music.isPlaying)
                Debug.LogWarning(
                    "[CombatZoneAudio] BGM did not start after retries. In the clip Import Settings, try " +
                    "<b>Load Type: Compressed In Memory</b> (recommended for looping music), or confirm no Audio Mixer is muting this bus.");
            _bgmRoutineRunning = false;
            yield break;
        }

        if (backgroundMusic.loadState == AudioDataLoadState.Unloaded)
            backgroundMusic.LoadAudioData();
        for (var t = 0f; t < 60f && backgroundMusic.loadState == AudioDataLoadState.Loading; t += Time.unscaledDeltaTime)
            yield return null;

        if (backgroundMusic.loadState == AudioDataLoadState.Failed)
        {
            Debug.LogWarning(
                "[CombatZoneAudio] backgroundMusic failed to load. In the Inspector, set this clip to Load Type: Streaming.");
            _bgmRoutineRunning = false;
            yield break;
        }

        PlayBackgroundMusic();
        _bgmRoutineRunning = false;
    }

    void EnsureSources()
    {
        if (_music == null)
        {
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.spatialBlend = 0f;
            _music.ignoreListenerPause = true;
            // When many one-shots play, low-priority sources can be virtualized; keep BGM audible.
            _music.priority = 0;
        }
        if (_sfx == null)
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.loop = false;
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
        }
        _music.volume = musicVolume;
        _sfx.volume = sfxVolume;
    }

    static bool _loggedBgmMissing;

    void LogBgmMissingOnce()
    {
        if (_loggedBgmMissing) return;
        _loggedBgmMissing = true;
#if UNITY_EDITOR
        var guidPath = UnityEditor.AssetDatabase.GUIDToAssetPath(GuidBgm);
        var disk = Path.Combine(Application.dataPath, BgmPathRelativeToAssetsFolder);
        Debug.LogWarning(
            "[CombatZoneAudio] BGM missing after load + reimport. Check the Console for <b>AudioImporter</b> errors on that .wav. " +
            "GUID→path: \"" + guidPath + "\". File on disk exists: " + File.Exists(disk) + ". " +
            "You can also run <b>Combat Zone ▸ Bake audio into CombatZone scene</b>.");
#else
        Debug.LogWarning(
            "[CombatZoneAudio] BGM missing. Bake the scene, or add clips under Resources/CombatZoneAudio/ (see TryLoadClipsFromResources).");
#endif
    }

#if !UNITY_EDITOR
    const string ResourcesAudioRoot = "CombatZoneAudio/";

    void TryLoadClipsFromResources()
    {
        if (backgroundMusic == null)
            backgroundMusic = Resources.Load<AudioClip>(ResourcesAudioRoot + "BgmTrips");
        if (gunshot == null)
            gunshot = Resources.Load<AudioClip>(ResourcesAudioRoot + "Gunshot");
        if (sadTrombone == null)
            sadTrombone = Resources.Load<AudioClip>(ResourcesAudioRoot + "SadTrombone");
        if (victory == null)
            victory = Resources.Load<AudioClip>(ResourcesAudioRoot + "Victory");
        if (fireworks == null)
            fireworks = Resources.Load<AudioClip>(ResourcesAudioRoot + "Fireworks");
    }
#endif

#if UNITY_EDITOR
    const string SoundsRoot = "Assets/Scripts/CombatZone/Sounds/";
    /// <summary>Default ambient BGM asset file (under <see cref="SoundsRoot"/>; same GUID as <see cref="GuidBgm"/>).</summary>
    const string BgmFilename = "Best songs to play shooting games! - Trips (128k).wav";
    const string BgmAssetPath = SoundsRoot + BgmFilename;
    /// <summary>Path under the project <c>Assets</c> folder; combine with <see cref="Application.dataPath"/> for disk I/O.</summary>
    const string BgmPathRelativeToAssetsFolder = "Scripts/CombatZone/Sounds/" + BgmFilename;
    const string GuidBgm = "97868e1f0d00d9343a00b866fcad6cb1";
    const string GuidGunshot = "ebaff7c25ae4d5c4ca765f9e31a40d97";
    const string GuidSadTrombone = "fe3ef97ac202a3146870e7b66810022d";
    const string GuidVictory = "9f8d382acf92b784da7f6cbc17484cb2";
    const string GuidFireworks = "849b2c469a3d9764999fa764df9688f3";
    static bool _refreshedAssetDbForAudio;

    void TryLoadClipsFromDefaultPathsEditor()
    {
        TryLoadClipsFromDefaultPathsEditorCore();
        if (backgroundMusic != null) return;
        if (_refreshedAssetDbForAudio) return;
        _refreshedAssetDbForAudio = true;
        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
        TryLoadClipsFromDefaultPathsEditorCore();
    }

    /// <summary>
    /// When the asset database cannot expose an <see cref="AudioClip"/> (importer issues, huge PCM, etc.),
    /// load the same bytes from disk for play mode only (Editor: under <c>Assets/</c>).
    /// </summary>
    IEnumerator LoadBackgroundMusicFromDiskThenPlay()
    {
        var full = Path.Combine(Application.dataPath, BgmPathRelativeToAssetsFolder);
        if (!File.Exists(full))
        {
            Debug.LogWarning("[CombatZoneAudio] BGM disk fallback: file not found at \"" + full + "\".");
            LogBgmMissingOnce();
            yield break;
        }

        var uri = new Uri(full);
        var url = uri.AbsoluteUri;
        AudioClip clip = null;
        foreach (var audioType in new[] { AudioType.WAV, AudioType.MPEG })
        {
            var handler = new DownloadHandlerAudioClip(url, audioType);
            handler.streamAudio = true;
            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, handler, null);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) continue;
            clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip != null && clip.length > 0f) break;
            clip = null;
        }

        if (clip == null || clip.length <= 0f)
        {
            Debug.LogWarning(
                "[CombatZoneAudio] BGM disk fallback: could not decode as WAV or MPEG. Try re-encoding to standard WAV or Ogg.");
            LogBgmMissingOnce();
            yield break;
        }

        clip.name = "BGM (ambient, disk)";
        backgroundMusic = clip;
        if (_bgmRoutineRunning) yield break;
        _bgmRoutineRunning = true;
        StartCoroutine(StartBackgroundMusicWhenReady());
    }

    void TryLoadClipsFromDefaultPathsEditorCore()
    {
        if (backgroundMusic == null)
            backgroundMusic = LoadClipByGuidOrPath(GuidBgm, SoundsRoot + "Best songs to play shooting games! - Trips (128k).wav");
        if (gunshot == null)
            gunshot = LoadClipByGuidOrPath(
                GuidGunshot, SoundsRoot + "Gunshot Sound Effect Single Shot - Birthday Song (128k).wav");
        if (sadTrombone == null)
            sadTrombone = LoadClipByGuidOrPath(
                GuidSadTrombone, SoundsRoot + "Sad Trombone - Sound Effect (HD) - Gaming Sound FX (128k).wav");
        if (victory == null)
            victory = LoadClipByGuidOrPath(
                GuidVictory, SoundsRoot + "Victory Sound Effect - Der Test (128k).wav");
        if (fireworks == null)
            fireworks = LoadClipByGuidOrPath(
                GuidFireworks, SoundsRoot + "Sound Effects - Fireworks - Effecism (128k).wav");
    }

    static AudioClip LoadClipByGuidOrPath(string guid, string fallbackAssetPath)
    {
        var byGuid = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(byGuid))
        {
            var c = LoadAudioClipAtPath(byGuid);
            if (c != null) return c;
        }
        return LoadAudioClipAtPath(fallbackAssetPath);
    }

    static AudioClip LoadAudioClipAtPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        if (clip != null) return clip;
        var main = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (main is AudioClip acMain) return acMain;
        foreach (var o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (o is AudioClip ac) return ac;
        }
        foreach (var o in UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath))
        {
            if (o is AudioClip ac) return ac;
        }
        return null;
    }
#endif

    public void PlayBackgroundMusic()
    {
#if UNITY_EDITOR
        TryLoadClipsFromDefaultPathsEditor();
#else
        TryLoadClipsFromResources();
#endif
        if (_music == null || backgroundMusic == null) return;
        _music.enabled = true;
        _music.Stop();
        _music.clip = backgroundMusic;
        _music.time = 0f;
        _music.loop = true;
        _music.volume = musicVolume;
        _music.mute = false;
        _music.Play();
    }

    public void StopBackgroundMusic()
    {
        if (_music == null) return;
        _music.Stop();
    }

    public void PlayGunshot()
    {
        if (_sfx == null || gunshot == null) return;
        _sfx.PlayOneShot(gunshot, sfxVolume);
    }

    public void PlayVictory()
    {
        if (_sfx == null || victory == null) return;
        _sfx.PlayOneShot(victory, sfxVolume);
    }

    public void PlaySadTrombone()
    {
        if (_sfx == null || sadTrombone == null) return;
        _sfx.PlayOneShot(sadTrombone, sfxVolume);
    }

    public void PlayFireworks()
    {
        if (_sfx == null || fireworks == null) return;
        _sfx.PlayOneShot(fireworks, sfxVolume);
    }
}
