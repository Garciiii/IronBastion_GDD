// IronBastionIntegration.cs — v4 FULL
// Menu: Iron Bastion/...
// 0 - Fix WaveManager (scene)
// 1 - Integrar Torres + Inimigos (prefabs only)
// 2 - Decorar Mapa (scene props + lighting)
// 3 - Corrigir Todos os Materiais

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class IronBastionIntegration
{
    const string MAT_FOLDER = "Assets/_Project/Art/Materials";

    // ─────────────────────────────────────────────────────────────────────────
    // 0 — FIX WAVEMANAGER
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Iron Bastion/0 - Fix WaveManager (waves na cena)")]
    public static void FixWaveManagerMenu()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Iron Bastion", "Sai do Play Mode antes de correr este comando.", "OK");
            return;
        }
        FixWaveManager();
        EditorUtility.DisplayDialog("Iron Bastion", "WaveManager configurado!\nGuarda a cena com Ctrl+S.", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1 — INTEGRAR TORRES + INIMIGOS (só prefabs)
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Iron Bastion/1 - Integrar Torres + Inimigos (so prefabs)")]
    public static void IntegrateAssetsMenu()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Iron Bastion", "Sai do Play Mode antes de correr este comando.", "OK");
            return;
        }

        Debug.Log("[IB] ===== INTEGRACAO DE ASSETS =====");
        EnsureFolder(MAT_FOLDER);
        ConvertURPMaterials();

        // Torres
        IntegrateTower(
            "Assets/_Project/Prefabs/Towers/Tower_Archer.prefab",
            "Assets/TD_Sci-Fi_Turret1_Example/Prefabs/Turret1/turret_1_1.prefab",
            new Vector3(0.5f, 0.5f, 0.5f),
            new Color(0f, 0.831f, 1f),
            "Mat_Turret_Archer",
            "turret_head1"
        );
        IntegrateTower(
            "Assets/_Project/Prefabs/Towers/Tower_Metralhadora.prefab",
            "Assets/Bruhassets/DefenceLazer/Prefabs/LazerTower.prefab",
            new Vector3(0.55f, 0.55f, 0.55f),
            new Color(1f, 0.843f, 0f),
            "Mat_Turret_MG",
            null
        );
        IntegrateTower(
            "Assets/_Project/Prefabs/Towers/Tower_Cannon.prefab",
            "Assets/Bruhassets/DefenceCannon/Prefabs/CannonTower.prefab",
            new Vector3(0.65f, 0.65f, 0.65f),
            new Color(1f, 0.42f, 0.208f),
            "Mat_Turret_Cannon",
            null
        );

        // Inimigos
        IntegrateEnemy(
            "Assets/_Project/Prefabs/Enemies/Enemy_Common.prefab",
            "Assets/FreeLowPolyRobot/Meshes_and_Animations/RandomModularRobots_Prefab.prefab",
            new Vector3(0.7f, 0.7f, 0.7f)
        );
        IntegrateEnemy(
            "Assets/_Project/Prefabs/Enemies/EnemyTank.prefab",
            "Assets/ithappy/Creative_Characters_FREE/Prefabs/Base_Mesh.prefab",
            new Vector3(0.9f, 0.9f, 0.9f)
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[IB] ===== INTEGRACAO CONCLUIDA =====");
        EditorUtility.DisplayDialog("Iron Bastion",
            "Prefabs integrados!\n\nAgora corre '0 - Fix WaveManager'\ne guarda com Ctrl+S.", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2 — DECORAR MAPA (cena)
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Iron Bastion/2 - Decorar Mapa (props + iluminacao na cena)")]
    public static void DecorateMapMenu()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Iron Bastion", "Sai do Play Mode antes de correr este comando.", "OK");
            return;
        }
        DecorateMap();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Iron Bastion", "Mapa decorado!\nGuarda com Ctrl+S.", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3 — CORRIGIR TODOS OS MATERIAIS
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Iron Bastion/3 - Corrigir Todos os Materiais (URP->Standard)")]
    public static void FixAllMaterialsMenu()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Iron Bastion", "Sai do Play Mode antes de correr este comando.", "OK");
            return;
        }
        ConvertURPMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Iron Bastion", "Materiais corrigidos!", "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TOWER INTEGRATION
    // ─────────────────────────────────────────────────────────────────────────
    static void IntegrateTower(string ibPath, string srcPath, Vector3 scale,
                                Color accent, string matName, string headName)
    {
        GameObject srcAsset = AssetDatabase.LoadAssetAtPath<GameObject>(srcPath);
        if (srcAsset == null) { Debug.LogWarning("[IB] SKIP source: " + srcPath); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(ibPath);
        if (root == null) { Debug.LogWarning("[IB] SKIP prefab: " + ibPath); return; }

        try
        {
            // Remove TurretVisual anterior
            Transform existing = root.transform.Find("TurretVisual");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Desativa visuais primitivos diretos (MeshRenderer sem Collider, ignora FirePoint)
            foreach (Transform child in root.transform)
            {
                if (child.name == "FirePoint" || child.name == "TurretVisual") continue;
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null && child.GetComponent<Collider>() == null)
                    child.gameObject.SetActive(false);
            }

            // Instancia visual
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(srcAsset, root.transform);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "TurretVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = scale;

            FixMaterials(visual);
            CreateAccentMaterial(matName, accent);

            // TurretHead
            Transform turretHead = null;
            if (!string.IsNullOrEmpty(headName))
                turretHead = FindDeep(visual.transform, headName);
            if (turretHead == null)
                turretHead = FindTurretHeadAuto(visual.transform);
            if (turretHead == null)
                turretHead = visual.transform;

            // FirePoint
            Transform fp = FindDeep(root.transform, "FirePoint");
            if (fp == null)
            {
                GameObject fpGo = new GameObject("FirePoint");
                fpGo.transform.SetParent(turretHead, false);
                fpGo.transform.localPosition = new Vector3(0f, 0f, 0.7f);
                fp = fpGo.transform;
            }
            else
            {
                fp.SetParent(turretHead, false);
                fp.localPosition = new Vector3(0f, 0f, 0.7f);
                fp.localRotation = Quaternion.identity;
            }

            // Wire TowerBase fields
            TowerBase tb = root.GetComponent<TowerBase>();
            if (tb != null)
            {
                SerializedObject so = new SerializedObject(tb);
                so.FindProperty("turretHead").objectReferenceValue = turretHead;
                so.FindProperty("firePoint").objectReferenceValue  = fp;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, ibPath);
            Debug.Log("[IB] OK Torre: " + Path.GetFileNameWithoutExtension(ibPath)
                      + "  head=" + turretHead.name);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ENEMY INTEGRATION
    // ─────────────────────────────────────────────────────────────────────────
    static void IntegrateEnemy(string ibPath, string srcPath, Vector3 scale)
    {
        GameObject srcAsset = AssetDatabase.LoadAssetAtPath<GameObject>(srcPath);
        if (srcAsset == null) { Debug.LogWarning("[IB] SKIP source: " + srcPath); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(ibPath);
        if (root == null) { Debug.LogWarning("[IB] SKIP prefab: " + ibPath); return; }

        try
        {
            // Remove EnemyVisual anterior
            Transform existing = root.transform.Find("EnemyVisual");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Desativa MeshRenderer do root (cápsula primitiva) — BUG FIX
            MeshRenderer rootMR = root.GetComponent<MeshRenderer>();
            if (rootMR != null) rootMR.enabled = false;

            // Desativa MeshRenderers diretos nos filhos (nao toca em Canvas/HealthBar)
            foreach (Transform child in root.transform)
            {
                if (child.GetComponent<Canvas>() != null) continue;
                if (child.name.Contains("HealthBar") || child.name.Contains("Canvas")) continue;
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            // Instancia visual
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(srcAsset, root.transform);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "EnemyVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = scale;

            FixMaterials(visual);

            // Adiciona bridge de animação se o modelo tiver Animator
            if (visual.GetComponentInChildren<Animator>() != null &&
                visual.GetComponent<EnemyAnimatorBridge>() == null)
                visual.AddComponent<EnemyAnimatorBridge>();

            // Ajusta HealthBar Y para 1.8 unidades acima do root
            HealthBar hb = root.GetComponentInChildren<HealthBar>(true);
            if (hb != null)
                hb.transform.localPosition = new Vector3(0f, 1.8f, 0f);

            PrefabUtility.SaveAsPrefabAsset(root, ibPath);
            Debug.Log("[IB] OK Inimigo: " + Path.GetFileNameWithoutExtension(ibPath));
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MAP DECORATION (cena)
    // ─────────────────────────────────────────────────────────────────────────
    static void DecorateMap()
    {
        // Prefabs industriais disponíveis
        string[] barrelPaths =
        {
            "Assets/RPG_FPS_game_assets_industrial/Barrels/Barrel_v1/Barrel_v1_LD1.prefab",
            "Assets/RPG_FPS_game_assets_industrial/Barrels/Barrel_v2/Barrel_v2_single.prefab",
            "Assets/RPG_FPS_game_assets_industrial/Barrels/Barrel_v3/Barrel_v3_single.prefab",
        };
        string[] boxPaths =
        {
            "Assets/RPG_FPS_game_assets_industrial/Boxes/Wooden_box_v1/Wooden_box_v1_LD1.prefab",
            "Assets/RPG_FPS_game_assets_industrial/Boxes/Wooden_box_v1/Wooden_box_v1_LD2.prefab",
        };
        string containerPath =
            "Assets/RPG_FPS_game_assets_industrial/Containers/Cargo_container_v1/Cargo_container_v1_LD1close.prefab";
        string fencePath =
            "Assets/RPG_FPS_game_assets_industrial/Fences/Concrete_fences/Concrete_fence_v1/Concrete_fence_v1_c1.prefab";

        // Encontrar ou criar grupo de props na cena
        GameObject propsRoot = GameObject.Find("MapProps");
        if (propsRoot == null)
            propsRoot = new GameObject("MapProps");

        // Remover props anteriores
        for (int i = propsRoot.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(propsRoot.transform.GetChild(i).gameObject);

        // Posições dos props ao longo das bordas
        // Ground: center (10, 0, 9), size 22x20 — borda em x=-2/22, z=-2/20
        var propSpots = new List<(Vector3 pos, float rot)>
        {
            // Canto inferior-esquerdo
            (new Vector3(-2f, 0f, -2f),   0f),
            (new Vector3(-1f, 0f, -2f),  45f),
            // Canto inferior-direito
            (new Vector3(22f, 0f, -2f),   0f),
            (new Vector3(21f, 0f, -2f),  90f),
            // Canto superior-esquerdo
            (new Vector3(-2f, 0f, 20f),   0f),
            (new Vector3(-1f, 0f, 20f),  135f),
            // Canto superior-direito
            (new Vector3(22f, 0f, 20f),   0f),
            (new Vector3(21f, 0f, 20f),  180f),
            // Bordo esquerdo (mid)
            (new Vector3(-2f, 0f,  5f),  30f),
            (new Vector3(-2f, 0f, 13f),  60f),
            // Bordo direito (mid)
            (new Vector3(22f, 0f,  5f), 210f),
            (new Vector3(22f, 0f, 13f), 270f),
        };

        // Carregar prefabs disponíveis
        var availableProps = new List<GameObject>();
        foreach (var p in barrelPaths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go != null) availableProps.Add(go);
        }
        foreach (var p in boxPaths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go != null) availableProps.Add(go);
        }
        var containerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(containerPath);
        var fencePrefab     = AssetDatabase.LoadAssetAtPath<GameObject>(fencePath);

        if (availableProps.Count == 0)
        {
            Debug.LogWarning("[IB] Nenhum prop industrial encontrado. Verifica os paths dos assets.");
        }

        // Colocar props
        int placed = 0;
        for (int i = 0; i < propSpots.Count && availableProps.Count > 0; i++)
        {
            var (pos, rot) = propSpots[i];
            GameObject prefab = availableProps[i % availableProps.Count];
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(propsRoot.transform, false);
            inst.transform.position = pos;
            inst.transform.rotation = Quaternion.Euler(0f, rot, 0f);
            placed++;
        }

        // Container grande no canto direito se disponível
        if (containerPrefab != null)
        {
            GameObject c = (GameObject)PrefabUtility.InstantiatePrefab(containerPrefab);
            c.transform.SetParent(propsRoot.transform, false);
            c.transform.position = new Vector3(22f, 0f, 9f);
            c.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            placed++;
        }

        // Melhorar iluminação
        FixLighting();

        Debug.Log("[IB] Decoração: " + placed + " props colocados.");
    }

    static void FixLighting()
    {
        // Directional Light
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light dirLight = null;
        foreach (var l in lights)
            if (l.type == LightType.Directional) { dirLight = l; break; }

        if (dirLight != null)
        {
            dirLight.color     = new Color(1f, 0.96f, 0.84f);
            dirLight.intensity = 0.8f;
            dirLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
        }

        // Ambient
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.20f);

        // Point light: Spawn (vermelho)
        AddOrUpdatePointLight("SpawnLight",
            FindSceneObjectByName("SpawnPoint")?.transform.position ?? new Vector3(1f, 1f, 1f),
            new Color(1f, 0.2f, 0.2f), 1f, 4f);

        // Point light: Base (ciano)
        AddOrUpdatePointLight("BaseLight",
            FindSceneObjectByName("BasePoint")?.transform.position ?? new Vector3(19f, 1f, 17f),
            new Color(0f, 0.83f, 1f), 1f, 4f);
    }

    static void AddOrUpdatePointLight(string name, Vector3 pos, Color col, float intensity, float range)
    {
        GameObject existing = GameObject.Find(name);
        if (existing == null)
        {
            existing = new GameObject(name);
            existing.AddComponent<Light>();
        }
        Light l = existing.GetComponent<Light>();
        if (l == null) l = existing.AddComponent<Light>();
        l.type      = LightType.Point;
        l.color     = col;
        l.intensity = intensity;
        l.range     = range;
        existing.transform.position = pos;
    }

    static GameObject FindSceneObjectByName(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (go.name == name) return go;
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WAVEMANAGER FIX (cena)
    // ─────────────────────────────────────────────────────────────────────────
    static void FixWaveManager()
    {
        var w1 = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/_Project/Data/Waves/WaveData_Wave1.asset");
        var w2 = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/_Project/Data/Waves/WaveData_Wave2.asset");
        var w3 = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/_Project/Data/Waves/WaveData_Wave3.asset");

        if (w1 == null || w2 == null || w3 == null)
        {
            Debug.LogError("[IB] WaveData assets nao encontrados.");
            return;
        }

        WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm == null) { Debug.LogWarning("[IB] WaveManager nao encontrado na cena."); return; }

        SerializedObject so = new SerializedObject(wm);
        SerializedProperty p = so.FindProperty("waves");
        p.arraySize = 3;
        p.GetArrayElementAtIndex(0).objectReferenceValue = w1;
        p.GetArrayElementAtIndex(1).objectReferenceValue = w2;
        p.GetArrayElementAtIndex(2).objectReferenceValue = w3;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[IB] WaveManager OK: " + w1.name + ", " + w2.name + ", " + w3.name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    static Transform FindTurretHeadAuto(Transform visual)
    {
        string[] kw = { "head", "barrel", "gun", "top", "rotate", "cannon",
                        "lazer", "laser", "weapon", "mount", "turret" };
        foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLower();
            foreach (var k in kw)
                if (n.Contains(k)) return t;
        }
        if (visual.childCount > 0) return visual.GetChild(0);
        return visual;
    }

    static void FixMaterials(GameObject obj)
    {
        EnsureFolder(MAT_FOLDER);
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string sh = mats[i].shader.name;
                if (sh.Contains("Universal Render Pipeline") ||
                    sh.Contains("URP") ||
                    sh.Contains("Shader Graphs") ||
                    sh == "Hidden/InternalErrorShader")
                {
                    string p = MAT_FOLDER + "/Fixed_" + mats[i].name + ".mat";
                    Material fm = AssetDatabase.LoadAssetAtPath<Material>(p);
                    if (fm == null)
                    {
                        fm = new Material(Shader.Find("Standard"));
                        // Tentar preservar cor
                        if (mats[i].HasProperty("_BaseColor"))
                            fm.color = mats[i].GetColor("_BaseColor");
                        else if (mats[i].HasProperty("_Color"))
                            fm.color = mats[i].GetColor("_Color");
                        // Tentar preservar textura
                        if (mats[i].HasProperty("_BaseMap") && mats[i].GetTexture("_BaseMap") != null)
                            fm.mainTexture = mats[i].GetTexture("_BaseMap");
                        else if (mats[i].HasProperty("_MainTex") && mats[i].GetTexture("_MainTex") != null)
                            fm.mainTexture = mats[i].GetTexture("_MainTex");
                        AssetDatabase.CreateAsset(fm, p);
                    }
                    mats[i] = fm;
                    changed = true;
                }
            }
            if (changed) r.sharedMaterials = mats;
        }
    }

    static void ConvertURPMaterials()
    {
        string[] folders =
        {
            "Assets/Bruhassets",
            "Assets/TD_Sci-Fi_Turret1_Example",
            "Assets/FreeLowPolyRobot",
            "Assets/ithappy",
            "Assets/RPG_FPS_game_assets_industrial",
            "Assets/FreeLowpolyScifiObjects",
        };
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Material", folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            string sh = mat.shader.name;
            if (sh.Contains("Universal Render Pipeline") ||
                sh.Contains("URP") ||
                sh.Contains("Shader Graphs") ||
                sh == "Hidden/InternalErrorShader")
            {
                Color col = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                          : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                          : Color.white;
                Texture tex = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap")
                            : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex")
                            : null;
                mat.shader = Shader.Find("Standard");
                mat.color  = col;
                if (tex != null) mat.mainTexture = tex;
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        if (count > 0)
            Debug.Log("[IB] " + count + " materiais convertidos para Standard.");
    }

    static void CreateAccentMaterial(string matName, Color color)
    {
        string path = MAT_FOLDER + "/" + matName + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = color * 0.6f;
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", color * 2f);
        EditorUtility.SetDirty(m);
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(
                Path.GetDirectoryName(path).Replace("\\", "/"),
                Path.GetFileName(path));
    }
}
