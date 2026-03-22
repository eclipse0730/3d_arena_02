using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class ArenaManager : MonoBehaviour
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    [SerializeField, Min(4)] private int width = 15;
    [SerializeField, Min(4)] private int height = 15;
    [SerializeField, Min(0.5f)] private float tileSize = 1f;
    [SerializeField, Min(0.1f)] private float tileHeight = 0.2f;
    [SerializeField, Min(0f)] private float tileGap = 0.05f;
    [SerializeField, Min(0f)] private float shrinkStartDelay = 10f;
    [SerializeField, Min(0.2f)] private float shrinkInterval = 5.8f;
    [SerializeField, Min(5)] private int minimumActiveWidth = 5;
    [SerializeField, Min(5)] private int minimumActiveHeight = 5;
    [SerializeField, Min(0.5f)] private float collapseFallDistance = 8f;
    [SerializeField, Min(0.5f)] private float collapseFallSpeed = 12f;
    [SerializeField] private Color tileBaseColor = new(0.72f, 0.76f, 0.84f, 1f);
    [SerializeField] private Color collapseWarningColor = new(0.92f, 0.22f, 0.18f, 1f);
    [SerializeField, Min(0f)] private float collapseWarningLeadTime = 1.15f;

    private const string TileRootName = "Tiles";
    private const string TileNamePrefix = "Tile_";
    private const string GroundName = "Ground";

    private Transform[,] tiles;
    private Renderer[,] tileRenderers;
    private readonly List<FallingTile> fallingTiles = new();
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
        roundActive = false;
        nextShrinkTime = 0f;

        if (tileSharedMaterial != null)
        {
            Destroy(tileSharedMaterial);
        }

        tiles = new Transform[width, height];
        tileRenderers = new Renderer[width, height];
        tileSharedMaterial = CreateTileMaterial();
        tilePropertyBlock ??= new MaterialPropertyBlock();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObject.name = $"{TileNamePrefix}{x}_{y}";
                tileObject.transform.SetParent(tileRoot, false);
                tileObject.transform.localPosition = CalculateTileLocalPosition(x, y);
                tileObject.transform.localRotation = Quaternion.identity;
                tileObject.transform.localScale = new Vector3(tileSize, tileHeight, tileSize);

                tiles[x, y] = tileObject.transform;
                var renderer = tileObject.GetComponent<Renderer>();
                tileRenderers[x, y] = renderer;

                if (renderer != null)
                {
                    if (tileSharedMaterial != null)
                    {
                        renderer.sharedMaterial = tileSharedMaterial;
                    }

                    ApplyTileColor(renderer, tileBaseColor);
                }
            }
        }

        ResetActiveBounds();
        FitGroundToArena();
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
        CollapseTileRange(activeMinX, activeMaxX, activeMinY, activeMinY);
        CollapseTileRange(activeMinX, activeMaxX, activeMaxY, activeMaxY);
        CollapseTileRange(activeMinX, activeMinX, activeMinY + 1, activeMaxY - 1);
        CollapseTileRange(activeMaxX, activeMaxX, activeMinY + 1, activeMaxY - 1);

        activeMinX++;
        activeMaxX--;
        activeMinY++;
        activeMaxY--;
    }

    private void CollapseTileRange(int minX, int maxX, int minY, int maxY)
    {
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                CollapseTile(x, y);
            }
        }
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
}
