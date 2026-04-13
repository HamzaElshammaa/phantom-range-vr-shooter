using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace VRShootingGame
{
    /// <summary>
    /// Optional hook for loading a scene after a teleport to this <see cref="TeleportationArea"/> completes.
    /// Leave <see cref="m_LoadSceneOnTeleport"/> disabled until you are ready to transition (e.g. to CombatZone).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TeleportationArea))]
    public class TeleportSceneTransitionHook : MonoBehaviour
    {
        [SerializeField]
        bool m_LoadSceneOnTeleport;

        [SerializeField]
        string m_NextSceneName = "CombatZone";

        TeleportationArea m_TeleportationArea;

        void Awake()
        {
            m_TeleportationArea = GetComponent<TeleportationArea>();
        }

        void OnEnable()
        {
            if (m_TeleportationArea != null)
                m_TeleportationArea.teleporting.AddListener(OnTeleporting);
        }

        void OnDisable()
        {
            if (m_TeleportationArea != null)
                m_TeleportationArea.teleporting.RemoveListener(OnTeleporting);
        }

        void OnTeleporting(TeleportingEventArgs args)
        {
            if (!m_LoadSceneOnTeleport)
                return;
            if (string.IsNullOrEmpty(m_NextSceneName))
                return;
            SceneManager.LoadScene(m_NextSceneName);
        }

        /// <summary>Enable or disable loading <see cref="m_NextSceneName"/> after a successful teleport (e.g. after training + quiz).</summary>
        public void SetLoadSceneOnTeleport(bool enabled)
        {
            m_LoadSceneOnTeleport = enabled;
        }
    }
}
