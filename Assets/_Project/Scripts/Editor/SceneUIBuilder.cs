using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneUIBuilder
{
    private const string ScenePath = "Assets/_Project/MainScenes.unity";
    private const string CanvasName = "GameplayCanvas";
    private const string ShrinkCountdownName = "ShrinkCountdown";
    private const string StateTextName = "StateText";
    private const string RoundCountdownName = "RoundCountdown";
    private const string ResultsPanelName = "ResultsPanel";
    private const string ResultsTitleName = "ResultsTitle";
    private const string ResultsBodyName = "ResultsBody";

    [InitializeOnLoadMethod]
    private static void RegisterAutoBuild()
    {
        EditorApplication.delayCall += TryEnsureSceneUIAfterReload;
    }

    [MenuItem("Tools/3D Arena/Ensure Scene UI")]
    private static void EnsureSceneUIFromMenu()
    {
        EnsureSceneUI();
    }

    private static void TryEnsureSceneUIAfterReload()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        var activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid() || activeScene.path != ScenePath)
        {
            return;
        }

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

        uiManager.BindSceneReferences(
            canvas,
            shrinkCountdown,
            stateText,
            roundCountdown,
            resultsPanel,
            resultsTitle,
            resultsBody);

        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(activeScene);
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
