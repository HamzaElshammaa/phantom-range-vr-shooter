using UnityEngine;

public class CelebrationManager : MonoBehaviour
{
    public static CelebrationManager Instance;
    public ParticleSystem confettiSystem;
    [Tooltip("Spawn confetti in front of the main camera if the system is far from the player.")]
    public bool positionNearCamera = true;
    public float cameraForwardOffset = 2.5f;
    public float cameraUpOffset = 0.4f;

    void Awake() { Instance = this; }

    public void Celebrate()
    {
        if (!confettiSystem) return;

        if (positionNearCamera && Camera.main != null)
        {
            Transform t = confettiSystem.transform;
            Transform cam = Camera.main.transform;
            t.position = cam.position + cam.forward * cameraForwardOffset + cam.up * cameraUpOffset;
            t.rotation = cam.rotation;
        }

        var main = confettiSystem.main;
        main.playOnAwake = false;

        var rend = confettiSystem.GetComponent<ParticleSystemRenderer>();
        if (rend != null && rend.sharedMaterial == null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh == null) sh = Shader.Find("Particles/Standard Unlit");
            if (sh == null) sh = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (sh != null)
                rend.sharedMaterial = new Material(sh) { color = Color.white };
        }

        confettiSystem.Clear();
        confettiSystem.Play();
        CombatZoneAudioManager.Instance?.PlayFireworks();
        CombatZoneDebugLog.Log("Celebrate", "confetti");
    }
}
