using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class VRChecklistItem : MonoBehaviour
{
    public Toggle checkbox;
    public TextMeshProUGUI taskText;
    [HideInInspector] public Color activeColor = new Color(0.16f, 0.85f, 0.95f, 1f);
    [HideInInspector] public Color lockedColor = new Color(0.10f, 0.28f, 0.34f, 1f);
    [HideInInspector] public Color doneColor = new Color(0.20f, 0.62f, 0.36f, 1f);
    private Image card;
    private void Awake()
    {
        card = GetComponent<Image>();
    }
    private void Start()
    {
        if (checkbox != null)
        {
            checkbox.onValueChanged.AddListener(onToggleValueChanged);
            updateVisuals();
        }
    }
    private void onToggleValueChanged(bool isOn)
    {
        updateVisuals();
        if (isOn)
        {
            checkbox.interactable = false;
            StartCoroutine(thanosSnapRoutine());
        }
    }
    public void setInteractable(bool isInteractable)
    {
        if (checkbox != null)
        {
            if (!checkbox.isOn)
            {
                checkbox.interactable = isInteractable;
                updateVisuals();
            }
        }
    }
    public void updateVisuals()
    {
        if (taskText == null || checkbox == null) return;
        if (checkbox.isOn)
        {
            taskText.fontStyle |= FontStyles.Strikethrough;
            taskText.color = new Color(0.93f, 0.98f, 0.94f, 1f);
            if (card != null)
                card.color = doneColor;
        }
        else if (checkbox.interactable)
        {
            taskText.fontStyle &= ~FontStyles.Strikethrough;
            taskText.color = new Color(0.04f, 0.09f, 0.11f, 1f);
            if (card != null)
                card.color = activeColor;
        }
        else
        {
            taskText.fontStyle &= ~FontStyles.Strikethrough;
            taskText.color = new Color(0.68f, 0.80f, 0.84f, 0.75f);
            if (card != null)
                card.color = lockedColor;
        }
    }
    private IEnumerator thanosSnapRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();
        RectTransform rect = GetComponent<RectTransform>();
        float startHeight = rect.rect.height;
        float duration = 1.0f; 
        float timer = 0f;
        Vector2 startPos = rect.anchoredPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            rect.anchoredPosition = startPos + new Vector2(Random.Range(-10f, 10f) * progress, 0);
            layoutElement.minHeight = Mathf.Lerp(startHeight, 0, progress);
            layoutElement.preferredHeight = Mathf.Lerp(startHeight, 0, progress);
            yield return null;
        }
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (checkbox != null)
        {
            checkbox.onValueChanged.RemoveListener(onToggleValueChanged);
        }
    }
}