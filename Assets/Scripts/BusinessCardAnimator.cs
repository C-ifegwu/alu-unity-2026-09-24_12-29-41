using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls entrance and idle floating animations for the AR business card elements.
/// Elements animate smoothly into place when the marker is acquired and reset when lost.
/// </summary>
public class BusinessCardAnimator : MonoBehaviour
{
    /// <summary>
    /// List of button transforms to animate in an orbital fan-out.
    /// </summary>
    [Tooltip("The social button transforms.")]
    public List<Transform> buttonTransforms = new List<Transform>();

    /// <summary>
    /// Transform of the name text element.
    /// </summary>
    [Tooltip("Transform of the name text header.")]
    public Transform nameTransform;

    /// <summary>
    /// Transform of the job title text element.
    /// </summary>
    [Tooltip("Transform of the profession/title text.")]
    public Transform titleTransform;

    /// <summary>
    /// Duration of the entrance spring animation in seconds.
    /// </summary>
    [Tooltip("Time taken for entrance animation.")]
    public float entranceDuration = 0.8f;

    /// <summary>
    /// Amplitude of the idle floating motion.
    /// </summary>
    [Tooltip("Magnitude of subtle idle hover.")]
    public float hoverAmplitude = 0.005f;

    /// <summary>
    /// Speed of the idle floating hover oscillation.
    /// </summary>
    [Tooltip("Frequency of idle hover oscillation.")]
    public float hoverFrequency = 2.0f;

    /// <summary>
    /// Starts the entrance animation for all card elements.
    /// </summary>
    public void PlayIntroAnimation()
    {
        if (activeAnimationCoroutine != null)
        {
            StopCoroutine(activeAnimationCoroutine);
        }
        activeAnimationCoroutine = StartCoroutine(EntranceAnimationRoutine());
    }

    /// <summary>
    /// Resets all card elements to their initial collapsed state.
    /// </summary>
    public void ResetAnimation()
    {
        if (activeAnimationCoroutine != null)
        {
            StopCoroutine(activeAnimationCoroutine);
            activeAnimationCoroutine = null;
        }

        isIdleFloating = false;

        // Reset buttons to center and zero scale
        for (int i = 0; i < buttonTransforms.Count; i++)
        {
            if (buttonTransforms[i] != null && i < targetButtonPositions.Count)
            {
                buttonTransforms[i].localPosition = Vector3.zero;
                buttonTransforms[i].localScale = Vector3.zero;
            }
        }

        // Reset text positions and scale
        if (nameTransform != null)
        {
            nameTransform.localPosition = targetNamePos + new Vector3(0.05f, 0f, 0f);
            nameTransform.localScale = Vector3.zero;
        }

        if (titleTransform != null)
        {
            titleTransform.localPosition = targetTitlePos + new Vector3(0.05f, 0f, 0f);
            titleTransform.localScale = Vector3.zero;
        }
    }

    // Cached target local positions for buttons
    private List<Vector3> targetButtonPositions = new List<Vector3>();

    // Cached target local scales for buttons
    private List<Vector3> targetButtonScales = new List<Vector3>();

    // Cached target local position for name text
    private Vector3 targetNamePos;

    // Cached target local position for title text
    private Vector3 targetTitlePos;

    // Cached target scale for name text
    private Vector3 targetNameScale = Vector3.one;

    // Cached target scale for title text
    private Vector3 targetTitleScale = Vector3.one;

    // Currently running coroutine
    private Coroutine activeAnimationCoroutine;

    // Whether the intro is finished and idle floating is active
    private bool isIdleFloating = false;

    // Time counter for idle floating
    private float hoverTimer = 0f;

    // Caches initial transform values on awake
    private void Awake()
    {
        CacheInitialTransforms();
    }

    // Automatically trigger intro animation when enabled
    private void OnEnable()
    {
        ResetAnimation();
        PlayIntroAnimation();
    }

    // Handles idle floating motion on active buttons
    private void Update()
    {
        if (!isIdleFloating)
        {
            return;
        }

        hoverTimer += Time.deltaTime * hoverFrequency;

        for (int i = 0; i < buttonTransforms.Count; i++)
        {
            if (buttonTransforms[i] != null && i < targetButtonPositions.Count)
            {
                // Unique phase offset per button
                float offset = i * 1.2f;
                float yDelta = Mathf.Sin(hoverTimer + offset) * hoverAmplitude;
                float zDelta = Mathf.Cos(hoverTimer + offset) * (hoverAmplitude * 0.5f);

                Vector3 basePos = targetButtonPositions[i];
                buttonTransforms[i].localPosition = new Vector3(basePos.x, basePos.y + yDelta, basePos.z + zDelta);
            }
        }
    }

    // Records the target layout positions configured in the scene editor
    private void CacheInitialTransforms()
    {
        targetButtonPositions.Clear();
        targetButtonScales.Clear();

        foreach (Transform btn in buttonTransforms)
        {
            if (btn != null)
            {
                targetButtonPositions.Add(btn.localPosition);
                targetButtonScales.Add(btn.localScale);
            }
        }

        if (nameTransform != null)
        {
            targetNamePos = nameTransform.localPosition;
            targetNameScale = nameTransform.localScale;
        }

        if (titleTransform != null)
        {
            targetTitlePos = titleTransform.localPosition;
            targetTitleScale = titleTransform.localScale;
        }
    }

    // Coroutine that orchestrates the smooth spring entrance animation
    private IEnumerator EntranceAnimationRoutine()
    {
        float elapsed = 0f;

        while (elapsed < entranceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);

            // Elastic ease out calculation
            float easeT = EvaluateEaseOutBack(t);

            // Animate buttons fanning out with staggered delays
            for (int i = 0; i < buttonTransforms.Count; i++)
            {
                if (buttonTransforms[i] != null && i < targetButtonPositions.Count)
                {
                    float buttonDelay = i * 0.08f;
                    float normalizedBtnTime = Mathf.Clamp01((elapsed - buttonDelay) / (entranceDuration - 0.2f));
                    float btnEase = EvaluateEaseOutBack(normalizedBtnTime);

                    buttonTransforms[i].localPosition = Vector3.LerpUnclamped(Vector3.zero, targetButtonPositions[i], btnEase);
                    buttonTransforms[i].localScale = Vector3.LerpUnclamped(Vector3.zero, targetButtonScales[i], btnEase);
                }
            }

            // Animate text sliding into place
            if (nameTransform != null)
            {
                float textEase = EvaluateEaseOutCubic(t);
                nameTransform.localPosition = Vector3.Lerp(targetNamePos + new Vector3(0.04f, 0f, 0f), targetNamePos, textEase);
                nameTransform.localScale = Vector3.Lerp(Vector3.zero, targetNameScale, textEase);
            }

            if (titleTransform != null)
            {
                float titleTime = Mathf.Clamp01((elapsed - 0.1f) / (entranceDuration - 0.1f));
                float titleEase = EvaluateEaseOutCubic(titleTime);
                titleTransform.localPosition = Vector3.Lerp(targetTitlePos + new Vector3(0.04f, 0f, 0f), targetTitlePos, titleEase);
                titleTransform.localScale = Vector3.Lerp(Vector3.zero, targetTitleScale, titleEase);
            }

            yield return null;
        }

        // Snap precisely to target transforms
        for (int i = 0; i < buttonTransforms.Count; i++)
        {
            if (buttonTransforms[i] != null && i < targetButtonPositions.Count)
            {
                buttonTransforms[i].localPosition = targetButtonPositions[i];
                buttonTransforms[i].localScale = targetButtonScales[i];
            }
        }

        if (nameTransform != null)
        {
            nameTransform.localPosition = targetNamePos;
            nameTransform.localScale = targetNameScale;
        }

        if (titleTransform != null)
        {
            titleTransform.localPosition = targetTitlePos;
            titleTransform.localScale = targetTitleScale;
        }

        isIdleFloating = true;
        activeAnimationCoroutine = null;
    }

    // Ease-out back curve for pleasant bouncy pop-in
    private float EvaluateEaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    // Cubic ease-out curve for text transitions
    private float EvaluateEaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}

