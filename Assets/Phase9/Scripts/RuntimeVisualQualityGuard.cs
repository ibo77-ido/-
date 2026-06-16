using UnityEngine;
using UnityEngine.Rendering;

public static class RuntimeVisualQualityGuard
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 4);
        ScalableBufferManager.ResizeBuffers(1f, 1f);
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

        ApplyRenderPipelineQuality(GraphicsSettings.currentRenderPipeline);

        RenderPipelineManager.beginCameraRendering -= DisableSoftPostProcessing;
        RenderPipelineManager.beginCameraRendering += DisableSoftPostProcessing;
    }

    private static void ApplyRenderPipelineQuality(RenderPipelineAsset renderPipelineAsset)
    {
        if (!renderPipelineAsset)
        {
            return;
        }

        SetFloatProperty(renderPipelineAsset, "renderScale", 1.25f);
        SetIntPropertyMinimum(renderPipelineAsset, "msaaSampleCount", 4);
        SetBoolProperty(renderPipelineAsset, "supportsCameraOpaqueTexture", false);
        SetBoolProperty(renderPipelineAsset, "supportsCameraDepthTexture", false);
    }

    private static void DisableSoftPostProcessing(ScriptableRenderContext context, Camera camera)
    {
        Component cameraData = camera.GetComponent("UniversalAdditionalCameraData");
        if (cameraData)
        {
            SetBoolProperty(cameraData, "renderPostProcessing", false);
            SetBoolProperty(cameraData, "allowXRRendering", false);
            SetEnumProperty(cameraData, "antialiasing", 0);
        }

        camera.allowDynamicResolution = false;
    }

    private static void SetFloatProperty(object target, string propertyName, float value)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value, null);
        }
    }

    private static void SetIntPropertyMinimum(object target, string propertyName, int minimum)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanRead && property.CanWrite)
        {
            int current = (int)property.GetValue(target, null);
            property.SetValue(target, Mathf.Max(current, minimum), null);
        }
    }

    private static void SetBoolProperty(object target, string propertyName, bool value)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value, null);
        }
    }

    private static void SetEnumProperty(object target, string propertyName, int enumValue)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite && property.PropertyType.IsEnum)
        {
            object value = System.Enum.ToObject(property.PropertyType, enumValue);
            property.SetValue(target, value, null);
        }
    }
}
