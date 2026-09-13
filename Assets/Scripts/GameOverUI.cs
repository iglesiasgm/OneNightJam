using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.8f;

    private bool isShowing = false;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowGameOver(float delay)
    {
        if (isShowing)
            return;

        isShowing = true;

        StartCoroutine(
            ShowGameOverRoutine(delay)
        );
    }

    private IEnumerator ShowGameOverRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            canvasGroup.alpha =
                Mathf.Clamp01(
                    elapsed / fadeDuration
                );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}