using System.Collections.Generic;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    public enum GamePhase
    {
        Setup,
        Countdown,
        Battle,
        Results
    }

    [Header("Round")]
    [Tooltip("현재 게임 진행 단계를 표시합니다.")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Setup;
    [Tooltip("카운트다운이 끝날 때까지 기다리는 시간입니다.")]
    [SerializeField, Min(0f)] private float roundCountdownDuration = 3f;

    [Header("Participants")]
    [Tooltip("이번 라운드에 참가할 인원 수입니다.")]
    [SerializeField, Range(2, 20)] private int participantCount = 2;
    [Tooltip("Play 시작과 동시에 자동으로 라운드를 시작할지 여부입니다.")]
    [SerializeField] private bool autoSpawnPlayersOnPlay = false;
    [Tooltip("셋업 화면에서 사용하는 참가자 이름 목록입니다.")]
    [SerializeField] private List<string> participantNames = new();

    [Header("Scene References")]
    [Tooltip("씬에 배치된 경기장 매니저입니다.")]
    [SerializeField] private ArenaManager arenaManager;
    [Tooltip("생성된 플레이어가 들어갈 부모 오브젝트입니다.")]
    [SerializeField] private Transform playersRoot;
    [Tooltip("플레이어 생성에 사용할 프리팹입니다.")]
    [SerializeField] private PlayerController playerPrefab;
    [Tooltip("셋업, HUD, 결과 화면을 관리하는 UI 매니저입니다.")]
    [SerializeField] private UIManager uiManager;

    [Header("Player Tuning")]
    [Tooltip("모든 플레이어에게 공통으로 적용할 런타임 설정입니다.")]
    [SerializeField] private PlayerRuntimeSettings playerSettings = new();

    [Header("Spawn Power")]
    [Tooltip("플레이어에게 순서대로 적용할 색상 목록입니다.")]
    [SerializeField] private Color[] playerColors =
    {
        new(0.91f, 0.32f, 0.28f),
        new(0.22f, 0.60f, 0.95f),
        new(0.98f, 0.77f, 0.20f),
        new(0.25f, 0.77f, 0.50f),
        new(0.74f, 0.42f, 0.93f),
        new(0.96f, 0.52f, 0.16f),
        new(0.18f, 0.78f, 0.82f),
        new(0.95f, 0.40f, 0.64f)
    };
    [Tooltip("파워 진동 적용 전의 최소 시작 파워입니다.")]
    [SerializeField, Range(1f, 10f)] private float minSpawnPower = 3f;
    [Tooltip("파워 진동 적용 전의 최대 시작 파워입니다.")]
    [SerializeField, Range(1f, 10f)] private float maxSpawnPower = 7f;

    private readonly List<PlayerController> spawnedPlayers = new();
    private readonly List<PlayerController> eliminationOrder = new();
    private float countdownEndTime;

    public GamePhase CurrentPhase => currentPhase;
    public IReadOnlyList<PlayerController> SpawnedPlayers => spawnedPlayers;
    public IReadOnlyList<PlayerController> EliminationOrder => eliminationOrder;
    public int ParticipantCount => participantCount;
    public IReadOnlyList<string> ParticipantNames => participantNames;
    public PlayerController Winner { get; private set; }
    public float CountdownRemaining => currentPhase == GamePhase.Countdown
        ? Mathf.Max(0f, countdownEndTime - Time.time)
        : 0f;
    public int LivingPlayerCount => GetLivingPlayerCount();

    private void OnValidate()
    {
        EnsureParticipantSlots();
        TryResolveReferences();
        TryResolvePlayerPrefab();
    }

    private void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Time.timeScale = 1f;
        EnsureParticipantSlots();
        TryResolveReferences();
        TryResolvePlayerPrefab();
        uiManager?.InitializeRuntimeUI();

        if (!autoSpawnPlayersOnPlay)
        {
            return;
        }

        SpawnPlayersForCurrentRound();
    }

    private void Update()
    {
        if (!Application.isPlaying || currentPhase != GamePhase.Countdown)
        {
            return;
        }

        if (Time.time < countdownEndTime)
        {
            return;
        }

        StartBattlePhase();
    }

    public void SetPhase(GamePhase nextPhase)
    {
        currentPhase = nextPhase;
    }

    public void AdjustParticipantCount(int delta)
    {
        participantCount = Mathf.Clamp(participantCount + delta, 2, 20);
        EnsureParticipantSlots();
    }

    public void SetParticipantName(int index, string value)
    {
        EnsureParticipantSlots();

        if (index < 0 || index >= participantNames.Count)
        {
            return;
        }

        participantNames[index] = value;
    }

    public void StartRoundFromSetup()
    {
        if (currentPhase != GamePhase.Setup)
        {
            return;
        }

        SpawnPlayersForCurrentRound();
    }

    public void ResetToSetup()
    {
        Time.timeScale = 1f;
        currentPhase = GamePhase.Setup;
        countdownEndTime = 0f;
        EnsureParticipantSlots();
        arenaManager?.EndRound();
        arenaManager?.RebuildArena();
        ClearSpawnedPlayers();
    }

    public void SpawnPlayersForCurrentRound()
    {
        Time.timeScale = 1f;
        TryResolveReferences();
        uiManager?.InitializeRuntimeUI();

        if (arenaManager == null || playersRoot == null || playerPrefab == null)
        {
            Debug.LogWarning("GameManager could not find ArenaManager, Players root, or Player prefab.");
            return;
        }

        ClearSpawnedPlayers();
        EnsureParticipantSlots();
        ConfigureArenaForParticipantCount();
        arenaManager.RebuildArena();

        var participantTotal = Mathf.Min(participantCount, arenaManager.Width * arenaManager.Height);
        var tileIndices = BuildShuffledTileIndices();

        for (var i = 0; i < participantTotal; i++)
        {
            var tileIndex = tileIndices[i];
            var tileX = tileIndex % arenaManager.Width;
            var tileY = tileIndex / arenaManager.Width;
            var spawnPosition = arenaManager.GetTileSurfaceWorldPosition(tileX, tileY) + new Vector3(0f, 1f, 0f);

            var controller = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, playersRoot);
            var playerName = GetParticipantName(i);
            var power = Random.Range(minSpawnPower, maxSpawnPower);
            controller.ApplyRuntimeSettings(playerSettings);
            controller.Configure(i, playerName, GetPlayerColor(i), power);
            controller.SetMovementEnabled(false);

            spawnedPlayers.Add(controller);
        }

        currentPhase = GamePhase.Countdown;
        countdownEndTime = Time.time + roundCountdownDuration;
    }

    public void RegisterElimination(PlayerController eliminatedPlayer)
    {
        if (currentPhase == GamePhase.Results || eliminatedPlayer == null || eliminationOrder.Contains(eliminatedPlayer))
        {
            return;
        }

        eliminationOrder.Add(eliminatedPlayer);

        if (GetLivingPlayerCount() > 1)
        {
            return;
        }

        FinishRound();
    }

    private void TryResolveReferences()
    {
        var root = transform.parent;

        if (root == null)
        {
            return;
        }

        var arenaReferenceIsInvalid =
            arenaManager == null ||
            !arenaManager.gameObject.scene.IsValid() ||
            arenaManager.transform.parent != root;

        if (arenaReferenceIsInvalid)
        {
            var arena = root.Find("Arena");

            if (arena != null)
            {
                arenaManager = arena.GetComponent<ArenaManager>();
            }
        }

        var playersRootIsInvalid =
            playersRoot == null ||
            !playersRoot.gameObject.scene.IsValid() ||
            playersRoot.parent != root;

        if (playersRootIsInvalid)
        {
            playersRoot = root.Find("Players");
        }

        var uiManagerReferenceIsInvalid =
            uiManager == null ||
            !uiManager.gameObject.scene.IsValid() ||
            uiManager.transform.parent != root;

        if (uiManagerReferenceIsInvalid)
        {
            var ui = root.Find("UI");

            if (ui != null)
            {
                uiManager = ui.GetComponent<UIManager>();
            }
        }
    }

    private void TryResolvePlayerPrefab()
    {
        if (playerPrefab != null)
        {
            return;
        }

        playerPrefab = Resources.Load<PlayerController>("Prefabs/Player");
    }

    private void ClearSpawnedPlayers()
    {
        spawnedPlayers.Clear();
        eliminationOrder.Clear();
        Winner = null;

        if (playersRoot == null)
        {
            return;
        }

        for (var i = playersRoot.childCount - 1; i >= 0; i--)
        {
            var child = playersRoot.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
                continue;
            }

            DestroyImmediate(child);
        }
    }

    private void EnsureParticipantSlots()
    {
        participantCount = Mathf.Clamp(participantCount, 2, 20);

        while (participantNames.Count < participantCount)
        {
            participantNames.Add($"Player {participantNames.Count + 1}");
        }

        while (participantNames.Count > participantCount)
        {
            participantNames.RemoveAt(participantNames.Count - 1);
        }
    }

    private void ConfigureArenaForParticipantCount()
    {
        if (arenaManager == null)
        {
            return;
        }

        var targetSize = participantCount switch
        {
            <= 8 => 15,
            <= 12 => 18,
            <= 16 => 21,
            _ => 24
        };

        arenaManager.ConfigureArenaSize(targetSize, targetSize);
    }

    private int[] BuildShuffledTileIndices()
    {
        var totalTiles = arenaManager.Width * arenaManager.Height;
        var indices = new int[totalTiles];

        for (var i = 0; i < totalTiles; i++)
        {
            indices[i] = i;
        }

        for (var i = totalTiles - 1; i > 0; i--)
        {
            var randomIndex = Random.Range(0, i + 1);
            (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
        }

        return indices;
    }

    private string GetParticipantName(int index)
    {
        if (index < participantNames.Count && !string.IsNullOrWhiteSpace(participantNames[index]))
        {
            return participantNames[index];
        }

        return $"Player {index + 1}";
    }

    private Color GetPlayerColor(int index)
    {
        if (playerColors == null || playerColors.Length == 0)
        {
            return Color.white;
        }

        return playerColors[index % playerColors.Length];
    }

    public string GetPhaseDisplayText()
    {
        return currentPhase switch
        {
            GamePhase.Setup => "State: Setup",
            GamePhase.Countdown => $"State: Countdown ({CountdownRemaining:0.0}s)",
            GamePhase.Battle => $"State: Battle ({LivingPlayerCount} alive)",
            GamePhase.Results => Winner != null
                ? $"State: Results - Winner {Winner.DisplayName}"
                : "State: Results",
            _ => "State: Unknown"
        };
    }

    public string GetResultsDisplayText()
    {
        if (spawnedPlayers.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>();
        var placement = 1;

        if (Winner != null)
        {
            lines.Add($"{placement}. {Winner.DisplayName} (Winner)");
            placement++;
        }

        for (var i = eliminationOrder.Count - 1; i >= 0; i--)
        {
            var eliminatedPlayer = eliminationOrder[i];

            if (eliminatedPlayer == null)
            {
                continue;
            }

            lines.Add($"{placement}. {eliminatedPlayer.DisplayName}");
            placement++;
        }

        return string.Join("\n", lines);
    }

    private int GetLivingPlayerCount()
    {
        var livingPlayers = 0;

        foreach (var player in spawnedPlayers)
        {
            if (player != null && !player.IsEliminated)
            {
                livingPlayers++;
            }
        }

        return livingPlayers;
    }

    private void StartBattlePhase()
    {
        arenaManager?.BeginRound();
        currentPhase = GamePhase.Battle;
        countdownEndTime = 0f;

        foreach (var player in spawnedPlayers)
        {
            if (player == null || player.IsEliminated)
            {
                continue;
            }

            player.SetMovementEnabled(true);
        }
    }

    private void FinishRound()
    {
        Winner = null;

        foreach (var player in spawnedPlayers)
        {
            if (player != null && !player.IsEliminated)
            {
                Winner = player;
                break;
            }
        }

        currentPhase = GamePhase.Results;
        countdownEndTime = 0f;
        arenaManager?.EndRound();

        foreach (var player in spawnedPlayers)
        {
            if (player == null || player.IsEliminated)
            {
                continue;
            }

            player.FreezeForResults();
        }

        Time.timeScale = 0f;

        if (Winner != null)
        {
            Debug.Log($"Winner: {Winner.DisplayName}");
        }
    }
}

