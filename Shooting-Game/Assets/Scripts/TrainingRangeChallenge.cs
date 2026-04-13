using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRShootingGame
{
    /// <summary>
    /// Tracks hits on <see cref="ScoreTarget"/>, shots, timer, accuracy; shows a multiple-choice quiz after the hit goal; unlocks exit scene transition.
    /// </summary>
    [DisallowMultipleComponent]
    public class TrainingRangeChallenge : MonoBehaviour
    {
        [Header("Goal")]
        [SerializeField]
        [Min(1)]
        int m_HitsToPass = 3;

        [Header("UI — stats (optional; auto-find ScoreText / InstructionsText)")]
        [SerializeField]
        Text m_ScoreLineText;

        [SerializeField]
        Text m_InstructionsText;

        [Header("Quiz UI (optional; built at runtime if missing)")]
        [SerializeField]
        GameObject m_QuizPanel;

        [SerializeField]
        Text m_QuizQuestionText;

        [SerializeField]
        Button[] m_QuizAnswerButtons;

        [SerializeField]
        Text[] m_QuizAnswerLabels;

        [SerializeField]
        Text m_QuizFeedbackText;

        [Header("Quiz content")]
        [SerializeField]
        [TextArea(2, 5)]
        string m_Question = "When are you allowed to move downrange in a live shooting range?";

        [SerializeField]
        string[] m_Answers =
        {
            "Whenever you want",
            "Only when the range is cold, cleared, and authorized",
            "When you run out of ammunition",
            "During active firing for any reason"
        };

        [SerializeField]
        [Range(0, 3)]
        int m_CorrectAnswerIndex = 1;

        [Header("Exit")]
        [SerializeField]
        TeleportSceneTransitionHook m_ExitSceneHook;

        [SerializeField]
        string m_ExitUnlockedMessage = "Quiz passed. Step on the exit pad to load the next area.";

        int m_ValidHits;
        int m_Shots;
        float m_TimerStart = -1f;
        float m_TimerFrozenAt = -1f;
        bool m_SkillComplete;
        bool m_QuizComplete;

        void Reset()
        {
            if (m_QuizPanel != null)
                m_QuizPanel.SetActive(false);
        }

        void Awake()
        {
            BindUiReferences();
            if (m_ExitSceneHook == null)
                m_ExitSceneHook = FindFirstObjectByType<TeleportSceneTransitionHook>();

            if (m_ExitSceneHook != null)
                m_ExitSceneHook.SetLoadSceneOnTeleport(false);

            if (m_QuizPanel == null)
                BuildRuntimeQuizUi();

            if (m_QuizPanel != null)
                m_QuizPanel.SetActive(false);
            if (m_QuizFeedbackText != null)
                m_QuizFeedbackText.text = string.Empty;

            RefreshStatsDisplay();
        }

        void BindUiReferences()
        {
            if (m_ScoreLineText == null)
            {
                var t = transform.Find("ScoreText");
                if (t != null)
                    m_ScoreLineText = t.GetComponent<Text>();
            }

            if (m_InstructionsText == null)
            {
                var t = transform.Find("InstructionsText");
                if (t != null)
                    m_InstructionsText = t.GetComponent<Text>();
            }
        }

        void BuildRuntimeQuizUi()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                return;

            var panel = new GameObject("QuizPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(transform, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.05f, 0.15f);
            panelRt.anchorMax = new Vector2(0.95f, 0.85f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var img = panel.GetComponent<Image>();
            img.color = new Color(0.05f, 0.06f, 0.1f, 0.97f);
            img.raycastTarget = true;

            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var qGo = new GameObject("QuizQuestion", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            qGo.transform.SetParent(panel.transform, false);
            m_QuizQuestionText = qGo.GetComponent<Text>();
            m_QuizQuestionText.fontSize = 26;
            m_QuizQuestionText.color = Color.white;
            m_QuizQuestionText.alignment = TextAnchor.UpperLeft;
            m_QuizQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            m_QuizQuestionText.verticalOverflow = VerticalWrapMode.Overflow;
            var qLe = qGo.AddComponent<LayoutElement>();
            qLe.minHeight = 80f;
            qLe.flexibleHeight = 0f;

            m_QuizAnswerButtons = new Button[4];
            m_QuizAnswerLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                var btnGo = new GameObject($"Answer{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(panel.transform, false);
                var btn = btnGo.GetComponent<Button>();
                var btnImg = btnGo.GetComponent<Image>();
                btnImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);
                btn.targetGraphic = btnImg;
                m_QuizAnswerButtons[i] = btn;

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelGo.transform.SetParent(btnGo.transform, false);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(12, 6);
                labelRt.offsetMax = new Vector2(-12, -6);
                var label = labelGo.GetComponent<Text>();
                label.font = m_QuizQuestionText.font;
                label.fontSize = 22;
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleLeft;
                m_QuizAnswerLabels[i] = label;

                var le = btnGo.AddComponent<LayoutElement>();
                le.minHeight = 48f;

                int idx = i;
                btn.onClick.AddListener(() => OnAnswerClicked(idx));
            }

            var fbGo = new GameObject("QuizFeedback", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            fbGo.transform.SetParent(panel.transform, false);
            m_QuizFeedbackText = fbGo.GetComponent<Text>();
            m_QuizFeedbackText.font = m_QuizQuestionText.font;
            m_QuizFeedbackText.fontSize = 22;
            m_QuizFeedbackText.color = new Color(1f, 0.85f, 0.35f);
            m_QuizFeedbackText.alignment = TextAnchor.MiddleCenter;
            var fbLe = fbGo.AddComponent<LayoutElement>();
            fbLe.minHeight = 36f;

            m_QuizPanel = panel;
        }

        void Update()
        {
            if (m_SkillComplete || m_TimerStart < 0f)
                return;
            RefreshStatsDisplay();
        }

        /// <summary>Called by <see cref="GrabbableGunFire"/> once per fired round (after cooldown).</summary>
        public void RegisterShot()
        {
            m_Shots++;
            if (m_TimerStart < 0f)
                m_TimerStart = Time.time;
            RefreshStatsDisplay();
        }

        /// <summary>Called by <see cref="GrabbableGunFire"/> when the raycast hits any collider.</summary>
        public void RegisterHit(RaycastHit hit)
        {
            var target = hit.collider.GetComponentInParent<ScoreTarget>();
            if (target != null && target.TryConsumeScore())
            {
                m_ValidHits++;
                if (m_ValidHits >= m_HitsToPass && !m_SkillComplete)
                    OnSkillGoalReached();
            }

            RefreshStatsDisplay();
        }

        void OnSkillGoalReached()
        {
            m_SkillComplete = true;
            if (m_TimerFrozenAt < 0f)
                m_TimerFrozenAt = Time.time;
            RefreshStatsDisplay();
            StartCoroutine(ShowQuizNextFrame());
        }

        IEnumerator ShowQuizNextFrame()
        {
            yield return null;
            if (m_QuizPanel != null)
            {
                m_QuizPanel.SetActive(true);
                SetupQuizUi();
            }
        }

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
            if (m_QuizComplete)
                return;

            if (index == m_CorrectAnswerIndex)
            {
                m_QuizComplete = true;
                if (m_QuizFeedbackText != null)
                    m_QuizFeedbackText.text = "Correct.";

                if (m_ExitSceneHook != null)
                    m_ExitSceneHook.SetLoadSceneOnTeleport(true);

                if (m_QuizPanel != null)
                    m_QuizPanel.SetActive(false);

                if (m_InstructionsText != null)
                    m_InstructionsText.text = m_ExitUnlockedMessage;
            }
            else
            {
                if (m_QuizFeedbackText != null)
                    m_QuizFeedbackText.text = "Not quite — try another answer.";
            }
        }

        void RefreshStatsDisplay()
        {
            if (m_ScoreLineText == null)
                return;

            float elapsed = GetElapsedTime();
            float acc = m_Shots > 0 ? 100f * m_ValidHits / m_Shots : 0f;
            string timeStr = m_TimerStart < 0f
                ? "—"
                : $"{elapsed:0.0}s";

            m_ScoreLineText.text =
                $"Hits: {m_ValidHits} / {m_HitsToPass}   |   Time: {timeStr}   |   Accuracy: {acc:0.0}% ({m_ValidHits}/{m_Shots} valid/total shots)";
        }

        float GetElapsedTime()
        {
            if (m_TimerStart < 0f)
                return 0f;
            float end = m_TimerFrozenAt > 0f ? m_TimerFrozenAt : Time.time;
            return end - m_TimerStart;
        }
    }
}
