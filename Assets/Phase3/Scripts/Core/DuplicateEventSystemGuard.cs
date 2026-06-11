using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-32000)]
public class DuplicateEventSystemGuard : MonoBehaviour
{
    private void Awake()
    {
        DisableIfDuplicateExists();
    }

    private void OnEnable()
    {
        DisableIfDuplicateExists();
    }

    private void DisableIfDuplicateExists()
    {
        EventSystem current = GetComponent<EventSystem>();
        if (current == null || !current.enabled)
        {
            return;
        }

        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        foreach (EventSystem eventSystem in eventSystems)
        {
            if (eventSystem != null && eventSystem != current && eventSystem.isActiveAndEnabled)
            {
                current.enabled = false;

                StandaloneInputModule inputModule = GetComponent<StandaloneInputModule>();
                if (inputModule != null)
                {
                    inputModule.enabled = false;
                }

                Debug.Log($"[DuplicateEventSystemGuard] Disabled duplicate EventSystem on '{gameObject.name}'");
                return;
            }
        }
    }
}
