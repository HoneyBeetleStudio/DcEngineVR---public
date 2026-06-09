using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VRChecklistItem : MonoBehaviour
{
    [Header("UI Components")]
    public Toggle checkbox;
    public TextMeshProUGUI taskText;

    private void Start()
    {
        if (checkbox != null)
        {
            checkbox.onValueChanged.AddListener(OnToggleValueChanged);
            UpdateVisuals();
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        UpdateVisuals();
        
        if (isOn)
        {
            // Geri alınmasını engelle
            checkbox.interactable = false;
            // Yok olma (Thanos) efektini başlat
            StartCoroutine(ThanosSnapRoutine());
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        if (checkbox != null)
        {
            // Eğer görev çoktan yapıldıysa (yok oluyorsa) tekrar açılmasını engelle
            if (!checkbox.isOn)
            {
                checkbox.interactable = isInteractable;
                UpdateVisuals();
            }
        }
    }

    public void UpdateVisuals()
    {
        if (taskText == null || checkbox == null) return;

        if (checkbox.isOn)
        {
            // Yapıldı: Üstü çizili ve YEŞİL (Başarı hissi)
            taskText.fontStyle |= FontStyles.Strikethrough;
            taskText.color = new Color(0.1f, 0.7f, 0.1f, 1f); 
        }
        else
        {
            // Yapılmadı: Çizgiyi kaldır
            taskText.fontStyle &= ~FontStyles.Strikethrough;
            
            if (checkbox.interactable)
            {
                // Sıra bunda: Tam opak SİYAH (Kalemle yeni yazılmış gibi net)
                taskText.color = new Color(0f, 0f, 0f, 1f);
            }
            else
            {
                // Henüz sıra gelmedi: Çok soluk SİYAH (Gelecekte yazılacak gibi)
                taskText.color = new Color(0f, 0f, 0f, 0.2f);
            }
        }
    }

    private IEnumerator ThanosSnapRoutine()
    {
        // Kullanıcının tikin yeşil olduğunu algılayabilmesi için 0.6 saniye bekle
        yield return new WaitForSeconds(0.6f);

        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();

        RectTransform rect = GetComponent<RectTransform>();
        float startHeight = rect.rect.height;

        float duration = 1.0f; // 1 saniyede toza dönüş
        float timer = 0f;
        
        Vector2 startPos = rect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Yavaşça şeffaflaş (silinme/toz olma hissi)
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            // X ekseninde rastgele titreme (parçalanma hissi)
            rect.anchoredPosition = startPos + new Vector2(Random.Range(-10f, 10f) * progress, 0);

            // Yavaşça küçülerek (Vertical Layout Group'un diğer maddeleri yukarı kaydırmasını sağla)
            layoutElement.minHeight = Mathf.Lerp(startHeight, 0, progress);
            layoutElement.preferredHeight = Mathf.Lerp(startHeight, 0, progress);

            yield return null;
        }

        // Tamamen yok et, tahtadan sil
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (checkbox != null)
        {
            checkbox.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}
