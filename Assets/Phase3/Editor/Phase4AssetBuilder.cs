using UnityEditor;
using UnityEngine;

public static class Phase4AssetBuilder
{
    private const string BasePath = "Assets/Phase3/Data";

    [MenuItem("Phase3/Create Phase4 Data Assets")]
    public static void CreateAllAssets()
    {
        CreateShapeConfigs();
        CreateGlazeConfigs();
        CreateFireConfig();
        CreateGameConfig();
        CreateOrderDataAssets();
        MoveLegacyAssets();

        AssetDatabase.SaveAssets();
        Debug.Log("[Phase4AssetBuilder] All assets created.");
    }

    // ─── ShapeConfigSO × 5 ──────────────────────────────────────

    private static void CreateShapeConfigs()
    {
        string dir = EnsureDir($"{BasePath}/ShapeConfigs");

        CreateShape(dir, "Shape_Bowl", "SHAPE_001", "碗", "Bowl", 0.85f, 0.00f, 0.00f, 0.60f, 0.35f, 1, 1, ShapeType.Bowl);
        CreateShape(dir, "Shape_Plate", "SHAPE_002", "盘", "Plate", 0.90f, 0.00f, 0.00f, 0.30f, 0.55f, 2, 1, ShapeType.Plate);
        CreateShape(dir, "Shape_Meiping", "SHAPE_003", "梅瓶", "Meiping", 0.15f, 0.10f, 0.85f, 0.55f, 0.40f, 4, 3, ShapeType.Meiping);
        CreateShape(dir, "Shape_YuhuChun", "SHAPE_004", "玉壶春瓶", "YuhuChun", 0.35f, 0.55f, 0.45f, 0.75f, 0.40f, 3, 2, ShapeType.YuhuChun);
        CreateShape(dir, "Shape_Jar", "SHAPE_005", "罐", "Jar", 0.55f, 0.05f, 0.70f, 0.80f, 0.60f, 3, 2, ShapeType.Jar);
    }

    private static void CreateShape(string dir, string fileName, string id, string cn, string en,
        float mouth, float neck, float shoulder, float belly, float foot,
        int diff, int unlock, ShapeType type)
    {
        string path = $"{dir}/{fileName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<ShapeConfigSO>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ShapeConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.shapeID = id;
        asset.nameCN = cn;
        asset.nameEN = en;
        asset.mouthRatio = mouth;
        asset.neckRatio = neck;
        asset.shoulderRatio = shoulder;
        asset.bellyRatio = belly;
        asset.footRatio = foot;
        asset.difficulty = diff;
        asset.unlockLevel = unlock;
        asset.shapeType = type;
        EditorUtility.SetDirty(asset);
    }

    // ─── GlazeConfigSO × 5 ──────────────────────────────────────

    private static void CreateGlazeConfigs()
    {
        string dir = EnsureDir($"{BasePath}/GlazeConfigs");

        CreateGlaze(dir, "Glaze_Yingqing", "GLAZE_001", "影青釉", "Yingqing", 0.00f, 0.020f, 0.00f, 1250f, 1280f, GlazeColorType.Oxidation, 2);
        CreateGlaze(dir, "Glaze_Tianbai", "GLAZE_002", "甜白釉", "Tianbai", 0.00f, 0.015f, 0.00f, 1250f, 1300f, GlazeColorType.Reduction, 2);
        CreateGlaze(dir, "Glaze_Qinghua", "GLAZE_003", "青花", "Qinghua", 0.00f, 0.000f, 0.02f, 1300f, 1330f, GlazeColorType.Reduction, 3);
        CreateGlaze(dir, "Glaze_Jihong", "GLAZE_004", "霁红釉", "Jihong", 0.01f, 0.000f, 0.00f, 1280f, 1320f, GlazeColorType.Oxidation, 3);
        CreateGlaze(dir, "Glaze_Dongqing", "GLAZE_005", "冬青釉", "Dongqing", 0.00f, 0.020f, 0.00f, 1250f, 1300f, GlazeColorType.Dual, 2);
    }

    private static void CreateGlaze(string dir, string fileName, string id, string cn, string en,
        float cu, float fe, float co, float tMin, float tMax, GlazeColorType colorType, int diff)
    {
        string path = $"{dir}/{fileName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<GlazeConfigSO>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<GlazeConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.glazeID = id;
        asset.nameCN = cn;
        asset.nameEN = en;
        asset.copper = cu;
        asset.iron = fe;
        asset.cobalt = co;
        asset.temperatureMin = tMin;
        asset.temperatureMax = tMax;
        asset.colorType = colorType;
        asset.difficulty = diff;
        EditorUtility.SetDirty(asset);
    }

    // ─── FireConfigSO × 1 ───────────────────────────────────────

    private static void CreateFireConfig()
    {
        string dir = EnsureDir($"{BasePath}/FireConfigs");
        string path = $"{dir}/FireConfig.asset";

        var asset = AssetDatabase.LoadAssetAtPath<FireConfigSO>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<FireConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
        }

        // 8 stages
        asset.stages = new KilnStageConfig[8];
        asset.stages[0] = NewStage("S1", "预热排水", "Preheat", 0f, 300f, 4f, 6f, 5f, "Oxidation", 3);
        asset.stages[1] = NewStage("S2", "晶体脱水", "Dehydration", 300f, 600f, 4f, 6f, 5f, "Oxidation", 3);
        asset.stages[2] = NewStage("S3", "氧化烧清", "OxidationClear", 600f, 1000f, 8f, 12f, 10f, "Oxidation", 4);
        asset.stages[3] = NewStage("S4", "还原烧成", "Reduction", 1000f, 1200f, 6f, 8f, 7f, "Reduction", 5);
        asset.stages[4] = NewStage("S5", "强还原保温", "StrongReduction", 1200f, 1280f, 2f, 4f, 3f, "Reduction", 3);
        asset.stages[5] = NewStage("S6", "高温成瓷", "HighTemp", 1280f, 1320f, 1f, 3f, 2f, "Reduction", 4);
        asset.stages[6] = NewStage("S7", "急冷段", "RapidCool", 1320f, 700f, 4f, 6f, 5f, "Neutral", 2);
        asset.stages[7] = NewStage("S8", "缓冷段", "SlowCool", 700f, 25f, 30f, 40f, 35f, "Neutral", 1);

        // 4 flame switches
        asset.flameSwitches = new FlameSwitchConfig[4];
        asset.flameSwitches[0] = NewFlameSwitch("FS1", "亮红色", "樱红色", 3);
        asset.flameSwitches[1] = NewFlameSwitch("FS2", "橘黄色", "暗黄色", 8);
        asset.flameSwitches[2] = NewFlameSwitch("FS3", "暗黄色", "亮黄色", 3);
        asset.flameSwitches[3] = NewFlameSwitch("FS4", "黄白色", "青白色", 6);

        // 11 defects
        asset.defects = new DefectConfig[11];
        asset.defects[0] = NewDefect("D1", "开裂/窑裂", "Crack", true, 0, false);
        asset.defects[1] = NewDefect("D2", "过烧(严重)", "SevereOverfire", true, 0, false);
        asset.defects[2] = NewDefect("D3", "生烧/欠烧", "Underfire", false, 20, false);
        asset.defects[3] = NewDefect("D4", "过烧(轻度)", "MildOverfire", false, 15, false);
        asset.defects[4] = NewDefect("D5", "变形", "Deformation", false, 10, false);
        asset.defects[5] = NewDefect("D6", "阴黄", "Yellowing", false, 10, false);
        asset.defects[6] = NewDefect("D7", "起泡/釉泡", "Blister", false, 8, true);
        asset.defects[7] = NewDefect("D8", "窑粘", "KilnAdhesion", false, 8, true);
        asset.defects[8] = NewDefect("D9", "烟熏", "Smoking", false, 7, false);
        asset.defects[9] = NewDefect("D10", "无光/失透", "Dullness", false, 10, false);
        asset.defects[10] = NewDefect("D11", "针孔", "Pinhole", false, 5, true);

        EditorUtility.SetDirty(asset);
    }

    private static KilnStageConfig NewStage(string id, string cn, string en,
        float tStart, float tEnd, float dMin, float dMax, float dMedian, string atmo, int cap)
    {
        return new KilnStageConfig
        {
            stageId = id, stageNameCN = cn, stageNameEN = en,
            tempStart = tStart, tempEnd = tEnd,
            durationMin = dMin, durationMax = dMax, durationMedian = dMedian,
            atmosphere = atmo, penaltyCap = cap
        };
    }

    private static FlameSwitchConfig NewFlameSwitch(string id, string from, string to, int penalty)
    {
        return new FlameSwitchConfig { switchId = id, fromColorCN = from, toColorCN = to, penaltyPoints = penalty };
    }

    private static DefectConfig NewDefect(string id, string cn, string en, bool fatal, int penalty, bool stackable)
    {
        return new DefectConfig
        {
            defectId = id, nameCN = cn, nameEN = en,
            isFatal = fatal, penaltyPoints = penalty, isStackable = stackable, triggerCondition = ""
        };
    }

    // ─── GameConfigSO × 1 ───────────────────────────────────────

    private static void CreateGameConfig()
    {
        string dir = EnsureDir($"{BasePath}/GameConfig");
        string path = $"{dir}/GameConfig.asset";

        var asset = AssetDatabase.LoadAssetAtPath<GameConfigSO>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<GameConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.shapeDMax = 0.35f;
        asset.shapeWeights = new float[] { 0.15f, 0.15f, 0.30f, 0.30f, 0.10f };
        asset.glazeDMax = 0.02f;
        asset.shapeWeight = 0.35f;
        asset.glazeWeight = 0.25f;
        asset.fireWeight = 0.40f;
        asset.gradeS_Min = 95f;
        asset.gradeA_Min = 85f;
        asset.gradeB_Min = 70f;
        asset.gradeC_Min = 50f;
        asset.gradeD_Min = 30f;
        asset.qualityGoldMultipliers = new float[] { 2.0f, 1.5f, 1.2f, 1.0f, 0.5f, 0.0f };
        asset.qualityRepMultipliers = new float[] { 1.5f, 1.2f, 1.0f, 0.8f, 0.3f, 0.0f };
        asset.orderRepMultipliers = new float[] { 1.5f, 1.2f, 1.0f, 0.0f };
        asset.difficultyGoldMultipliers = new float[] { 1.0f, 1.2f, 1.5f, 1.8f, 2.2f };
        asset.difficultyRepMultipliers = new float[] { 1.0f, 1.1f, 1.3f, 1.5f, 1.8f };
        asset.goldMax = 9999999;
        asset.repMax = 999999;

        EditorUtility.SetDirty(asset);
    }

    // ─── OrderData × 3 ──────────────────────────────────────────

    private static void CreateOrderDataAssets()
    {
        string dir = EnsureDir($"{BasePath}/Orders");

        CreateOrder(dir, "Order_QingYuWan", "青釉碗", "ORDER_001", "SHAPE_001", "GLAZE_001", 1, 100, 10);
        CreateOrder(dir, "Order_BaiYuWan", "白釉碗", "ORDER_002", "SHAPE_001", "GLAZE_002", 2, 120, 12);
        CreateOrder(dir, "Order_JiHongWan", "祭红碗", "ORDER_003", "SHAPE_001", "GLAZE_004", 3, 180, 18);
    }

    private static void CreateOrder(string dir, string fileName, string name, string orderID,
        string shapeID, string glazeID, int diff, int gold, int rep)
    {
        string path = $"{dir}/{fileName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<OrderData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<OrderData>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.orderName = name;
        asset.orderID = orderID;
        asset.requiredShapeID = shapeID;
        asset.requiredGlazeID = glazeID;
        asset.difficulty = diff;
        asset.baseGold = gold;
        asset.baseReputation = rep;
        EditorUtility.SetDirty(asset);
    }

    // ─── Legacy ─────────────────────────────────────────────────

    private static void MoveLegacyAssets()
    {
        string legacyDir = EnsureDir($"{BasePath}/Legacy");

        TryMove($"{BasePath}/ShapeRecipes/Shape_Bowl.asset", $"{legacyDir}/Shape_Bowl.asset");
        TryMove($"{BasePath}/GlazeRecipes/Glaze_Qingyu.asset", $"{legacyDir}/Glaze_Qingyu.asset");
        TryMove($"{BasePath}/GlazeRecipes/Glaze_Baiyu.asset", $"{legacyDir}/Glaze_Baiyu.asset");
        TryMove($"{BasePath}/GlazeRecipes/Glaze_Jihong.asset", $"{legacyDir}/Glaze_Jihong.asset");
    }

    private static void TryMove(string src, string dst)
    {
        if (System.IO.File.Exists(System.IO.Path.GetFullPath(src)) && !System.IO.File.Exists(System.IO.Path.GetFullPath(dst)))
        {
            AssetDatabase.MoveAsset(src, dst);
        }
    }

    // ─── Utility ────────────────────────────────────────────────

    private static string EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
        return path;
    }
}
