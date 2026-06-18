using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Phase9XZSceneNormalizer
{
    private const string ScenePath = "Assets/Phase9/Scenes/SampleScene.unity";
    private const string RequestPath = "Library/Phase9XZSceneNormalizer.request";
    private const string ReportPath = "Assets/Screenshots/phase9-xz-normalize-report.txt";
    private const string BakeMeshAssetPath = "Assets/Generated/NavMesh/NavMeshWalkableBakeMesh_FromSprite.asset";
    private const string BakeMeshObjectName = "NavMeshWalkableBakeMesh_FromSprite";
    private const string RoadLayerName = "Road";
    private const float DefaultNavY = 3.26f;

    static Phase9XZSceneNormalizer()
    {
        EditorApplication.delayCall += RunIfRequested;
    }

    [MenuItem("Phase9/Repair/Normalize XZ NavMesh Scene")]
    public static void NormalizeCurrentSceneFromMenu()
    {
        NormalizeScene(false);
    }

    public static void RunFromCommandLine()
    {
        bool success = NormalizeScene(true);
        EditorApplication.Exit(success ? 0 : 1);
    }

    private static void RunIfRequested()
    {
        if (!File.Exists(RequestPath))
        {
            return;
        }

        try
        {
            File.Delete(RequestPath);
        }
        catch (IOException)
        {
            Debug.LogWarning("[Phase9XZSceneNormalizer] Request marker could not be deleted; continuing.");
        }

        NormalizeScene(false);
    }

    private static bool NormalizeScene(bool commandLine)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("Phase9 XZ scene normalization report");
        report.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        try
        {
            Scene scene = OpenTargetScene();
            int roadLayer = EnsureLayer(RoadLayerName);
            float navY = ResolveNavPlaneY(scene);

            report.AppendLine("Scene: " + scene.path);
            report.AppendLine("Road layer: " + roadLayer);
            report.AppendLine("Nav plane Y: " + navY.ToString("F3"));

            int convertedCount = NormalizeKnownSceneObjects(scene, navY);
            int hiddenGuideCount = HideEditorGuideVisuals(scene);
            GameObject walkable = FindWalkableObject(scene);
            if (walkable == null)
            {
                throw new InvalidOperationException("Missing NavMesh-walkable.");
            }

            AlignWalkableToStaticLayer(scene, walkable, navY);
            EnsureWalkableVisualSource(walkable);
            Mesh walkableMesh = BuildMeshFromWalkableSprite(walkable);
            GameObject bakeMeshObject = EnsureBakeMeshObject(scene, walkable, walkableMesh, roadLayer);
            NavMeshSurface surface = EnsureNavMeshSurface(scene, roadLayer);
            BindPhase9RuntimeReferences(scene, navY);
            ConfigureMainCamera(scene, navY);

            string buildGeometry = BuildRoadNavMesh(surface, bakeMeshObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            ValidationResult validation = ValidateNavMesh(scene, walkable, bakeMeshObject, triangulation, navY);

            report.AppendLine("Converted objects: " + convertedCount);
            report.AppendLine("Hidden guide renderers: " + hiddenGuideCount);
            report.AppendLine("Bake mesh vertices: " + walkableMesh.vertexCount);
            report.AppendLine("Build geometry: " + buildGeometry);
            report.AppendLine("NavMesh vertices: " + triangulation.vertices.Length);
            report.AppendLine("NavMesh triangles: " + (triangulation.indices.Length / 3));
            report.AppendLine("NavMesh Y range: " + validation.MinY.ToString("F3") + " .. " + validation.MaxY.ToString("F3"));
            report.AppendLine("Nav vertices inside walkable sprite mesh: " + validation.VerticesInside + "/" + validation.TotalVertices);
            report.AppendLine("Outside probe leaks: " + validation.OutsideProbeLeaks);
            report.AppendLine("Player reachable entry points: " + validation.ReachableEntries + "/" + validation.TotalEntries);
            report.AppendLine("Result: " + (validation.Passed ? "PASS" : "CHECK_REQUIRED"));

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(ReportPath);

            Debug.Log("[Phase9XZSceneNormalizer] Completed.\n" + report);
            return validation.Passed || !commandLine;
        }
        catch (Exception ex)
        {
            report.AppendLine("ERROR: " + ex);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(ReportPath);
            Debug.LogError("[Phase9XZSceneNormalizer] Failed.\n" + report);
            return false;
        }
    }

    private static Scene OpenTargetScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() && active.path == ScenePath)
        {
            return active;
        }

        return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static int EnsureLayer(string layerName)
    {
        int existingLayer = LayerMask.NameToLayer(layerName);
        if (existingLayer >= 0)
        {
            return existingLayer;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue))
            {
                continue;
            }

            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return i;
        }

        throw new InvalidOperationException("No empty Unity layer slot is available for " + layerName + ".");
    }

    private static float ResolveNavPlaneY(Scene scene)
    {
        GameObject staticLayer = FindSceneObject(scene, "静态层");
        if (staticLayer != null && IsApproximatelyXZ(staticLayer.transform))
        {
            return staticLayer.transform.position.y;
        }

        GameObject walkable = FindWalkableObject(scene);
        if (walkable != null && IsApproximatelyXZ(walkable.transform))
        {
            return walkable.transform.position.y;
        }

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices != null && triangulation.vertices.Length > 0)
        {
            return triangulation.vertices[0].y;
        }

        return DefaultNavY;
    }

    private static int NormalizeKnownSceneObjects(Scene scene, float navY)
    {
        int count = 0;
        string[] flatSpriteObjects =
            {
                "静态层",
                "水体内部静态",
                "水体外部流动",
                "水体外部动态",
                "NavMesh-walkable (1)",
                "Walkable",
                "NavMesh_dev"
            };

        for (int i = 0; i < flatSpriteObjects.Length; i++)
        {
            GameObject obj = FindSceneObject(scene, flatSpriteObjects[i]);
            if (obj != null && NormalizeFlatSprite(obj.transform, navY, false))
            {
                count++;
            }
        }

        GameObject player = FindSceneObject(scene, "女主");
        if (player != null)
        {
            if (NormalizeActorOrMarker(player.transform, navY))
            {
                count++;
            }

            EnsurePlayerMovementComponents(player, navY);
            player.SetActive(true);
        }

        string[] interactionPoints =
        {
            "Order-interact",
            "Shape-interact",
            "Glaze-interact",
            "Kiln-interact",
            "Storage-interact",
            "Material-interact"
        };

        for (int i = 0; i < interactionPoints.Length; i++)
        {
            GameObject point = FindSceneObject(scene, interactionPoints[i]);
            if (point != null && NormalizeActorOrMarker(point.transform, navY))
            {
                count++;
            }
        }

        count += NormalizeContainerChildren(scene, navY);
        return count;
    }

    private static int NormalizeContainerChildren(Scene scene, float planeY)
    {
        int count = 0;
        string[] containerNames =
        {
            "tree",
            "Tree",
            "Bush",
            "Bamboo",
            "BamBoo",
            "bamboo",
            "Rock",
            "SignPost",
            "Flower"
        };

        for (int i = 0; i < containerNames.Length; i++)
        {
            GameObject container = FindSceneObject(scene, containerNames[i]);
            if (container == null)
            {
                continue;
            }

            for (int childIndex = 0; childIndex < container.transform.childCount; childIndex++)
            {
                Transform child = container.transform.GetChild(childIndex);
                if (NormalizePlanarChild(child, planeY))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool NormalizePlanarChild(Transform transform, float planeY)
    {
        if (transform == null)
        {
            return false;
        }

        if (IsApproximatelyXZ(transform) && Mathf.Abs(transform.position.y - planeY) < 0.2f)
        {
            return false;
        }

        Vector3 oldPosition = transform.position;
        float oldZRotation = transform.eulerAngles.z;
        transform.SetPositionAndRotation(
            new Vector3(oldPosition.x, planeY, oldPosition.y),
            Quaternion.Euler(90f, 0f, oldZRotation));
        return true;
    }

    private static bool NormalizeFlatSprite(Transform transform, float navY, bool forceNavY)
    {
        if (transform == null)
        {
            return false;
        }

        if (IsApproximatelyXZ(transform))
        {
            if (forceNavY && Mathf.Abs(transform.position.y - navY) > 0.001f)
            {
                Vector3 position = transform.position;
                position.y = navY;
                transform.position = position;
                return true;
            }

            return false;
        }

        Vector3 oldPosition = transform.position;
        float oldZRotation = transform.eulerAngles.z;
        transform.position = new Vector3(oldPosition.x, forceNavY ? navY : oldPosition.z, oldPosition.y);
        transform.rotation = Quaternion.Euler(90f, 0f, oldZRotation);
        return true;
    }

    private static void AlignWalkableToStaticLayer(Scene scene, GameObject walkable, float navY)
    {
        GameObject staticLayer = FindSceneObject(scene, "静态层");
        if (staticLayer == null)
        {
            NormalizeFlatSprite(walkable.transform, navY, true);
            return;
        }

        Transform source = staticLayer.transform;
        Transform target = walkable.transform;
        target.SetPositionAndRotation(source.position, source.rotation);
        target.localScale = source.lossyScale;
    }

    private static bool NormalizeActorOrMarker(Transform transform, float navY)
    {
        if (transform == null)
        {
            return false;
        }

        Vector3 oldPosition = transform.position;
        bool wasXZ = IsApproximatelyXZ(transform);
        Vector3 newPosition = wasXZ
            ? new Vector3(oldPosition.x, navY, oldPosition.z)
            : new Vector3(oldPosition.x, navY, oldPosition.y);

        bool changed = (newPosition - oldPosition).sqrMagnitude > 0.000001f;
        transform.position = newPosition;

        if (!wasXZ)
        {
            transform.rotation = Quaternion.Euler(90f, 0f, transform.eulerAngles.z);
            changed = true;
        }

        return changed;
    }

    private static bool IsApproximatelyXZ(Transform transform)
    {
        return Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, 90f)) < 2f;
    }

    private static bool ShouldSkipVisualConversion(GameObject obj)
    {
        if (obj == null)
        {
            return true;
        }

        if (obj.GetComponentInParent<Canvas>(true) != null)
        {
            return true;
        }

        if (obj.GetComponentInParent<Camera>(true) != null)
        {
            return true;
        }

        string name = obj.name;
        return name == BakeMeshObjectName
            || name == "GameplayCanvasRoot"
            || name == "EventSystem"
            || name.Contains("Camera")
            || name.Contains("Light");
    }

    private static int HideEditorGuideVisuals(Scene scene)
    {
        int hidden = 0;
        string[] guideObjects =
        {
            "NavMesh-walkable (1)",
            "Walkable",
            "NavMesh_dev",
            BakeMeshObjectName
        };

        for (int i = 0; i < guideObjects.Length; i++)
        {
            GameObject obj = FindSceneObject(scene, guideObjects[i]);
            if (obj == null)
            {
                continue;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                if (renderers[r].enabled)
                {
                    renderers[r].enabled = false;
                    hidden++;
                }
            }
        }

        return hidden;
    }

    private static void EnsureWalkableVisualSource(GameObject walkable)
    {
        SpriteRenderer renderer = walkable.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            throw new InvalidOperationException("NavMesh-walkable (1) must have a SpriteRenderer with a sprite.");
        }

        BoxCollider boxCollider = walkable.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = walkable.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;

        NavMeshModifier modifier = walkable.GetComponent<NavMeshModifier>();
        if (modifier == null)
        {
            modifier = walkable.AddComponent<NavMeshModifier>();
        }

        modifier.ignoreFromBuild = true;
    }

    private static Mesh BuildMeshFromWalkableSprite(GameObject walkable)
    {
        SpriteRenderer renderer = walkable.GetComponent<SpriteRenderer>();
        Sprite sprite = renderer.sprite;
        Vector2[] spriteVertices = sprite.vertices;
        ushort[] spriteTriangles = sprite.triangles;

        const float thickness = 0.05f;
        Vector3[] vertices = new Vector3[spriteVertices.Length * 2];
        for (int i = 0; i < spriteVertices.Length; i++)
        {
            vertices[i] = new Vector3(spriteVertices[i].x, 0f, spriteVertices[i].y);
            vertices[i + spriteVertices.Length] = new Vector3(spriteVertices[i].x, -thickness, spriteVertices[i].y);
        }

        int[] topTriangles = new int[spriteTriangles.Length];
        for (int i = 0; i < spriteTriangles.Length; i++)
        {
            topTriangles[i] = spriteTriangles[i];
        }

        int[] triangles = topTriangles;
        Mesh testMesh = new Mesh();
        testMesh.vertices = vertices;
        testMesh.triangles = triangles;
        testMesh.RecalculateNormals();
        if (HasDownwardAverageNormal(testMesh))
        {
            FlipTriangleWinding(topTriangles);
        }

        UnityEngine.Object.DestroyImmediate(testMesh);

        List<int> meshTriangles = new List<int>(spriteTriangles.Length * 2);
        meshTriangles.AddRange(topTriangles);
        for (int i = 0; i < topTriangles.Length; i += 3)
        {
            meshTriangles.Add(topTriangles[i + 2] + spriteVertices.Length);
            meshTriangles.Add(topTriangles[i + 1] + spriteVertices.Length);
            meshTriangles.Add(topTriangles[i] + spriteVertices.Length);
        }

        AddBoundarySides(spriteVertices, spriteTriangles, meshTriangles);
        triangles = meshTriangles.ToArray();
        testMesh = new Mesh();
        testMesh.vertices = vertices;
        testMesh.triangles = triangles;
        testMesh.RecalculateNormals();
        if (HasDownwardAverageNormal(testMesh))
        {
            FlipTriangleWinding(triangles);
        }

        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(BakeMeshAssetPath);
        if (mesh == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BakeMeshAssetPath));
            mesh = new Mesh();
            mesh.name = BakeMeshObjectName;
            AssetDatabase.CreateAsset(mesh, BakeMeshAssetPath);
        }

        mesh.Clear();
        mesh.name = BakeMeshObjectName;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        UnityEngine.Object.DestroyImmediate(testMesh);
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static void AddBoundarySides(Vector2[] vertices, ushort[] triangles, List<int> meshTriangles)
    {
        Dictionary<EdgeKey, int> edgeUseCounts = new Dictionary<EdgeKey, int>();
        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            CountEdge(edgeUseCounts, triangles[i], triangles[i + 1]);
            CountEdge(edgeUseCounts, triangles[i + 1], triangles[i + 2]);
            CountEdge(edgeUseCounts, triangles[i + 2], triangles[i]);
        }

        int offset = vertices.Length;
        foreach (KeyValuePair<EdgeKey, int> edge in edgeUseCounts)
        {
            if (edge.Value != 1)
            {
                continue;
            }

            int a = edge.Key.A;
            int b = edge.Key.B;
            meshTriangles.Add(a);
            meshTriangles.Add(b);
            meshTriangles.Add(b + offset);
            meshTriangles.Add(a);
            meshTriangles.Add(b + offset);
            meshTriangles.Add(a + offset);
        }
    }

    private static void CountEdge(Dictionary<EdgeKey, int> edgeUseCounts, int a, int b)
    {
        EdgeKey key = new EdgeKey(a, b);
        int count;
        edgeUseCounts.TryGetValue(key, out count);
        edgeUseCounts[key] = count + 1;
    }

    private struct EdgeKey
    {
        public readonly int A;
        public readonly int B;

        public EdgeKey(int a, int b)
        {
            if (a < b)
            {
                A = a;
                B = b;
            }
            else
            {
                A = b;
                B = a;
            }
        }
    }

    private static bool HasDownwardAverageNormal(Mesh mesh)
    {
        Vector3[] normals = mesh.normals;
        if (normals == null || normals.Length == 0)
        {
            return false;
        }

        float y = 0f;
        for (int i = 0; i < normals.Length; i++)
        {
            y += normals[i].y;
        }

        return y < 0f;
    }

    private static void FlipTriangleWinding(int[] triangles)
    {
        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            int swap = triangles[i + 1];
            triangles[i + 1] = triangles[i + 2];
            triangles[i + 2] = swap;
        }
    }

    private static GameObject EnsureBakeMeshObject(Scene scene, GameObject walkable, Mesh mesh, int roadLayer)
    {
        GameObject bakeMeshObject = FindSceneObject(scene, BakeMeshObjectName);
        if (bakeMeshObject == null)
        {
            bakeMeshObject = new GameObject(BakeMeshObjectName);
            SceneManager.MoveGameObjectToScene(bakeMeshObject, scene);
        }

        bakeMeshObject.layer = roadLayer;
        bakeMeshObject.transform.SetPositionAndRotation(walkable.transform.position, Quaternion.identity);
        bakeMeshObject.transform.localScale = walkable.transform.lossyScale;

        MeshFilter meshFilter = bakeMeshObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = bakeMeshObject.AddComponent<MeshFilter>();
        }

        meshFilter.sharedMesh = mesh;

        MeshCollider meshCollider = bakeMeshObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = bakeMeshObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
        meshCollider.isTrigger = false;
        meshCollider.enabled = true;

        MeshRenderer meshRenderer = bakeMeshObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = bakeMeshObject.AddComponent<MeshRenderer>();
        }

        meshRenderer.enabled = false;
        bakeMeshObject.SetActive(true);
        return bakeMeshObject;
    }

    private static NavMeshSurface EnsureNavMeshSurface(Scene scene, int roadLayer)
    {
        GameObject root = FindSceneObject(scene, "Phase9_NavMeshSurface");
        if (root == null)
        {
            root = new GameObject("Phase9_NavMeshSurface");
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        root.transform.localScale = Vector3.one;

        NavMeshSurface surface = root.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = root.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.layerMask = 1 << roadLayer;
        surface.defaultArea = 0;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        return surface;
    }

    private static string BuildRoadNavMesh(NavMeshSurface surface, GameObject bakeMeshObject)
    {
        MeshRenderer bakeRenderer = bakeMeshObject.GetComponent<MeshRenderer>();
        bool previousRendererState = bakeRenderer != null && bakeRenderer.enabled;
        if (bakeRenderer != null)
        {
            bakeRenderer.enabled = true;
        }

        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices != null && triangulation.vertices.Length > 0)
        {
            if (bakeRenderer != null)
            {
                bakeRenderer.enabled = previousRendererState;
            }

            return "PhysicsColliders";
        }

        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surface.BuildNavMesh();
        if (bakeRenderer != null)
        {
            bakeRenderer.enabled = previousRendererState;
        }

        return "RenderMeshesFallback";
    }

    private static void BindPhase9RuntimeReferences(Scene scene, float navY)
    {
        GameObject bridgeObject = FindSceneObject(scene, "_BridgeRoot");
        Phase9InteractionBridge bridge = bridgeObject != null
            ? bridgeObject.GetComponent<Phase9InteractionBridge>()
            : null;

        if (bridge == null)
        {
            bridge = FindSceneComponent<Phase9InteractionBridge>(scene);
        }

        GameObject player = FindSceneObject(scene, "女主");
        PlayerCharacter playerCharacter = player != null ? player.GetComponent<PlayerCharacter>() : null;
        MovementController movementController = player != null ? player.GetComponent<MovementController>() : null;
        Camera camera = FindMainCamera(scene);

        if (bridge == null)
        {
            return;
        }

        SerializedObject serializedBridge = new SerializedObject(bridge);
        SetObjectReference(serializedBridge, "playerCharacter", playerCharacter);
        SetObjectReference(serializedBridge, "movementController", movementController);
        SetObjectReference(serializedBridge, "targetCamera", camera);
        SetFloat(serializedBridge, "navMeshPlaneY", navY);
        serializedBridge.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bridge);
    }

    private static void EnsurePlayerMovementComponents(GameObject player, float navY)
    {
        PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
        if (playerCharacter == null)
        {
            playerCharacter = player.AddComponent<PlayerCharacter>();
        }

        MovementController movementController = player.GetComponent<MovementController>();
        if (movementController == null)
        {
            movementController = player.AddComponent<MovementController>();
        }

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = player.AddComponent<NavMeshAgent>();
        }

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = Mathf.Max(agent.speed, 2.5f);
        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, 0.08f);
        agent.radius = Mathf.Clamp(agent.radius, 0.05f, 0.22f);
        agent.height = Mathf.Max(agent.height, 0.2f);
        agent.baseOffset = 0f;

        movementController.SetMapNavMeshZToTransformY(false);
        movementController.SetMappedNavMeshY(navY);
        movementController.SetSampleDistances(2f, 0.35f);
        movementController.SetFallbackMovementSettings(agent.speed, agent.stoppingDistance);

        SerializedObject serializedPlayer = new SerializedObject(playerCharacter);
        SetObjectReference(serializedPlayer, "movementController", movementController);
        SetObjectReference(serializedPlayer, "navMeshAgent", agent);
        serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(playerCharacter);
        EditorUtility.SetDirty(movementController);
        EditorUtility.SetDirty(agent);
    }

    private static void ConfigureMainCamera(Scene scene, float navY)
    {
        Camera camera = FindMainCamera(scene);
        if (camera == null)
        {
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = 2.2f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 1000f;
        camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Vector3 position = camera.transform.position;
        camera.transform.position = new Vector3(position.x, Mathf.Max(10f, navY + 6f), position.z);

        CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
        if (follow != null)
        {
            SerializedObject serializedFollow = new SerializedObject(follow);
            SetBool(serializedFollow, "followXZPlane", true);
            SetObjectReference(serializedFollow, "target", FindSceneObject(scene, "女主")?.transform);
            SetObjectReference(serializedFollow, "boundsRoot", FindSceneObject(scene, "静态层")?.transform);
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(follow);
        }

        EditorUtility.SetDirty(camera);
    }

    private static ValidationResult ValidateNavMesh(
        Scene scene,
        GameObject walkable,
        GameObject bakeMeshObject,
        NavMeshTriangulation triangulation,
        float navY)
    {
        ValidationResult result = new ValidationResult();
        result.TotalVertices = triangulation.vertices != null ? triangulation.vertices.Length : 0;
        result.MinY = float.PositiveInfinity;
        result.MaxY = float.NegativeInfinity;

        MeshFilter meshFilter = bakeMeshObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Matrix4x4 worldToLocal = bakeMeshObject.transform.worldToLocalMatrix;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < result.TotalVertices; i++)
        {
            Vector3 vertex = triangulation.vertices[i];
            result.MinY = Mathf.Min(result.MinY, vertex.y);
            result.MaxY = Mathf.Max(result.MaxY, vertex.y);
            if (IsPointInsideMeshXZ(worldToLocal.MultiplyPoint3x4(vertex), vertices, triangles, 0.04f))
            {
                result.VerticesInside++;
            }
        }

        Bounds bounds = walkable.GetComponent<SpriteRenderer>().bounds;
        result.OutsideProbeLeaks = CountOutsideProbeLeaks(bounds, bakeMeshObject, navY);
        ValidateReachableEntries(scene, navY, ref result);
        result.Passed = result.TotalVertices > 0
            && result.VerticesInside == result.TotalVertices
            && result.OutsideProbeLeaks == 0
            && result.TotalEntries > 0
            && result.ReachableEntries == result.TotalEntries;
        return result;
    }

    private static int CountOutsideProbeLeaks(Bounds bounds, GameObject bakeMeshObject, float navY)
    {
        int leaks = 0;
        Matrix4x4 worldToLocal = bakeMeshObject.transform.worldToLocalMatrix;
        Mesh mesh = bakeMeshObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        const int steps = 10;
        for (int x = 0; x <= steps; x++)
        {
            for (int z = 0; z <= steps; z++)
            {
                Vector3 probe = new Vector3(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, x / (float)steps),
                    navY,
                    Mathf.Lerp(bounds.min.z, bounds.max.z, z / (float)steps));

                bool insideRoad = IsPointInsideMeshXZ(worldToLocal.MultiplyPoint3x4(probe), vertices, triangles, 0.02f);
                if (insideRoad)
                {
                    continue;
                }

                NavMeshHit hit;
                if (NavMesh.SamplePosition(probe, out hit, 0.05f, NavMesh.AllAreas))
                {
                    leaks++;
                }
            }
        }

        return leaks;
    }

    private static void ValidateReachableEntries(Scene scene, float navY, ref ValidationResult result)
    {
        GameObject player = FindSceneObject(scene, "女主");
        if (player == null)
        {
            return;
        }

        NavMeshHit playerHit;
        if (!NavMesh.SamplePosition(new Vector3(player.transform.position.x, navY, player.transform.position.z), out playerHit, 0.6f, NavMesh.AllAreas))
        {
            return;
        }

        string[] entries =
        {
            "Order-interact",
            "Shape-interact",
            "Glaze-interact",
            "Kiln-interact"
        };

        for (int i = 0; i < entries.Length; i++)
        {
            GameObject entry = FindSceneObject(scene, entries[i]);
            if (entry == null)
            {
                continue;
            }

            result.TotalEntries++;
            NavMeshHit entryHit;
            if (!NavMesh.SamplePosition(new Vector3(entry.transform.position.x, navY, entry.transform.position.z), out entryHit, 0.8f, NavMesh.AllAreas))
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(playerHit.position, entryHit.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete)
            {
                result.ReachableEntries++;
            }
        }
    }

    private static bool IsPointInsideMeshXZ(Vector3 point, Vector3[] vertices, int[] triangles, float tolerance)
    {
        Vector2 p = new Vector2(point.x, point.z);
        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            Vector2 a = new Vector2(vertices[triangles[i]].x, vertices[triangles[i]].z);
            Vector2 b = new Vector2(vertices[triangles[i + 1]].x, vertices[triangles[i + 1]].z);
            Vector2 c = new Vector2(vertices[triangles[i + 2]].x, vertices[triangles[i + 2]].z);
            if (PointInTriangleWithTolerance(p, a, b, c, tolerance))
            {
                return true;
            }
        }

        return false;
    }

    private static bool PointInTriangleWithTolerance(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float tolerance)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNegative = d1 < -tolerance || d2 < -tolerance || d3 < -tolerance;
        bool hasPositive = d1 > tolerance || d2 > tolerance || d3 > tolerance;
        return !(hasNegative && hasPositive);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static Camera FindMainCamera(Scene scene)
    {
        foreach (GameObject obj in GetSceneObjects(scene))
        {
            Camera camera = obj.GetComponent<Camera>();
            if (camera != null && (camera.CompareTag("MainCamera") || camera.name == "Main Camera"))
            {
                return camera;
            }
        }

        return null;
    }

    private static T FindSceneComponent<T>(Scene scene) where T : Component
    {
        foreach (GameObject obj in GetSceneObjects(scene))
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        foreach (GameObject obj in GetSceneObjects(scene))
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }

    private static GameObject FindWalkableObject(Scene scene)
    {
        GameObject walkable = FindSceneObject(scene, "NavMesh-walkable");
        if (walkable != null)
        {
            return walkable;
        }

        return FindSceneObject(scene, "NavMesh-walkable (1)");
    }

    private static IEnumerable<GameObject> GetSceneObjects(Scene scene)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject obj = allObjects[i];
            if (obj != null && obj.scene == scene)
            {
                yield return obj;
            }
        }
    }

    private struct ValidationResult
    {
        public int TotalVertices;
        public int VerticesInside;
        public int OutsideProbeLeaks;
        public int TotalEntries;
        public int ReachableEntries;
        public float MinY;
        public float MaxY;
        public bool Passed;
    }
}
