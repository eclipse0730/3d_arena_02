using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class UIManager : MonoBehaviour
{
    private const string EliminationPanelName = "EliminationPanel";
    private const string EliminationTitleName = "EliminationTitle";
    private const string EliminationBodyName = "EliminationBody";
    private const string ResultsListContentName = "ResultsListContent";
    private const string ResultsWinnerCrownName = "ResultsWinnerCrown";

    [Tooltip("셋업, HUD, 결과 UI가 들어 있는 루트 캔버스입니다.")]
    [SerializeField] private Canvas rootCanvas;
    [Tooltip("다음 축소까지 남은 시간을 보여주는 좌측 상단 텍스트입니다.")]
    [SerializeField] private Text shrinkCountdownText;
    [Tooltip("현재 경기 상태를 보여주는 좌측 상단 텍스트입니다.")]
    [SerializeField] private Text stateText;
    [Tooltip("라운드 시작 전 중앙에 표시되는 카운트다운 텍스트입니다.")]
    [SerializeField] private Text roundCountdownText;
    [Tooltip("라운드 종료 후 표시되는 결과 패널입니다.")]
    [SerializeField] private Image resultsPanel;
    [Tooltip("결과 패널 상단의 우승자 제목 텍스트입니다.")]
    [SerializeField] private Text resultsTitleText;
    [Tooltip("기본 결과 목록 텍스트입니다. 동적 결과 레이아웃의 기준 위치로 사용됩니다.")]
    [SerializeField] private Text resultsBodyText;
    [SerializeField] private Texture winnerCrownTexture;
    [SerializeField] private Texture firstPlaceTexture;
    [SerializeField] private Texture secondPlaceTexture;
    [SerializeField] private Texture thirdPlaceTexture;
    [Tooltip("게임 시작 전 참가자 설정에 사용하는 패널입니다.")]
    [SerializeField] private Image setupPanel;
    [Tooltip("현재 참가 인원 수를 보여주는 텍스트입니다.")]
    [SerializeField] private Text playerCountText;
    [Tooltip("플레이어 이름 입력 행이 생성될 콘텐츠 루트입니다.")]
    [SerializeField] private RectTransform playerListContent;
    [Tooltip("참가 인원을 줄이는 버튼입니다.")]
    [SerializeField] private Button decreasePlayersButton;
    [Tooltip("참가 인원을 늘리는 버튼입니다.")]
    [SerializeField] private Button increasePlayersButton;
    [Tooltip("현재 설정으로 게임을 시작하는 버튼입니다.")]
    [SerializeField] private Button startButton;
    [Tooltip("결과 화면에서 셋업 화면으로 돌아가는 버튼입니다.")]
    [SerializeField] private Button restartButton;

    private Font builtinFont;
    private GameManager gameManager;
    private ArenaManager arenaManager;
    private readonly List<InputField> playerNameInputs = new();
    private readonly List<Text> resultEntryTexts = new();
    private int lastRenderedParticipantCount = -1;
    private bool missingSceneUiWarningLogged;
    private RectTransform resultsListContent;
    private RawImage winnerCrownImage;
    private Image eliminationPanel;
    private Text eliminationTitleText;
    private Text eliminationBodyText;
    private string lastEliminationSignature = string.Empty;
    private string lastResultsSignature = string.Empty;

    public Canvas RootCanvas => rootCanvas;

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        InitializeRuntimeUI();
    }

    public void InitializeRuntimeUI()
    {
        builtinFont = LoadBuiltinFont();
        ResolveReferences();

        if (!HasBoundSceneUI())
        {
            LogMissingSceneUiWarning();
            return;
        }

        WireButtonEvents();
        EnsureEliminationPanel();
        EnsureResultsListContent();
        EnsureWinnerCrownGraphic();
        RefreshUI();
    }

    public void BindSceneReferences(
        Canvas canvas,
        Text shrinkText,
        Text stateLabel,
        Text countdownLabel,
        Image panel,
        Text panelTitle,
        Text panelBody,
        Image setupRoot,
        Text participantCountLabel,
        RectTransform nameListContent,
        Button decreaseButton,
        Button increaseButton,
        Button startRoundButton,
        Button restartRoundButton)
    {
        rootCanvas = canvas;
        shrinkCountdownText = shrinkText;
        stateText = stateLabel;
        roundCountdownText = countdownLabel;
        resultsPanel = panel;
        resultsTitleText = panelTitle;
        resultsBodyText = panelBody;
        setupPanel = setupRoot;
        playerCountText = participantCountLabel;
        playerListContent = nameListContent;
        decreasePlayersButton = decreaseButton;
        increasePlayersButton = increaseButton;
        startButton = startRoundButton;
        restartButton = restartRoundButton;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveReferences();

        if (!HasBoundSceneUI())
        {
            LogMissingSceneUiWarning();
            return;
        }

        HandleNameInputTabNavigation();
        RefreshUI();
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
        RefreshAdaptivePanelLayout();
        RefreshSetupPanel();
        RefreshStateText();
        RefreshShrinkCountdown();
        RefreshRoundCountdown();
        RefreshEliminationPanel();
        RefreshResultsPanel();
    }

    private void RefreshSetupPanel()
    {
        if (setupPanel == null)
        {
            return;
        }

        var showSetup = gameManager != null && gameManager.CurrentPhase == GameManager.GamePhase.Setup;
        setupPanel.gameObject.SetActive(showSetup);

        if (!showSetup)
        {
            return;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"인원: {gameManager.ParticipantCount}";
        }

        ConfigureSetupListLayout(gameManager.ParticipantCount);
        EnsurePlayerNameInputs();
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

    private void RefreshEliminationPanel()
    {
        EnsureEliminationPanel();

        if (eliminationPanel == null || eliminationTitleText == null || eliminationBodyText == null)
        {
            return;
        }

        var showEliminationPanel =
            gameManager != null &&
            gameManager.CurrentPhase == GameManager.GamePhase.Battle &&
            gameManager.EliminationOrder.Count > 0;

        eliminationPanel.gameObject.SetActive(showEliminationPanel);

        if (!showEliminationPanel)
        {
            lastEliminationSignature = string.Empty;
            eliminationBodyText.text = string.Empty;
            return;
        }

        eliminationTitleText.text = "탈락 순위";

        var eliminationText = gameManager.GetEliminationDisplayText();
        var signature = $"{gameManager.ParticipantCount}:{eliminationText}";

        if (lastEliminationSignature == signature)
        {
            return;
        }

        lastEliminationSignature = signature;
        eliminationBodyText.text = eliminationText;
    }

    private void RefreshResultsPanel()
    {
        if (resultsPanel == null || resultsTitleText == null || resultsBodyText == null)
        {
            return;
        }

        EnsureWinnerCrownGraphic();

        var showResults = gameManager != null && gameManager.CurrentPhase == GameManager.GamePhase.Results;
        resultsPanel.gameObject.SetActive(showResults);

        if (!showResults)
        {
            lastResultsSignature = string.Empty;
            HideResultEntries();
            SetWinnerCrownVisible(false);
            return;
        }

        var winnerName = gameManager.Winner != null ? gameManager.Winner.DisplayName : "우승자 없음";
        resultsTitleText.text = $"우승: {winnerName}";
        SetWinnerCrownVisible(winnerCrownTexture != null);
        RefreshResultEntries(gameManager.ParticipantCount);
    }

    private void WireButtonEvents()
    {
        if (decreasePlayersButton != null)
        {
            decreasePlayersButton.onClick.RemoveListener(HandleDecreasePlayers);
            decreasePlayersButton.onClick.AddListener(HandleDecreasePlayers);
        }

        if (increasePlayersButton != null)
        {
            increasePlayersButton.onClick.RemoveListener(HandleIncreasePlayers);
            increasePlayersButton.onClick.AddListener(HandleIncreasePlayers);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartRound);
            startButton.onClick.AddListener(HandleStartRound);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(HandleRestartToSetup);
            restartButton.onClick.AddListener(HandleRestartToSetup);
        }
    }

    private void EnsurePlayerNameInputs()
    {
        if (gameManager == null || playerListContent == null)
        {
            return;
        }

        if (lastRenderedParticipantCount == gameManager.ParticipantCount && playerNameInputs.Count == gameManager.ParticipantCount)
        {
            return;
        }

        for (var i = playerListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(playerListContent.GetChild(i).gameObject);
        }

        playerNameInputs.Clear();
        var twoColumns = gameManager.ParticipantCount >= 11;
        var compactLayout = twoColumns;

        for (var i = 0; i < gameManager.ParticipantCount; i += twoColumns ? 2 : 1)
        {
            var row = CreateNameInputRow(i / (twoColumns ? 2 : 1), compactLayout);
            playerNameInputs.Add(CreatePlayerNameInput(row, i, compactLayout));

            if (twoColumns && i + 1 < gameManager.ParticipantCount)
            {
                playerNameInputs.Add(CreatePlayerNameInput(row, i + 1, compactLayout));
            }
        }

        lastRenderedParticipantCount = gameManager.ParticipantCount;
    }

    private Transform CreateNameInputRow(int rowIndex, bool compactLayout)
    {
        var rowHeight = compactLayout ? 36f : 42f;

        var rowObject = new GameObject($"PlayerRow_{rowIndex + 1}", typeof(RectTransform));
        rowObject.transform.SetParent(playerListContent, false);

        var layout = rowObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = compactLayout;
        layout.spacing = compactLayout ? 16f : 0f;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var rowLayoutElement = rowObject.AddComponent<LayoutElement>();
        rowLayoutElement.minHeight = rowHeight;
        rowLayoutElement.preferredHeight = rowHeight;

        return rowObject.transform;
    }

    private InputField CreatePlayerNameInput(Transform parent, int index, bool compactLayout)
    {
        var rowHeight = compactLayout ? 36f : 42f;
        var labelWidth = compactLayout ? 30f : 36f;
        var fontSize = compactLayout ? 18 : 20;

        var entryObject = new GameObject($"PlayerEntry_{index + 1}", typeof(RectTransform));
        entryObject.transform.SetParent(parent, false);

        var layout = entryObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 12f;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var entryLayoutElement = entryObject.AddComponent<LayoutElement>();
        entryLayoutElement.minHeight = rowHeight;
        entryLayoutElement.preferredHeight = rowHeight;
        entryLayoutElement.flexibleWidth = 1f;

        var labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(entryObject.transform, false);
        var label = labelObject.AddComponent<Text>();
        label.font = builtinFont;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(0.95f, 0.96f, 1f, 1f);
        label.text = $"{index + 1}.";

        var labelLayout = labelObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = labelWidth;
        labelLayout.minWidth = labelWidth;

        var inputObject = new GameObject("InputField", typeof(RectTransform));
        inputObject.transform.SetParent(entryObject.transform, false);
        var inputImage = inputObject.AddComponent<Image>();
        inputImage.color = new Color(0.12f, 0.15f, 0.2f, 0.94f);
        var inputField = inputObject.AddComponent<InputField>();

        var inputLayout = inputObject.AddComponent<LayoutElement>();
        inputLayout.flexibleWidth = 1f;
        inputLayout.minHeight = rowHeight;

        var textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(inputObject.transform, false);
        var text = textObject.AddComponent<Text>();
        text.font = builtinFont;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.supportRichText = false;

        var placeholderObject = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObject.transform.SetParent(inputObject.transform, false);
        var placeholder = placeholderObject.AddComponent<Text>();
        placeholder.font = builtinFont;
        placeholder.fontSize = fontSize;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = new Color(1f, 1f, 1f, 0.35f);
        placeholder.text = $"Player {index + 1}";

        ConfigureInputTextRect(text.rectTransform);
        ConfigureInputTextRect(placeholder.rectTransform);

        inputField.targetGraphic = inputImage;
        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        inputField.text = GetParticipantName(index);
        inputField.onValueChanged.AddListener(value => gameManager?.SetParticipantName(index, value));
        RegisterPlayerNameInputSelectEvent(inputField);

        return inputField;
    }

    private void RegisterPlayerNameInputSelectEvent(InputField inputField)
    {
        if (inputField == null)
        {
            return;
        }

        var trigger = inputField.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = inputField.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.Select);

        var selectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(_ => HandlePlayerNameInputSelected(inputField));
        trigger.triggers.Add(selectEntry);
    }

    private void HandlePlayerNameInputSelected(InputField inputField)
    {
        if (inputField == null)
        {
            return;
        }

        inputField.text = string.Empty;
        inputField.caretPosition = 0;
        inputField.selectionAnchorPosition = 0;
        inputField.selectionFocusPosition = 0;
        inputField.ForceLabelUpdate();
    }

    private void HandleNameInputTabNavigation()
    {
        if (playerNameInputs.Count == 0 || Keyboard.current == null || !Keyboard.current.tabKey.wasPressedThisFrame)
        {
            return;
        }

        var eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            return;
        }

        var currentSelection = eventSystem.currentSelectedGameObject;
        var currentIndex = -1;

        for (var i = 0; i < playerNameInputs.Count; i++)
        {
            if (playerNameInputs[i] != null && playerNameInputs[i].gameObject == currentSelection)
            {
                currentIndex = i;
                break;
            }
        }

        var nextIndex = currentIndex >= 0
            ? (currentIndex + 1) % playerNameInputs.Count
            : 0;

        ActivatePlayerNameInput(nextIndex);
    }

    private void ActivatePlayerNameInput(int index)
    {
        if (index < 0 || index >= playerNameInputs.Count)
        {
            return;
        }

        var targetInput = playerNameInputs[index];
        var eventSystem = EventSystem.current;

        if (targetInput == null || eventSystem == null)
        {
            return;
        }

        eventSystem.SetSelectedGameObject(targetInput.gameObject);
        HandlePlayerNameInputSelected(targetInput);
        targetInput.ActivateInputField();
    }

    private static void ConfigureInputTextRect(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(12f, 6f);
        rectTransform.offsetMax = new Vector2(-12f, -6f);
    }

    private string GetParticipantName(int index)
    {
        if (gameManager == null || index < 0 || index >= gameManager.ParticipantNames.Count)
        {
            return string.Empty;
        }

        return gameManager.ParticipantNames[index];
    }

    private void RefreshAdaptivePanelLayout()
    {
        var participantCount = gameManager != null ? gameManager.ParticipantCount : 0;

        RefreshSetupPanelLayout(participantCount);
        RefreshResultsPanelLayout(participantCount);
    }

    private void RefreshSetupPanelLayout(int participantCount)
    {
        if (setupPanel == null)
        {
            return;
        }

        var setupRect = setupPanel.rectTransform;

        if (participantCount >= 11)
        {
            setupRect.sizeDelta = new Vector2(920f, 900f);
        }
        else
        {
            setupRect.sizeDelta = new Vector2(720f, 900f);
        }
    }

    private void RefreshResultsPanelLayout(int participantCount)
    {
        if (resultsPanel == null)
        {
            return;
        }

        var resultsRect = resultsPanel.rectTransform;

        if (participantCount >= 11)
        {
            resultsRect.sizeDelta = new Vector2(780f, 520f);
        }
        else
        {
            resultsRect.sizeDelta = new Vector2(520f, 420f);
        }

        if (resultsTitleText != null)
        {
            resultsTitleText.fontSize = participantCount >= 11 ? 30 : 34;
            var titleRect = resultsTitleText.rectTransform;
            titleRect.anchoredPosition = new Vector2(0f, participantCount >= 11 ? -44f : -40f);
            titleRect.sizeDelta = new Vector2(-48f, participantCount >= 11 ? 84f : 82f);
        }

        if (resultsBodyText != null)
        {
            resultsBodyText.fontSize = participantCount >= 11 ? 20 : 26;
            resultsBodyText.alignment = TextAnchor.UpperLeft;
            resultsBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            resultsBodyText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        UpdateWinnerCrownLayout(participantCount);
    }

    private void ConfigureSetupListLayout(int participantCount)
    {
        if (playerListContent == null)
        {
            return;
        }

        var twoColumns = participantCount >= 11;
        var fitter = playerListContent.GetComponent<ContentSizeFitter>();

        if (fitter == null)
        {
            fitter = playerListContent.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var contentLayout = playerListContent.GetComponent<VerticalLayoutGroup>();

        if (contentLayout == null)
        {
            contentLayout = playerListContent.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        contentLayout.enabled = true;
        contentLayout.padding = new RectOffset(0, 0, 0, 0);
        contentLayout.spacing = twoColumns ? 12f : 10f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
    }

    private void EnsureResultsListContent()
    {
        if (resultsPanel == null)
        {
            return;
        }

        if (resultsListContent == null)
        {
            var existing = resultsPanel.transform.Find(ResultsListContentName);

            if (existing != null)
            {
                resultsListContent = existing as RectTransform;
            }
        }

        if (resultsListContent == null)
        {
            var contentObject = new GameObject(ResultsListContentName, typeof(RectTransform));
            contentObject.transform.SetParent(resultsPanel.transform, false);
            resultsListContent = contentObject.GetComponent<RectTransform>();
        }

        resultsListContent.anchorMin = new Vector2(0f, 0f);
        resultsListContent.anchorMax = new Vector2(1f, 1f);
        resultsListContent.pivot = new Vector2(0.5f, 0.5f);
        resultsListContent.offsetMin = new Vector2(36f, 90f);
        resultsListContent.offsetMax = new Vector2(-36f, -100f);

        if (resultsBodyText != null)
        {
            resultsBodyText.gameObject.SetActive(false);
        }
    }

    private void EnsureWinnerCrownGraphic()
    {
        if (resultsPanel == null)
        {
            return;
        }

        if (winnerCrownImage == null)
        {
            var existing = resultsPanel.transform.Find(ResultsWinnerCrownName);

            if (existing != null)
            {
                winnerCrownImage = existing.GetComponent<RawImage>();
            }
        }

        if (winnerCrownImage == null)
        {
            var crownObject = new GameObject(ResultsWinnerCrownName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            crownObject.transform.SetParent(resultsPanel.transform, false);
            winnerCrownImage = crownObject.GetComponent<RawImage>();
        }

        winnerCrownImage.raycastTarget = false;
        winnerCrownImage.color = Color.white;
        winnerCrownImage.texture = winnerCrownTexture;
        UpdateWinnerCrownLayout(gameManager != null ? gameManager.ParticipantCount : 0);
    }

    private void UpdateWinnerCrownLayout(int participantCount)
    {
        if (winnerCrownImage == null)
        {
            return;
        }

        var crownRect = winnerCrownImage.rectTransform;
        crownRect.anchorMin = new Vector2(0.5f, 1f);
        crownRect.anchorMax = new Vector2(0.5f, 1f);
        crownRect.pivot = new Vector2(0.5f, 1f);
        crownRect.anchoredPosition = new Vector2(0f, participantCount >= 11 ? -10f : -8f);

        var crownHeight = participantCount >= 11 ? 54f : 60f;
        var crownWidth = crownHeight;

        if (winnerCrownTexture != null && winnerCrownTexture.height > 0)
        {
            crownWidth = crownHeight * winnerCrownTexture.width / (float)winnerCrownTexture.height;
        }

        crownRect.sizeDelta = new Vector2(crownWidth, crownHeight);
    }

    private void SetWinnerCrownVisible(bool visible)
    {
        if (winnerCrownImage == null)
        {
            return;
        }

        winnerCrownImage.gameObject.SetActive(visible);
    }

    private void ConfigureResultsListLayout()
    {
        if (resultsListContent == null)
        {
            return;
        }

        var fitter = resultsListContent.GetComponent<ContentSizeFitter>();

        if (fitter == null)
        {
            fitter = resultsListContent.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var contentLayout = resultsListContent.GetComponent<VerticalLayoutGroup>();

        if (contentLayout == null)
        {
            contentLayout = resultsListContent.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        contentLayout.enabled = true;
        contentLayout.padding = new RectOffset(0, 0, 0, 0);
        contentLayout.spacing = 8f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
    }

    private void EnsureEliminationPanel()
    {
        if (rootCanvas == null)
        {
            return;
        }

        if (eliminationPanel == null)
        {
            var existingPanel = rootCanvas.transform.Find(EliminationPanelName);

            if (existingPanel != null)
            {
                eliminationPanel = existingPanel.GetComponent<Image>();
            }
        }

        if (eliminationPanel == null)
        {
            var panelObject = new GameObject(EliminationPanelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(rootCanvas.transform, false);
            eliminationPanel = panelObject.GetComponent<Image>();
        }

        var panelRect = eliminationPanel.rectTransform;
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-28f, -120f);
        panelRect.sizeDelta = new Vector2(280f, 0f);

        eliminationPanel.color = new Color(0.08f, 0.11f, 0.16f, 0.88f);
        eliminationPanel.raycastTarget = false;

        var layout = eliminationPanel.GetComponent<VerticalLayoutGroup>();

        if (layout == null)
        {
            layout = eliminationPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = new RectOffset(18, 18, 16, 16);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var fitter = eliminationPanel.GetComponent<ContentSizeFitter>();

        if (fitter == null)
        {
            fitter = eliminationPanel.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        eliminationTitleText = EnsurePanelText(
            eliminationPanel.transform,
            EliminationTitleName,
            builtinFont,
            24,
            FontStyle.Bold,
            new Color(0.98f, 0.94f, 0.82f, 1f));

        eliminationBodyText = EnsurePanelText(
            eliminationPanel.transform,
            EliminationBodyName,
            builtinFont,
            20,
            FontStyle.Bold,
            new Color(0.93f, 0.96f, 1f, 1f));

        eliminationPanel.gameObject.SetActive(false);
    }

    private Text EnsurePanelText(Transform parent, string objectName, Font font, int fontSize, FontStyle fontStyle, Color color)
    {
        var existing = parent.Find(objectName);
        Text text;

        if (existing != null)
        {
            text = existing.GetComponent<Text>();
        }
        else
        {
            var textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            text = textObject.AddComponent<Text>();
        }

        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        text.raycastTarget = false;

        var layoutElement = text.GetComponent<LayoutElement>();

        if (layoutElement == null)
        {
            layoutElement = text.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.flexibleWidth = 1f;
        layoutElement.minHeight = fontSize + 8f;
        return text;
    }

    private void RebuildResultRows(IReadOnlyList<string> lines, int participantCount)
    {
        if (resultsListContent == null)
        {
            return;
        }

        for (var i = resultsListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(resultsListContent.GetChild(i).gameObject);
        }

        resultEntryTexts.Clear();

        var twoColumns = participantCount >= 11;
        var rowIndex = 0;
        var placement = 1;

        for (var i = 0; i < lines.Count; i += twoColumns ? 2 : 1)
        {
            var row = CreateResultRow(rowIndex++);
            resultEntryTexts.Add(CreateResultEntry(row, lines[i], participantCount, placement));
            placement++;

            if (twoColumns && i + 1 < lines.Count)
            {
                resultEntryTexts.Add(CreateResultEntry(row, lines[i + 1], participantCount, placement));
                placement++;
            }
        }
    }

    private Transform CreateResultRow(int rowIndex)
    {
        var rowObject = new GameObject($"ResultRow_{rowIndex + 1}", typeof(RectTransform));
        rowObject.transform.SetParent(resultsListContent, false);

        var layout = rowObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 18f;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 34f;
        layoutElement.preferredHeight = 34f;

        return rowObject.transform;
    }

    private Text CreateResultEntry(Transform parent, string value, int participantCount, int placement)
    {
        var entryObject = new GameObject("ResultEntry", typeof(RectTransform));
        entryObject.transform.SetParent(parent, false);

        var layoutElement = entryObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 30f;
        layoutElement.preferredHeight = 34f;
        layoutElement.flexibleWidth = 1f;

        var entryLayout = entryObject.AddComponent<HorizontalLayoutGroup>();
        entryLayout.childAlignment = TextAnchor.MiddleLeft;
        entryLayout.childControlHeight = true;
        entryLayout.childControlWidth = true;
        entryLayout.childForceExpandHeight = false;
        entryLayout.childForceExpandWidth = false;
        entryLayout.spacing = 10f;
        entryLayout.padding = new RectOffset(0, 0, 0, 0);

        var placementTexture = GetPlacementTexture(placement);

        if (placementTexture != null)
        {
            CreatePlacementBadge(entryObject.transform, placementTexture, participantCount);
        }

        var textObject = new GameObject("Label", typeof(RectTransform));
        textObject.transform.SetParent(entryObject.transform, false);

        var textLayoutElement = textObject.AddComponent<LayoutElement>();
        textLayoutElement.flexibleWidth = 1f;
        textLayoutElement.minHeight = 30f;
        textLayoutElement.preferredHeight = 34f;

        var text = textObject.AddComponent<Text>();
        text.font = builtinFont;
        text.fontStyle = FontStyle.Bold;
        text.fontSize = participantCount >= 11 ? 22 : 26;
        text.color = new Color(0.93f, 0.96f, 1f, 1f);
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = value;
        return text;
    }

    private Texture GetPlacementTexture(int placement)
    {
        return placement switch
        {
            1 => firstPlaceTexture,
            2 => secondPlaceTexture,
            3 => thirdPlaceTexture,
            _ => null
        };
    }

    private void CreatePlacementBadge(Transform parent, Texture texture, int participantCount)
    {
        var badgeObject = new GameObject("PlacementBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        badgeObject.transform.SetParent(parent, false);

        var badgeLayoutElement = badgeObject.AddComponent<LayoutElement>();
        var badgeHeight = participantCount >= 11 ? 24f : 28f;
        var badgeWidth = badgeHeight;

        if (texture != null && texture.height > 0)
        {
            badgeWidth = badgeHeight * texture.width / (float)texture.height;
        }

        badgeLayoutElement.minWidth = badgeWidth;
        badgeLayoutElement.preferredWidth = badgeWidth;
        badgeLayoutElement.minHeight = badgeHeight;
        badgeLayoutElement.preferredHeight = badgeHeight;
        badgeLayoutElement.flexibleWidth = 0f;

        var badge = badgeObject.GetComponent<RawImage>();
        badge.texture = texture;
        badge.color = Color.white;
        badge.raycastTarget = false;
    }

    private void RefreshResultEntries(int participantCount)
    {
        EnsureResultsListContent();

        if (resultsListContent == null || gameManager == null)
        {
            return;
        }

        var resultsText = gameManager.GetResultsDisplayText();
        var signature = $"{participantCount}:{resultsText}";

        if (lastResultsSignature == signature)
        {
            return;
        }

        lastResultsSignature = signature;

        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(resultsText))
        {
            lines.AddRange(resultsText.Split('\n'));
        }

        ConfigureResultsListLayout();
        RebuildResultRows(lines, participantCount);
    }

    private void HideResultEntries()
    {
        if (resultsListContent == null)
        {
            return;
        }

        for (var i = 0; i < resultsListContent.childCount; i++)
        {
            resultsListContent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void HandleDecreasePlayers()
    {
        gameManager?.AdjustParticipantCount(-1);
        lastRenderedParticipantCount = -1;
        EnsurePlayerNameInputs();
    }

    private void HandleIncreasePlayers()
    {
        gameManager?.AdjustParticipantCount(1);
        lastRenderedParticipantCount = -1;
        EnsurePlayerNameInputs();
    }

    private void HandleStartRound()
    {
        gameManager?.StartRoundFromSetup();
    }

    private void HandleRestartToSetup()
    {
        gameManager?.ResetToSetup();
        lastRenderedParticipantCount = -1;
        EnsurePlayerNameInputs();
    }

    private bool HasBoundSceneUI()
    {
        return rootCanvas != null &&
               shrinkCountdownText != null &&
               stateText != null &&
               roundCountdownText != null &&
               resultsPanel != null &&
               resultsTitleText != null &&
               resultsBodyText != null &&
               setupPanel != null &&
               playerCountText != null &&
               playerListContent != null &&
               decreasePlayersButton != null &&
               increasePlayersButton != null &&
               startButton != null &&
               restartButton != null;
    }

    private void LogMissingSceneUiWarning()
    {
        if (missingSceneUiWarningLogged)
        {
            return;
        }

        missingSceneUiWarningLogged = true;
        Debug.LogWarning(
            "UIManager는 씬 배치형 UI만 사용하도록 설정되어 있습니다. " +
            "UI 참조가 비어 있다면 MainScenes를 연 뒤 Tools > 3D Arena > Ensure Scene UI를 실행해 주세요.",
            this);
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
