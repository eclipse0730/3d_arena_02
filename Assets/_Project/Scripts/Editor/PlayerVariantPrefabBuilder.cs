#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PlayerVariantPrefabBuilder
{
    private const string BasePlayerPrefabPath = "Assets/_Project/Resources/Prefabs/Player.prefab";
    private const string AltPlayerPrefabPath = "Assets/_Project/Resources/Prefabs/Player 1.prefab";
    private const string ThirdPlayerPrefabPath = "Assets/_Project/Resources/Prefabs/Player 2.prefab";
    private const string SparrowVisualPrefabPath = "Assets/Quirky Series Ultimate/FREE/Prefabs/Sparrow.prefab";
    private const string SparrowTexturePath = "Assets/Quirky Series Ultimate/FREE/Textures/T_Sparrow.png";
    private const string PartyMonsterVisualPrefabPath = "Assets/PartyMonsterDuo/Prefab/P01.prefab";
    private const string PartyMonsterTexturePath = "Assets/PartyMonsterDuo/Texture/DefaultPolyart.png";
    private const string BurrowVisualPrefabPath = "Assets/Free Burrow Cute Series/Prefabs/Free Burrow.prefab";
    private const string BurrowTexturePath = "Assets/Free Burrow Cute Series/Textures/Free Burrow.psd";
    private const string CharacterVisualName = "CharacterVisual";

    [MenuItem("Tools/3D Arena/Rebuild Player Variant Prefabs")]
    public static void RebuildPlayerVariantPrefabs()
    {
        RebuildVariant(
            BasePlayerPrefabPath,
            SparrowVisualPrefabPath,
            SparrowTexturePath,
            new Vector3(0f, -1f, 0f),
            Vector3.zero,
            new Vector3(2.4f, 2.4f, 2.4f));

        RebuildVariant(
            AltPlayerPrefabPath,
            PartyMonsterVisualPrefabPath,
            PartyMonsterTexturePath,
            new Vector3(0f, -0.45f, 0f),
            Vector3.zero,
            new Vector3(1.6f, 1.6f, 1.6f));

        RebuildVariant(
            ThirdPlayerPrefabPath,
            BurrowVisualPrefabPath,
            BurrowTexturePath,
            new Vector3(0f, -0.55f, 0f),
            Vector3.zero,
            new Vector3(1.8f, 1.8f, 1.8f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Rebuilt player variant prefabs.");
    }

    private static void RebuildVariant(
        string playerPrefabPath,
        string visualPrefabPath,
        string texturePath,
        Vector3 localPosition,
        Vector3 localEulerAngles,
        Vector3 localScale)
    {
        var root = PrefabUtility.LoadPrefabContents(playerPrefabPath);

        if (root == null)
        {
            return;
        }

        try
        {
            var controller = root.GetComponent<PlayerController>();

            if (controller == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("characterVisualPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
            serializedObject.FindProperty("characterVisualMainTexture").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            serializedObject.FindProperty("characterVisualLocalPosition").vector3Value = localPosition;
            serializedObject.FindProperty("characterVisualLocalEulerAngles").vector3Value = localEulerAngles;
            serializedObject.FindProperty("characterVisualLocalScale").vector3Value = localScale;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var existingVisual = root.transform.Find(CharacterVisualName);

            if (existingVisual != null)
            {
                Object.DestroyImmediate(existingVisual.gameObject);
            }

            var visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);

            if (visualPrefab != null)
            {
                var visualInstance = Object.Instantiate(visualPrefab, root.transform);
                visualInstance.name = CharacterVisualName;
                visualInstance.transform.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(localEulerAngles));
                visualInstance.transform.localScale = localScale;

                foreach (var collider in visualInstance.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = false;
                }
            }

            var rootRenderer = root.GetComponent<Renderer>();

            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            PrefabUtility.SaveAsPrefabAsset(root, playerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
