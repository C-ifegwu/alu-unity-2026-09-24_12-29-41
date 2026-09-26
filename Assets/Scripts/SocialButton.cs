using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles interactive social and contact links on the AR business card.
/// Provides visual and audio feedback when pressed and opens the URL.
/// </summary>
public class SocialButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// The URL or mailto link to open when the button is activated.
    /// </summary>
    [Tooltip("The URL to open when pressed.")]
    public string targetUrl = "https://github.com";

    /// <summary>
    /// Audio clip played when the button is clicked.
    /// </summary>
    [Tooltip("Sound played on button press.")]
    public AudioClip clickSound;

    /// <summary>
    /// Scale factor applied during button press for visual punch feedback.
    /// </summary>
    [Tooltip("Scale factor when pressed.")]
    public float pressScaleFactor = 0.9f;

    /// <summary>
    /// Optional custom color to highlight the button on press.
    /// </summary>
    [Tooltip("Color applied to the graphic when pressed.")]
    public Color pressedColor = new Color(1f, 0.7f, 0.7f, 1f);

    /// <summary>
    /// Opens the configured URL and triggers feedback.
    /// </summary>
    public void OpenLink()
    {
        ExecuteClickFeedback();

        if (!string.IsNullOrEmpty(targetUrl))
        {
            Debug.Log($"Opening URL: {targetUrl}");
            Application.OpenURL(targetUrl);
        }
        else
        {
            Debug.LogWarning("Target URL is null or empty on SocialButton.");
        }
    }

    /// <summary>
    /// Triggers audio and visual feedback without opening the link.
    /// </summary>
    public void PlayFeedback()
    {
        ExecuteClickFeedback();
    }

    /// <summary>
    /// Handles Unity UI pointer click events.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        OpenLink();
    }

    /// <summary>
    /// Handles Unity UI pointer down events for visual press feedback.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressedVisuals(true);
    }

    /// <summary>
    /// Handles Unity UI pointer up events to release visual press feedback.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        SetPressedVisuals(false);
    }

    // Original local scale of the button transform
    private Vector3 initialScale;

    // Original graphic color
    private Color initialColor;

    // Attached Image component if available
    private Image buttonImage;

    // Attached AudioSource component
    private AudioSource audioSource;

    // Active animation coroutine
    private Coroutine feedbackCoroutine;

    // Initialization of components and initial properties
    private void Awake()
    {
        initialScale = transform.localScale;

        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            initialColor = buttonImage.color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D UI sound
        }

        // Connect Unity UI Button onClick if present
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OpenLink);
        }
    }

    // Executes audio and visual bounce feedback
    private void ExecuteClickFeedback()
    {
        // Play audio click feedback
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // Run visual bounce animation
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(BounceAnimationRoutine());
    }

    // Toggles visual pressed state
    private void SetPressedVisuals(bool isPressed)
    {
        if (buttonImage != null)
        {
            buttonImage.color = isPressed ? pressedColor : initialColor;
        }

        if (isPressed)
        {
            transform.localScale = initialScale * pressScaleFactor;
        }
        else
        {
            transform.localScale = initialScale;
        }
    }

    // Coroutine for a smooth punch scale animation on click
    private IEnumerator BounceAnimationRoutine()
    {
        Vector3 punchScale = initialScale * pressScaleFactor;
        float duration = 0.15f;
        float elapsed = 0f;

        // Shrink
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(initialScale, punchScale, t);
            yield return null;
        }

        elapsed = 0f;
        // Bounce back with slight overshoot
        Vector3 overshootScale = initialScale * 1.08f;
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            transform.localScale = Vector3.Lerp(punchScale, overshootScale, t);
            yield return null;
        }

        elapsed = 0f;
        // Return to normal
        while (elapsed < duration * 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.2f);
            transform.localScale = Vector3.Lerp(overshootScale, initialScale, t);
            yield return null;
        }

        transform.localScale = initialScale;
        if (buttonImage != null)
        {
            buttonImage.color = initialColor;
        }
        feedbackCoroutine = null;
    }
}
