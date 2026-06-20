using UnityEngine;

public class Phase11UIBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject hudCanvasPrefab;
    [SerializeField] private bool instantiateOnAwake = true;
    [SerializeField] private bool skipIfHudAlreadyExists = true;

    private GameObject runtimeHudCanvas;

    private void Awake()
    {
        if (instantiateOnAwake)
        {
            InstantiateHudIfNeeded();
        }
    }

    public GameObject InstantiateHudIfNeeded()
    {
        if (runtimeHudCanvas != null)
        {
            return runtimeHudCanvas;
        }

        if (hudCanvasPrefab == null)
        {
            Debug.LogWarning("[Phase11UIBootstrapper] HUDCanvas prefab is not assigned.", this);
            return null;
        }

        if (skipIfHudAlreadyExists)
        {
            var existingCanvas = GameObject.Find(hudCanvasPrefab.name);
            if (existingCanvas != null)
            {
                runtimeHudCanvas = existingCanvas;
                return runtimeHudCanvas;
            }
        }

        runtimeHudCanvas = Instantiate(hudCanvasPrefab);
        runtimeHudCanvas.name = hudCanvasPrefab.name;
        return runtimeHudCanvas;
    }
}
