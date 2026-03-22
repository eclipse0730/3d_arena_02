using UnityEngine;

[DisallowMultipleComponent]
public sealed class SceneSetupBootstrap : MonoBehaviour
{
    [SerializeField] private int defaultArenaWidth = 15;
    [SerializeField] private int defaultArenaHeight = 15;
    [SerializeField] private Vector3 cameraPosition = new(0f, 14f, -14f);
    [SerializeField] private Vector3 cameraEuler = new(45f, 0f, 0f);
    [SerializeField] private Vector3 lightPosition = new(0f, 10f, 0f);
    [SerializeField] private Vector3 lightEuler = new(50f, -30f, 0f);

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ApplySceneScaffolding();
    }

    private void ApplySceneScaffolding()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        var managers = FindOrCreateChild("Managers");
        var arena = FindOrCreateChild("Arena");
        FindOrCreateChild("Players");
        var ui = FindOrCreateChild("UI");

        GetOrAddComponent<GameManager>(managers.gameObject);
        var arenaManager = GetOrAddComponent<ArenaManager>(arena.gameObject);
        var uiManager = GetOrAddComponent<UIManager>(ui.gameObject);

        UpgradeLegacyArenaSize(arenaManager);
        RemoveLegacyGround(arena);
        uiManager.InitializeRuntimeUI();
        ConfigureCamera();
        ConfigureDirectionalLight();
    }

    private Transform FindOrCreateChild(string childName)
    {
        var child = transform.Find(childName);

        if (child != null)
        {
            return child;
        }

        var childObject = new GameObject(childName);
        childObject.transform.SetParent(transform, false);
        return childObject.transform;
    }

    private void UpgradeLegacyArenaSize(ArenaManager arenaManager)
    {
        if (arenaManager.Width == 10 && arenaManager.Height == 10)
        {
            arenaManager.ConfigureArenaSize(defaultArenaWidth, defaultArenaHeight);
        }
    }

    private void RemoveLegacyGround(Transform arena)
    {
        var ground = arena.Find("Ground");

        if (ground == null)
        {
            return;
        }

        Destroy(ground.gameObject);
    }

    private void ConfigureCamera()
    {
        var targetCamera = Camera.main;

        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        if (targetCamera == null)
        {
            return;
        }

        targetCamera.transform.position = cameraPosition;
        targetCamera.transform.rotation = Quaternion.Euler(cameraEuler);
    }

    private void ConfigureDirectionalLight()
    {
        var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (var light in lights)
        {
            if (light.type != LightType.Directional)
            {
                continue;
            }

            light.transform.position = lightPosition;
            light.transform.rotation = Quaternion.Euler(lightEuler);
            return;
        }
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (target.TryGetComponent<T>(out var existing))
        {
            return existing;
        }

        return target.AddComponent<T>();
    }
}
