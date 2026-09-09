using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class VRChecklistManager : MonoBehaviour
{
    public Transform contentContainer;
    public GameObject checklistItemPrefab;
    public TextMeshProUGUI progressText;
    [Tooltip("Floating panel that shows only the task the user should be doing right now.")]
    public TMP_Text activeTaskText;
    [Header("Item sizing")]
    public float checkboxWidth = 2.3f;
    public float itemVerticalPadding = 0.35f;
    public float minItemHeight = 1.1f;
    [Header("Task highlighting")]
    [Tooltip("Object to highlight for each task, aligned with the tasks list. Leave empty for no highlight.")]
    public GameObject[] taskTargets;
    public Color highlightColor = new Color(1f, 0.85f, 0.1f);
    public bool highlightEnabled = true;
    [Header("Card look")]
    public Sprite cardSprite;
    public Color cardColor = new Color(0.16f, 0.85f, 0.95f, 1f);
    public Color cardLockedColor = new Color(0.10f, 0.28f, 0.34f, 1f);
    public Color cardDoneColor = new Color(0.20f, 0.62f, 0.36f, 1f);
    [Tooltip("Filled Image that shows overall progress.")]
    public Image progressFill;
    public float cardCornerScale = 40f;
    public float taskFontSize = 0.45f;
    public List<string> tasks = new List<string>
    {
        "Check for physical risks (impact, looseness, arcing, leakage).",
        "If no abnormalities, secure the operation area within the laboratory cage.",
        "Remove and lock the High Voltage (HV) manual service disconnect (MSD).",
        "Disconnect the 12V battery.",
        "Confirm de-energization on the dashboard (perform manual test if necessary).",
        "Test the high voltage tester on the 12V battery (Red:+, Blue:-).",
        "Short-circuit the gigaohmmeter to verify zero error.",
        "Test the internal integrity of the intermediate measuring unit with a multimeter.",
        "Connect the probes and confirm one last time that the system is de-energized.",
        "Set the gigaohmmeter to the proper voltage (e.g., 500V) and run HV+ isolation test (vs chassis).",
        "Switch the probe and run the HV- isolation test (vs chassis).",
        "If present, repeat isolation tests for additional components (e.g., A/C compressor).",
        "Confirm and report that the results comply with the standard (500 ohm/V)."
    };
    private List<VRChecklistItem> instantiatedItems = new List<VRChecklistItem>();
    private string activeSuffix = "";
    private int lastActiveIndex = -2;
    private readonly List<Renderer> highlighted = new List<Renderer>();
    private int highlightIndex = -2;
    private MaterialPropertyBlock highlightBlock;
    public int TotalCount => tasks.Count;
    public int CompletedCount
    {
        get
        {
            int done = 0;
            for (int i = 0; i < instantiatedItems.Count; i++)
            {
                var item = instantiatedItems[i];
                if (item == null || item.checkbox == null || item.checkbox.isOn)
                    done++;
            }
            return done;
        }
    }
    private void Start()
    {
        populateChecklist();
        initializeSequentialLogic();
        updateProgress();
    }
    private void populateChecklist()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        instantiatedItems.Clear();
        foreach (string taskText in tasks)
        {
            GameObject newItem = Instantiate(checklistItemPrefab, contentContainer);
            VRChecklistItem itemScript = newItem.GetComponent<VRChecklistItem>();
            if (itemScript != null)
            {
                applyCardLook(newItem);
                if (itemScript.taskText != null)
                {
                    itemScript.taskText.text = taskText;
                    var textRect = itemScript.taskText.rectTransform;
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                    itemScript.taskText.margin = new Vector4(checkboxWidth, 0.15f, 0.35f, 0.15f);
                    if (taskFontSize > 0f)
                    {
                        itemScript.taskText.enableAutoSizing = false;
                        itemScript.taskText.fontSize = taskFontSize;
                    }
                }
                if (itemScript.checkbox != null)
                {
                    itemScript.checkbox.isOn = false;
                }
                instantiatedItems.Add(itemScript);
            }
        }
        fitAllItemHeights();
    }
    private void fitAllItemHeights()
    {
        var container = contentContainer as RectTransform;
        if (container == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        float textWidth = container.rect.width - checkboxWidth - 0.35f;
        if (textWidth <= 0.01f)
            return;

        for (int i = 0; i < instantiatedItems.Count; i++)
        {
            var item = instantiatedItems[i];
            if (item == null || item.taskText == null)
                continue;
            float needed = item.taskText.GetPreferredValues(item.taskText.text, textWidth, 0f).y
                           + itemVerticalPadding;
            var layout = item.GetComponent<LayoutElement>();
            if (layout == null)
                layout = item.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = Mathf.Max(minItemHeight, needed);
            layout.preferredHeight = layout.minHeight;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }
    private void applyCardLook(GameObject item)
    {
        var script = item.GetComponent<VRChecklistItem>();
        if (script != null)
        {
            script.activeColor = cardColor;
            script.lockedColor = cardLockedColor;
            script.doneColor = cardDoneColor;
        }
        var card = item.GetComponent<Image>();
        if (card == null)
            return;
        if (cardSprite != null)
        {
            card.sprite = cardSprite;
            card.type = Image.Type.Sliced;
            card.pixelsPerUnitMultiplier = cardCornerScale;
        }
        card.color = cardColor;
    }
    private void initializeSequentialLogic()
    {
        for (int i = 0; i < instantiatedItems.Count; i++)
        {
            int index = i;
            var item = instantiatedItems[i];
            item.checkbox.onValueChanged.AddListener((isOn) => onItemToggled(index, isOn));
            item.setInteractable(i == 0);
        }
    }
    private void onItemToggled(int index, bool isOn)
    {
        if (isOn)
        {
            if (index + 1 < instantiatedItems.Count)
            {
                var nextItem = instantiatedItems[index + 1];
                if (nextItem != null)
                {
                    nextItem.setInteractable(true);
                }
            }
        }
        else
        {
            for (int i = index + 1; i < instantiatedItems.Count; i++)
            {
                if (instantiatedItems[i] == null || instantiatedItems[i].checkbox == null)
                    continue;
                instantiatedItems[i].checkbox.isOn = false;
                instantiatedItems[i].setInteractable(false);
            }
        }
        updateProgress();
    }
    private void updateProgress()
    {
        if (progressText != null)
            progressText.text = $"{CompletedCount} / {TotalCount}";
        if (progressFill != null)
            progressFill.fillAmount = TotalCount > 0 ? (float)CompletedCount / TotalCount : 0f;
        updateActiveTaskText();
    }
    public int ActiveIndex
    {
        get
        {
            for (int i = 0; i < instantiatedItems.Count; i++)
            {
                var item = instantiatedItems[i];
                if (item != null && item.checkbox != null && !item.checkbox.isOn)
                    return i;
            }
            return -1;
        }
    }
    public void SetActiveSuffix(string suffix)
    {
        activeSuffix = suffix;
        updateActiveTaskText();
    }
    private void updateActiveTaskText()
    {
        int index = ActiveIndex;
        if (index != lastActiveIndex)
        {
            lastActiveIndex = index;
            activeSuffix = "";
        }
        if (activeTaskText == null)
            return;
        if (index < 0 || index >= tasks.Count)
        {
            activeTaskText.text = "All tasks complete.";
            return;
        }
        activeTaskText.text = string.IsNullOrEmpty(activeSuffix)
            ? tasks[index]
            : $"{tasks[index]} {activeSuffix}";
    }
    private void LateUpdate()
    {
        if (!highlightEnabled)
            return;

        int index = ActiveIndex;
        if (index != highlightIndex)
        {
            highlightIndex = index;
            rebuildHighlight(index);
        }
        drawHighlight();
    }
    private void rebuildHighlight(int index)
    {
        for (int i = 0; i < highlighted.Count; i++)
        {
            if (highlighted[i] != null)
                highlighted[i].SetPropertyBlock(null);
        }
        highlighted.Clear();

        if (taskTargets == null || index < 0 || index >= taskTargets.Length)
            return;
        GameObject target = taskTargets[index];
        if (target == null)
            return;

        foreach (var renderer in target.GetComponentsInChildren<Renderer>())
        {
            if (!(renderer is LineRenderer))
                highlighted.Add(renderer);
        }
    }
    private void drawHighlight()
    {
        if (highlighted.Count == 0)
            return;
        if (highlightBlock == null)
            highlightBlock = new MaterialPropertyBlock();

        float pulse = 0.45f + 0.35f * Mathf.PingPong(Time.time * 1.6f, 1f);
        Color color = Color.Lerp(Color.white, highlightColor, pulse);
        highlightBlock.SetColor("_BaseColor", color);
        highlightBlock.SetColor("_Color", color);
        for (int i = 0; i < highlighted.Count; i++)
        {
            if (highlighted[i] != null)
                highlighted[i].SetPropertyBlock(highlightBlock);
        }
    }
    public void CompleteCurrentTask()
    {
        for (int i = 0; i < instantiatedItems.Count; i++)
        {
            var item = instantiatedItems[i];
            if (item == null || item.checkbox == null || item.checkbox.isOn)
                continue;
            item.checkbox.interactable = true;
            item.checkbox.isOn = true;
            return;
        }
    }
    public void CompleteTask(int index)
    {
        if (index < 0 || index >= instantiatedItems.Count)
            return;
        var item = instantiatedItems[index];
        if (item == null || item.checkbox == null || item.checkbox.isOn)
            return;
        item.checkbox.interactable = true;
        item.checkbox.isOn = true;
    }
}
