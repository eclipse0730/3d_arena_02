#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlayerPrefabAutoBuilder
{
    private const string ResourcesFolderPath = "Assets/_Project/Resources";
    private const string PrefabsFolderPath = "Assets/_Project/Resources/Prefabs";
    private const string PrefabPath = "Assets/_Project/Resources/Prefabs/Player.prefab";

    [MenuItem("Tools/3D Arena/Ensure Player Prefab")]
    public static void EnsurePlayerPrefabExists()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (File.Exists(PrefabPath))
        {
            return;
        }

        EnsureFolder("Assets", "_Project");
        EnsureFolder("Assets/_Project", "Resources");
        EnsureFolder(ResourcesFolderPath, "Prefabs");

        var tempPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tempPlayer.name = "Player";
        tempPlayer.transform.position = Vector3.zero;
        tempPlayer.transform.rotation = Quaternion.identity;
        tempPlayer.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

        if (!tempPlayer.TryGetComponent<Rigidbody>(out _))
        {
            tempPlayer.AddComponent<Rigidbody>();
        }

        if (!tempPlayer.TryGetComponent<PlayerController>(out _))
        {
            tempPlayer.AddComponent<PlayerController>();
        }

        PrefabUtility.SaveAsPrefabAsset(tempPlayer, PrefabPath);
        Object.DestroyImmediate(tempPlayer);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created player prefab at {PrefabPath}");
    }

    private static void EnsureFolder(string parentPath, string folderName)
    {
        var combinedPath = $"{parentPath}/{folderName}";

        if (!AssetDatabase.IsValidFolder(combinedPath))
        {
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }
}
#endif
