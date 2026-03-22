using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneUIBuilder
{
    private const string ScenePath = "Assets/_Project/MainScenes.unity";
    private const string CanvasName = "GameplayCanvas";
    private const string ShrinkCountdownName = "ShrinkCountdown";
    private const string StateTextName = "StateText";
    private const string RoundCountdownName = "RoundCountdown";
    private const string SetupPanelName = "SetupPanel";
    private const string SetupTitleName = "SetupTitle";
    private const string PlayerCountName = "PlayerCountLabel";
    private const string DecreasePlayersButtonName = "DecreasePlayersButton";
    private const string IncreasePlayersButtonName = "IncreasePlayersButton";
    private const string PlayerListScrollViewName = "PlayerListScrollView";
    private const string PlayerListViewportName = "Viewport";
    private const string PlayerListContentName = "PlayerListContent";
    private const string StartButtonName = "StartButton";
    private const string ResultsPanelName = "ResultsPanel";
    private const string ResultsTitleName = "ResultsTitle";
    private const string ResultsBodyName = "ResultsBody";
    private const string RestartButtonName = "RestartButton";

    [MenuItem("Tools/3D Arena/Ensure Scene UI")]
    private static void EnsureSceneUIFromMenu()
    {
        EnsureSceneUI();
    }

    private static void EnsureSceneUI()
    {
        var activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid() || activeScene.path != ScenePath)
        {
            return;
        }

        var gameRoot = GameObject.Find("GameRoot");

        if (gameRoot == null)
        {
            return;
        }

        var uiRoot = FindOrCreateChild(gameRoot.transform, "UI");
        var uiManager = uiRoot.GetComponent<UIManager>();

        EnsureEventSystem(activeScene);

        if (uiManager == null)
        {
            uiManager = Undo.AddComponent<UIManager>(uiRoot.gameObject);
        }

        var canvasTransform = FindOrCreateChild(uiRoot, CanvasName);
        var canvas = canvasTransform.GetComponent<Canvas>();
        var createdCanvas = false;

        if (canvas == null)
        {
            canvas = Undo.AddComponent<Canvas>(canvasTransform.gameObject);
            createdCanvas = true;
        }

        if (createdCanvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 100;
        }

        var scaler = canvasTransform.GetComponent<CanvasScaler>();
        var createdScaler = false;

        if (scaler == null)
        {
            scaler = Undo.AddComponent<CanvasScaler>(canvasTransform.gameObject);
            createdScaler = true;
        }

        if (createdScaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (canvasTransform.GetComponent<GraphicRaycaster>() == null)
        {
            Undo.AddComponent<GraphicRaycaster>(canvasTransform.gameObject);
        }

        var shrinkCountdown = EnsureText(
            canvasTransform,
            ShrinkCountdownName,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -32f),
            new Vector2(420f, 56f),
            28,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Color(1f, 0.92f, 0.35f, 1f));

        var stateText = EnsureText(
            canvasTransform,
            StateTextName,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -88f),
            new Vector2(620f, 64f),
            24,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Color(0.9f, 0.96f, 1f, 1f));

        var roundCountdown = EnsureText(
            canvasTransform,
            RoundCountdownName,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(540f, 120f),
            72,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Color(1f, 0.98f, 0.88f, 1f));

        var setupPanelTransform = FindOrCreateChild(canvasTransform, SetupPanelName);
        var setupPanel = setupPanelTransform.GetComponent<Image>();

        if (setupPanel == null)
        {
            setupPanel = Undo.AddComponent<Image>(setupPanelTransform.gameObject);
            var setupRect = setupPanel.rectTransform;
            setupRect.anchorMin = new Vector2(0.5f, 0.5f);
            setupRect.anchorMax = new Vector2(0.5f, 0.5f);
            setupRect.pivot = new Vector2(0.5f, 0.5f);
            setupRect.anchoredPosition = Vector2.zero;
            setupRect.sizeDelta = new Vector2(720f, 900f);
            setupPanel.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);
        }

        EnsureText(
            setupPanelTransform,
            SetupTitleName,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(-64f, 60f),
            34,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Color(1f, 0.96f, 0.82f, 1f)).text = "Arena Setup";

        var playerCountText = EnsureText(
            setupPanelTransform,
            PlayerCountName,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -96f),
            new Vector2(-220f, 40f),
            24,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Color.white);

        var decreasePlayersButton = EnsureButton(
            setupPanelTransform,
            DecreasePlayersButtonName,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(36f, -92f),
            new Vector2(72f, 40f),
            "-");

        var increasePlayersButton = EnsureButton(
            setupPanelTransform,
            IncreasePlayersButtonName,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-36f, -92f),
            new Vector2(72f, 40f),
            "+");

        var scrollViewTransform = FindOrCreateChild(setupPanelTransform, PlayerListScrollViewName);
        var scrollViewImage = scrollViewTransform.GetComponent<Image>();

        if (scrollViewImage == null)
        {
            scrollViewImage = Undo.AddComponent<Image>(scrollViewTransform.gameObject);
            var scrollRect = scrollViewImage.rectTransform;
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -8f);
            scrollRect.offsetMin = new Vector2(36f, 110f);
            scrollRect.offsetMax = new Vector2(-36f, -170f);
            scrollViewImage.color = new Color(0.1f, 0.12f, 0.18f, 0.88f);
        }

        var scrollRectComponent = scrollViewTransform.GetComponent<ScrollRect>();

        if (scrollRectComponent == null)
        {
            scrollRectComponent = Undo.AddComponent<ScrollRect>(scrollViewTransform.gameObject);
        }

        var viewportTransform = FindOrCreateChild(scrollViewTransform, PlayerListViewportName);
        var viewportImage = viewportTransform.GetComponent<Image>();

        if (viewportImage == null)
        {
            viewportImage = Undo.AddComponent<Image>(viewportTransform.gameObject);
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        }

        if (viewportTransform.GetComponent<Mask>() == null)
        {
            var mask = Undo.AddComponent<Mask>(viewportTransform.gameObject);
            mask.showMaskGraphic = false;
        }

        var viewportRect = (RectTransform)viewportTransform;
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12f, 12f);
        viewportRect.offsetMax = new Vector2(-12f, -12f);

        var contentTransform = FindOrCreateChild(viewportTransform, PlayerListContentName);
        var contentRect = (RectTransform)contentTransform;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var contentLayout = contentTransform.GetComponent<VerticalLayoutGroup>();

        if (contentLayout == null)
        {
            contentLayout = Undo.AddComponent<VerticalLayoutGroup>(contentTransform.gameObject);
            contentLayout.padding = new RectOffset(0, 0, 0, 0);
            contentLayout.spacing = 10f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
        }

        if (contentTransform.GetComponent<ContentSizeFitter>() == null)
        {
            var fitter = Undo.AddComponent<ContentSizeFitter>(contentTransform.gameObject);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        scrollRectComponent.viewport = viewportRect;
        scrollRectComponent.content = contentRect;
        scrollRectComponent.horizontal = false;
        scrollRectComponent.vertical = true;
        scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;

        var startButton = EnsureButton(
            setupPanelTransform,
            StartButtonName,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 36f),
            new Vector2(240f, 56f),
            "Start");

        var resultsPanelTransform = FindOrCreateChild(canvasTransform, ResultsPanelName);
        var resultsPanel = resultsPanelTransform.GetComponent<Image>();
        var createdResultsPanel = false;

        if (resultsPanel == null)
        {
            resultsPanel = Undo.AddComponent<Image>(resultsPanelTransform.gameObject);
            createdResultsPanel = true;
        }

        if (createdResultsPanel)
        {
            var resultsPanelRect = resultsPanel.rectTransform;
            resultsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultsPanelRect.pivot = new Vector2(0.5f, 0.5f);
            resultsPanelRect.anchoredPosition = Vector2.zero;
            resultsPanelRect.sizeDelta = new Vector2(520f, 420f);
            resultsPanel.color = new Color(0.05f, 0.08f, 0.12f, 0.84f);
            resultsPanel.gameObject.SetActive(false);
        }

        var resultsTitle = EnsureText(
            resultsPanelTransform,
            ResultsTitleName,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(-48f, 90f),
            34,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Color(1f, 0.95f, 0.78f, 1f));

        var resultsBody = EnsureText(
            resultsPanelTransform,
            ResultsBodyName,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -24f),
            new Vector2(-56f, -132f),
            26,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Color(0.93f, 0.96f, 1f, 1f));

        var restartButton = EnsureButton(
            resultsPanelTransform,
            RestartButtonName,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 28f),
            new Vector2(220f, 52f),
            "Restart");

        uiManager.BindSceneReferences(
            canvas,
            shrinkCountdown,
            stateText,
            roundCountdown,
            resultsPanel,
            resultsTitle,
            resultsBody,
            setupPanel,
            playerCountText,
            contentRect,
            decreasePlayersButton,
            increasePlayersButton,
            startButton,
            restartButton);

        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(activeScene);
    }

    private static void EnsureEventSystem(Scene activeScene)
    {
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
            eventSystem = Undo.AddComponent<EventSystem>(eventSystemObject);
            var createdModule = Undo.AddComponent<InputSystemUIInputModule>(eventSystemObject);
            EnsureInputSystemUiActions(createdModule);
            EditorSceneManager.MarkSceneDirty(activeScene);
            return;
        }

        var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
        var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();

        if (standaloneModule != null)
        {
            Undo.DestroyObjectImmediate(standaloneModule);
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        if (inputSystemModule == null)
        {
            inputSystemModule = Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        if (EnsureInputSystemUiActions(inputSystemModule))
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
    }

    private static bool EnsureInputSystemUiActions(InputSystemUIInputModule inputSystemModule)
    {
        if (inputSystemModule == null || inputSystemModule.actionsAsset != null)
        {
            return false;
        }

        inputSystemModule.AssignDefaultActions();
        return true;
    }

    private static Transform FindOrCreateChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);

        if (child != null)
        {
            return child;
        }

        var childObject = new GameObject(childName, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(childObject, $"Create {childName}");
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private static Text EnsureText(
        Transform parent,
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
        var textTransform = FindOrCreateChild(parent, objectName);
        var text = textTransform.GetComponent<Text>();
        var createdText = false;

        if (text == null)
        {
            text = Undo.AddComponent<Text>(textTransform.gameObject);
            createdText = true;
        }

        if (createdText)
        {
            var rectTransform = text.rectTransform;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            text.font = LoadBuiltinFont();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = string.Empty;
        }

        return text;
    }

    private static Button EnsureButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        string labelText)
    {
        var buttonTransform = FindOrCreateChild(parent, objectName);
        var buttonImage = buttonTransform.GetComponent<Image>();

        if (buttonImage == null)
        {
            buttonImage = Undo.AddComponent<Image>(buttonTransform.gameObject);
            buttonImage.color = new Color(0.2f, 0.53f, 0.88f, 0.96f);
        }

        var button = buttonTransform.GetComponent<Button>();

        if (button == null)
        {
            button = Undo.AddComponent<Button>(buttonTransform.gameObject);
        }

        var rectTransform = (RectTransform)buttonTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        var label = EnsureText(
            buttonTransform,
            "Label",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Color.white);
        label.text = labelText;

        return button;
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
