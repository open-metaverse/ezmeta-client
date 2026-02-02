using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressUI : MonoBehaviour
{
    [SerializeField]
    private Image _barFill;

    [SerializeField]
    private TMP_Text _progressText;

    [SerializeField]
    private GameObject _root;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private float _completeHoldSeconds = 0.2f;

    private float _hideAfterTime;
    private bool _lastActive;

    private void Awake()
    {
        if (_root == null)
        {
            _root = gameObject;
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_barFill == null)
        {
            Transform fill = transform.Find("LoadingProgressBarBG/LoadingProgressBarFill");
            if (fill != null)
            {
                _barFill = fill.GetComponent<Image>();
            }
        }

        if (_progressText == null)
        {
            Transform text = transform.Find("LoadingProgressText");
            if (text != null)
            {
                _progressText = text.GetComponent<TMP_Text>();
            }
        }

        UpdateUI(0f, false);
    }

    private void OnEnable()
    {
        LoadingProgress.ProgressChanged += HandleProgressChanged;
        UpdateUI(LoadingProgress.Progress, LoadingProgress.IsActive);
    }

    private void OnDisable()
    {
        LoadingProgress.ProgressChanged -= HandleProgressChanged;
    }

    private void HandleProgressChanged(float progress, bool active)
    {
        UpdateUI(progress, active);
    }

    private void UpdateUI(float progress, bool active)
    {
        _lastActive = active;
        if (!active && progress >= 1f)
        {
            _hideAfterTime = Time.unscaledTime + _completeHoldSeconds;
        }

        bool visible = active || Time.unscaledTime < _hideAfterTime;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
        else if (_root != null)
        {
            _root.SetActive(visible);
        }

        if (_barFill != null)
        {
            _barFill.fillAmount = progress;
        }

        if (_progressText != null)
        {
            int percent = Mathf.RoundToInt(progress * 100f);
            _progressText.text = $"{percent}%";
        }
    }

    private void Update()
    {
        if (_lastActive)
        {
            return;
        }

        if (Time.unscaledTime < _hideAfterTime)
        {
            return;
        }

        if (_canvasGroup != null)
        {
            if (_canvasGroup.alpha > 0f)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
        else if (_root != null && _root.activeSelf)
        {
            _root.SetActive(false);
        }
    }
}
