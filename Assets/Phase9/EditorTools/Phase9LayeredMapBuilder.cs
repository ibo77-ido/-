using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Phase9LayeredMapBuilder
{
    private const string ScenePath = "Assets/Phase9/Scenes/Phase9_MapPrototype.unity";
    private const string GeneratedFolder = "Assets/Phase9/Generated";
    private const string BaseGroundGeneratedPath = GeneratedFolder + "/Phase9_BaseGround_From_SourceSprites.png";
    private const string ConstructionRootName = "Phase9_TileConstruction";
    private const string BaseLayerName = "01_BaseGround_GeneratedFromSourceSprites";
    private const string TilemapPlainRootName = "01_BaseGround_TilemapGrassPlain";
    private const string ReferenceRootName = "Phase9_GeneratedMap";

    private static readonly string BaseReferencePath = "Assets/地图素材/地图分层/1地基层.png";

    private static readonly string[] GrassPaths =
    {
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Grass_01.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Grass_02.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Grass_03.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Grass_04.png"
    };

    private static readonly string[] MudPaths =
    {
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Mud_01.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Mud_02.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Mud_03.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Mud_04.png"
    };

    private static readonly string[] StonePaths =
    {
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Stone_01.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Stone_02.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Stone_03.png",
        "Assets/地图素材/地表基础 Tile（Ground Tiles）/T_Stone_04.png"
    };

    [MenuItem("Phase9/Build/01 Base Ground From Source Sprites")]
    public static void BuildBaseGroundOnly()
    {
        EnsureFolder(GeneratedFolder);
        Texture2D generated = GenerateBaseGroundTexture();
        SavePngAsset(generated, BaseGroundGeneratedPath, 100f);
        BuildBaseGroundSceneLayer();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Phase9 base ground rebuilt from source sprites and saved.");
    }

    [MenuItem("Phase9/Build/01 Tilemap Grass Plain Only")]
    public static void BuildTilemapGrassPlainOnly()
    {
        EnsureFolder(GeneratedFolder);
        EnsureFolder("Assets/Phase9/Tiles/Ground");
        OpenSceneIfNeeded();
        CreateSolidGrassTileAssets();

        GameObject construction = FindOrCreateRoot(ConstructionRootName);
        DestroyChild(construction.transform, BaseLayerName);
        DestroyChild(construction.transform, TilemapPlainRootName);
        DestroyChild(construction.transform, "01_BaseGround_Rebuilt_From_ScaleAware_Sprites");
        DestroyChild(construction.transform, "01_BaseGround_Rebuilt_From_ReferenceSampling");

        GameObject root = new GameObject(TilemapPlainRootName);
        root.transform.SetParent(construction.transform, false);

        Grid grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.24f, 0.24f, 0f);
        grid.cellLayout = GridLayout.CellLayout.Rectangle;
        grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

        GameObject mapObject = new GameObject("Tilemap_ContinuousGrassPlain_noise_mixed");
        mapObject.transform.SetParent(root.transform, false);
        Tilemap tilemap = mapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = mapObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 12;
        renderer.mode = TilemapRenderer.Mode.Chunk;

        TileBase[] tiles =
        {
            AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Phase9/Tiles/Ground/Phase9_SolidGrass_Light.asset"),
            AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Phase9/Tiles/Ground/Phase9_SolidGrass_Mid.asset"),
            AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Phase9/Tiles/Ground/Phase9_SolidGrass_Deep.asset")
        };

        int minX = -27;
        int maxX = 27;
        int minY = -14;
        int maxY = 8;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float n = (ValueNoise01(x / 4, y / 4, 71) - 0.5f) * 0.08f;
                float band = Mathf.Sin((x * 0.11f) + (y * 0.31f)) * 0.18f;
                float valley = Mathf.Sin((x * -0.06f) + (y * 0.17f) + 1.7f) * 0.12f;
                float heightBias = Mathf.InverseLerp(minY, maxY, y) * -0.08f;
                int index = 1;
                float score = 0.5f + n + band + valley + heightBias;
                if (score > 0.66f) index = 0;
                if (score < 0.34f) index = 2;
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[index]);
            }
        }

        tilemap.CompressBounds();
        AddMountains(root.transform);
        ConfigureReferenceLayer();
        ConfigureCamera();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Phase9 tilemap grass plain rebuilt with noise-mixed solid grass tiles.");
    }

    public static void BuildBaseGroundSceneLayer()
    {
        OpenSceneIfNeeded();

        GameObject construction = FindOrCreateRoot(ConstructionRootName);
        DestroyChild(construction.transform, "01_BaseGround_Rebuilt_From_ScaleAware_Sprites");
        DestroyChild(construction.transform, "01_BaseGround_Rebuilt_From_ReferenceSampling");
        DestroyChild(construction.transform, BaseLayerName);
        DestroyChild(construction.transform, "HeightLines_ShadowAndRidgeOverlay_2_5D");

        GameObject layer = new GameObject(BaseLayerName);
        layer.transform.SetParent(construction.transform, false);

        Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BaseGroundGeneratedPath);
        if (baseSprite == null)
        {
            throw new InvalidOperationException("Generated base sprite missing: " + BaseGroundGeneratedPath);
        }

        AddSprite(layer.transform, "Generated_BaseGround_Texture_not_reference_png", baseSprite, Vector3.zero, Vector3.one, 10, Color.white, 0f);
        AddMountains(layer.transform);
        ConfigureReferenceLayer();
        ConfigureCamera();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void CreateSolidGrassTileAssets()
    {
        CreateSolidGrassTile("Phase9_SolidGrass_Light", new Color(0.66f, 0.80f, 0.46f, 1f), new Color(0.74f, 0.86f, 0.54f, 1f), 11);
        CreateSolidGrassTile("Phase9_SolidGrass_Mid", new Color(0.56f, 0.72f, 0.40f, 1f), new Color(0.66f, 0.80f, 0.48f, 1f), 23);
        CreateSolidGrassTile("Phase9_SolidGrass_Deep", new Color(0.46f, 0.64f, 0.36f, 1f), new Color(0.56f, 0.72f, 0.42f, 1f), 37);
    }

    private static void CreateSolidGrassTile(string name, Color low, Color high, int salt)
    {
        string texturePath = GeneratedFolder + "/" + name + ".png";
        string tilePath = "Assets/Phase9/Tiles/Ground/" + name + ".asset";
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float n = ValueNoise01(x / 4, y / 4, salt);
                float softBand = 0.5f + Mathf.Sin(x * 0.10f + y * 0.055f + salt) * 0.08f;
                Color color = Color.Lerp(low, high, Mathf.Clamp01(n * 0.35f + softBand * 0.65f));
                color.a = 1f;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        SavePngAsset(texture, texturePath, 64f);

        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, tilePath);
        }

        tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        tile.color = Color.white;
        tile.colliderType = Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();
    }

    private static Texture2D GenerateBaseGroundTexture()
    {
        Texture2D reference = LoadPng(BaseReferencePath);
        int width = reference.width;
        int height = reference.height;
        Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color rc = reference.GetPixel(x, y);
                pixels[y * width + x] = ClassifyBaseColor(rc, x, y, width, height);
            }
        }

        output.SetPixels32(pixels);
        output.Apply(false, false);

        PaintHeightAndSlopeStructure(output);
        PaintOutLayerOneNonGroundArtifacts(output);
        output.Apply(false, false);
        return output;
    }

    private static Texture2D GenerateHeightLineTexture(int width, int height)
    {
        Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] clear = new Color32[width * height];
        for (int i = 0; i < clear.Length; i++)
        {
            clear[i] = new Color32(0, 0, 0, 0);
        }
        output.SetPixels32(clear);

        DrawStroke(output, new Vector2[] { WorldToPixel(-5.7f, 1.85f, output), WorldToPixel(-4.9f, 2.45f, output), WorldToPixel(-3.8f, 2.10f, output), WorldToPixel(-2.4f, 2.38f, output), WorldToPixel(-1.0f, 2.22f, output) }, 16f, new Color(0.15f, 0.30f, 0.22f, 0.18f));
        DrawStroke(output, new Vector2[] { WorldToPixel(-1.2f, 1.88f, output), WorldToPixel(0.6f, 2.10f, output), WorldToPixel(1.7f, 2.88f, output), WorldToPixel(3.0f, 2.25f, output), WorldToPixel(4.6f, 2.08f, output) }, 18f, new Color(0.14f, 0.28f, 0.22f, 0.20f));
        DrawStroke(output, new Vector2[] { WorldToPixel(0.0f, 1.72f, output), WorldToPixel(0.9f, 1.92f, output), WorldToPixel(1.8f, 2.48f, output), WorldToPixel(2.5f, 1.76f, output) }, 13f, new Color(0.30f, 0.22f, 0.17f, 0.13f));

        DrawStroke(output, new Vector2[] { WorldToPixel(-5.4f, 0.55f, output), WorldToPixel(-4.2f, 0.20f, output), WorldToPixel(-3.0f, 0.55f, output), WorldToPixel(-1.8f, 0.08f, output), WorldToPixel(-0.4f, 0.35f, output) }, 17f, new Color(0.24f, 0.42f, 0.28f, 0.10f));
        DrawStroke(output, new Vector2[] { WorldToPixel(0.2f, 0.36f, output), WorldToPixel(1.4f, -0.05f, output), WorldToPixel(2.9f, 0.38f, output), WorldToPixel(4.4f, 0.08f, output), WorldToPixel(5.2f, 0.46f, output) }, 17f, new Color(0.24f, 0.42f, 0.28f, 0.10f));
        DrawStroke(output, new Vector2[] { WorldToPixel(-5.3f, -1.30f, output), WorldToPixel(-4.2f, -1.68f, output), WorldToPixel(-2.5f, -1.18f, output), WorldToPixel(-1.2f, -1.72f, output), WorldToPixel(0.4f, -1.28f, output) }, 22f, new Color(0.22f, 0.38f, 0.25f, 0.10f));
        DrawStroke(output, new Vector2[] { WorldToPixel(0.8f, -1.50f, output), WorldToPixel(2.0f, -1.86f, output), WorldToPixel(3.3f, -1.32f, output), WorldToPixel(4.9f, -1.68f, output) }, 22f, new Color(0.22f, 0.38f, 0.25f, 0.10f));

        DrawStroke(output, new Vector2[] { WorldToPixel(-5.2f, -2.70f, output), WorldToPixel(-4.2f, -2.50f, output), WorldToPixel(-3.0f, -2.88f, output), WorldToPixel(-1.5f, -2.45f, output) }, 18f, new Color(0.65f, 0.48f, 0.32f, 0.10f));
        DrawStroke(output, new Vector2[] { WorldToPixel(-0.8f, -2.38f, output), WorldToPixel(0.7f, -2.78f, output), WorldToPixel(2.0f, -2.38f, output), WorldToPixel(3.6f, -2.70f, output) }, 18f, new Color(0.28f, 0.44f, 0.28f, 0.10f));

        output.Apply(false, false);
        return output;
    }

    private static Color ClassifyBaseColor(Color rc, int x, int y, int width, int height)
    {
        float brightness = (rc.r + rc.g + rc.b) / 3f;
        float worldX = (x / 100f) - (width / 200f);
        float worldY = (y / 100f) - (height / 200f);
        bool skyLike = worldY > 2.25f && rc.b > rc.g && brightness > 0.70f;
        if (skyLike)
        {
            return new Color(0f, 0f, 0f, 0f);
        }

        float broadNoise = 0.5f + Mathf.Sin(x * 0.018f + y * 0.011f) * 0.18f + Mathf.Sin(x * -0.009f + y * 0.027f) * 0.12f;
        broadNoise = Mathf.Clamp01(broadNoise);
        bool mountainLike = worldY > 1.18f || brightness < 0.48f;
        bool earthLike = rc.r > 0.56f && rc.g > 0.48f && (rc.r - rc.b) > 0.10f && rc.r >= rc.g * 0.88f;

        Color grassLow = new Color(0.40f, 0.60f, 0.30f, 0.66f);
        Color grassHigh = new Color(0.72f, 0.86f, 0.48f, 0.76f);
        Color earthLow = new Color(0.58f, 0.56f, 0.36f, 0.68f);
        Color earthHigh = new Color(0.84f, 0.76f, 0.50f, 0.74f);
        Color mountainLow = new Color(0.24f, 0.43f, 0.34f, 0.78f);
        Color mountainHigh = new Color(0.56f, 0.72f, 0.42f, 0.82f);

        Color baseColor = Color.Lerp(grassLow, grassHigh, Mathf.Clamp01(rc.g * 0.72f + broadNoise * 0.22f));
        if (earthLike)
        {
            baseColor = Color.Lerp(baseColor, Color.Lerp(earthLow, earthHigh, Mathf.Clamp01(rc.r * 0.55f + broadNoise * 0.25f)), 0.45f);
        }
        if (mountainLike)
        {
            float mountainBlend = Mathf.Clamp01((worldY - 0.92f) * 0.85f + (0.54f - brightness) * 0.90f);
            baseColor = Color.Lerp(baseColor, Color.Lerp(mountainLow, mountainHigh, Mathf.Clamp01(rc.g * 0.55f + broadNoise * 0.20f)), Mathf.Lerp(0.48f, 0.82f, mountainBlend));
        }

        float edgeFade = Mathf.SmoothStep(0f, 0.45f, Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y)) / 100f);
        baseColor.a = Mathf.Max(baseColor.a, 0.96f) * Mathf.Lerp(0.94f, 1f, edgeFade);
        Color mixed = Color.Lerp(baseColor, new Color(rc.r, rc.g, rc.b, 1f), 0.06f);
        baseColor.r = mixed.r;
        baseColor.g = mixed.g;
        baseColor.b = mixed.b;
        return baseColor;
    }

    private static void PaintHeightAndSlopeStructure(Texture2D output)
    {
        // Broad, hand-placed height bands copy the reference layout: top mountain mass,
        // middle green slopes, pale valley, and lower foreground hills.
        DrawRotatedEllipse(output, WorldToPixel(-4.9f, 2.05f, output), 160f, 58f, 18f, new Color(0.25f, 0.46f, 0.32f, 0.46f));
        DrawRotatedEllipse(output, WorldToPixel(-2.2f, 2.35f, output), 210f, 68f, 4f, new Color(0.30f, 0.52f, 0.34f, 0.40f));
        DrawRotatedEllipse(output, WorldToPixel(1.9f, 2.20f, output), 260f, 78f, -12f, new Color(0.24f, 0.44f, 0.32f, 0.50f));
        DrawRotatedEllipse(output, WorldToPixel(4.6f, 2.05f, output), 220f, 78f, -20f, new Color(0.28f, 0.50f, 0.34f, 0.42f));

        DrawRotatedEllipse(output, WorldToPixel(-4.3f, 0.70f, output), 240f, 55f, -18f, new Color(0.32f, 0.55f, 0.34f, 0.30f));
        DrawRotatedEllipse(output, WorldToPixel(-1.2f, 0.28f, output), 300f, 66f, 12f, new Color(0.36f, 0.60f, 0.36f, 0.30f));
        DrawRotatedEllipse(output, WorldToPixel(2.6f, 0.55f, output), 260f, 58f, -15f, new Color(0.34f, 0.57f, 0.35f, 0.28f));
        DrawRotatedEllipse(output, WorldToPixel(4.4f, -0.30f, output), 240f, 55f, 14f, new Color(0.32f, 0.54f, 0.33f, 0.30f));

        DrawRotatedEllipse(output, WorldToPixel(-2.9f, -0.58f, output), 250f, 62f, 18f, new Color(0.82f, 0.78f, 0.50f, 0.22f));
        DrawRotatedEllipse(output, WorldToPixel(0.2f, -1.40f, output), 330f, 70f, -8f, new Color(0.82f, 0.77f, 0.48f, 0.24f));
        DrawRotatedEllipse(output, WorldToPixel(3.0f, -1.68f, output), 280f, 66f, 16f, new Color(0.46f, 0.66f, 0.36f, 0.26f));
        DrawRotatedEllipse(output, WorldToPixel(-4.4f, -2.70f, output), 250f, 58f, 7f, new Color(0.82f, 0.72f, 0.48f, 0.24f));

        DrawRotatedEllipse(output, WorldToPixel(-4.0f, -1.55f, output), 240f, 44f, -24f, new Color(0.30f, 0.52f, 0.30f, 0.30f));
        DrawRotatedEllipse(output, WorldToPixel(-1.0f, -0.92f, output), 310f, 46f, -16f, new Color(0.34f, 0.56f, 0.32f, 0.28f));
        DrawRotatedEllipse(output, WorldToPixel(2.6f, -0.82f, output), 280f, 44f, 14f, new Color(0.34f, 0.56f, 0.32f, 0.26f));
        DrawRotatedEllipse(output, WorldToPixel(4.9f, -1.15f, output), 230f, 44f, 22f, new Color(0.28f, 0.48f, 0.30f, 0.32f));
        DrawRotatedEllipse(output, WorldToPixel(-2.8f, -2.05f, output), 280f, 42f, 10f, new Color(0.88f, 0.80f, 0.54f, 0.24f));
        DrawRotatedEllipse(output, WorldToPixel(0.8f, -2.42f, output), 320f, 46f, -8f, new Color(0.86f, 0.78f, 0.52f, 0.22f));
        DrawRotatedEllipse(output, WorldToPixel(3.8f, -2.36f, output), 240f, 42f, 10f, new Color(0.38f, 0.58f, 0.32f, 0.24f));

        DrawRotatedEllipse(output, WorldToPixel(-4.6f, -0.18f, output), 220f, 34f, 17f, new Color(0.86f, 0.80f, 0.56f, 0.30f));
        DrawRotatedEllipse(output, WorldToPixel(-2.7f, 0.72f, output), 180f, 28f, 22f, new Color(0.88f, 0.82f, 0.58f, 0.26f));
        DrawRotatedEllipse(output, WorldToPixel(-0.4f, 0.58f, output), 230f, 34f, -4f, new Color(0.84f, 0.78f, 0.54f, 0.24f));
        DrawRotatedEllipse(output, WorldToPixel(2.8f, 0.85f, output), 210f, 32f, -16f, new Color(0.84f, 0.78f, 0.54f, 0.26f));
        DrawRotatedEllipse(output, WorldToPixel(4.2f, 0.12f, output), 160f, 28f, 17f, new Color(0.86f, 0.80f, 0.56f, 0.24f));

        DrawRotatedEllipse(output, WorldToPixel(0.2f, 1.95f, output), 250f, 32f, -7f, new Color(0.48f, 0.38f, 0.28f, 0.34f));
        DrawRotatedEllipse(output, WorldToPixel(2.2f, 2.35f, output), 210f, 35f, -22f, new Color(0.44f, 0.34f, 0.27f, 0.38f));
        DrawRotatedEllipse(output, WorldToPixel(-4.8f, 1.40f, output), 180f, 30f, -30f, new Color(0.24f, 0.38f, 0.28f, 0.32f));
    }

    private static void PaintOutLayerOneNonGroundArtifacts(Texture2D output)
    {
        // Layer 1 should be an empty valley floor. Stone platforms, stone rings,
        // rocks, structures, and object-like marks belong to later reference layers.
        DrawRotatedEllipse(output, WorldToPixel(5.20f, 1.78f, output), 210f, 92f, -12f, new Color(0.50f, 0.68f, 0.40f, 0.78f));
        DrawRotatedEllipse(output, WorldToPixel(5.82f, 1.60f, output), 160f, 78f, -12f, new Color(0.48f, 0.66f, 0.38f, 0.72f));
        DrawRotatedEllipse(output, WorldToPixel(2.45f, -0.52f, output), 132f, 88f, -8f, new Color(0.56f, 0.72f, 0.42f, 0.76f));
        DrawRotatedEllipse(output, WorldToPixel(2.62f, -0.42f, output), 108f, 64f, -8f, new Color(0.60f, 0.74f, 0.44f, 0.68f));
    }

    private static void StampSourceTextures(Texture2D output, Texture2D reference, Texture2D[] grass, Texture2D[] mud, Texture2D[] stone)
    {
        int stampIndex = 0;
        for (int y = 110; y < output.height - 165; y += 190)
        {
            int row = y / 86;
            for (int x = 110 + ((row % 2) * 95); x < output.width - 100; x += 220)
            {
                Color rc = reference.GetPixel(x, y);
                float brightness = (rc.r + rc.g + rc.b) / 3f;
                float worldY = (y / 100f) - (output.height / 200f);
                bool skyLike = (rc.b > rc.g && brightness > 0.67f) || (brightness > 0.88f && worldY > 1.0f);
                if (skyLike)
                {
                    continue;
                }

                bool mudLike = rc.r > 0.66f && rc.g > 0.57f && (rc.r - rc.b) > 0.20f && rc.r >= rc.g * 0.98f;
                Texture2D source = mudLike ? mud[stampIndex % mud.Length] : grass[stampIndex % grass.Length];
                float scale = mudLike ? 0.30f : 0.68f;
                Color tint = mudLike ? new Color(1f, 0.88f, 0.68f, 0.035f) : new Color(0.92f, 1f, 0.78f, 0.045f);
                Stamp(output, source, x + Mathf.RoundToInt((ValueNoise01(row, stampIndex, 3) - 0.5f) * 40f), y + Mathf.RoundToInt((ValueNoise01(stampIndex, row, 5) - 0.5f) * 32f), scale, tint);
                stampIndex++;
            }
        }

        StampManualMud(output, mud);
        StampManualStone(output, stone);
    }

    private static void StampManualMud(Texture2D output, Texture2D[] mud)
    {
        Vector2[] points =
        {
            WorldToPixel(-4.5f, -2.8f, output), WorldToPixel(-3.2f, 0.25f, output), WorldToPixel(-1.5f, -0.8f, output),
            WorldToPixel(0.2f, 0.8f, output), WorldToPixel(1.0f, -1.55f, output), WorldToPixel(3.45f, 0.7f, output),
            WorldToPixel(4.5f, -0.05f, output), WorldToPixel(4.8f, 1.25f, output)
        };

        for (int i = 0; i < points.Length; i++)
        {
            Stamp(output, mud[i % mud.Length], Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y), 0.30f, new Color(1f, 0.88f, 0.66f, 0.26f));
        }
    }

    private static void StampManualStone(Texture2D output, Texture2D[] stone)
    {
        Vector2[] points =
        {
            WorldToPixel(4.85f, 2.05f, output), WorldToPixel(5.55f, 1.78f, output), WorldToPixel(6.02f, 1.62f, output)
        };

        for (int i = 0; i < points.Length; i++)
        {
            Stamp(output, stone[i % stone.Length], Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y), 0.95f, new Color(1f, 1f, 1f, 0.82f));
        }
    }

    private static void Stamp(Texture2D target, Texture2D source, int centerX, int centerY, float scale, Color tint)
    {
        int stampW = Mathf.Max(8, Mathf.RoundToInt(source.width * scale));
        int stampH = Mathf.Max(8, Mathf.RoundToInt(source.height * scale));
        int startX = centerX - stampW / 2;
        int startY = centerY - stampH / 2;

        for (int y = 0; y < stampH; y++)
        {
            int ty = startY + y;
            if (ty < 0 || ty >= target.height) continue;
            float sv = y / Mathf.Max(1f, stampH - 1f);
            for (int x = 0; x < stampW; x++)
            {
                int tx = startX + x;
                if (tx < 0 || tx >= target.width) continue;
                float su = x / Mathf.Max(1f, stampW - 1f);
                Color sc = source.GetPixelBilinear(su, sv) * tint;
                if (sc.a < 0.01f) continue;
                Color dc = target.GetPixel(tx, ty);
                Color blended = Color.Lerp(dc, new Color(sc.r, sc.g, sc.b, Mathf.Max(dc.a, sc.a)), sc.a);
                blended.a = Mathf.Max(dc.a, sc.a);
                target.SetPixel(tx, ty, blended);
            }
        }
    }

    private static void DrawRotatedEllipse(Texture2D target, Vector2 center, float radiusX, float radiusY, float degrees, Color color)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - Mathf.Max(radiusX, radiusY) - 2f));
        int maxX = Mathf.Min(target.width - 1, Mathf.CeilToInt(center.x + Mathf.Max(radiusX, radiusY) + 2f));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - Mathf.Max(radiusX, radiusY) - 2f));
        int maxY = Mathf.Min(target.height - 1, Mathf.CeilToInt(center.y + Mathf.Max(radiusX, radiusY) + 2f));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float rx = dx * cos + dy * sin;
                float ry = -dx * sin + dy * cos;
                float d = (rx * rx) / (radiusX * radiusX) + (ry * ry) / (radiusY * radiusY);
                if (d > 1f) continue;

                float feather = Mathf.SmoothStep(1f, 0f, d);
                Color source = target.GetPixel(x, y);
                Color blended = Color.Lerp(source, color, color.a * feather);
                blended.a = Mathf.Max(source.a, color.a * feather);
                target.SetPixel(x, y, blended);
            }
        }
    }

    private static void DrawStroke(Texture2D target, Vector2[] points, float radius, Color color)
    {
        if (points == null || points.Length < 2) return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[i + 1];
            float distance = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(1f, radius * 0.45f)));
            for (int step = 0; step <= steps; step++)
            {
                Vector2 p = Vector2.Lerp(a, b, step / (float)steps);
                DrawSoftCircle(target, p, radius, color);
            }
        }
    }

    private static void DrawSoftCircle(Texture2D target, Vector2 center, float radius, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius - 2f));
        int maxX = Mathf.Min(target.width - 1, Mathf.CeilToInt(center.x + radius + 2f));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius - 2f));
        int maxY = Mathf.Min(target.height - 1, Mathf.CeilToInt(center.y + radius + 2f));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / Mathf.Max(1f, radius);
                if (d > 1f) continue;

                float feather = Mathf.SmoothStep(1f, 0f, d);
                Color source = target.GetPixel(x, y);
                Color blended = Color.Lerp(source, color, color.a * feather);
                blended.a = Mathf.Max(source.a, color.a * feather);
                target.SetPixel(x, y, blended);
            }
        }
    }

    private static void AddMountains(Transform layer)
    {
        Transform group = new GameObject("MountainBoundary_large_base_sprites").transform;
        group.SetParent(layer, false);
        AddSprite(group, "top_far_mountain_back_reference_band", LoadSprite("Assets/地图素材/天空与山体/Mountain_Back.png"), new Vector3(-0.15f, 2.82f, 0f), new Vector3(0.46f, 0.26f, 1f), 28, new Color(0.62f, 0.80f, 0.65f, 0.54f), 0f);
        AddSprite(group, "top_mid_mountain_ridge_reference_band", LoadSprite("Assets/地图素材/天空与山体/Mountain_Mid.png"), new Vector3(0.25f, 2.52f, 0f), new Vector3(0.48f, 0.26f, 1f), 29, new Color(0.58f, 0.76f, 0.62f, 0.52f), 0f);
        AddSprite(group, "bottom_left_mountain_edge", LoadSprite("Assets/地图素材/天空与山体/Mountain_Foreground.png"), new Vector3(-5.95f, -3.58f, 0f), new Vector3(0.18f, 0.17f, 1f), 29, new Color(0.58f, 0.74f, 0.66f, 0.48f), 0f);
        AddSprite(group, "bottom_right_mountain_edge", LoadSprite("Assets/地图素材/天空与山体/Mountain_Foreground.png"), new Vector3(5.95f, -3.58f, 0f), new Vector3(-0.18f, 0.17f, 1f), 29, new Color(0.58f, 0.74f, 0.66f, 0.48f), 0f);
    }

    private static void AddPlainsAndSlopeTerrain(Transform layer)
    {
        Transform group = new GameObject("PlainAndSlopeTerrain_reference_layer1_only").transform;
        group.SetParent(layer, false);
        Sprite[] grass = LoadSprites(GrassPaths);
        Sprite[] mud = LoadSprites(MudPaths);

        Vector3[] grassPos =
        {
            new Vector3(-4.7f, 0.68f, 0f), new Vector3(-3.0f, 0.28f, 0f), new Vector3(-1.2f, 0.48f, 0f),
            new Vector3(0.7f, 0.14f, 0f), new Vector3(2.6f, 0.42f, 0f), new Vector3(4.35f, 0.05f, 0f),
            new Vector3(-4.5f, -1.10f, 0f), new Vector3(-2.6f, -1.58f, 0f), new Vector3(-0.5f, -1.18f, 0f),
            new Vector3(1.6f, -1.58f, 0f), new Vector3(3.55f, -1.18f, 0f), new Vector3(4.9f, -1.78f, 0f)
        };
        Vector3[] grassScale =
        {
            new Vector3(1.20f, 0.34f, 1f), new Vector3(1.18f, 0.30f, 1f), new Vector3(1.25f, 0.34f, 1f),
            new Vector3(1.28f, 0.32f, 1f), new Vector3(1.18f, 0.30f, 1f), new Vector3(1.16f, 0.30f, 1f),
            new Vector3(1.22f, 0.34f, 1f), new Vector3(1.30f, 0.34f, 1f), new Vector3(1.32f, 0.36f, 1f),
            new Vector3(1.25f, 0.34f, 1f), new Vector3(1.20f, 0.32f, 1f), new Vector3(1.12f, 0.30f, 1f)
        };
        float[] grassRot = { -18f, 12f, -8f, 8f, -14f, 14f, 18f, -12f, 10f, -9f, 12f, -15f };

        for (int i = 0; i < grassPos.Length; i++)
        {
            Color tint = (i % 3 == 0)
                ? new Color(0.54f, 0.72f, 0.40f, 0.20f)
                : new Color(0.74f, 0.84f, 0.50f, 0.18f);
            AddSprite(group, "slope_grass_mass_" + i.ToString("00"), grass[i % grass.Length], grassPos[i], grassScale[i], 24 + (i % 2), tint, grassRot[i]);
        }

        Vector3[] mudPos =
        {
            new Vector3(-4.35f, -0.10f, 0f), new Vector3(-2.70f, 0.82f, 0f), new Vector3(-0.10f, 0.72f, 0f),
            new Vector3(2.88f, 0.78f, 0f), new Vector3(4.12f, 0.08f, 0f), new Vector3(-3.10f, -2.10f, 0f),
            new Vector3(0.35f, -2.28f, 0f)
        };
        Vector3[] mudScale =
        {
            new Vector3(0.26f, 0.070f, 1f), new Vector3(0.20f, 0.055f, 1f), new Vector3(0.25f, 0.060f, 1f),
            new Vector3(0.23f, 0.060f, 1f), new Vector3(0.18f, 0.052f, 1f), new Vector3(0.28f, 0.065f, 1f),
            new Vector3(0.34f, 0.075f, 1f)
        };
        float[] mudRot = { 16f, 20f, -4f, -16f, 18f, 10f, -8f };
        for (int i = 0; i < mudPos.Length; i++)
        {
            AddSprite(group, "plain_mud_wash_" + i.ToString("00"), mud[i % mud.Length], mudPos[i], mudScale[i], 30, new Color(0.88f, 0.76f, 0.50f, 0.24f), mudRot[i]);
        }
    }

    private static void AddReferenceAlignedAccents(Transform layer)
    {
        Transform group = new GameObject("IndependentStoneAndGroundAccents").transform;
        group.SetParent(layer, false);
        Sprite[] stone = LoadSprites(StonePaths);
        Sprite[] mud = LoadSprites(MudPaths);

        Vector3[] platformPos = { new Vector3(4.85f, 2.05f, 0f), new Vector3(5.55f, 1.78f, 0f), new Vector3(6.02f, 1.62f, 0f) };
        for (int i = 0; i < platformPos.Length; i++)
        {
            AddSprite(group, "shadow_upper_right_foundation_stone_" + (i + 1), stone[i % stone.Length], platformPos[i] + new Vector3(0.08f, -0.08f, 0f), new Vector3(0.82f, 0.72f, 1f), 39, new Color(0.10f, 0.12f, 0.10f, 0.16f), -10f + i * 8f);
            AddSprite(group, "upper_right_foundation_stone_" + (i + 1), stone[i % stone.Length], platformPos[i], new Vector3(0.82f, 0.72f, 1f), 45, new Color(0.88f, 0.88f, 0.80f, 0.80f), -10f + i * 8f);
        }

        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI * 2f / 10f;
            Vector3 pos = new Vector3(2.46f + Mathf.Cos(angle) * 0.42f, -0.52f + Mathf.Sin(angle) * 0.27f, 0f);
            AddSprite(group, "shadow_central_small_earth_ring_" + i, mud[i % mud.Length], pos + new Vector3(0.06f, -0.06f, 0f), new Vector3(0.085f, 0.060f, 1f), 40, new Color(0.10f, 0.10f, 0.08f, 0.10f), Mathf.Rad2Deg * angle);
            AddSprite(group, "central_small_earth_ring_" + i, mud[i % mud.Length], pos, new Vector3(0.085f, 0.060f, 1f), 43, new Color(0.76f, 0.68f, 0.48f, 0.60f), Mathf.Rad2Deg * angle);
        }
    }

    private static void ConfigureReferenceLayer()
    {
        GameObject referenceRoot = GameObject.Find(ReferenceRootName);
        if (referenceRoot == null) return;
        referenceRoot.SetActive(false);

        SpriteRenderer[] renderers = referenceRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.gameObject.SetActive(renderer.gameObject.name == "REF_01_1地基层");
            if (renderer.gameObject.name == "REF_01_1地基层")
            {
                renderer.color = new Color(1f, 1f, 1f, 0f);
                renderer.sortingOrder = 900;
            }
        }
    }

    private static void ConfigureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        camera.orthographic = true;
        camera.orthographicSize = 4.45f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.58f, 0.70f, 0.73f, 1f);
    }

    private static void OpenSceneIfNeeded()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            throw new InvalidOperationException("Missing sprite: " + path);
        }
        return sprite;
    }

    private static Sprite[] LoadSprites(string[] paths)
    {
        Sprite[] sprites = new Sprite[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            sprites[i] = LoadSprite(paths[i]);
        }
        return sprites;
    }

    private static Texture2D[] LoadAll(string[] paths)
    {
        Texture2D[] textures = new Texture2D[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            textures[i] = LoadPng(paths[i]);
        }
        return textures;
    }

    private static Texture2D LoadPng(string path)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path.Replace('/', Path.DirectorySeparatorChar));
        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        if (!ImageConversion.LoadImage(texture, bytes))
        {
            throw new InvalidOperationException("Could not load png: " + path);
        }
        return texture;
    }

    private static void SavePngAsset(Texture2D texture, string assetPath, float pixelsPerUnit)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing;
        return new GameObject(name);
    }

    private static void DestroyChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void AddSprite(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale, int order, Color color, float rotationZ)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        renderer.color = color;
    }

    private static Vector2 WorldToPixel(float x, float y, Texture2D texture)
    {
        return new Vector2(x * 100f + texture.width * 0.5f, y * 100f + texture.height * 0.5f);
    }

    private static float ValueNoise01(int x, int y, int salt)
    {
        unchecked
        {
            int n = x * 73856093 ^ y * 19349663 ^ salt * 83492791;
            n = (n << 13) ^ n;
            return Mathf.Clamp01(1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f);
        }
    }
}
