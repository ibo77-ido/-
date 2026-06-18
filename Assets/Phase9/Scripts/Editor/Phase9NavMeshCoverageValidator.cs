using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Phase9 NavMesh 蓝色连通区域覆盖验证工具。
/// 读取当前 NavMesh triangulation，从女主起点对蓝色区域采样点逐个 CalculatePath，
/// 输出可达/不可达报告，并单独校验 4 个核心交互点。
/// 纯只读：不修改 Scene / Asset / Runtime。
/// </summary>
[InitializeOnLoad]
public static class Phase9NavMeshCoverageValidator
{
	private const string ReportPath = "Assets/Screenshots/phase9-navmesh-coverage-report.txt";
	private const string RequestPath = "Library/Phase9NavMeshCoverageValidator.request";
	private const string PlayerName = "女主";
	private const float OriginSampleDistance = 0.6f;
	private const float SamplePositionDistance = 0.1f;
	private const int MaxSamplePoints = 300;
	private const float EdgeTolerance = 0.1f;
	private const string ScenePath = "Assets/Phase9/Scenes/SampleScene.unity";

	private static readonly string[] InteractionEntries =
	{
		"Order-interact",
		"Shape-interact",
		"Glaze-interact",
		"Kiln-interact"
	};

	static Phase9NavMeshCoverageValidator()
	{
		EditorApplication.delayCall += RunIfRequested;
	}

	[MenuItem("Phase9/Verify/NavMesh Coverage Report")]
	public static void RunValidation()
	{
		Scene scene = SceneManager.GetActiveScene();
		bool success = RunValidationForScene(scene);
		if (!success)
		{
			Debug.LogError("[Phase9NavMeshCoverageValidator] Manual validation FAILED.");
		}
	}

	public static void RunFromCommandLine()
	{
		EditorSceneManager.OpenScene(ScenePath);
		bool success = RunValidationForScene(SceneManager.GetActiveScene());
		EditorApplication.Exit(success ? 0 : 1);
	}

	private static void RunIfRequested()
	{
		if (!File.Exists(RequestPath))
		{
			return;
		}

		File.Delete(RequestPath);
		Scene scene = SceneManager.GetActiveScene();
		bool success = RunValidationForScene(scene);
		EditorApplication.Exit(success ? 0 : 1);
	}

	private static bool RunValidationForScene(Scene scene)
	{
		StringBuilder log = new StringBuilder();
		log.AppendLine("[Phase9NavMeshCoverageValidator] Scene=" + scene.name);

		GameObject player = FindSceneObject(scene, PlayerName);
		if (player == null)
		{
			log.AppendLine("FAIL: 女主 not found in scene.");
			WriteReport(log);
			Debug.LogError(log.ToString());
			return false;
		}

		NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
		if (triangulation.vertices == null || triangulation.vertices.Length == 0
			|| triangulation.indices == null || triangulation.indices.Length < 3)
		{
			log.AppendLine("FAIL: NavMesh triangulation empty.");
			WriteReport(log);
			Debug.LogError(log.ToString());
			return false;
		}

		float navY = triangulation.vertices[0].y;
		log.AppendLine("NavMesh vertices=" + triangulation.vertices.Length
			+ " triangles=" + (triangulation.indices.Length / 3)
			+ " navY=" + navY.ToString("F4"));

		Vector3 originPlayerPos = new Vector3(player.transform.position.x, navY, player.transform.position.z);
		NavMeshHit originHit;
		if (!NavMesh.SamplePosition(originPlayerPos, out originHit, OriginSampleDistance, NavMesh.AllAreas))
		{
			log.AppendLine("FAIL: 女主起点无法吸附到 NavMesh (sample distance=" + OriginSampleDistance + ").");
			WriteReport(log);
			Debug.LogError(log.ToString());
			return false;
		}

		log.AppendLine("Origin (女主): " + originHit.position.ToString("F4"));

		List<Vector3> samplePoints = BuildSamplePoints(triangulation, navY);
		log.AppendLine("SamplePoints total=" + samplePoints.Count);

		int reachable = 0;
		int unreachable = 0;
		List<Vector3> unreachableCoords = new List<Vector3>();
		List<Vector3> edgeUnreachable = new List<Vector3>();

		Bounds walkableBounds = GetWalkableBounds(scene);

		for (int i = 0; i < samplePoints.Count; i++)
		{
			Vector3 sample = samplePoints[i];
			NavMeshHit sampleHit;
			if (!NavMesh.SamplePosition(sample, out sampleHit, SamplePositionDistance, NavMesh.AllAreas))
			{
				unreachable++;
				unreachableCoords.Add(sample);
				continue;
			}

			NavMeshPath path = new NavMeshPath();
			if (NavMesh.CalculatePath(originHit.position, sampleHit.position, NavMesh.AllAreas, path)
				&& path.status == NavMeshPathStatus.PathComplete)
			{
				reachable++;
			}
			else
			{
				unreachable++;
				unreachableCoords.Add(sampleHit.position);

				if (IsNearBoundsEdge(sampleHit.position, walkableBounds))
				{
					edgeUnreachable.Add(sampleHit.position);
				}
			}
		}

		log.AppendLine();
		log.AppendLine("---- Coverage Summary ----");
		log.AppendLine("reachable=" + reachable);
		log.AppendLine("unreachable=" + unreachable);
		log.AppendLine("unreachableCoordsCount=" + unreachableCoords.Count);
		for (int i = 0; i < unreachableCoords.Count; i++)
		{
			log.AppendLine("  unreachable[" + i + "]=" + unreachableCoords[i].ToString("F4"));
		}

		log.AppendLine();
		log.AppendLine("---- Interaction Points ----");
		int interactionPass = 0;
		int interactionFail = 0;
		Dictionary<string, string> interactionResults = new Dictionary<string, string>();

		for (int i = 0; i < InteractionEntries.Length; i++)
		{
			string entryName = InteractionEntries[i];
			GameObject entry = FindSceneObject(scene, entryName);
			if (entry == null)
			{
				interactionResults[entryName] = "MISSING";
				interactionFail++;
				log.AppendLine(entryName + ": MISSING (对象不存在)");
				continue;
			}

			Vector3 entryPos = new Vector3(entry.transform.position.x, navY, entry.transform.position.z);
			NavMeshHit entryHit;
			if (!NavMesh.SamplePosition(entryPos, out entryHit, 0.8f, NavMesh.AllAreas))
			{
				interactionResults[entryName] = "NO_NAVMESH";
				interactionFail++;
				log.AppendLine(entryName + ": FAIL (SamplePosition 失败) pos=" + entryPos.ToString("F4"));
				continue;
			}

			NavMeshPath path = new NavMeshPath();
			bool reachable2 = NavMesh.CalculatePath(originHit.position, entryHit.position, NavMesh.AllAreas, path)
				&& path.status == NavMeshPathStatus.PathComplete;
			float distance = Vector3.Distance(originHit.position, entryHit.position);

			if (reachable2)
			{
				interactionResults[entryName] = "PASS";
				interactionPass++;
				log.AppendLine(entryName + ": PASS, reachable, distance=" + distance.ToString("F3")
					+ " pos=" + entryHit.position.ToString("F4"));
			}
			else
			{
				interactionResults[entryName] = "UNREACHABLE";
				interactionFail++;
				log.AppendLine(entryName + ": FAIL (CalculatePath 非 PathComplete) pos=" + entryHit.position.ToString("F4"));
			}
		}

		log.AppendLine();
		log.AppendLine("---- Diagnostics ----");
		if (unreachable > 0)
		{
			if (edgeUnreachable.Count > 0 && edgeUnreachable.Count >= unreachable * 0.5f)
			{
				log.AppendLine("DIAG: 多数不可达点位于 walkable bounds 边缘 (count=" + edgeUnreachable.Count
					+ ")，提示【窄路被 agent 半径切断】或【蓝色区域边缘超出 NavMesh 实际生成范围】。");
			}
			else
			{
				log.AppendLine("DIAG: 不可达点未集中在边缘，提示【NavMesh-walkable 图片存在断开的蓝色区域（孤岛）】。");
			}
		}

		for (int i = 0; i < InteractionEntries.Length; i++)
		{
			string entryName = InteractionEntries[i];
			if (interactionResults.ContainsKey(entryName) && interactionResults[entryName] != "PASS")
			{
				log.AppendLine("DIAG: " + entryName + " 不可达，提示【交互点位置在可走区域边缘外 / 交互半径过小 / 需要独立的 interactionReachPoint】。");
			}
		}

		log.AppendLine();
		log.AppendLine("---- Verdict ----");
		bool coveragePass = unreachable == 0;
		bool interactionAllPass = interactionFail == 0;
		bool overall = coveragePass && interactionAllPass;
		log.AppendLine("Coverage=" + (coveragePass ? "PASS" : "FAIL")
			+ " Interaction=" + (interactionAllPass ? "PASS" : "FAIL")
			+ " Overall=" + (overall ? "PASS" : "FAIL"));

		WriteReport(log);

		if (overall)
		{
			Debug.Log("[Phase9NavMeshCoverageValidator] PASS. Report at " + ReportPath);
			return true;
		}
		else
		{
			Debug.LogError("[Phase9NavMeshCoverageValidator] FAIL. Report at " + ReportPath);
			return false;
		}
	}

	private static List<Vector3> BuildSamplePoints(NavMeshTriangulation triangulation, float navY)
	{
		List<Vector3> points = new List<Vector3>();
		int triangleCount = triangulation.indices.Length / 3;
		int step = Mathf.Max(1, triangleCount / MaxSamplePoints);

		for (int i = 0; i + 2 < triangulation.indices.Length; i += 3 * step)
		{
			Vector3 a = triangulation.vertices[triangulation.indices[i]];
			Vector3 b = triangulation.vertices[triangulation.indices[i + 1]];
			Vector3 c = triangulation.vertices[triangulation.indices[i + 2]];
			Vector3 centroid = new Vector3((a.x + b.x + c.x) / 3f, navY, (a.z + b.z + c.z) / 3f);
			points.Add(centroid);

			if (points.Count >= MaxSamplePoints)
			{
				break;
			}
		}

		return points;
	}

	private static Bounds GetWalkableBounds(Scene scene)
	{
		GameObject walkable = FindSceneObject(scene, "NavMesh-walkable");
		if (walkable == null)
		{
			return new Bounds(Vector3.zero, Vector3.zero);
		}

		SpriteRenderer renderer = walkable.GetComponent<SpriteRenderer>();
		if (renderer != null)
		{
			return renderer.bounds;
		}

		return new Bounds(walkable.transform.position, Vector3.zero);
	}

	private static bool IsNearBoundsEdge(Vector3 point, Bounds bounds)
	{
		if (bounds.size == Vector3.zero)
		{
			return false;
		}

		float dx = Mathf.Min(Mathf.Abs(point.x - bounds.min.x), Mathf.Abs(point.x - bounds.max.x));
		float dz = Mathf.Min(Mathf.Abs(point.z - bounds.min.z), Mathf.Abs(point.z - bounds.max.z));
		return dx < EdgeTolerance || dz < EdgeTolerance;
	}

	private static GameObject FindSceneObject(Scene scene, string objectName)
	{
		GameObject[] rootObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootObjects.Length; i++)
		{
			GameObject root = rootObjects[i];
			if (root.name == objectName)
			{
				return root;
			}

			Transform child = root.transform.Find(objectName);
			if (child != null)
			{
				return child.gameObject;
			}

			Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
			for (int j = 0; j < descendants.Length; j++)
			{
				if (descendants[j].name == objectName)
				{
					return descendants[j].gameObject;
				}
			}
		}

		return null;
	}

	private static void WriteReport(StringBuilder content)
	{
		string directory = Path.GetDirectoryName(ReportPath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		File.WriteAllText(ReportPath, content.ToString());
		AssetDatabase.ImportAsset(ReportPath, ImportAssetOptions.ForceUpdate);
	}
}
