using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    private const string CanvasRootName = "RuntimeCanvas";

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Text shrinkCountdownText;
    [SerializeField] private Text stateText;
    [SerializeField] private Text roundCountdownText;
    [SerializeField] private Image resultsPanel;
    [SerializeField] private Text resultsTitleText;
    [SerializeField] private Text resultsBodyText;

    private Font builtinFont;
    private GameManager gameManager;
    private ArenaManager arenaManager;

    public Canvas RootCanvas => rootCanvas;

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureUI();
        ResolveReferences();
    }

    public void InitializeRuntimeUI()
    {
        EnsureUI();
        ResolveReferences();
        RefreshUI();
    }

    public void BindSceneReferences(
        Canvas canvas,
        Text shrinkText,
        Text stateLabel,
        Text countdownLabel,
        Image panel,
        Text panelTitle,
        Text panelBody)
    {
        rootCanvas = canvas;
        shrinkCountdownText = shrinkText;
        stateText = stateLabel;
        roundCountdownText = countdownLabel;
        resultsPanel = panel;
        resultsTitleText = panelTitle;
        resultsBodyText = panelBody;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveReferences();
        RefreshUI();
    }

    private void EnsureUI()
    {
        builtinFont = LoadBuiltinFont();
        var hasBoundSceneUI = HasBoundSceneUI();

        EnsureCanvasRoot();

        var createdScaler = false;
        if (!rootCanvas.TryGetComponent<CanvasScaler>(out var scaler))
        {
            scaler = rootCanvas.gameObject.AddComponent<CanvasScaler>();
            createdScaler = true;
        }

        if (!hasBoundSceneUI)
        {
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.pixelPerfect = true;
            rootCanvas.sortingOrder = 100;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
        else if (createdScaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (!rootCanvas.TryGetComponent<GraphicRaycaster>(out _))
        {
            rootCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (hasBoundSceneUI)
        {
            return;
        }

        shrinkCountdownText = EnsureText(
            ref shrinkCountdownText,
            "ShrinkCountdown",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -32f),
            new Vector2(420f, 56f),
            28,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Color(1f, 0.92f, 0.35f, 1f));

        stateText = EnsureText(
            ref stateText,
            "StateText",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -88f),
            new Vector2(620f, 64f),
            24,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Color(0.9f, 0.96f, 1f, 1f));

        roundCountdownText = EnsureText(
            ref roundCountdownText,
            "RoundCountdown",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(540f, 120f),
            72,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Color(1f, 0.98f, 0.88f, 1f));

        EnsureResultsPanel();
    }

    private void ResolveReferences()
    {
        var root = transform.parent;

        if (root == null)
        {
            return;
        }

        if (gameManager == null || !gameManager.gameObject.scene.IsValid())
        {
            var managers = root.Find("Managers");

            if (managers != null)
            {
                gameManager = managers.GetComponent<GameManager>();
            }
        }

        if (arenaManager == null || !arenaManager.gameObject.scene.IsValid())
        {
            var arena = root.Find("Arena");

            if (arena != null)
            {
                arenaManager = arena.GetComponent<ArenaManager>();
            }
        }
    }

    private void RefreshUI()
    {
        RefreshStateText();
        RefreshShrinkCountdown();
        RefreshRoundCountdown();
        RefreshResultsPanel();
    }

    private void RefreshStateText()
    {
        if (stateText == null)
        {
            return;
        }

        stateText.text = gameManager != null
            ? gameManager.GetPhaseDisplayText()
            : "State: Waiting for GameManager";
    }

    private void RefreshShrinkCountdown()
    {
        if (shrinkCountdownText == null)
        {
            return;
        }

        var isBattle = gameManager != null && gameManager.CurrentPhase == GameManager.GamePhase.Battle;
        var canShow = isBattle && arenaManager != null && arenaManager.RoundActive && arenaManager.CanContinueShrinking;

        shrinkCountdownText.enabled = canShow;
        shrinkCountdownText.text = canShow
            ? $"Shrink In: {arenaManager.SecondsUntilNextShrink:0.0}"
            : string.Empty;
    }

    private void RefreshRoundCountdown()
    {
        if (roundCountdownText == null)
        {
            return;
        }

        var isCountdown = gameManager != null && gameManager.CurrentPhase == GameManager.GamePhase.Countdown;
        roundCountdownText.enabled = isCountdown;

        if (!isCountdown)
        {
            roundCountdownText.text = string.Empty;
            return;
        }

        roundCountdownText.text = $"START IN\n{Mathf.CeilToInt(gameManager.CountdownRemaining)}";
    }

    private void RefreshResultsPanel()
    {
        if (resultsPanel == null || resultsTitleText == null || resultsBodyText == null)
        {
            return;
        }

        var showResults = gameManager != null && gameManager.CurrentPhase == GameManager.GamePhase.Results;
        resultsPanel.gameObject.SetActive(showResults);

        if (!showResults)
        {
            return;
        }

        var winnerName = gameManager.Winner != null ? gameManager.Winner.DisplayName : "No Winner";
        resultsTitleText.text = $"Results\nWinner: {winnerName}";
        resultsBodyText.text = gameManager.GetResultsDisplayText();
    }

    private void EnsureResultsPanel()
    {
        if (resultsPanel == null)
        {
            var existingPanel = GetUIRootTransform().Find("ResultsPanel");

            if (existingPanel != null)
            {
                resultsPanel = existingPanel.GetComponent<Image>();
            }
        }

        if (resultsPanel == null)
        {
            var panelObject = new GameObject("ResultsPanel", typeof(RectTransform));
            panelObject.transform.SetParent(GetUIRootTransform(), false);
            resultsPanel = panelObject.AddComponent<Image>();
        }

        var panelRect = resultsPanel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(520f, 420f);
        resultsPanel.color = new Color(0.05f, 0.08f, 0.12f, 0.84f);

        resultsTitleText = EnsureChildText(
            resultsPanel.transform,
            ref resultsTitleText,
            "ResultsTitle",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(-48f, 90f),
            34,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Color(1f, 0.95f, 0.78f, 1f));

        resultsBodyText = EnsureChildText(
            resultsPanel.transform,
            ref resultsBodyText,
            "ResultsBody",
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -24f),
            new Vector2(-56f, -132f),
            26,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Color(0.93f, 0.96f, 1f, 1f));

        resultsPanel.gameObject.SetActive(false);
    }

    private Text EnsureText(
        ref Text target,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        var existing = GetUIRootTransform().Find(objectName);

        if (target == null && existing != null)
        {
            target = existing.GetComponent<Text>();
        }

        if (target == null)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(GetUIRootTransform(), false);
            target = textObject.AddComponent<Text>();
        }

        ConfigureText(target, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, fontSize, fontStyle, alignment, color);
        return target;
    }

    private Text EnsureChildText(
        Transform parent,
        ref Text target,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        var existing = parent.Find(objectName);

        if (target == null && existing != null)
        {
            target = existing.GetComponent<Text>();
        }

        if (target == null)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            target = textObject.AddComponent<Text>();
        }

        ConfigureText(target, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, fontSize, fontStyle, alignment, color);
        return target;
    }

    private void ConfigureText(
        Text target,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        if (target == null)
        {
            return;
        }

        var rectTransform = target.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        target.font = builtinFont;
        target.fontSize = fontSize;
        target.fontStyle = fontStyle;
        target.alignment = alignment;
        target.color = color;
        target.horizontalOverflow = HorizontalWrapMode.Wrap;
        target.verticalOverflow = VerticalWrapMode.Overflow;
        target.text = string.Empty;
    }

    private void EnsureCanvasRoot()
    {
        if (rootCanvas != null)
        {
            return;
        }

        var existing = transform.Find(CanvasRootName);

        if (existing != null)
        {
            rootCanvas = existing.GetComponent<Canvas>();
        }

        if (rootCanvas != null)
        {
            return;
        }

        var canvasObject = new GameObject(
            CanvasRootName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        rootCanvas = canvasObject.GetComponent<Canvas>();
    }

    private Transform GetUIRootTransform()
    {
        return rootCanvas != null ? rootCanvas.transform : transform;
    }

    private bool HasBoundSceneUI()
    {
        return rootCanvas != null &&
               shrinkCountdownText != null &&
               stateText != null &&
               roundCountdownText != null &&
               resultsPanel != null &&
               resultsTitleText != null &&
               resultsBodyText != null;
    }

    private static Font LoadBuiltinFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font != null)
        {
            return font;
        }

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
