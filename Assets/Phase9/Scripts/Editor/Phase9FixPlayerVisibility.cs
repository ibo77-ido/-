using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Phase9 女主 Play Mode 可见性修复。
/// 将 SpriteRenderer 和 Animator 从女主本体移到新建的 VisualRoot 子物体上，
/// VisualRoot 用 localEuler.x=90 保持 Sprite 面向相机，
/// 避免 Animator / NavMeshAgent 在运行时重置本体旋转导致 Sprite 变成侧面不可见。
///
/// 菜单: Phase9 > Fix > Player Visibility
/// </summary>
public static class Phase9FixPlayerVisibility
{
	private const string PlayerName = "女主";
	private const string VisualRootName = "VisualRoot";

	[MenuItem("Phase9/Fix/Player Visibility")]
	public static void Fix()
	{
		GameObject player = GameObject.Find(PlayerName);
		if (player == null)
		{
			Debug.LogError("[Phase9FixPlayerVisibility] " + PlayerName + " not found.");
			return;
		}

		Undo.SetCurrentGroupName("Phase9 Fix Player Visibility");

		// 1. 创建或复用 VisualRoot 子物体
		Transform existingVisualRoot = player.transform.Find(VisualRootName);
		GameObject visualRoot;
		if (existingVisualRoot != null)
		{
			visualRoot = existingVisualRoot.gameObject;
			Debug.Log("[Phase9FixPlayerVisibility] Reusing existing VisualRoot.");
		}
		else
		{
			visualRoot = new GameObject(VisualRootName);
			Undo.RegisterCreatedObjectUndo(visualRoot, "Create VisualRoot");
			visualRoot.transform.SetParent(player.transform, false);
			Debug.Log("[Phase9FixPlayerVisibility] Created VisualRoot child.");
		}

		// VisualRoot 保持面向相机的旋转 (euler.x=90)
		visualRoot.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
		visualRoot.transform.localPosition = Vector3.zero;
		visualRoot.transform.localScale = Vector3.one;

		// 2. 迁移 SpriteRenderer 到 VisualRoot
		MoveComponent<SpriteRenderer>(player, visualRoot);

		// 3. 迁移 Animator 到 VisualRoot
		MoveComponent<Animator>(player, visualRoot);

		EditorUtility.SetDirty(player);
		Debug.Log("[Phase9FixPlayerVisibility] Done. VisualRoot euler=" + visualRoot.transform.localEulerAngles.ToString("F1"));
	}

	public static void RunFromCommandLine()
	{
		Fix();
		EditorApplication.Exit(0);
	}

	private static void MoveComponent<T>(GameObject from, GameObject to) where T : Component
	{
		T source = from.GetComponent<T>();
		if (source == null)
		{
			Debug.LogWarning("[Phase9FixPlayerVisibility] No " + typeof(T).Name + " found on " + from.name);
			return;
		}

		// 在目标上创建同类型组件
		T dest = to.AddComponent<T>();

		// 用 ComponentUtility 复制所有序列化值（包括引用、动画 controller、sprite 等）
		if (ComponentUtility.CopyComponent(source))
		{
			if (ComponentUtility.PasteComponentValues(dest))
			{
				Debug.Log("[Phase9FixPlayerVisibility] Copied " + typeof(T).Name + " values via ComponentUtility.");
			}
			else
			{
				Debug.LogWarning("[Phase9FixPlayerVisibility] PasteComponentValues failed for " + typeof(T).Name + ", keeping defaults.");
			}
		}
		else
		{
			Debug.LogWarning("[Phase9FixPlayerVisibility] CopyComponent failed for " + typeof(T).Name + ", keeping defaults.");
		}

		// 删除源组件 (支持 Undo)
		Undo.DestroyObjectImmediate(source);

		Debug.Log("[Phase9FixPlayerVisibility] Moved " + typeof(T).Name + " → " + to.name);
	}
}
