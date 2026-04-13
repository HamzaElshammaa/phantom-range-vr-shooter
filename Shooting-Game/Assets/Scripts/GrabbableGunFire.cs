using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRShootingGame
{
    /// <summary>
    /// Hitscan shots from <see cref="m_Muzzle"/> when the grabbable is <see cref="XRGrabInteractable.activated"/> (trigger).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabbableGunFire : MonoBehaviour
    {
        [SerializeField]
        XRGrabInteractable m_GrabInteractable;

        [SerializeField]
        Transform m_Muzzle;

        [SerializeField]
        float m_FireCooldown = 0.15f;

        [SerializeField]
        float m_MaxDistance = 100f;

        [SerializeField]
        LayerMask m_HitLayerMask = ~0;

        [SerializeField]
        RaycastHitUnityEvent m_OnHit;

        [SerializeField]
        AudioClip m_FireSound;

        [SerializeField]
        AudioSource m_FireAudioSource;

        TutorialManager m_TutorialManager;

        float m_LastFireTime;

        void Reset()
        {
            m_GrabInteractable = GetComponent<XRGrabInteractable>();
        }

        void Awake()
        {
            if (m_GrabInteractable == null)
                m_GrabInteractable = GetComponent<XRGrabInteractable>();
            EnsureFireAudioSource();
        }

        void EnsureFireAudioSource()
        {
            if (m_FireAudioSource != null)
                return;
            if (m_Muzzle != null)
                m_FireAudioSource = m_Muzzle.GetComponent<AudioSource>();
            if (m_FireAudioSource == null)
            {
                var attach = m_Muzzle != null ? m_Muzzle.gameObject : gameObject;
                m_FireAudioSource = attach.AddComponent<AudioSource>();
            }

            m_FireAudioSource.playOnAwake = false;
            m_FireAudioSource.spatialBlend = 1f;
            m_FireAudioSource.minDistance = 0.5f;
            m_FireAudioSource.maxDistance = 40f;
        }

        void OnEnable()
        {
            if (m_GrabInteractable == null)
                return;
            m_GrabInteractable.activated.AddListener(OnActivated);
            m_GrabInteractable.deactivated.AddListener(OnDeactivated);
        }

        void OnDisable()
        {
            if (m_GrabInteractable == null)
                return;
            m_GrabInteractable.activated.RemoveListener(OnActivated);
            m_GrabInteractable.deactivated.RemoveListener(OnDeactivated);
        }

        void OnActivated(ActivateEventArgs args)
        {
            TryFire();
        }

        void OnDeactivated(DeactivateEventArgs args)
        {
        }

        void TryFire()
{
    if (m_Muzzle == null) return;
    if (Time.time - m_LastFireTime < m_FireCooldown) return;

    m_LastFireTime = Time.time;
    
    // Play sound
    if (m_FireSound != null && m_FireAudioSource != null)
        m_FireAudioSource.PlayOneShot(m_FireSound);

    if (m_TutorialManager == null)
        m_TutorialManager = FindFirstObjectByType<TutorialManager>();
    m_TutorialManager?.RegisterShot();

    Vector3 fireDirection = Camera.main.transform.forward;

    if (Physics.Raycast(m_Muzzle.position, fireDirection, out RaycastHit hit, m_MaxDistance, m_HitLayerMask, QueryTriggerInteraction.Ignore))
    {
        m_OnHit?.Invoke(hit);
        m_TutorialManager?.RegisterHit(hit);

        var targetHit = hit.collider.GetComponentInParent<TargetHit>();
        if (targetHit != null)
            targetHit.OnHit();

        Debug.Log($"Hit: {hit.collider.name}");
    }
}

        [System.Serializable]
        public class RaycastHitUnityEvent : UnityEvent<RaycastHit>
        {
        }
    }
}
