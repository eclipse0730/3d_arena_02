using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public sealed class PlayerRuntimeSettings
{
    [Header("Movement")]
    [Tooltip("기본 이동 속도입니다.")]
    [Min(0.5f)] public float moveSpeed = 3.5f;
    [Tooltip("목표 속도까지 가속되는 정도입니다.")]
    [Min(0.5f)] public float moveAcceleration = 14f;
    [Tooltip("이동 방향을 다시 고르는 평균 간격입니다.")]
    [Min(0.5f)] public float directionChangeInterval = 1.75f;
    [Tooltip("경기장 중앙 쪽으로 당기는 기본 힘입니다.")]
    [Min(0f)] public float centerPullStrength = 0.45f;
    [Tooltip("가장자리 판정을 조금 안쪽으로 당길 여유 폭입니다.")]
    [Min(0f)] public float edgeMargin = 1.1f;
    [Tooltip("가장자리 근처에서 중앙으로 꺾는 보정 강도입니다.")]
    [Min(0f)] public float edgeSteeringStrength = 2.2f;
    [Tooltip("회전 반응 속도입니다.")]
    [Min(1f)] public float turnSpeed = 8f;

    [Header("Collision")]
    [Tooltip("충돌 시 밀려나는 기본 힘입니다.")]
    [Min(0.5f)] public float knockbackForce = 5.5f;
    [Tooltip("충돌 시 위로 뜨는 힘입니다.")]
    [Min(0f)] public float upwardKnockbackForce = 3.6f;
    [Tooltip("연속 충돌 판정 사이의 최소 간격입니다.")]
    [Min(0f)] public float collisionCooldown = 0.15f;
    [Tooltip("충돌 직후 이동 제어를 잠시 멈추는 시간입니다.")]
    [Min(0f)] public float collisionRecoveryDuration = 0.45f;

    [Header("Power")]
    [Tooltip("기본 파워를 기준으로 얼마나 크게 오르내릴지 정합니다.")]
    [Min(0f)] public float powerOscillationAmplitude = 1.75f;
    [Tooltip("파워가 한 번 강해졌다 약해지는 전체 주기입니다.")]
    [Min(0.1f)] public float powerOscillationCycleDuration = 4.5f;

    [Header("Elimination")]
    [Tooltip("경기장 밖 판정에 사용할 추가 여유입니다.")]
    [Min(0f)] public float outsideArenaMargin = 0.3f;
    [Tooltip("경기장 밖에 나간 뒤 탈락되기까지의 시간입니다.")]
    [Min(0f)] public float outsideArenaEliminationDelay = 0.2f;
    [Tooltip("이 높이 아래로 떨어지면 즉시 탈락합니다.")]
    public float fallThresholdY = -2f;
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PlayerController : MonoBehaviour
{
    private const string OverheadCanvasName = "OverheadCanvas";
    private const string NameLabelName = "NameLabel";
    private const string PowerBarBackgroundName = "PowerBarBackground";
    private const string PowerBarFillName = "PowerBarFill";

    [SerializeField] private string displayName = "Player";
    [SerializeField] private int playerIndex;
    [SerializeField, Range(1f, 10f)] private float power = 5f;
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private bool autoMoveEnabled = true;
    [SerializeField] private PlayerRuntimeSettings runtimeSettings = new();

    private Rigidbody cachedRigidbody;
    private CapsuleCollider cachedCollider;
    private Renderer cachedRenderer;
    private Canvas overheadCanvas;
    private Text overheadNameText;
    private Image powerBarFillImage;
    private RectTransform powerBarFillRect;
    private ArenaManager arenaManager;
    private GameManager gameManager;
    private Camera cachedMainCamera;
    private Vector3 moveDirection = Vector3.forward;
    private float basePower = 5f;
    private float currentPower = 5f;
    private float powerOscillationOffset;
    private float nextDirectionChangeTime;
    private float lastCollisionTime = -999f;
    private float movementResumeTime;
    private float outsideArenaStartTime = -1f;
    private bool isEliminated;

    public string DisplayName => displayName;
    public int PlayerIndex => playerIndex;
    public float Power => currentPower;
    public Rigidbody CachedRigidbody => cachedRigidbody;
    public bool IsEliminated => isEliminated;

    private void Awake()
    {
        EnsureSetup();
        CacheArenaManager();
        CacheGameManager();
        EnsureOverheadUI();
        RefreshOverheadUI();
    }

    private void OnValidate()
    {
        EnsureSetup();
        ApplyVisuals();
    }

    private void Start()
    {
        CacheArenaManager();
        CacheGameManager();
        EnsureOverheadUI();
        RefreshOverheadUI();
        ChooseNewDirection();
    }

    private void Update()
    {
        if (!Application.isPlaying || isEliminated)
        {
            return;
        }

        UpdateDynamicPower();
        RefreshOverheadUI();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateOverheadFacing();
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying || !autoMoveEnabled || cachedRigidbody == null)
        {
            return;
        }

        if (!isEliminated && transform.position.y <= runtimeSettings.fallThresholdY)
        {
            Eliminate();
            return;
        }

        if (isEliminated)
        {
            return;
        }

        if (Time.time >= nextDirectionChangeTime)
        {
            ChooseNewDirection();
        }

        if (Time.time < movementResumeTime)
        {
            return;
        }

        var desiredDirection = moveDirection;

        if (arenaManager != null)
        {
            var centerOffset = arenaManager.GetArenaCenterWorldPosition() - transform.position;
            centerOffset.y = 0f;
            var localPosition = arenaManager.transform.InverseTransformPoint(transform.position);
            var halfWidth = Mathf.Max(0.1f, (arenaManager.CurrentBoardWidth * 0.5f) - runtimeSettings.edgeMargin);
            var halfHeight = Mathf.Max(0.1f, (arenaManager.CurrentBoardHeight * 0.5f) - runtimeSettings.edgeMargin);
            var normalizedX = halfWidth > 0f ? Mathf.Abs(localPosition.x) / halfWidth : 0f;
            var normalizedZ = halfHeight > 0f ? Mathf.Abs(localPosition.z) / halfHeight : 0f;
            var edgeFactor = Mathf.Max(normalizedX, normalizedZ);
            var edgePull = Mathf.Clamp01((edgeFactor - 0.55f) / 0.45f);

            if (centerOffset.sqrMagnitude > 0.001f)
            {
                desiredDirection += centerOffset.normalized * (runtimeSettings.centerPullStrength + (edgePull * runtimeSettings.edgeSteeringStrength));
            }

            if (!arenaManager.IsInsideArena(transform.position, runtimeSettings.outsideArenaMargin))
            {
                outsideArenaStartTime = outsideArenaStartTime < 0f ? Time.time : outsideArenaStartTime;

                desiredDirection = centerOffset.sqrMagnitude > 0.001f
                    ? centerOffset.normalized
                    : -transform.forward;

                if (Time.time >= outsideArenaStartTime + runtimeSettings.outsideArenaEliminationDelay)
                {
                    Eliminate();
                    return;
                }
            }
            else
            {
                outsideArenaStartTime = -1f;
            }
        }

        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        desiredDirection.Normalize();

        var currentVelocity = cachedRigidbody.linearVelocity;
        var currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        var targetHorizontalVelocity = desiredDirection * runtimeSettings.moveSpeed;
        var nextHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            runtimeSettings.moveAcceleration * Time.fixedDeltaTime);

        cachedRigidbody.linearVelocity = new Vector3(nextHorizontalVelocity.x, currentVelocity.y, nextHorizontalVelocity.z);

        var targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, runtimeSettings.turnSpeed * Time.fixedDeltaTime);
    }

    public void Configure(int index, string newDisplayName, Color newTint, float newPower)
    {
        playerIndex = index;
        displayName = newDisplayName;
        tint = newTint;
        power = Mathf.Clamp(newPower, 1f, 10f);
        basePower = power;
        currentPower = power;
        powerOscillationOffset = Random.Range(0f, Mathf.PI * 2f);
        gameObject.name = newDisplayName;

        EnsureSetup();
        ApplyVisuals();
        EnsureOverheadUI();
        RefreshOverheadUI();
    }

    public void ApplyRuntimeSettings(PlayerRuntimeSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        runtimeSettings.moveSpeed = settings.moveSpeed;
        runtimeSettings.moveAcceleration = settings.moveAcceleration;
        runtimeSettings.directionChangeInterval = settings.directionChangeInterval;
        runtimeSettings.centerPullStrength = settings.centerPullStrength;
        runtimeSettings.edgeMargin = settings.edgeMargin;
        runtimeSettings.edgeSteeringStrength = settings.edgeSteeringStrength;
        runtimeSettings.turnSpeed = settings.turnSpeed;
        runtimeSettings.knockbackForce = settings.knockbackForce;
        runtimeSettings.upwardKnockbackForce = settings.upwardKnockbackForce;
        runtimeSettings.powerOscillationAmplitude = settings.powerOscillationAmplitude;
        runtimeSettings.powerOscillationCycleDuration = settings.powerOscillationCycleDuration;
        runtimeSettings.collisionCooldown = settings.collisionCooldown;
        runtimeSettings.collisionRecoveryDuration = settings.collisionRecoveryDuration;
        runtimeSettings.outsideArenaMargin = settings.outsideArenaMargin;
        runtimeSettings.outsideArenaEliminationDelay = settings.outsideArenaEliminationDelay;
        runtimeSettings.fallThresholdY = settings.fallThresholdY;
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        autoMoveEnabled = isEnabled;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.linearVelocity = new Vector3(0f, cachedRigidbody.linearVelocity.y, 0f);
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        if (isEnabled)
        {
            ChooseNewDirection();
        }
    }

    public void FreezeForResults()
    {
        autoMoveEnabled = false;

        if (cachedRigidbody == null)
        {
            return;
        }

        cachedRigidbody.linearVelocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
        cachedRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        cachedRigidbody.Sleep();
    }

    private void EnsureOverheadUI()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        var existingCanvasTransform = transform.Find(OverheadCanvasName);

        if (overheadCanvas == null && existingCanvasTransform != null)
        {
            overheadCanvas = existingCanvasTransform.GetComponent<Canvas>();
        }

        if (overheadCanvas == null)
        {
            var canvasObject = new GameObject(
                OverheadCanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            overheadCanvas = canvasObject.GetComponent<Canvas>();
        }

        overheadCanvas.renderMode = RenderMode.WorldSpace;
        overheadCanvas.sortingOrder = 25;
        overheadCanvas.worldCamera = Camera.main;

        var canvasRect = overheadCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(220f, 80f);
        canvasRect.localScale = Vector3.one * 0.01f;
        canvasRect.localPosition = new Vector3(0f, 1.65f, 0f);
        canvasRect.localRotation = Quaternion.identity;

        if (overheadCanvas.TryGetComponent<CanvasScaler>(out var scaler))
        {
            scaler.dynamicPixelsPerUnit = 16f;
        }

        overheadNameText = EnsureOverheadText();
        powerBarFillImage = EnsurePowerBarFill();
        powerBarFillRect = powerBarFillImage != null ? powerBarFillImage.rectTransform : null;
    }

    private Text EnsureOverheadText()
    {
        var existing = overheadCanvas.transform.Find(NameLabelName);
        Text text = null;

        if (existing != null)
        {
            text = existing.GetComponent<Text>();
        }

        if (text == null)
        {
            var textObject = new GameObject(NameLabelName, typeof(RectTransform));
            textObject.transform.SetParent(overheadCanvas.transform, false);
            text = textObject.AddComponent<Text>();
        }

        var rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -4f);
        rectTransform.sizeDelta = new Vector2(200f, 30f);

        text.font = LoadBuiltinFont();
        text.fontSize = 22;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = string.Empty;
        return text;
    }

    private Image EnsurePowerBarFill()
    {
        var backgroundTransform = overheadCanvas.transform.Find(PowerBarBackgroundName);
        Image backgroundImage = null;

        if (backgroundTransform != null)
        {
            backgroundImage = backgroundTransform.GetComponent<Image>();
        }

        if (backgroundImage == null)
        {
            var backgroundObject = new GameObject(PowerBarBackgroundName, typeof(RectTransform));
            backgroundObject.transform.SetParent(overheadCanvas.transform, false);
            backgroundImage = backgroundObject.AddComponent<Image>();
        }

        var backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = new Vector2(0.5f, 1f);
        backgroundRect.anchorMax = new Vector2(0.5f, 1f);
        backgroundRect.pivot = new Vector2(0.5f, 1f);
        backgroundRect.anchoredPosition = new Vector2(0f, -36f);
        backgroundRect.sizeDelta = new Vector2(144f, 16f);
        backgroundImage.color = new Color(0.12f, 0.15f, 0.2f, 0.92f);

        var fillTransform = backgroundImage.transform.Find(PowerBarFillName);
        Image fillImage = null;

        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponent<Image>();
        }

        if (fillImage == null)
        {
            var fillObject = new GameObject(PowerBarFillName, typeof(RectTransform));
            fillObject.transform.SetParent(backgroundImage.transform, false);
            fillImage = fillObject.AddComponent<Image>();
        }

        var fillRect = fillImage.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(4f, 0f);
        fillRect.sizeDelta = new Vector2(136f, -4f);
        return fillImage;
    }

    private void RefreshOverheadUI()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureOverheadUI();

        if (overheadNameText != null)
        {
            overheadNameText.text = displayName;
        }

        if (powerBarFillImage == null || powerBarFillRect == null)
        {
            return;
        }

        var normalizedPower = Mathf.InverseLerp(1f, 10f, currentPower);
        powerBarFillRect.sizeDelta = new Vector2(Mathf.Lerp(12f, 136f, normalizedPower), -4f);
        powerBarFillImage.color = Color.Lerp(
            new Color(0.93f, 0.34f, 0.22f, 1f),
            new Color(0.27f, 0.88f, 0.46f, 1f),
            normalizedPower);
    }

    private void UpdateOverheadFacing()
    {
        if (overheadCanvas == null)
        {
            return;
        }

        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
        }

        if (cachedMainCamera == null)
        {
            return;
        }

        overheadCanvas.worldCamera = cachedMainCamera;
        var canvasTransform = overheadCanvas.transform;
        var forward = cachedMainCamera.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        canvasTransform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    }

    private void UpdateDynamicPower()
    {
        var amplitude = Mathf.Max(0f, runtimeSettings.powerOscillationAmplitude);
        var cycleDuration = Mathf.Max(0.1f, runtimeSettings.powerOscillationCycleDuration);

        if (amplitude <= 0f)
        {
            currentPower = basePower;
            return;
        }

        var oscillation = Mathf.Sin(((Time.time / cycleDuration) * Mathf.PI * 2f) + powerOscillationOffset);
        currentPower = Mathf.Clamp(basePower + (oscillation * amplitude), 1f, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Application.isPlaying || isEliminated)
        {
            return;
        }

        if (Time.time < lastCollisionTime + runtimeSettings.collisionCooldown)
        {
            return;
        }

        if (!collision.gameObject.TryGetComponent<PlayerController>(out var otherPlayer))
        {
            return;
        }

        if (otherPlayer.IsEliminated)
        {
            return;
        }

        if (GetInstanceID() > otherPlayer.GetInstanceID())
        {
            return;
        }

        var contactDirection = otherPlayer.transform.position - transform.position;
        contactDirection.y = 0f;

        if (contactDirection.sqrMagnitude < 0.001f)
        {
            contactDirection = transform.forward;
        }

        contactDirection.Normalize();

        ApplyCollisionImpulse(this, -contactDirection, otherPlayer.Power);
        ApplyCollisionImpulse(otherPlayer, contactDirection, currentPower);
    }

    private void EnsureSetup()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<CapsuleCollider>();
        cachedRenderer = GetComponent<Renderer>();

        if (cachedRigidbody == null || cachedCollider == null)
        {
            return;
        }

        cachedRigidbody.mass = 1f;
        cachedRigidbody.linearDamping = 0.45f;
        cachedRigidbody.angularDamping = 2f;
        cachedRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        cachedCollider.height = 2f;
        cachedCollider.radius = 0.5f;
        cachedCollider.center = Vector3.zero;

        transform.localScale = new Vector3(0.8f, 1f, 0.8f);
    }

    private void ApplyVisuals()
    {
        if (cachedRenderer == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            var sharedMaterial = EnsureSharedMaterial();

            if (sharedMaterial == null)
            {
                return;
            }

            sharedMaterial.color = tint;
            return;
        }
#endif

        if (Application.isPlaying)
        {
            var runtimeMaterial = cachedRenderer.material;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = tint;
            }

            return;
        }

        var editorMaterial = EnsureSharedMaterial();

        if (editorMaterial != null)
        {
            editorMaterial.color = tint;
        }
    }

    private Material EnsureSharedMaterial()
    {
        if (cachedRenderer == null)
        {
            return null;
        }

        if (cachedRenderer.sharedMaterial != null)
        {
            return cachedRenderer.sharedMaterial;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        cachedRenderer.sharedMaterial = new Material(shader);
        return cachedRenderer.sharedMaterial;
    }

    private void CacheArenaManager()
    {
        if (arenaManager != null)
        {
            return;
        }

        arenaManager = FindFirstObjectByType<ArenaManager>();
    }

    private void CacheGameManager()
    {
        if (gameManager != null)
        {
            return;
        }

        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void ChooseNewDirection()
    {
        var randomX = Random.Range(-1f, 1f);
        var randomZ = Random.Range(-1f, 1f);
        var randomDirection = new Vector3(randomX, 0f, randomZ);

        if (randomDirection.sqrMagnitude < 0.001f)
        {
            randomDirection = Vector3.forward;
        }

        moveDirection = randomDirection.normalized;
        nextDirectionChangeTime = Time.time + Random.Range(runtimeSettings.directionChangeInterval * 0.75f, runtimeSettings.directionChangeInterval * 1.25f);
    }

    private static void ApplyCollisionImpulse(PlayerController targetPlayer, Vector3 direction, float sourcePower)
    {
        if (targetPlayer == null || targetPlayer.cachedRigidbody == null)
        {
            return;
        }

        var totalKnockback = targetPlayer.runtimeSettings.knockbackForce * (sourcePower / 4f);
        var impulse = (direction * totalKnockback) + (Vector3.up * targetPlayer.runtimeSettings.upwardKnockbackForce);
        targetPlayer.cachedRigidbody.AddForce(impulse, ForceMode.Impulse);
        targetPlayer.lastCollisionTime = Time.time;
        targetPlayer.movementResumeTime = Time.time + targetPlayer.runtimeSettings.collisionRecoveryDuration;
    }

    private void Eliminate()
    {
        if (isEliminated)
        {
            return;
        }

        isEliminated = true;
        autoMoveEnabled = false;
        gameManager?.RegisterElimination(this);
        gameObject.SetActive(false);
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
