using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Phase9 女主可见性 + NavMesh 判定中心修复。
///
/// 方案：HeroineRoot (父) + GirlModel (子)
/// - HeroineRoot: 逻辑层，pivot 在脚底地面，挂 NavMeshAgent + 移动脚本
/// - GirlModel: 视觉层（原女主本体），作为子物体，挂 SpriteRenderer + Animator
/// - GirlModel localEuler.x=90 保持面向相机，不受 NavMeshAgent/Animator 旋转干扰
///
/// 菜单: Phase9 > Fix > Heroine Root Restructure
/// </summary>
public static class Phase9FixPlayerVisibility
{
	private const string PlayerName = "女主";
	private const string HeroineRootName = "HeroineRoot";
	private const string GirlModelName = "GirlModel";
	private const string VisualRootName = "VisualRoot";

	[MenuItem("Phase9/Fix/Heroine Root Restructure")]
	public static void Fix()
	{
		GameObject player = GameObject.Find(PlayerName);
		if (player == null)
		{
			// 可能已经被重命名为 GirlModel
			player = GameObject.Find(GirlModelName);
			if (player == null)
			{
				Debug.LogError("[Phase9FixPlayerVisibility] " + PlayerName + " not found.");
				return;
			}
		}

		// 如果已经存在 HeroineRoot，说明已执行过，跳过
		GameObject existingRoot = GameObject.Find(HeroineRootName);
		if (existingRoot != null && existingRoot.transform.Find(GirlModelName) != null)
		{
			Debug.Log("[Phase9FixPlayerVisibility] HeroineRoot already exists with GirlModel. Skip.");
			return;
		}

		Undo.SetCurrentGroupName("Phase9 Heroine Root Restructure");

		Vector3 playerPos = player.transform.position;
		Vector3 playerScale = player.transform.localScale;

		// 0. 清理之前 VisualRoot 方案的残留
		CleanupVisualRoot(player);

		// 0b. 确保女主本体有 SpriteRenderer 和 Animator（之前可能被删了）
		EnsureSpriteRenderer(player);
		EnsureAnimator(player);

		// 1. 创建 HeroineRoot 父物体（pivot 在女主当前位置 = 脚底地面点）
		GameObject heroineRoot = new GameObject(HeroineRootName);
		Undo.RegisterCreatedObjectUndo(heroineRoot, "Create HeroineRoot");
		heroineRoot.transform.position = playerPos;
		heroineRoot.transform.rotation = Quaternion.identity;
		heroineRoot.transform.localScale = Vector3.one;

		// 2. 把女主本体作为 HeroineRoot 的子物体，重命名为 GirlModel
		player.transform.SetParent(heroineRoot.transform, true);
		player.name = GirlModelName;

		// GirlModel 局部旋转 euler.x=90 让 Sprite 面向相机
		player.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		player.transform.localScale = playerScale;
		player.transform.localPosition = Vector3.zero;

		// 3. 把 NavMeshAgent 从 GirlModel 移到 HeroineRoot
		MoveComponent<NavMeshAgent>(player, heroineRoot);

		// 4. 把 PlayerCharacter 和 MovementController 也移到 HeroineRoot
		MoveComponent<PlayerCharacter>(player, heroineRoot);
		MoveComponent<MovementController>(player, heroineRoot);

		// 5. 设置 NavMeshAgent 参数
		NavMeshAgent agent = heroineRoot.GetComponent<NavMeshAgent>();
		if (agent != null)
		{
			agent.updatePosition = false;
			agent.updateRotation = false;
			agent.baseOffset = 0f;
		}

		EditorUtility.SetDirty(heroineRoot);
		EditorUtility.SetDirty(player);

		Debug.Log("[Phase9FixPlayerVisibility] Done. HeroineRoot=" + heroineRoot.transform.position.ToString("F3")
			+ " GirlModel localEuler=" + player.transform.localEulerAngles.ToString("F1"));
	}

	public static void RunFromCommandLine()
	{
		Fix();
		EditorApplication.Exit(0);
	}

	private static void CleanupVisualRoot(GameObject player)
	{
		Transform visualRoot = player.transform.Find(VisualRootName);
		if (visualRoot == null)
		{
			return;
		}

		// 把 VisualRoot 上的 SpriteRenderer 和 Animator 拷回女主本体
		SpriteRenderer vrSprite = visualRoot.GetComponent<SpriteRenderer>();
		if (vrSprite != null && player.GetComponent<SpriteRenderer>() == null)
		{
			SpriteRenderer newSprite = player.AddComponent<SpriteRenderer>();
			if (ComponentUtility.CopyComponent(vrSprite))
			{
				ComponentUtility.PasteComponentValues(newSprite);
			}
		}

		Animator vrAnimator = visualRoot.GetComponent<Animator>();
		if (vrAnimator != null && player.GetComponent<Animator>() == null)
		{
			Animator newAnimator = player.AddComponent<Animator>();
			if (ComponentUtility.CopyComponent(vrAnimator))
			{
				ComponentUtility.PasteComponentValues(newAnimator);
			}
		}

		Undo.DestroyObjectImmediate(visualRoot.gameObject);
		Debug.Log("[Phase9FixPlayerVisibility] Cleaned up VisualRoot.");
	}

	private static void EnsureSpriteRenderer(GameObject player)
	{
		if (player.GetComponent<SpriteRenderer>() != null)
		{
			return;
		}

		SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
		sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/人物素材/女主/正面.png");
		sr.color = Color.white;
		sr.sortingOrder = 3;
		sr.enabled = true;
		Debug.Log("[Phase9FixPlayerVisibility] Restored SpriteRenderer on " + player.name);
	}

	private static void EnsureAnimator(GameObject player)
	{
		if (player.GetComponent<Animator>() != null)
		{
			return;
		}

		Animator anim = player.AddComponent<Animator>();
		RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
			"Assets/Animation/Female Lead Animator Controller.controller");
		anim.runtimeAnimatorController = controller;
		anim.applyRootMotion = false;
		anim.enabled = true;
		Debug.Log("[Phase9FixPlayerVisibility] Restored Animator on " + player.name);
	}

	private static void MoveComponent<T>(GameObject from, GameObject to) where T : Component
	{
		T source = from.GetComponent<T>();
		if (source == null)
		{
			Debug.LogWarning("[Phase9FixPlayerVisibility] No " + typeof(T).Name + " found on " + from.name);
			return;
		}

		T dest = to.AddComponent<T>();

		if (ComponentUtility.CopyComponent(source))
		{
			if (ComponentUtility.PasteComponentValues(dest))
			{
				Debug.Log("[Phase9FixPlayerVisibility] Copied " + typeof(T).Name + " values.");
			}
		}

		Undo.DestroyObjectImmediate(source);
		Debug.Log("[Phase9FixPlayerVisibility] Moved " + typeof(T).Name + " → " + to.name);
	}
}
