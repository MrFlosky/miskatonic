using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class CreditsRoller : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The parent Credits GameObject (contains the CanvasGroup + Scroller).")]
    public GameObject creditsRoot;

    [Tooltip("The Scroller child inside Credits that holds all the credit entries.")]
    public RectTransform scroller;

    [Tooltip("The Home screen GameObject with your main menu UI.")]
    public GameObject homeScreen;

    [Tooltip("Button on the main screen that opens the credits.")]
    public Button creditsButton;

    [Tooltip("The X button shown during credits that closes them early.")]
    public Button closeButton;

    [Header("Scroll Settings")]
    [Tooltip("Starting Y position of the Scroller (below the screen). Usually negative, e.g. -600.")]
    public float scrollStartY = -700f;

    [Tooltip("Ending Y position of the Scroller (above the screen). Usually positive, e.g. 1800.")]
    public float scrollEndY = 1800f;

    [Tooltip("How many seconds the credits take to scroll from start to end.")]
    public float scrollDuration = 20f;

    [Header("Fade Settings")]
    [Tooltip("How long the credits fade in (seconds).")]
    public float fadeInDuration = 1.2f;

    [Tooltip("How long the credits fade out before returning home (seconds).")]
    public float fadeOutDuration = 1f;

    // -------------------------------------------------------------------------

    private CanvasGroup _creditsCanvasGroup;
    private Coroutine _creditsCoroutine;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Grabs Canvas on Credits object to fade
        if (creditsRoot != null)
        {
            _creditsCanvasGroup = creditsRoot.GetComponent<CanvasGroup>();
            if (_creditsCanvasGroup == null)
                _creditsCanvasGroup = creditsRoot.AddComponent<CanvasGroup>();
        }

        // Hide credits, show home on startup.
        SetCreditsVisible(false, instant: true);
        if (homeScreen != null) homeScreen.SetActive(true);
    }

    private void Start()
    {
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCredits);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCredits);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void OpenCredits()
    {
        if (_creditsCoroutine != null)
            StopCoroutine(_creditsCoroutine);

        _creditsCoroutine = StartCoroutine(RunCredits());
    }

    public void CloseCredits()
    {
        if (_creditsCoroutine != null)
        {
            StopCoroutine(_creditsCoroutine);
            _creditsCoroutine = null;
        }

        StartCoroutine(FadeOutAndReturnHome());
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator RunCredits()
    {
        // --- Prepare ---
        // Reset scroller position BEFORE activating the credits object so it's never seen snapping.
        if (scroller != null)
        {
            Vector2 pos = scroller.anchoredPosition;
            pos.y = scrollStartY;
            scroller.anchoredPosition = pos;
        }

        if (homeScreen != null) homeScreen.SetActive(false);
        SetCreditsVisible(true, instant: false);

        // --- Fade In ---
        yield return StartCoroutine(FadeCanvasGroup(_creditsCanvasGroup, 0f, 1f, fadeInDuration));

        // --- Scroll ---
        float elapsed = 0f;
        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);

            if (scroller != null)
            {
                Vector2 pos = scroller.anchoredPosition;
                pos.y = Mathf.Lerp(scrollStartY, scrollEndY, t);
                scroller.anchoredPosition = pos;
            }

            yield return null;
        }

        // --- Finished naturally → fade out & return home ---
        yield return StartCoroutine(FadeOutAndReturnHome());
    }

    private IEnumerator FadeOutAndReturnHome()
    {
        yield return StartCoroutine(FadeCanvasGroup(_creditsCanvasGroup, _creditsCanvasGroup.alpha, 0f, fadeOutDuration));

        SetCreditsVisible(false, instant: true);

        if (homeScreen != null) homeScreen.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetCreditsVisible(bool visible, bool instant)
    {
        if (creditsRoot == null) return;

        creditsRoot.SetActive(visible);

        if (_creditsCanvasGroup != null)
        {
            _creditsCanvasGroup.alpha = instant ? (visible ? 1f : 0f) : 0f;
            _creditsCanvasGroup.interactable = visible;
            _creditsCanvasGroup.blocksRaycasts = visible;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        cg.alpha = to;
    }
}