#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Swaps the placeholder cube mesh on the enemy prefabs for KayKit skeleton models.
// Edits the prefab assets through Unity's prefab-contents API (no hand-edited YAML).
// Logic components (Rigidbody, BoxCollider, EnemyCube) are left untouched; we only add a
// visual child and hide the cube renderer, so "Undo Reskin Enemies" reverts cleanly.
public static class EnemyReskinner
{
    private const string SKEL = "Assets/ThirdParty/KayKit_Skeletons_1.1_FREE/characters/fbx/";
    private const string SKIN_NAME = "__Skin";

    // which skeleton stands in for each enemy (swap the file names to taste)
    private const string NORMAL_PREFAB = "Assets/Prefabs/EnemyCube.prefab";
    private const string NORMAL_MODEL  = SKEL + "Skeleton_Minion.fbx";
    private const string BOSS_PREFAB   = "Assets/Prefabs/BossCube.prefab";
    private const string BOSS_MODEL    = SKEL + "Skeleton_Warrior.fbx";

    [MenuItem("Tools/Reskin Enemies")]
    public static void Reskin()
    {
        Apply(NORMAL_PREFAB, NORMAL_MODEL);
        Apply(BOSS_PREFAB, BOSS_MODEL);
        AssetDatabase.SaveAssets();
        Debug.Log("[EnemyReskinner] Done. Enemies now use skeleton models.");
    }

    [MenuItem("Tools/Undo Reskin Enemies")]
    public static void UndoReskin()
    {
        Revert(NORMAL_PREFAB);
        Revert(BOSS_PREFAB);
        AssetDatabase.SaveAssets();
        Debug.Log("[EnemyReskinner] Reverted to placeholder cubes.");
    }

    private static void Apply(string prefabPath, string modelPath)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null) { Debug.LogWarning($"[EnemyReskinner] Missing model: {modelPath}"); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            // remove any previous skin so re-running is idempotent
            var existing = root.transform.Find(SKIN_NAME);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var skin = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            skin.name = SKIN_NAME;
            // cube collider is 1 tall centred on the origin; drop the model so its feet sit at the base
            skin.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            skin.transform.localRotation = Quaternion.identity;   // KayKit characters face +Z, same as the cube
            skin.transform.localScale = Vector3.one;
            StripColliders(skin);

            SetCubeRenderer(root, false);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void Revert(string prefabPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var existing = root.transform.Find(SKIN_NAME);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            SetCubeRenderer(root, true);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // toggle only the cube's own MeshRenderer (the one on the prefab root), not the skin's
    private static void SetCubeRenderer(GameObject root, bool on)
    {
        var r = root.GetComponent<MeshRenderer>();
        if (r != null) r.enabled = on;
    }

    private static void StripColliders(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);
    }
}
#endif
