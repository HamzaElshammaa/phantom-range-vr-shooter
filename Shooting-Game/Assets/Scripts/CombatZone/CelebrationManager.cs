using UnityEngine;

public class CelebrationManager : MonoBehaviour
{
    public static CelebrationManager Instance;
    public ParticleSystem confettiSystem;

    void Awake() { Instance = this; }

    public void Celebrate()
    {
        if (confettiSystem) confettiSystem.Play();
    }
}
