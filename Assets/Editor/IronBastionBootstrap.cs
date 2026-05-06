// IronBastionBootstrap.cs
// Editor-only script that creates all game assets and the playable scene.
// Run via: IronBastion > Build Complete Game

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class IronBastionBootstrap
{
    // ─── LAYER INDICES ───────────────────────────────────────────────────────
    private const int LayerEnemy      = 8;
    private const int LayerTower      = 9;
    private const int LayerBuildable  = 10;

    // ─── PATHS ───────────────────────────────────────────────────────────────
    private const string PrefabsPath  = "Assets/_Project/Prefabs";
    private const string DataPath     = "Assets/_Project/Data";
    private const string ScenesPath   = "Assets/_Project/Scenes/Production";
    private const string MaterialPath = "Assets/_Project/Art/Materials";

    [MenuItem("IronBastion/Build Complete Game")]
    public static void BuildCompleteGame()
    {
        Debug.Log("[Bootstrap] Starting Iron Bastion build...");

        EnsureFolders();
        AddLayers();

        // Materials
        var matGround      = CreateMaterial("Mat_Ground",         new Color(0.35f, 0.25f, 0.15f));
        var matEnemy       = CreateMaterial("Mat_Enemy",          Color.red);
        var matTower       = CreateMaterial("Mat_Tower",          new Color(0.5f, 0.5f, 0.55f));
        var matProjectile  = CreateMaterial("Mat_Projectile",     new Color(1f, 0.9f, 0f));
        var matCellBuild   = CreateMaterial("Mat_Cell_Build",     new Color(0.2f, 0.6f, 0.2f, 0.4f), transparent: true);
        var matCellBlocked = CreateMaterial("Mat_Cell_Blocked",   new Color(0.4f, 0.3f, 0.2f, 0.6f), transparent: true);
        var matCellOccupied= CreateMaterial("Mat_Cell_Occupied",  new Color(0.5f, 0.5f, 0.55f, 0.5f), transparent: true);
        var matHighValid   = CreateMaterial("Mat_Highlight_Valid",   new Color(0.2f, 1f, 0.2f, 0.5f), transparent: true);
        var matHighInvalid = CreateMaterial("Mat_Highlight_Invalid", new Color(1f, 0.2f, 0.2f, 0.5f), transparent: true);

        // Prefabs
        var projBasicPrefab  = CreateProjectileBasicPrefab(matProjectile);
        var projSplashPrefab = CreateProjectileSplashPrefab(new Color(1f, 0.5f, 0f));
        var enemyPrefab      = CreateEnemyCommonPrefab(matEnemy);

        var archerPrefab  = CreateTowerPrefab("Tower_Archer",  matTower, typeof(TowerArcher),  projBasicPrefab);
        var cannonPrefab  = CreateTowerPrefab("Tower_Cannon",  matTower, typeof(TowerCannon),  projSplashPrefab);

        // ScriptableObjects
        var enemyDataCommon = CreateEnemyData("EnemyData_Common", enemyPrefab,
            hp: 100f, speed: 2f, gold: 10, lives: 1);

        var tdArcher = CreateTowerData("TowerData_Archer", archerPrefab, projBasicPrefab,
            cost: 75,  upgrade: 38,  dmg1: 15f, rng1: 3.5f, rate1: 1.5f,
                                      dmg2: 22f, rng2: 4f,   rate2: 2.1f);

        var tdCannon = CreateTowerData("TowerData_Cannon", cannonPrefab, projSplashPrefab,
            cost: 150, upgrade: 75,  dmg1: 80f, rng1: 3f,   rate1: 0.6f,
                                      dmg2: 110f,rng2: 3.5f, rate2: 0.8f,
            splashR1: 1.2f, splashR2: 1.8f);

        // Assign TowerData projectile prefabs
        tdArcher.projectilePrefab = projBasicPrefab;
        tdCannon.projectilePrefab = projSplashPrefab;
        EditorUtility.SetDirty(tdArcher);
        EditorUtility.SetDirty(tdCannon);

        var wave1 = CreateWaveData("WaveData_Wave1", enemyDataCommon, count: 5,  interval: 1.5f, waveNum: 1);
        var wave2 = CreateWaveData("WaveData_Wave2", enemyDataCommon, count: 8,  interval: 1.2f, waveNum: 2);
        var wave3 = CreateWaveData("WaveData_Wave3", enemyDataCommon, count: 12, interval: 1f,   waveNum: 3);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Build scene
        BuildScene(matGround, matCellBuild, matCellBlocked, matHighValid, matHighInvalid,
                   enemyPrefab, archerPrefab, cannonPrefab,
                   tdArcher, tdCannon,
                   new WaveData[] { wave1, wave2, wave3 });

        Debug.Log("[Bootstrap] Iron Bastion build complete!");
        EditorUtility.DisplayDialog("Iron Bastion", "Build complete!\n\nPress Play to test the game.", "OK");
    }

    // ─── FOLDERS ─────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        string[] folders = {
            "Assets/_Project/Art/Materials",
            "Assets/_Project/Prefabs/Enemies",
            "Assets/_Project/Prefabs/Towers",
            "Assets/_Project/Prefabs/Projectiles",
            "Assets/_Project/Prefabs/UI",
            "Assets/_Project/Data/Enemies",
            "Assets/_Project/Data/Towers",
            "Assets/_Project/Data/Waves",
            "Assets/_Project/Scenes/Production",
        };
        foreach (string path in folders)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string parent = current;
                current = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(current))
                    AssetDatabase.CreateFolder(parent, parts[i]);
            }
        }
        AssetDatabase.Refresh();
    }

    // ─── LAYERS ──────────────────────────────────────────────────────────────

    static void AddLayers()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");

        SetLayer(layers, LayerEnemy,     "Enemy");
        SetLayer(layers, LayerTower,     "Tower");
        SetLayer(layers, LayerBuildable, "Buildable");

        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetLayer(SerializedProperty layers, int index, string name)
    {
        SerializedProperty slot = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(slot.stringValue))
            slot.stringValue = name;
    }

    // ─── MATERIALS ───────────────────────────────────────────────────────────

    static Material CreateMaterial(string name, Color color, bool transparent = false)
    {
        string path = $"{MaterialPath}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            if (transparent)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    // ─── PREFABS ─────────────────────────────────────────────────────────────

    static GameObject CreateProjectileBasicPrefab(Material mat)
    {
        string path = $"{PrefabsPath}/Projectiles/Projectile_Basic.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Projectile_Basic";
        go.transform.localScale = Vector3.one * 0.2f;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<SphereCollider>());

        go.AddComponent<ProjectileBase>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateProjectileSplashPrefab(Color color)
    {
        string path = $"{PrefabsPath}/Projectiles/Projectile_Splash.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var mat = CreateMaterial("Mat_ProjectileSplash", color);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Projectile_Splash";
        go.transform.localScale = Vector3.one * 0.3f;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<SphereCollider>());

        ProjectileSplash splash = go.AddComponent<ProjectileSplash>();
        SerializedObject soSplash = new SerializedObject(splash);
        soSplash.FindProperty("enemyLayerMask").intValue = 1 << LayerEnemy;
        soSplash.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateEnemyCommonPrefab(Material mat)
    {
        string path = $"{PrefabsPath}/Enemies/Enemy_Common.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        // Root
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Enemy_Common";
        go.layer = LayerEnemy;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        // Rigidbody (kinematic, set by EnemyBase)
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        go.AddComponent<EnemyCommon>();

        // HealthBar canvas (world space)
        GameObject hbCanvas = new GameObject("HealthBarCanvas");
        hbCanvas.transform.SetParent(go.transform);
        hbCanvas.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        hbCanvas.transform.localScale    = new Vector3(0.02f, 0.02f, 0.02f);

        Canvas canvas = hbCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRT = hbCanvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(100f, 12f);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(hbCanvas.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(hbCanvas.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // HealthBar component
        HealthBar hb = hbCanvas.AddComponent<HealthBar>();
        SerializedObject so = new SerializedObject(hb);
        so.FindProperty("fillImage").objectReferenceValue = fillImg;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateTowerPrefab(string name, Material mat, System.Type towerScript, GameObject projectilePrefab)
    {
        string path = $"{PrefabsPath}/Towers/{name}.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        // Root cylinder (body)
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.layer = LayerTower;
        go.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());

        // Add a box collider for tower selection raycasting
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.size = new Vector3(1f, 2f, 1f);

        // Barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrel.name = "Barrel";
        barrel.transform.SetParent(go.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.5f, 0.55f);
        barrel.transform.localScale    = new Vector3(0.25f, 0.25f, 0.7f);
        barrel.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.DestroyImmediate(barrel.GetComponent<BoxCollider>());

        // FirePoint
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(go.transform);
        fp.transform.localPosition = new Vector3(0f, 0.5f, 1f);

        // Tower script
        Component towerComp = go.AddComponent(towerScript);

        // Wire firePoint via serialized field
        SerializedObject soTower = new SerializedObject(towerComp);
        SerializedProperty fpProp = soTower.FindProperty("firePoint");
        if (fpProp != null) { fpProp.objectReferenceValue = fp.transform; soTower.ApplyModifiedPropertiesWithoutUndo(); }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ─── SCRIPTABLE OBJECTS ──────────────────────────────────────────────────

    static EnemyData CreateEnemyData(string name, GameObject prefab,
        float hp, float speed, int gold, int lives)
    {
        string path = $"{DataPath}/Enemies/{name}.asset";
        EnemyData d = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (d != null) return d;

        d = ScriptableObject.CreateInstance<EnemyData>();
        d.prefab              = prefab;
        d.enemyName           = "Corrompido Comum";
        d.maxHealth           = hp;
        d.moveSpeed           = speed;
        d.goldReward          = gold;
        d.livesLost           = lives;
        d.modelScale          = 1f;
        d.placeholderColor    = Color.red;
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    static TowerData CreateTowerData(string name, GameObject prefab, GameObject projPrefab,
        int cost, int upgrade,
        float dmg1, float rng1, float rate1,
        float dmg2, float rng2, float rate2,
        float splashR1 = 0f, float splashR2 = 0f)
    {
        string path = $"{DataPath}/Towers/{name}.asset";
        TowerData d = AssetDatabase.LoadAssetAtPath<TowerData>(path);
        if (d != null) return d;

        d = ScriptableObject.CreateInstance<TowerData>();
        d.towerName         = name == "TowerData_Archer" ? "Arqueira" : "Canhão";
        d.prefab            = prefab;
        d.projectilePrefab  = projPrefab;
        d.cost              = cost;
        d.upgradeCost       = upgrade;
        d.damageLevel1      = dmg1;   d.rangeLevel1   = rng1;   d.fireRateLevel1 = rate1;
        d.damageLevel2      = dmg2;   d.rangeLevel2   = rng2;   d.fireRateLevel2 = rate2;
        d.splashRadiusLevel1 = splashR1;
        d.splashRadiusLevel2 = splashR2;
        d.projectileSpeed   = 10f;
        d.defaultTargeting  = TargetingMode.First;
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    static WaveData CreateWaveData(string name, EnemyData enemyData, int count, float interval, int waveNum)
    {
        string path = $"{DataPath}/Waves/{name}.asset";
        WaveData d = AssetDatabase.LoadAssetAtPath<WaveData>(path);
        if (d != null) return d;

        d = ScriptableObject.CreateInstance<WaveData>();
        d.waveNumber    = waveNum;
        d.spawnInterval = interval;
        d.enemyGroups   = new EnemyGroup[]
        {
            new EnemyGroup { enemyData = enemyData, count = count, healthOverride = 0f }
        };
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    // ─── SCENE BUILD ─────────────────────────────────────────────────────────

    static void BuildScene(
        Material matGround, Material matCellBuild, Material matCellBlocked,
        Material matHighValid, Material matHighInvalid,
        GameObject enemyPrefab, GameObject archerPrefab, GameObject cannonPrefab,
        TowerData tdArcher, TowerData tdCannon,
        WaveData[] waves)
    {
        string scenePath = $"{ScenesPath}/MainGame.unity";

        // Create or open scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Object Pool ──────────────────────────────────────────────────────
        GameObject pool = new GameObject("ObjectPool");
        pool.AddComponent<ObjectPool>();

        // ── Path ─────────────────────────────────────────────────────────────
        // S-shaped path: spawn at left, winds across, reaches right
        Vector3[] wps = {
            new Vector3(-2f,  0.5f, 10f),   // WP0 — spawn (off left edge)
            new Vector3( 3f,  0.5f, 10f),   // WP1
            new Vector3( 3f,  0.5f,  3f),   // WP2
            new Vector3(10f,  0.5f,  3f),   // WP3
            new Vector3(10f,  0.5f, 15f),   // WP4
            new Vector3(17f,  0.5f, 15f),   // WP5
            new Vector3(17f,  0.5f,  9f),   // WP6
            new Vector3(22f,  0.5f,  9f),   // WP7 — base (off right edge)
        };

        GameObject pathGO = new GameObject("PathManager");
        pathGO.AddComponent<PathManager>();
        for (int i = 0; i < wps.Length; i++)
        {
            GameObject wp = new GameObject($"WP{i}");
            wp.transform.SetParent(pathGO.transform);
            wp.transform.position = wps[i];
        }

        // ── Enemy Spawner ────────────────────────────────────────────────────
        GameObject spawnerGO = new GameObject("EnemySpawner");
        spawnerGO.transform.position = wps[0];
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        {
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("spawnPoint").objectReferenceValue = spawnerGO.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wave Manager ─────────────────────────────────────────────────────
        GameObject waveGO = new GameObject("WaveManager");
        WaveManager wm = waveGO.AddComponent<WaveManager>();
        {
            SerializedObject so = new SerializedObject(wm);
            SerializedProperty wavesProp = so.FindProperty("waves");
            wavesProp.arraySize = waves.Length;
            for (int i = 0; i < waves.Length; i++)
                wavesProp.GetArrayElementAtIndex(i).objectReferenceValue = waves[i];
            so.FindProperty("timeBetweenWaves").floatValue = 12f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Game Manager ─────────────────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();
        {
            SerializedObject so = new SerializedObject(gm);
            so.FindProperty("startingGold").intValue  = 150;
            so.FindProperty("startingLives").intValue = 20;
            so.FindProperty("nextLevelSceneIndex").intValue = -1;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Grid Manager ─────────────────────────────────────────────────────
        GameObject gridGO = new GameObject("GridManager");
        GridManager gridMgr = gridGO.AddComponent<GridManager>();

        // ── Ground plane ─────────────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(10f, -0.05f, 9f);
        ground.transform.localScale = new Vector3(2.2f, 1f, 2f);
        ground.GetComponent<MeshRenderer>().sharedMaterial = matGround;
        ground.layer = LayerBuildable;

        // ── Grid Cells ───────────────────────────────────────────────────────
        // 18x16 grid covering x=0..17, z=0..15 (step=1)
        // Cells near the path are marked Blocked

        GameObject gridParent = new GameObject("Grid");
        for (int x = 0; x <= 17; x++)
        {
            for (int z = 0; z <= 17; z++)
            {
                Vector3 cellPos = new Vector3(x + 0.5f, 0f, z + 0.5f);
                bool blocked = IsNearPath(cellPos, wps, 0.9f);

                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.name = $"Cell_{x}_{z}";
                cell.transform.SetParent(gridParent.transform);
                cell.transform.position = cellPos;
                cell.transform.localScale = new Vector3(0.94f, 0.06f, 0.94f);
                cell.layer = LayerBuildable;

                // Remove the default box collider and add one tall enough for raycasting
                // (cell is visually flat at scale 0.06 in Y, so we scale the collider up)
                Object.DestroyImmediate(cell.GetComponent<BoxCollider>());
                BoxCollider bc = cell.AddComponent<BoxCollider>();
                bc.size   = new Vector3(1f, 20f, 1f);   // world height = 0.06 * 20 = 1.2 units
                bc.center = Vector3.zero;

                GridCell gc = cell.AddComponent<GridCell>();
                CellState initState = blocked ? CellState.Blocked : CellState.Buildable;

                SerializedObject so = new SerializedObject(gc);
                so.FindProperty("initialState").enumValueIndex         = (int)initState;
                so.FindProperty("materialBuildable").objectReferenceValue    = matCellBuild;
                so.FindProperty("materialPath").objectReferenceValue         = matCellBlocked;
                so.FindProperty("materialHighlightValid").objectReferenceValue   = matHighValid;
                so.FindProperty("materialHighlightInvalid").objectReferenceValue = matHighInvalid;
                so.ApplyModifiedPropertiesWithoutUndo();

                cell.GetComponent<MeshRenderer>().sharedMaterial = blocked ? matCellBlocked : matCellBuild;
            }
        }

        // Wire GridManager layerMask AFTER cells are created
        {
            SerializedObject so = new SerializedObject(gridMgr);
            so.FindProperty("buildableLayerMask").intValue = 1 << LayerBuildable;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Camera ───────────────────────────────────────────────────────────
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.fieldOfView       = 60f;
        cam.nearClipPlane     = 0.1f;
        cam.farClipPlane      = 200f;
        cam.backgroundColor   = new Color(0.2f, 0.3f, 0.4f);
        camGO.transform.position = new Vector3(9f, 18f, -4f);
        camGO.transform.eulerAngles = new Vector3(55f, 0f, 0f);
        CameraController cc = camGO.AddComponent<CameraController>();
        camGO.AddComponent<AudioListener>();

        // Wire camera to GridManager
        {
            SerializedObject so = new SerializedObject(gridMgr);
            so.FindProperty("mainCamera").objectReferenceValue = cam;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Directional Light ────────────────────────────────────────────────
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 1.2f;
        light.color     = new Color(1f, 0.95f, 0.8f);
        lightGO.transform.eulerAngles = new Vector3(50f, -30f, 0f);

        // ── UI Canvas ────────────────────────────────────────────────────────
        BuildUI(tdArcher, tdCannon);

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[Bootstrap] Scene saved to {scenePath}");

        // Add to build settings
        AddSceneToBuildSettings(scenePath);
    }

    // ─── UI BUILDER ──────────────────────────────────────────────────────────

    static void BuildUI(TowerData tdArcher, TowerData tdCannon)
    {
        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // UIManager on canvas
        UIManager uiMgr = canvasGO.AddComponent<UIManager>();

        // ── HUD Bar (top) ─────────────────────────────────────────────────────
        GameObject hudBar = CreatePanel(canvasGO.transform, "HUD_TopBar",
            anchorMin: new Vector2(0f, 0.92f), anchorMax: Vector2.one,
            color: new Color(0f, 0f, 0f, 0.7f));

        var textGold  = CreateTMPText(hudBar.transform, "Text_Gold",  "Gold: 150",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0.33f, 1f), fontSize: 28f, color: new Color(1f, 0.9f, 0.2f));
        var textLives = CreateTMPText(hudBar.transform, "Text_Lives", "Lives: 20",
            anchorMin: new Vector2(0.34f, 0f), anchorMax: new Vector2(0.66f, 1f), fontSize: 28f, color: Color.white);
        var textWave  = CreateTMPText(hudBar.transform, "Text_Wave",  "Wave 0 / 3",
            anchorMin: new Vector2(0.67f, 0f), anchorMax: Vector2.one, fontSize: 28f, color: Color.white);

        // Countdown text (center)
        var textCountdown = CreateTMPText(canvasGO.transform, "Text_Countdown", "",
            anchorMin: new Vector2(0.3f, 0.85f), anchorMax: new Vector2(0.7f, 0.92f), fontSize: 26f, color: Color.yellow);
        textCountdown.gameObject.SetActive(false);

        // ── Start Wave Button ─────────────────────────────────────────────────
        GameObject startWaveBtn = CreateButton(canvasGO.transform, "Btn_StartWave", "► START WAVE",
            anchorMin: new Vector2(0.4f, 0.01f), anchorMax: new Vector2(0.6f, 0.08f),
            bgColor: new Color(0.1f, 0.6f, 0.1f));

        // ── Pause Button ──────────────────────────────────────────────────────
        GameObject pauseBtn = CreateButton(canvasGO.transform, "Btn_Pause", "II PAUSE",
            anchorMin: new Vector2(0.92f, 0.92f), anchorMax: new Vector2(1f, 1f),
            bgColor: new Color(0.3f, 0.3f, 0.3f));

        // ── Tower Shop ────────────────────────────────────────────────────────
        GameObject shopPanel = CreatePanel(canvasGO.transform, "Panel_TowerShop",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0.18f, 0.5f),
            color: new Color(0.05f, 0.05f, 0.05f, 0.85f));

        CreateTMPText(shopPanel.transform, "Label_Shop", "TOWERS",
            anchorMin: new Vector2(0.1f, 0.9f), anchorMax: new Vector2(0.9f, 1f), fontSize: 22f, color: Color.white);

        // Archer tower button
        GameObject archerBtnGO = CreateButton(shopPanel.transform, "Btn_Archer", "",
            anchorMin: new Vector2(0.05f, 0.6f), anchorMax: new Vector2(0.95f, 0.88f),
            bgColor: new Color(0.15f, 0.4f, 0.15f));
        CreateTMPText(archerBtnGO.transform, "Label_ArcherName", "Arqueira",
            anchorMin: new Vector2(0f, 0.5f), anchorMax: Vector2.one, fontSize: 18f, color: Color.white);
        var archerCostLabel = CreateTMPText(archerBtnGO.transform, "Label_ArcherCost", "75g",
            anchorMin: Vector2.zero, anchorMax: new Vector2(1f, 0.5f), fontSize: 16f, color: new Color(1f, 0.9f, 0.2f));

        // Cannon tower button
        GameObject cannonBtnGO = CreateButton(shopPanel.transform, "Btn_Cannon", "",
            anchorMin: new Vector2(0.05f, 0.3f), anchorMax: new Vector2(0.95f, 0.58f),
            bgColor: new Color(0.4f, 0.2f, 0.1f));
        CreateTMPText(cannonBtnGO.transform, "Label_CannonName", "Canhão",
            anchorMin: new Vector2(0f, 0.5f), anchorMax: Vector2.one, fontSize: 18f, color: Color.white);
        var cannonCostLabel = CreateTMPText(cannonBtnGO.transform, "Label_CannonCost", "150g",
            anchorMin: Vector2.zero, anchorMax: new Vector2(1f, 0.5f), fontSize: 16f, color: new Color(1f, 0.9f, 0.2f));

        TowerShopUI shopUI = shopPanel.AddComponent<TowerShopUI>();
        Button archerBtn = archerBtnGO.GetComponent<Button>();
        Button cannonBtn = cannonBtnGO.GetComponent<Button>();

        SerializedObject soShop = new SerializedObject(shopUI);
        SerializedProperty tbArray = soShop.FindProperty("towerButtons");
        tbArray.arraySize = 2;

        SerializedProperty tb0 = tbArray.GetArrayElementAtIndex(0);
        tb0.FindPropertyRelative("towerData").objectReferenceValue = tdArcher;
        tb0.FindPropertyRelative("button").objectReferenceValue    = archerBtn;
        tb0.FindPropertyRelative("costLabel").objectReferenceValue = archerCostLabel;
        tb0.FindPropertyRelative("nameLabel").objectReferenceValue = archerBtnGO.transform.Find("Label_ArcherName").GetComponent<TextMeshProUGUI>();

        SerializedProperty tb1 = tbArray.GetArrayElementAtIndex(1);
        tb1.FindPropertyRelative("towerData").objectReferenceValue = tdCannon;
        tb1.FindPropertyRelative("button").objectReferenceValue    = cannonBtn;
        tb1.FindPropertyRelative("costLabel").objectReferenceValue = cannonCostLabel;
        tb1.FindPropertyRelative("nameLabel").objectReferenceValue = cannonBtnGO.transform.Find("Label_CannonName").GetComponent<TextMeshProUGUI>();

        soShop.ApplyModifiedPropertiesWithoutUndo();

        // ── Tower Info Panel ──────────────────────────────────────────────────
        GameObject towerInfoPanel = CreatePanel(canvasGO.transform, "Panel_TowerInfo",
            anchorMin: new Vector2(0.82f, 0.3f), anchorMax: new Vector2(1f, 0.85f),
            color: new Color(0.05f, 0.05f, 0.1f, 0.9f));
        towerInfoPanel.SetActive(false);

        var tiName    = CreateTMPText(towerInfoPanel.transform, "Text_TowerName",    "Tower Nv.1", new Vector2(0.05f, 0.85f), new Vector2(0.95f, 1f),    22f, Color.white);
        var tiDamage  = CreateTMPText(towerInfoPanel.transform, "Text_Damage",       "Dmg: 15",    new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.85f), 18f, Color.white);
        var tiRange   = CreateTMPText(towerInfoPanel.transform, "Text_Range",        "Range: 3.5u",new Vector2(0.05f, 0.59f), new Vector2(0.95f, 0.72f), 18f, Color.white);
        var tiLevel   = CreateTMPText(towerInfoPanel.transform, "Text_Level",        "Level 1",    new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.58f), 16f, Color.yellow);
        var tiUpgrade = CreateTMPText(towerInfoPanel.transform, "Text_UpgradeCost",  "38",         new Vector2(0.05f, 0.37f), new Vector2(0.55f, 0.47f), 16f, new Color(1f, 0.9f, 0.2f));
        var tiSell    = CreateTMPText(towerInfoPanel.transform, "Text_SellValue",    "37",         new Vector2(0.55f, 0.37f), new Vector2(0.95f, 0.47f), 16f, new Color(0.3f, 1f, 0.3f));

        GameObject upgBtn  = CreateButton(towerInfoPanel.transform, "Btn_Upgrade", "UPGRADE",
            new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.35f), new Color(0.1f, 0.4f, 0.7f));
        GameObject sellBtn = CreateButton(towerInfoPanel.transform, "Btn_Sell",    "SELL",
            new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.17f), new Color(0.6f, 0.15f, 0.1f));

        TowerInfoPanel tip = towerInfoPanel.AddComponent<TowerInfoPanel>();
        SerializedObject soTIP = new SerializedObject(tip);
        soTIP.FindProperty("textTowerName").objectReferenceValue  = tiName;
        soTIP.FindProperty("textDamage").objectReferenceValue     = tiDamage;
        soTIP.FindProperty("textRange").objectReferenceValue      = tiRange;
        soTIP.FindProperty("textLevel").objectReferenceValue      = tiLevel;
        soTIP.FindProperty("textUpgradeCost").objectReferenceValue= tiUpgrade;
        soTIP.FindProperty("textSellValue").objectReferenceValue  = tiSell;
        soTIP.FindProperty("buttonUpgrade").objectReferenceValue  = upgBtn.GetComponent<Button>();
        soTIP.FindProperty("buttonSell").objectReferenceValue     = sellBtn.GetComponent<Button>();
        soTIP.ApplyModifiedPropertiesWithoutUndo();

        // ── Pause Panel ───────────────────────────────────────────────────────
        GameObject pausePanel = CreatePanel(canvasGO.transform, "Panel_Pause",
            anchorMin: new Vector2(0.35f, 0.3f), anchorMax: new Vector2(0.65f, 0.7f),
            color: new Color(0f, 0f, 0f, 0.9f));
        pausePanel.SetActive(false);

        CreateTMPText(pausePanel.transform, "Text_Paused", "PAUSED",
            new Vector2(0.1f, 0.75f), new Vector2(0.9f, 1f), 36f, Color.white);
        GameObject resumeBtn  = CreateButton(pausePanel.transform, "Btn_Resume",  "RESUME",
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.72f), new Color(0.1f, 0.5f, 0.1f));
        GameObject restartBtn = CreateButton(pausePanel.transform, "Btn_Restart", "RESTART",
            new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.48f), new Color(0.4f, 0.2f, 0.05f));
        GameObject menuBtn    = CreateButton(pausePanel.transform, "Btn_Menu",    "MENU",
            new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.25f), new Color(0.2f, 0.2f, 0.2f));

        // ── Game Over Panel ───────────────────────────────────────────────────
        GameObject goPanel = CreatePanel(canvasGO.transform, "Panel_GameOver",
            anchorMin: new Vector2(0.3f, 0.3f), anchorMax: new Vector2(0.7f, 0.7f),
            color: new Color(0.4f, 0f, 0f, 0.95f));
        goPanel.SetActive(false);

        CreateTMPText(goPanel.transform, "Text_GameOver", "GAME OVER",
            new Vector2(0.1f, 0.7f), new Vector2(0.9f, 1f), 42f, Color.white);
        var goWaveText = CreateTMPText(goPanel.transform, "Text_GoWave", "Wave: 0",
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.7f), 24f, Color.white);
        GameObject retryBtn   = CreateButton(goPanel.transform, "Btn_Retry",   "RETRY",
            new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.48f), new Color(0.1f, 0.5f, 0.1f));
        GameObject goMenuBtn  = CreateButton(goPanel.transform, "Btn_GoMenu",  "MENU",
            new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.23f), new Color(0.2f, 0.2f, 0.2f));

        // ── Victory Panel ─────────────────────────────────────────────────────
        GameObject vicPanel = CreatePanel(canvasGO.transform, "Panel_Victory",
            anchorMin: new Vector2(0.3f, 0.3f), anchorMax: new Vector2(0.7f, 0.7f),
            color: new Color(0f, 0.2f, 0.4f, 0.95f));
        vicPanel.SetActive(false);

        CreateTMPText(vicPanel.transform, "Text_Victory", "VICTORY!",
            new Vector2(0.1f, 0.7f), new Vector2(0.9f, 1f), 42f, Color.yellow);
        var vicScore  = CreateTMPText(vicPanel.transform, "Text_VicScore",   "Score: 0",
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.7f), 22f, Color.white);
        var vicLives  = CreateTMPText(vicPanel.transform, "Text_VicLives",   "Lives: 20/20",
            new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.55f), 18f, Color.white);
        var vicPerf   = CreateTMPText(vicPanel.transform, "Text_VicPerfect", "Perfect: 0/3",
            new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.42f), 18f, Color.white);
        GameObject nextLvlBtn = CreateButton(vicPanel.transform, "Btn_NextLevel", "MENU",
            new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.27f), new Color(0.1f, 0.5f, 0.1f));

        // ── Wire UIManager ────────────────────────────────────────────────────
        SerializedObject soUI = new SerializedObject(uiMgr);
        soUI.FindProperty("textGold").objectReferenceValue          = textGold;
        soUI.FindProperty("textLives").objectReferenceValue         = textLives;
        soUI.FindProperty("textWave").objectReferenceValue          = textWave;
        soUI.FindProperty("textCountdown").objectReferenceValue     = textCountdown;
        soUI.FindProperty("buttonStartWave").objectReferenceValue   = startWaveBtn.GetComponent<Button>();
        soUI.FindProperty("buttonPause").objectReferenceValue       = pauseBtn.GetComponent<Button>();
        soUI.FindProperty("panelPause").objectReferenceValue        = pausePanel;
        soUI.FindProperty("panelVictory").objectReferenceValue      = vicPanel;
        soUI.FindProperty("panelGameOver").objectReferenceValue     = goPanel;
        soUI.FindProperty("buttonResume").objectReferenceValue      = resumeBtn.GetComponent<Button>();
        soUI.FindProperty("buttonRestart").objectReferenceValue     = restartBtn.GetComponent<Button>();
        soUI.FindProperty("buttonMainMenu").objectReferenceValue    = menuBtn.GetComponent<Button>();
        soUI.FindProperty("buttonNextLevel").objectReferenceValue   = nextLvlBtn.GetComponent<Button>();
        soUI.FindProperty("buttonVictoryMenu").objectReferenceValue = nextLvlBtn.GetComponent<Button>();
        soUI.FindProperty("textVictoryScore").objectReferenceValue  = vicScore;
        soUI.FindProperty("textVictoryLives").objectReferenceValue  = vicLives;
        soUI.FindProperty("textVictoryPerfect").objectReferenceValue= vicPerf;
        soUI.FindProperty("textGameOverWave").objectReferenceValue  = goWaveText;
        soUI.FindProperty("buttonRetry").objectReferenceValue       = retryBtn.GetComponent<Button>();
        soUI.FindProperty("buttonGameOverMenu").objectReferenceValue= goMenuBtn.GetComponent<Button>();

        SerializedProperty starsProp = soUI.FindProperty("starObjects");
        starsProp.arraySize = 0;

        soUI.ApplyModifiedPropertiesWithoutUndo();
    }

    // ─── UI HELPERS ──────────────────────────────────────────────────────────

    static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
        return go;
    }

    static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, float fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        if (!string.IsNullOrEmpty(label))
        {
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 20f;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
        }

        return go;
    }

    // ─── PATH BLOCKING HELPER ────────────────────────────────────────────────

    static bool IsNearPath(Vector3 cellCenter, Vector3[] waypoints, float threshold)
    {
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            float d = DistancePointToSegmentXZ(cellCenter, waypoints[i], waypoints[i + 1]);
            if (d < threshold) return true;
        }
        return false;
    }

    static float DistancePointToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        float ax = a.x, az = a.z, bx = b.x, bz = b.z, px = p.x, pz = p.z;
        float dx = bx - ax, dz = bz - az;
        float len2 = dx * dx + dz * dz;
        if (len2 < 0.0001f) return Mathf.Sqrt((px - ax) * (px - ax) + (pz - az) * (pz - az));
        float t = Mathf.Clamp01(((px - ax) * dx + (pz - az) * dz) / len2);
        float nx = ax + t * dx - px, nz = az + t * dz - pz;
        return Mathf.Sqrt(nx * nx + nz * nz);
    }

    // ─── BUILD SETTINGS ──────────────────────────────────────────────────────

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        bool found = false;
        foreach (var s in scenes)
            if (s.path == scenePath) { found = true; break; }
        if (!found)
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
