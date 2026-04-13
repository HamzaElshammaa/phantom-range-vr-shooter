using UnityEngine;

namespace VRShootingGame
{
    /// <summary>
    /// Plays the assigned intro/ambient clip on loop for the whole scene.
    /// </summary>
    public class IntroSceneLoopAudio : MonoBehaviour
    {
        [SerializeField]
        AudioClip m_IntroClip;

        [SerializeField]
        [Range(0f, 1f)]
        float m_Volume = 0.75f;

        void Awake()
        {
            if (m_IntroClip == null)
                return;

            var src = gameObject.AddComponent<AudioSource>();
            src.clip = m_IntroClip;
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = m_Volume;
            src.Play();
        }
    }
}
