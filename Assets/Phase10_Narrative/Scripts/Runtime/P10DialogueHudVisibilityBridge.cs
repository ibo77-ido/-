using System.Reflection;
using UnityEngine;

namespace Phase10_Narrative
{
    [DisallowMultipleComponent]
    public sealed class P10DialogueHudVisibilityBridge : MonoBehaviour
    {
        [SerializeField] private P10DialogueController dialogueController;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        [SerializeField] private string hudCanvasName = "HUDCanvas";
        [SerializeField] private bool hideDuringDialogueLog = true;
        [SerializeField] private bool hideDuringCurrentOrderPanel = true;

        private static readonly FieldInfo RuntimeSurfaceField =
            typeof(P10DialogueController).GetField("runtimeSurface", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool hiddenByBridge;
        private bool capturedHudState;
        private float previousAlpha = 1f;
        private bool previousInteractable = true;
        private bool previousBlocksRaycasts = true;
        private float nextResolveTime;

        private void Awake()
        {
            Debug.Log("[P10DialogueHudVisibilityBridge] Awake");
            ResolveHudCanvasGroup();
            ResolveDialogueController();
            ApplyHudVisibility();
        }

        private void OnEnable()
        {
            ApplyHudVisibility();
        }

        private void Update()
        {
            ApplyHudVisibility();
        }

        private void OnDisable()
        {
            RestoreHudIfHidden();
        }

        private void OnDestroy()
        {
            RestoreHudIfHidden();
        }

        private void ApplyHudVisibility()
        {
            ResolveHudCanvasGroup();
            if (hudCanvasGroup == null)
            {
                return;
            }

            bool shouldHide = IsDialogueUiBlockingMainHud();
            if (shouldHide)
            {
                HideHud();
                return;
            }

            RestoreHudIfHidden();
        }

        private bool IsDialogueUiBlockingMainHud()
        {
            ResolveDialogueController();
            if (dialogueController == null)
            {
                return false;
            }

            if (IsCurrentDialoguePanelVisible())
            {
                return true;
            }

            if (hideDuringDialogueLog && dialogueController.IsDialogueLogVisible)
            {
                return true;
            }

            return hideDuringCurrentOrderPanel && dialogueController.IsCurrentOrderPanelVisible;
        }

        private bool IsCurrentDialoguePanelVisible()
        {
            if (dialogueController == null || RuntimeSurfaceField == null)
            {
                return false;
            }

            object runtimeSurface = RuntimeSurfaceField.GetValue(dialogueController);
            if (runtimeSurface == null)
            {
                return false;
            }

            PropertyInfo visibleProperty = runtimeSurface.GetType().GetProperty("IsDialogueVisible", BindingFlags.Instance | BindingFlags.Public);
            if (visibleProperty == null)
            {
                return false;
            }

            object value = visibleProperty.GetValue(runtimeSurface, null);
            return value is bool && (bool)value;
        }

        private void HideHud()
        {
            if (hiddenByBridge || hudCanvasGroup == null)
            {
                return;
            }

            previousAlpha = hudCanvasGroup.alpha;
            previousInteractable = hudCanvasGroup.interactable;
            previousBlocksRaycasts = hudCanvasGroup.blocksRaycasts;
            capturedHudState = true;

            hudCanvasGroup.alpha = 0f;
            hudCanvasGroup.interactable = false;
            hudCanvasGroup.blocksRaycasts = false;
            hiddenByBridge = true;
        }

        private void RestoreHudIfHidden()
        {
            if (!hiddenByBridge || hudCanvasGroup == null)
            {
                return;
            }

            if (capturedHudState)
            {
                hudCanvasGroup.alpha = previousAlpha;
                hudCanvasGroup.interactable = previousInteractable;
                hudCanvasGroup.blocksRaycasts = previousBlocksRaycasts;
            }

            hiddenByBridge = false;
            capturedHudState = false;
        }

        private void ResolveDialogueController()
        {
            if (dialogueController != null)
            {
                return;
            }

            if (Time.unscaledTime < nextResolveTime)
            {
                return;
            }

            nextResolveTime = Time.unscaledTime + 0.5f;
            dialogueController = FindObjectOfType<P10DialogueController>();
        }

        private void ResolveHudCanvasGroup()
        {
            if (hudCanvasGroup != null)
            {
                return;
            }

            if (Time.unscaledTime < nextResolveTime)
            {
                return;
            }

            nextResolveTime = Time.unscaledTime + 0.5f;
            GameObject hudCanvas = GameObject.Find(hudCanvasName);
            if (hudCanvas == null)
            {
                return;
            }

            hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
            if (hudCanvasGroup == null)
            {
                hudCanvasGroup = hudCanvas.AddComponent<CanvasGroup>();
            }
        }
    }
}
