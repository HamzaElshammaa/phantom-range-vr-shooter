using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

namespace VRShootingGame
{
    /// <summary>
    /// Unified tutorial + quiz controller.
    /// Flow: Start Menu → Look → Move → Grab → Find → Hit (×5) → Success → Quiz → Exit Unlock.
    /// Replaces both the old TutorialManager and TrainingRangeChallenge.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject m_StartMenuPanel;
        [SerializeField] GameObject m_ChecklistPanel;
        [SerializeField] GameObject m_SuccessPanel;
        [SerializeField] GameObject m_QuizPanel;
        [SerializeField] GameObject m_PauseMenuPanel;

        [Header("Start Menu Buttons")]
        [SerializeField] Button m_PlayTutorialButton;
        [SerializeField] Button m_PracticeGunButton;

        [Header("Checklist Labels")]
        [SerializeField] TextMeshProUGUI m_LookLabel;
        [SerializeField] TextMeshProUGUI m_MoveLabel;
        [SerializeField] TextMeshProUGUI m_GrabLabel;
        [SerializeField] TextMeshProUGUI m_FindLabel;
        [SerializeField] TextMeshProUGUI m_HitLabel;
        [SerializeField] TextMeshProUGUI m_InstructionText;

        [Header("Success Panel")]
        [SerializeField] Button m_TakeQuizButton;

        [Header("Quiz UI")]
        [SerializeField] TextMeshProUGUI m_QuizQuestionText;
        [SerializeField] Button[] m_QuizAnswerButtons;
        [SerializeField] TextMeshProUGUI[] m_QuizAnswerLabels;
        [SerializeField] TextMeshProUGUI m_QuizFeedbackText;

        [Header("Quiz Content")]
        [SerializeField] [TextArea(2, 5)]
        string m_Question = "When are you allowed to move downrange in a live shooting range?";
        [SerializeField] string[] m_Answers = {
            "Whenever you want",
            "Only when the range is cold, cleared, and authorized",
            "When you run out of ammunition",
            "During active firing for any reason"
        };
        [SerializeField] [Range(0, 3)] int m_CorrectAnswerIndex = 1;

        [Header("Pause Menu Buttons")]
        [SerializeField] Button m_ResumeButton;
        [SerializeField] Button m_ExitTutorialButton;

        [Header("References")]
        [SerializeField] Transform m_Gun;
        [SerializeField] Transform m_TargetArea;
        [SerializeField] TeleportSceneTransitionHook m_ExitSceneHook;

        [Header("Thresholds")]
        [SerializeField] float m_LookAngleThreshold = 30f;
        [SerializeField] float m_MoveDistanceThreshold = 1f;
        [SerializeField] float m_FindDotThreshold = 0.7f;
        [SerializeField] int m_HitsRequired = 5;

        // States: 0=StartMenu, 1=Look, 2=Move, 3=Grab, 4=Find, 5=Hit, 6=Success, 7=Quiz, 8=Done
        int m_State = -1;
        Quaternion m_InitialCamRot;
        Vector3 m_InitialCamPos;
        bool m_GunGrabbed;
        int m_HitCount;
        int m_TotalShots;
        float m_TimerStart = -1f;
        float m_TimerFrozenAt = -1f;
        bool m_Paused;
        bool m_QuizComplete;

        static readonly string Unchecked = "○  ";
        static readonly string Checked   = "●  ";

        void Awake()
        {
            // Button listeners
            if (m_PlayTutorialButton != null)
                m_PlayTutorialButton.onClick.AddListener(OnPlayTutorial);
            if (m_PracticeGunButton != null)
                m_PracticeGunButton.onClick.AddListener(OnPracticeGun);
            if (m_ResumeButton != null)
                m_ResumeButton.onClick.AddListener(OnResume);
            if (m_ExitTutorialButton != null)
                m_ExitTutorialButton.onClick.AddListener(OnExitTutorial);
            if (m_TakeQuizButton != null)
                m_TakeQuizButton.onClick.AddListener(OnTakeQuiz);

            // Wire quiz answer buttons
            if (m_QuizAnswerButtons != null)
            {
                for (int i = 0; i < m_QuizAnswerButtons.Length; i++)
                {
                    int idx = i;
                    if (m_QuizAnswerButtons[i] != null)
                        m_QuizAnswerButtons[i].onClick.AddListener(() => OnAnswerClicked(idx));
                }
            }

            // Listen to gun grab
            if (m_Gun != null)
            {
                var grab = m_Gun.GetComponent<XRGrabInteractable>();
                if (grab != null)
                    grab.selectEntered.AddListener(OnGunGrabbed);
            }

            // Listen to target hits
            var targets = FindObjectsByType<TargetHit>(FindObjectsSortMode.None);
            foreach (var t in targets)
                t.onHitCallback += OnTargetHit;

            // Lock exit until quiz is passed
            if (m_ExitSceneHook == null)
                m_ExitSceneHook = FindFirstObjectByType<TeleportSceneTransitionHook>();
            if (m_ExitSceneHook != null)
                m_ExitSceneHook.SetLoadSceneOnTeleport(false);

            ShowState0();
        }

        void OnDestroy()
        {
            var targets = FindObjectsByType<TargetHit>(FindObjectsSortMode.None);
            foreach (var t in targets)
                t.onHitCallback -= OnTargetHit;
        }

        // ══════════════════════════════════════
        // State transitions
        // ══════════════════════════════════════

        void ShowState0()
        {
            m_State = 0;
            SetPanel(start: true);
        }

        void EnterState(int state)
        {
            m_State = state;
            SetPanel(checklist: true);

            switch (state)
            {
                case 1:
                    m_InitialCamRot = Camera.main.transform.rotation;
                    SetInstruction("Look around by moving your head.");
                    break;
                case 2:
                    m_InitialCamPos = Camera.main.transform.position;
                    SetInstruction("Move toward the gun using WASD or teleport.");
                    break;
                case 3:
                    SetInstruction("Grab the gun with the Grip button (G).");
                    break;
                case 4:
                    SetInstruction("Look toward the targets downrange.");
                    break;
                case 5:
                    m_HitCount = 0;
                    m_TotalShots = 0;
                    m_TimerStart = -1f;
                    m_TimerFrozenAt = -1f;
                    SetInstruction($"Shoot the targets! ({m_HitCount}/{m_HitsRequired})");
                    break;
            }

            RefreshChecklist();
        }

        void CompleteState(int state)
        {
            RefreshChecklist();
            if (state < 5)
                EnterState(state + 1);
            else
                ShowSuccess();
        }

        void ShowSuccess()
        {
            m_State = 6;
            if (m_TimerFrozenAt < 0f && m_TimerStart > 0f)
                m_TimerFrozenAt = Time.time;
            SetPanel(success: true);
        }

        void ShowQuiz()
        {
            m_State = 7;
            SetPanel(quiz: true);
            SetupQuizUi();
        }

        void ShowDone()
        {
            m_State = 8;
            // Unlock exit teleport
            if (m_ExitSceneHook != null)
                m_ExitSceneHook.SetLoadSceneOnTeleport(true);
            SetPanel(success: true);
        }

        // ══════════════════════════════════════
        // Update checks
        // ══════════════════════════════════════

        void Update()
        {
            if (m_Paused) return;

            switch (m_State)
            {
                case 1:
                    if (Camera.main != null)
                    {
                        float angle = Quaternion.Angle(m_InitialCamRot, Camera.main.transform.rotation);
                        if (angle >= m_LookAngleThreshold)
                            CompleteState(1);
                    }
                    break;

                case 2:
                    if (Camera.main != null)
                    {
                        float dist = Vector3.Distance(m_InitialCamPos, Camera.main.transform.position);
                        if (dist >= m_MoveDistanceThreshold)
                            CompleteState(2);
                    }
                    break;

                case 3:
                    if (m_GunGrabbed)
                        CompleteState(3);
                    break;

                case 4:
                    if (Camera.main != null && m_TargetArea != null)
                    {
                        Vector3 toTarget = (m_TargetArea.position - Camera.main.transform.position).normalized;
                        float dot = Vector3.Dot(Camera.main.transform.forward, toTarget);
                        if (dot >= m_FindDotThreshold)
                            CompleteState(4);
                    }
                    break;
            }
        }

        // ══════════════════════════════════════
        // Public methods called by GrabbableGunFire
        // ══════════════════════════════════════

        /// <summary>Called by GrabbableGunFire on every trigger pull.</summary>
        public void RegisterShot()
        {
            m_TotalShots++;
            if (m_TimerStart < 0f)
                m_TimerStart = Time.time;
        }

        /// <summary>Called by GrabbableGunFire when raycast hits a collider.</summary>
        public void RegisterHit(RaycastHit hit)
        {
            var target = hit.collider.GetComponentInParent<ScoreTarget>();
            if (target != null && target.TryConsumeScore())
            {
                // ScoreTarget handles once-only logic
            }
        }

        // ══════════════════════════════════════
        // Event callbacks
        // ══════════════════════════════════════

        void OnGunGrabbed(SelectEnterEventArgs args)
        {
            m_GunGrabbed = true;
        }

        void OnTargetHit()
        {
            if (m_State != 5) return;
            m_HitCount++;
            SetInstruction($"Shoot the targets! ({m_HitCount}/{m_HitsRequired})");
            RefreshChecklist();
            if (m_HitCount >= m_HitsRequired)
                CompleteState(5);
        }

        // ══════════════════════════════════════
        // Button callbacks
        // ══════════════════════════════════════

        void OnPlayTutorial()
        {
            EnterState(1);
        }

        void OnPracticeGun()
        {
            m_State = -1;
            SetPanel();
        }

        void OnTakeQuiz()
        {
            ShowQuiz();
        }

        public void TogglePause()
        {
            if (m_State == 0 || m_State >= 6) return;
            m_Paused = !m_Paused;
            if (m_PauseMenuPanel != null)
                m_PauseMenuPanel.SetActive(m_Paused);
        }

        void OnResume()
        {
            m_Paused = false;
            if (m_PauseMenuPanel != null)
                m_PauseMenuPanel.SetActive(false);
        }

        void OnExitTutorial()
        {
            m_Paused = false;
            ShowState0();
        }

        // ══════════════════════════════════════
        // Quiz
        // ══════════════════════════════════════

        void SetupQuizUi()
        {
            if (m_QuizQuestionText != null)
                m_QuizQuestionText.text = m_Question;

            for (int i = 0; i < 4; i++)
            {
                if (m_QuizAnswerLabels != null && i < m_QuizAnswerLabels.Length && m_QuizAnswerLabels[i] != null)
                {
                    string label = i < m_Answers.Length ? m_Answers[i] : string.Empty;
                    char letter = (char)('A' + i);
                    m_QuizAnswerLabels[i].text = $"{letter}. {label}";
                }
            }

            if (m_QuizFeedbackText != null)
                m_QuizFeedbackText.text = "Select the correct answer.";
        }

        void OnAnswerClicked(int index)
        {
            if (m_QuizComplete) return;

            if (index == m_CorrectAnswerIndex)
            {
                m_QuizComplete = true;
                if (m_QuizFeedbackText != null)
                    m_QuizFeedbackText.text = "<color=green>Correct!</color> Step on the exit pad to enter the combat zone.";

                // Brief delay then show done state
                Invoke(nameof(ShowDone), 2f);
            }
            else
            {
                if (m_QuizFeedbackText != null)
                    m_QuizFeedbackText.text = "<color=red>Not quite</color> — try another answer.";
            }
        }

        // ══════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════

        void SetPanel(bool start = false, bool checklist = false, bool success = false, bool quiz = false, bool pause = false)
        {
            if (m_StartMenuPanel != null) m_StartMenuPanel.SetActive(start);
            if (m_ChecklistPanel != null) m_ChecklistPanel.SetActive(checklist);
            if (m_SuccessPanel != null) m_SuccessPanel.SetActive(success);
            if (m_QuizPanel != null) m_QuizPanel.SetActive(quiz);
            if (m_PauseMenuPanel != null) m_PauseMenuPanel.SetActive(pause);
        }

        void SetInstruction(string text)
        {
            if (m_InstructionText != null)
                m_InstructionText.text = text;
        }

        void RefreshChecklist()
        {
            SetLabel(m_LookLabel, "Look Around", m_State > 1);
            SetLabel(m_MoveLabel, "Move to the Gun", m_State > 2);
            SetLabel(m_GrabLabel, "Grab the Gun", m_State > 3);
            SetLabel(m_FindLabel, "Find the Targets", m_State > 4);
            SetLabel(m_HitLabel, $"Hit Targets ({m_HitCount}/{m_HitsRequired})", m_State > 5);
        }

        void SetLabel(TextMeshProUGUI label, string text, bool done)
        {
            if (label == null) return;
            label.text = (done ? Checked : Unchecked) + text;
            label.color = done ? Color.green : Color.white;
        }
    }
}
