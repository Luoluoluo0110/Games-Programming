#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Dresses Main.unity with KayKit Dungeon visuals: a clean, uniform corridor.
// Visuals spawn under "__SceneDressing__" (plus name-prefixed children on the doors and weapon).
// It also adjusts two serialized values for the exit-portal flow (boss auto-win off, player z-bound
// extended); "Undo Dress Scene" removes the visuals and restores both values.
//
// Anti-clip rules baked in:
//   - every spawned visual has its colliders removed, so it never blocks the player or bullets
//   - side walls sit just outside the player bound (x = 4) with their inner face on the bound
//   - props are wall-mounted or flush to the walls, clear of the x[-4,4] walkway
public static class SceneDresser
{
    private const string ROOT_NAME = "__SceneDressing__";
    private const string CHILD_PREFIX = "__Dress_";
    private const string FBX = "Assets/ThirdParty/KayKit_Dungeon/fbx(unity)/";
    private const string WEAPON_FBX = "Assets/ThirdParty/KayKit_Skeletons_1.1_FREE/assets/fbx(unity)/Skeleton_Crossbow.fbx";

    // Play area (from PlayerController bounds): x[-4,4], z[-10,54]. Rooms sit at z = 10/28/46.
    private const float HALF_WIDTH = 4f;    // player x-bound (the "air wall")
    // rough starting x for the walls; BuildWalls then snaps each one from its real mesh bounds so
    // the inner face lands exactly on the air wall. Camera near-clip is tightened (below) so
    // standing against the wall doesn't see through its face.
    private const float WALL_X = HALF_WIDTH + 0.5f;
    // push the wall face this far past the air wall so the camera keeps clear of it (no see-through).
    // bump this up if you still clip, down if the wall feels too far from where you stop.
    private const float WALL_MARGIN = 0.6f;
    private const float CAMERA_NEAR_CLIP = 0.05f;
    private const float FLOOR_Y = 0.11f;    // existing floor top is 0.1; sit just above it
    private const float Z_MIN = -12f;
    private const float Z_MAX = 58f;        // extended past the boss door to fit the exit chamber
    private const float STEP = 4f;          // KayKit wall / large floor tile footprint

    // Exit portal: sits past the boss door (z 54), reachable only once that door opens on clear.
    // Entering it wins the level, so the boss room's auto-win is turned off while dressed and the
    // player's z-bound is extended to let them step through. Undressing restores both.
    private const float PORTAL_Z = 56f;
    private const float PLAYER_MAXZ_DRESSED = 57f;
    private const float PLAYER_MAXZ_DEFAULT = 55f;

    // Keep floor tiling optional: set false to leave the existing flat floor and cut ~30 objects.
    private const bool LAY_FLOOR = true;

    [MenuItem("Tools/Dress Scene")]
    public static void Dress()
    {
        if (GameObject.Find(ROOT_NAME) != null)
        {
            if (!EditorUtility.DisplayDialog("Dress Scene",
                "Scene already dressed. Clear and redo?", "Yes", "Cancel"))
                return;
            Undress();
        }

        var root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Dress Scene");

        BuildFloor(root.transform);
        BuildWalls(root.transform);
        DressDoors();
        BuildDecor(root.transform);
        BuildExitPortal(root.transform);
        DressWeapon();

        // first-person view never sees the player body; hide it so it casts no stray shadow
        ToggleRenderer("Player", false);

        // tighten the near-clip so pressing against a wall doesn't see through its face
        SetCameraNearClip(CAMERA_NEAR_CLIP);

        // win at the portal instead of on the last kill; let the player reach past the boss door
        SetBossAutoWin(false);
        SetPlayerMaxZ(PLAYER_MAXZ_DRESSED);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneDresser] Done. Press Play to check.");
    }

    [MenuItem("Tools/Undo Dress Scene")]
    public static void Undress()
    {
        var root = GameObject.Find(ROOT_NAME);
        if (root != null) Object.DestroyImmediate(root);

        foreach (var go in Object.FindObjectsOfType<GameObject>())
            if (go != null && go.name.StartsWith(CHILD_PREFIX))
                Object.DestroyImmediate(go);

        ToggleRenderer("Floor", true);
        ToggleRenderer("Door1", true);
        ToggleRenderer("Door2", true);
        ToggleRenderer("BossDoor", true);
        ToggleRenderer("Player", true);
        SetCameraNearClip(0.3f);   // Unity's default

        // restore the original win flow (boss clear = win) and the original player bound
        SetBossAutoWin(true);
        SetPlayerMaxZ(PLAYER_MAXZ_DEFAULT);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneDresser] Reverted.");
    }

    // ---- floor: one uniform tile type, grid-aligned, no variants ----
    private static void BuildFloor(Transform root)
    {
        if (!LAY_FLOOR) return;
        // leave the original floor on as a base layer, so no void shows at the tile edges or
        // in the strip between the play bound and the walls; tiles just sit on top of it

        var tile = Load(FBX + "floor_tile_large.fbx");
        if (tile == null) return;

        var group = NewGroup("Floor", root);
        // tile past the walls (x -8..8) so the stone runs under the wall base with no gap showing
        for (float x = -6f; x <= 6f; x += STEP)
            for (float z = Z_MIN + 2f; z <= Z_MAX; z += STEP)
                PlaceUnder(group, tile, new Vector3(x, FLOOR_Y, z), 0f);
    }

    // ---- side walls: one type, both sides, inner face on the play bound ----
    private static void BuildWalls(Transform root)
    {
        var wall = Load(FBX + "wall.fbx");
        if (wall == null) return;

        var group = NewGroup("Walls", root);
        for (float z = Z_MIN; z <= Z_MAX; z += STEP)
        {
            // place roughly, then snap each inner face just past the air wall (±(HALF_WIDTH+margin))
            float face = HALF_WIDTH + WALL_MARGIN;
            var right = PlaceUnder(group, wall, new Vector3( WALL_X, 0.1f, z),  90f); // faces -x
            SnapInnerFaceX(right, face);
            var left = PlaceUnder(group, wall, new Vector3(-WALL_X, 0.1f, z), -90f);  // faces +x
            SnapInnerFaceX(left, -face);
        }
        // close both ends so the corridor (and the exit chamber) aren't open to the void
        foreach (float endZ in new[] { Z_MIN, Z_MAX })
            for (float x = -6f; x <= 6f; x += STEP)
                PlaceUnder(group, wall, new Vector3(x, 0.1f, endZ), 0f);
    }

    // ---- doors: swap each placeholder box for a gated-wall visual ----
    private static void DressDoors()
    {
        AttachDoorVisual("Door1", "wall_gated.fbx");
        AttachDoorVisual("Door2", "wall_gated.fbx");
        AttachDoorVisual("BossDoor", "wall_gated.fbx");
    }

    private static void AttachDoorVisual(string doorName, string fbx)
    {
        var door = GameObject.Find(doorName);
        if (door == null) { Debug.LogWarning($"[SceneDresser] {doorName} not found"); return; }
        ToggleRendererOn(door, false);

        var prefab = Load(FBX + fbx);
        if (prefab == null) return;

        // The door box is scaled (8,3,0.5) and floats at ~y2.5. A wrapper child cancels that scale
        // (no rotation between them, so its world scale is a clean uniform 1) and drops to the floor;
        // the model then lives under a uniform frame and won't shear.
        var wrapper = new GameObject(CHILD_PREFIX + "DoorVisual");
        Undo.RegisterCreatedObjectUndo(wrapper, "Dress Scene");
        wrapper.transform.SetParent(door.transform, false);

        var s = door.transform.lossyScale;
        float dropWorld = FLOOR_Y - door.transform.position.y;   // bring the model base to the floor
        wrapper.transform.localPosition = new Vector3(0f, dropWorld / s.y, 0f);
        wrapper.transform.localRotation = Quaternion.identity;
        wrapper.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);

        // wrapper now has a clean uniform 1-unit world frame, so 1 local unit = 1 metre here.
        // Tile gate pieces across the whole corridor so the gate is flush with both side walls.
        float corridor = 2f * (HALF_WIDTH + WALL_MARGIN);   // distance between the two wall faces

        // measure one piece to know how wide it is, then tile enough to span the corridor
        var probe = (GameObject)PrefabUtility.InstantiatePrefab(prefab, wrapper.transform);
        probe.transform.localPosition = Vector3.zero;
        probe.transform.localRotation = Quaternion.identity;
        probe.transform.localScale = Vector3.one;
        float pieceW = TryWorldBounds(probe, out var pb) && pb.size.x > 0.1f ? pb.size.x : STEP;
        Object.DestroyImmediate(probe);

        int count = Mathf.Max(1, Mathf.CeilToInt(corridor / pieceW));
        float start = -(count - 1) * pieceW * 0.5f;        // centre the row on x = 0
        for (int i = 0; i < count; i++)
        {
            var piece = Spawn(prefab, wrapper.transform);
            piece.transform.localPosition = new Vector3(start + i * pieceW, 0f, 0f);
            piece.transform.localRotation = Quaternion.identity;
            piece.transform.localScale = Vector3.one;       // uniform, so no shearing
            StripColliders(piece);
        }
    }

    // ---- decor: sparse, wall-mounted or flush, nothing in the walkway ----
    private static void BuildDecor(Transform root)
    {
        var g = NewGroup("Decor", root);
        float mountX = HALF_WIDTH + WALL_MARGIN - 0.1f;   // on the wall face (just inside it)

        // wall torches for each room (high on the side walls, out of reach)
        foreach (float z in new[] { 10f, 28f, 46f })
        {
            Place(g, "torch_mounted.fbx", new Vector3( mountX, 1.9f, z),  90f);
            Place(g, "torch_mounted.fbx", new Vector3(-mountX, 1.9f, z), -90f);
        }

        // boss room: two banners on the side walls
        Place(g, "banner_red.fbx", new Vector3( mountX, 2.6f, 46f),  90f);
        Place(g, "banner_red.fbx", new Vector3(-mountX, 2.6f, 46f), -90f);

        // reward chest sits at the very end, beside the exit portal, in the chamber past the door
        Place(g, "chest_gold.fbx", new Vector3(2.2f, FLOOR_Y, PORTAL_Z - 0.3f), 210f);
    }

    // ---- exit portal: a doorway arch + glowing pane with a win trigger, past the boss door ----
    private static void BuildExitPortal(Transform root)
    {
        var portal = new GameObject(CHILD_PREFIX + "ExitPortal");
        Undo.RegisterCreatedObjectUndo(portal, "Dress Scene");
        portal.transform.SetParent(root, false);
        portal.transform.position = new Vector3(0f, FLOOR_Y, PORTAL_Z);

        // the trigger you walk into to finish the level
        var box = portal.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 1.3f, 0f);
        box.size = new Vector3(3f, 2.6f, 1.2f);
        portal.AddComponent<LevelExitPortal>();

        // a doorway arch frames it, using the dungeon pack's own material
        var arch = Load(FBX + "wall_doorway.fbx");
        if (arch != null)
        {
            var a = Spawn(arch, portal.transform);
            a.transform.localPosition = Vector3.zero;
            a.transform.localRotation = Quaternion.identity;
            a.transform.localScale = Vector3.one;
            StripColliders(a);
        }

        // a glowing pane inside the arch, facing the approaching player (-z)
        var glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Undo.RegisterCreatedObjectUndo(glow, "Dress Scene");
        glow.name = "Glow";
        glow.transform.SetParent(portal.transform, false);
        glow.transform.localPosition = new Vector3(0f, 1.4f, -0.05f);
        glow.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        glow.transform.localScale = new Vector3(2.2f, 2.8f, 1f);
        Object.DestroyImmediate(glow.GetComponent<Collider>());   // visual only
        var mr = glow.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = MakePortalMaterial();
    }

    // shader-pipeline-agnostic glowing material (Built-in or URP)
    private static Material MakePortalMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader);
        var c = new Color(0.35f, 0.75f, 1f, 1f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 2f);
        }
        return mat;
    }

    // toggle the boss room's auto-win (serialized field) without editing RoomManager
    private static void SetBossAutoWin(bool on)
    {
        var boss = GameObject.Find("BossRoom");
        var rm = boss != null ? boss.GetComponent<RoomManager>() : null;
        if (rm == null) return;
        var so = new SerializedObject(rm);
        var p = so.FindProperty("isBossRoom");
        if (p != null) { p.boolValue = on; so.ApplyModifiedProperties(); }
    }

    private static void SetPlayerMaxZ(float value)
    {
        var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
        var pc = player != null ? player.GetComponent<PlayerController>() : null;
        if (pc == null) return;
        var so = new SerializedObject(pc);
        var p = so.FindProperty("maxZ");
        if (p != null) { p.floatValue = value; so.ApplyModifiedProperties(); }
    }

    // ---- weapon: a crossbow held close so it doesn't clip the camera or walls ----
    private static void DressWeapon()
    {
        var muzzle = GameObject.Find("Muzzle");
        if (muzzle == null || muzzle.transform.parent == null)
        {
            Debug.LogWarning("[SceneDresser] Muzzle not found; skipping weapon.");
            return;
        }
        var parent = muzzle.transform.parent;
        var old = parent.Find(CHILD_PREFIX + "WeaponVisual");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // a crossbow fits a projectile shooter, and it's a single clean mesh (no bulky shield)
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WEAPON_FBX);
        if (prefab == null) { Debug.LogWarning($"[SceneDresser] Missing weapon: {WEAPON_FBX}"); return; }
        var weapon = Spawn(prefab, parent);
        weapon.name = CHILD_PREFIX + "WeaponVisual";
        // held low-centre, pointing forward; tune these three in the Inspector with live feedback
        weapon.transform.localPosition = new Vector3(0.18f, -0.28f, 0.45f);
        weapon.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        weapon.transform.localScale = Vector3.one * 0.7f;
        StripColliders(weapon);
    }

    // ---- helpers ----
    private static Transform NewGroup(string name, Transform parent)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    private static GameObject Load(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null) Debug.LogWarning($"[SceneDresser] Missing asset: {path}");
        return go;
    }

    private static GameObject Spawn(GameObject prefab, Transform parent)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(go, "Dress Scene");
        return go;
    }

    private static GameObject PlaceUnder(Transform parent, GameObject prefab, Vector3 pos, float yaw)
    {
        var go = Spawn(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        StripColliders(go);
        return go;
    }

    // shift a placed object along x so its inner face (the side toward room centre) lands exactly
    // on faceX; measured from the real mesh bounds, so it doesn't depend on the model's pivot
    private static void SnapInnerFaceX(GameObject go, float faceX)
    {
        if (!TryWorldBounds(go, out var b)) return;
        float innerEdge = faceX >= 0f ? b.min.x : b.max.x;
        go.transform.position += new Vector3(faceX - innerEdge, 0f, 0f);
    }

    private static bool TryWorldBounds(GameObject go, out Bounds bounds)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (rends.Length == 0) return false;
        bounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) bounds.Encapsulate(rends[i].bounds);
        return true;
    }

    private static void Place(Transform parent, string fbx, Vector3 pos, float yaw)
    {
        var prefab = Load(FBX + fbx);
        if (prefab != null) PlaceUnder(parent, prefab, pos, yaw);
    }

    private static void StripColliders(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);
    }

    private static void ToggleRenderer(string name, bool on)
    {
        var go = GameObject.Find(name);
        if (go != null) ToggleRendererOn(go, on);
    }

    private static void ToggleRendererOn(GameObject go, bool on)
    {
        var r = go.GetComponent<MeshRenderer>();
        if (r != null) r.enabled = on;
    }

    private static void SetCameraNearClip(float near)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = GameObject.Find("Main Camera");
            if (go != null) cam = go.GetComponent<Camera>();
        }
        if (cam != null) cam.nearClipPlane = near;
    }
}
#endif
