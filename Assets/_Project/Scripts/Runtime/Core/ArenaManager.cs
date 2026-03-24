using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class ArenaManager : MonoBehaviour
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseMapProperty = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    [SerializeField, Min(4)] private int width = 15;
    [SerializeField, Min(4)] private int height = 15;
    [SerializeField, Min(0.5f)] private float tileSize = 1f;
    [SerializeField, Min(0.1f)] private float tileHeight = 0.2f;
    [SerializeField, Min(0f)] private float tileGap = 0.05f;
    [SerializeField] private GameObject tileVisualPrefab;
    [SerializeField, Min(0f)] private float shrinkStartDelay = 10f;
    [SerializeField, Min(0.2f)] private float shrinkInterval = 5.8f;
    [SerializeField, Min(5)] private int minimumActiveWidth = 5;
    [SerializeField, Min(5)] private int minimumActiveHeight = 5;
    [SerializeField, Min(0.5f)] private float collapseFallDistance = 8f;
    [SerializeField, Min(0.5f)] private float collapseFallSpeed = 12f;
    [SerializeField, Min(0f)] private float ringCollapseCascadeDelay = 0.03f;
    [SerializeField] private bool randomizeRingCollapseOrder = true;
    [SerializeField, Min(0f)] private float ringCollapseDelayJitter = 0.045f;
    [SerializeField] private Color tileBaseColor = new(0.72f, 0.76f, 0.84f, 1f);
    [SerializeField] private Color collapseWarningColor = new(0.92f, 0.22f, 0.18f, 1f);
    [SerializeField, Min(0f)] private float collapseWarningLeadTime = 1.15f;

    private const string TileRootName = "Tiles";
    private const string TileNamePrefix = "Tile_";
    private const string GroundName = "Ground";

    private Transform[,] tiles;
    private Renderer[,] tileRenderers;
    private readonly List<FallingTile> fallingTiles = new();
    private readonly List<ScheduledCollapse> scheduledCollapses = new();
    private Material tileSharedMaterial;
    private MaterialPropertyBlock tilePropertyBlock;
    private bool roundActive;
    private float nextShrinkTime;
    private int activeMinX;
    private int activeMaxX;
    private int activeMinY;
    private int activeMaxY;

    public int Width => width;
    public int Height => height;
    public float TileSize => tileSize;
    public float BoardWidth => (width * tileSize) + ((width - 1) * tileGap);
    public float BoardHeight => (height * tileSize) + ((height - 1) * tileGap);
    public float CurrentBoardWidth => GetCurrentBoardWidth();
    public float CurrentBoardHeight => GetCurrentBoardHeight();
    public bool RoundActive => roundActive;
    public bool CanContinueShrinking => CanShrink();
    public float SecondsUntilNextShrink => !roundActive || !CanShrink() ? 0f : Mathf.Max(0f, nextShrinkTime - Time.time);

    public void ConfigureArenaSize(int newWidth, int newHeight)
    {
        var clampedWidth = Mathf.Max(4, newWidth);
        var clampedHeight = Mathf.Max(4, newHeight);

        if (width == clampedWidth && height == clampedHeight)
        {
            return;
        }

        width = clampedWidth;
        height = clampedHeight;
        
        if (Application.isPlaying)
        {
            RebuildArena();
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        RebuildArena();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateFallingTiles();
        UpdateScheduledCollapses();
        UpdateTileWarningVisuals();

        if (!roundActive)
        {
            return;
        }

        if (Time.time < nextShrinkTime || !CanShrink())
        {
            return;
        }

        CollapseOuterRing();
        nextShrinkTime = Time.time + shrinkInterval;
    }

    public Vector3 GetTileCenterWorldPosition(int x, int y)
    {
        return transform.TransformPoint(CalculateTileLocalPosition(x, y));
    }

    public Vector3 GetTileSurfaceWorldPosition(int x, int y)
    {
        var localPosition = CalculateTileLocalPosition(x, y);
        localPosition.y += tileHeight * 0.5f;
        return transform.TransformPoint(localPosition);
    }

    public Vector3 GetRandomSpawnPosition(float yOffset = 0.75f)
    {
        var randomX = Random.Range(0, width);
        var randomY = Random.Range(0, height);
        var tileCenter = GetTileSurfaceWorldPosition(randomX, randomY);
        tileCenter.y += yOffset;
        return tileCenter;
    }

    public Vector3 GetArenaCenterWorldPosition()
    {
        return transform.position;
    }

    public bool IsInsideArena(Vector3 worldPosition, float margin = 0f)
    {
        var localPosition = transform.InverseTransformPoint(worldPosition);
        var halfWidth = (GetCurrentBoardWidth() * 0.5f) - margin;
        var halfHeight = (GetCurrentBoardHeight() * 0.5f) - margin;

        return Mathf.Abs(localPosition.x) <= halfWidth && Mathf.Abs(localPosition.z) <= halfHeight;
    }

    public void BeginRound()
    {
        ResetActiveBounds();
        roundActive = true;
        nextShrinkTime = Time.time + shrinkStartDelay;
        UpdateTileWarningVisuals();
    }

    public void EndRound()
    {
        roundActive = false;
        UpdateTileWarningVisuals();
    }

    public void RebuildArena()
    {
        var tileRoot = FindOrCreateTileRoot();
        ClearChildren(tileRoot);
        fallingTiles.Clear();
        scheduledCollapses.Clear();
        roundActive = false;
        nextShrinkTime = 0f;

        if (tileSharedMaterial != null)
        {
            Destroy(tileSharedMaterial);
        }

        tiles = new Transform[width, height];
        tileRenderers = new Renderer[width, height];
        tileSharedMaterial = tileVisualPrefab == null ? CreateTileMaterial() : null;
        tilePropertyBlock ??= new MaterialPropertyBlock();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tileObject = CreateTileObject(tileRoot, x, y);
                var renderers = tileObject.GetComponentsInChildren<Renderer>(true);
                var renderer = renderers.Length > 0 ? renderers[0] : null;

                tiles[x, y] = tileObject.transform;
                tileRenderers[x, y] = renderer;

                if (renderers.Length > 0)
                {
                    if (tileSharedMaterial == null)
                    {
                        tileSharedMaterial = CreateTileMaterialFromSource(renderers);
                    }

                    ApplyTileMaterial(renderers);
                }

                if (renderer != null)
                {
                    ApplyTileColor(renderer, tileBaseColor);
                }
            }
        }

        ResetActiveBounds();
        FitGroundToArena();
    }

    private GameObject CreateTileObject(Transform tileRoot, int x, int y)
    {
        var tileObject = tileVisualPrefab != null
            ? Instantiate(tileVisualPrefab, tileRoot)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);

        tileObject.name = $"{TileNamePrefix}{x}_{y}";
        tileObject.transform.SetParent(tileRoot, false);
        tileObject.transform.localRotation = Quaternion.identity;

        if (tileVisualPrefab == null)
        {
            tileObject.transform.localPosition = CalculateTileLocalPosition(x, y);
            tileObject.transform.localScale = new Vector3(tileSize, tileHeight, tileSize);
            return tileObject;
        }

        var visualBounds = GetCombinedRendererLocalBounds(tileObject.transform);
        var safeSize = new Vector3(
            Mathf.Max(visualBounds.size.x, 0.001f),
            Mathf.Max(visualBounds.size.y, 0.001f),
            Mathf.Max(visualBounds.size.z, 0.001f));

        var targetScale = new Vector3(
            tileSize / safeSize.x,
            tileHeight / safeSize.y,
            tileSize / safeSize.z);

        tileObject.transform.localScale = targetScale;
        tileObject.transform.localPosition = CalculateTileLocalPosition(x, y) - Vector3.Scale(visualBounds.center, targetScale);

        EnsureTileCollider(tileObject, visualBounds);
        return tileObject;
    }

    private Transform FindOrCreateTileRoot()
    {
        var tileRoot = transform.Find(TileRootName);

        if (tileRoot != null)
        {
            return tileRoot;
        }

        var tileRootObject = new GameObject(TileRootName);
        tileRootObject.transform.SetParent(transform, false);
        return tileRootObject.transform;
    }

    private void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;

            Destroy(child);
        }
    }

    private Bounds GetCombinedRendererLocalBounds(Transform root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        var initialized = false;
        var combinedBounds = new Bounds(Vector3.zero, Vector3.one);

        foreach (var renderer in renderers)
        {
            var rendererBounds = GetRendererBoundsInRootSpace(root, renderer);

            if (!initialized)
            {
                combinedBounds = rendererBounds;
                initialized = true;
                continue;
            }

            combinedBounds.Encapsulate(rendererBounds.min);
            combinedBounds.Encapsulate(rendererBounds.max);
        }

        return initialized ? combinedBounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private Bounds GetRendererBoundsInRootSpace(Transform root, Renderer renderer)
    {
        var localBounds = renderer.localBounds;
        var extents = localBounds.extents;
        var initialized = false;
        var bounds = new Bounds(Vector3.zero, Vector3.zero);

        for (var x = -1; x <= 1; x += 2)
        {
            for (var y = -1; y <= 1; y += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var rendererLocalPoint = localBounds.center + Vector3.Scale(extents, new Vector3(x, y, z));
                    var worldPoint = renderer.transform.TransformPoint(rendererLocalPoint);
                    var rootLocalPoint = root.InverseTransformPoint(worldPoint);

                    if (!initialized)
                    {
                        bounds = new Bounds(rootLocalPoint, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(rootLocalPoint);
                    }
                }
            }
        }

        return initialized ? bounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private void EnsureTileCollider(GameObject tileObject, Bounds localBounds)
    {
        var collider = tileObject.GetComponent<BoxCollider>();

        if (collider == null)
        {
            collider = tileObject.AddComponent<BoxCollider>();
        }

        collider.center = localBounds.center;
        collider.size = localBounds.size;
    }

    private Vector3 CalculateTileLocalPosition(int x, int y)
    {
        var step = tileSize + tileGap;
        var originX = -((width - 1) * step) * 0.5f;
        var originZ = -((height - 1) * step) * 0.5f;
        var tileCenterY = tileHeight * 0.5f;

        return new Vector3(
            originX + (x * step),
            tileCenterY,
            originZ + (y * step));
    }

    private void ResetActiveBounds()
    {
        activeMinX = 0;
        activeMaxX = width - 1;
        activeMinY = 0;
        activeMaxY = height - 1;
    }

    private bool CanShrink()
    {
        return GetCurrentActiveWidth() > minimumActiveWidth && GetCurrentActiveHeight() > minimumActiveHeight;
    }

    private void CollapseOuterRing()
    {
        var ringTiles = new List<Vector2Int>();
        CollectTileRange(ringTiles, activeMinX, activeMaxX, activeMinY, activeMinY);
        CollectTileRange(ringTiles, activeMinX, activeMaxX, activeMaxY, activeMaxY);
        CollectTileRange(ringTiles, activeMinX, activeMinX, activeMinY + 1, activeMaxY - 1);
        CollectTileRange(ringTiles, activeMaxX, activeMaxX, activeMinY + 1, activeMaxY - 1);

        if (randomizeRingCollapseOrder)
        {
            ShuffleRingTiles(ringTiles);
        }

        ScheduleRingCollapse(ringTiles);

        activeMinX++;
        activeMaxX--;
        activeMinY++;
        activeMaxY--;
    }

    private void CollectTileRange(List<Vector2Int> ringTiles, int minX, int maxX, int minY, int maxY)
    {
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (tiles == null || x < 0 || y < 0 || x >= width || y >= height || tiles[x, y] == null)
                {
                    continue;
                }

                ringTiles.Add(new Vector2Int(x, y));
            }
        }
    }

    private void ShuffleRingTiles(List<Vector2Int> ringTiles)
    {
        for (var i = ringTiles.Count - 1; i > 0; i--)
        {
            var randomIndex = Random.Range(0, i + 1);
            (ringTiles[i], ringTiles[randomIndex]) = (ringTiles[randomIndex], ringTiles[i]);
        }
    }

    private void ScheduleRingCollapse(List<Vector2Int> ringTiles)
    {
        for (var i = 0; i < ringTiles.Count; i++)
        {
            var tile = ringTiles[i];
            var jitter = randomizeRingCollapseOrder ? Random.Range(0f, ringCollapseDelayJitter) : 0f;
            ScheduleCollapse(tile.x, tile.y, (i * ringCollapseCascadeDelay) + jitter);
        }
    }

    private void ScheduleCollapse(int x, int y, float delay)
    {
        if (tiles == null || x < 0 || y < 0 || x >= width || y >= height || tiles[x, y] == null)
        {
            return;
        }

        scheduledCollapses.Add(new ScheduledCollapse(x, y, Time.time + delay));
    }

    private void CollapseTile(int x, int y)
    {
        if (tiles == null || x < 0 || y < 0 || x >= width || y >= height)
        {
            return;
        }

        var tile = tiles[x, y];

        if (tile == null)
        {
            return;
        }

        tiles[x, y] = null;
        tileRenderers[x, y] = null;

        if (tile.TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }

        fallingTiles.Add(new FallingTile(tile, tile.localPosition.y - collapseFallDistance));
    }

    private void UpdateScheduledCollapses()
    {
        for (var i = scheduledCollapses.Count - 1; i >= 0; i--)
        {
            var scheduledCollapse = scheduledCollapses[i];

            if (Time.time < scheduledCollapse.TriggerTime)
            {
                continue;
            }

            CollapseTile(scheduledCollapse.X, scheduledCollapse.Y);
            scheduledCollapses.RemoveAt(i);
        }
    }

    private void UpdateFallingTiles()
    {
        for (var i = fallingTiles.Count - 1; i >= 0; i--)
        {
            var fallingTile = fallingTiles[i];

            if (fallingTile.Transform == null)
            {
                fallingTiles.RemoveAt(i);
                continue;
            }

            var localPosition = fallingTile.Transform.localPosition;
            localPosition.y = Mathf.MoveTowards(localPosition.y, fallingTile.TargetLocalY, collapseFallSpeed * Time.deltaTime);
            fallingTile.Transform.localPosition = localPosition;

            if (Mathf.Approximately(localPosition.y, fallingTile.TargetLocalY))
            {
                fallingTile.Transform.gameObject.SetActive(false);
                fallingTiles.RemoveAt(i);
            }
        }
    }

    private int GetCurrentActiveWidth()
    {
        return activeMaxX - activeMinX + 1;
    }

    private int GetCurrentActiveHeight()
    {
        return activeMaxY - activeMinY + 1;
    }

    private float GetCurrentBoardWidth()
    {
        var activeWidth = roundActive ? GetCurrentActiveWidth() : width;
        return (activeWidth * tileSize) + ((activeWidth - 1) * tileGap);
    }

    private float GetCurrentBoardHeight()
    {
        var activeHeight = roundActive ? GetCurrentActiveHeight() : height;
        return (activeHeight * tileSize) + ((activeHeight - 1) * tileGap);
    }

    private void FitGroundToArena()
    {
        var ground = transform.Find(GroundName);

        if (ground == null)
        {
            return;
        }

        var boardWidth = (width * tileSize) + ((width - 1) * tileGap) + 1f;
        var boardHeight = (height * tileSize) + ((height - 1) * tileGap) + 1f;

        ground.localPosition = new Vector3(0f, -0.5f, 0f);
        ground.localRotation = Quaternion.identity;
        ground.localScale = new Vector3(boardWidth, 1f, boardHeight);
    }

    private Material CreateTileMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        return new Material(shader)
        {
            color = tileBaseColor
        };
    }

    private Material CreateTileMaterialFromSource(Renderer[] renderers)
    {
        var material = CreateTileMaterial();

        if (material == null)
        {
            return null;
        }

        material.name = $"{name}_TileMaterial";

        var sourceMaterial = FindSourceMaterial(renderers);

        if (sourceMaterial == null)
        {
            return material;
        }

        CopyTextureIfPresent(sourceMaterial, BaseMapProperty, material, BaseMapProperty);
        CopyTextureIfPresent(sourceMaterial, BaseMapProperty, material, MainTexProperty);
        CopyTextureIfPresent(sourceMaterial, MainTexProperty, material, BaseMapProperty);
        CopyTextureIfPresent(sourceMaterial, MainTexProperty, material, MainTexProperty);

        return material;
    }

    private Material FindSourceMaterial(Renderer[] renderers)
    {
        if (renderers == null)
        {
            return null;
        }

        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            var sharedMaterials = renderer.sharedMaterials;

            if (sharedMaterials == null)
            {
                continue;
            }

            foreach (var material in sharedMaterials)
            {
                if (material != null)
                {
                    return material;
                }
            }
        }

        return null;
    }

    private void CopyTextureIfPresent(Material sourceMaterial, int sourceProperty, Material destinationMaterial, int destinationProperty)
    {
        if (sourceMaterial == null || destinationMaterial == null)
        {
            return;
        }

        if (!sourceMaterial.HasProperty(sourceProperty) || !destinationMaterial.HasProperty(destinationProperty))
        {
            return;
        }

        var texture = sourceMaterial.GetTexture(sourceProperty);

        if (texture != null)
        {
            destinationMaterial.SetTexture(destinationProperty, texture);
        }
    }

    private void ApplyTileMaterial(Renderer[] renderers)
    {
        if (tileSharedMaterial == null || renderers == null)
        {
            return;
        }

        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            var sharedMaterials = renderer.sharedMaterials;

            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                renderer.sharedMaterial = tileSharedMaterial;
                continue;
            }

            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i] = tileSharedMaterial;
            }

            renderer.sharedMaterials = sharedMaterials;
        }
    }

    private void UpdateTileWarningVisuals()
    {
        if (tileRenderers == null)
        {
            return;
        }

        var warningActive = roundActive && CanShrink() && SecondsUntilNextShrink <= collapseWarningLeadTime;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var renderer = tileRenderers[x, y];

                if (renderer == null)
                {
                    continue;
                }

                var targetColor = warningActive && IsCurrentOuterRing(x, y)
                    ? collapseWarningColor
                    : tileBaseColor;

                ApplyTileColor(renderer, targetColor);
            }
        }
    }

    private bool IsCurrentOuterRing(int x, int y)
    {
        return x == activeMinX || x == activeMaxX || y == activeMinY || y == activeMaxY;
    }

    private void ApplyTileColor(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        tilePropertyBlock ??= new MaterialPropertyBlock();
        renderer.GetPropertyBlock(tilePropertyBlock);
        tilePropertyBlock.SetColor(BaseColorProperty, color);
        tilePropertyBlock.SetColor(ColorProperty, color);
        renderer.SetPropertyBlock(tilePropertyBlock);
    }

    private sealed class FallingTile
    {
        public FallingTile(Transform tileTransform, float targetLocalY)
        {
            Transform = tileTransform;
            TargetLocalY = targetLocalY;
        }

        public Transform Transform { get; }
        public float TargetLocalY { get; }
    }

    private readonly struct ScheduledCollapse
    {
        public ScheduledCollapse(int x, int y, float triggerTime)
        {
            X = x;
            Y = y;
            TriggerTime = triggerTime;
        }

        public int X { get; }
        public int Y { get; }
        public float TriggerTime { get; }
    }
}
