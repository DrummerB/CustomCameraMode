using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This static class adds a depth and a normals camera mode to the scene view.
/// </summary>
public static class CustomCameraMode
{
    static SceneView.CameraMode depthCameraMode;
    static SceneView.CameraMode normalsCameraMode;
    
    static Dictionary<Camera, CommandBuffer> commandBuffers = new Dictionary<Camera, CommandBuffer>();

    static int depthBlitPass;
    static int normalsBlitPass;
    static Material blitMaterial;
    static Material BlitMaterial
    {
        get
        {
            if (blitMaterial == null)
            {
                blitMaterial = new Material(Shader.Find("CustomCameraMode/CameraDepthNormals"));
                depthBlitPass = blitMaterial.FindPass("CameraDepthBlit");
                normalsBlitPass = blitMaterial.FindPass("CameraNormalsBlit");
            }
            return blitMaterial;
        }
    }

    [InitializeOnLoadMethod]
    static void Initialize()
    {
        depthCameraMode = SceneView.AddCameraMode("Camera Depth", "Shading Mode");
        normalsCameraMode = SceneView.AddCameraMode("Camera Normals", "Shading Mode");
        Camera.onPreRender += OnPreRender;
    }
    
    static void OnPreRender(Camera cam)
    {
        var sceneView = SceneView.currentDrawingSceneView;
        if (sceneView != null)
        {
            if (sceneView.cameraMode == depthCameraMode)
            {
                cam.depthTextureMode |= DepthTextureMode.Depth;
                UpdateCommandBuffer(cam, depthCameraMode);
            }
            else if (sceneView.cameraMode == normalsCameraMode)
            {
                cam.depthTextureMode |= DepthTextureMode.DepthNormals;
                UpdateCommandBuffer(cam, normalsCameraMode);
            }
            else
            {
                RemoveCommandBuffer(cam);
            }
        }
    }

    static void UpdateCommandBuffer(Camera cam, SceneView.CameraMode mode)
    {
        // If this is the first time we're rendering this camera, create a command buffer and add it to the camera.
        if (!commandBuffers.TryGetValue(cam, out var buffer))
        {
            buffer = new CommandBuffer();
            buffer.name = $"Custom Camera Mode";
            cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
            commandBuffers[cam] = buffer;
        }
        
        // Select the right source texture and blit it into the camera target.
        var source = mode == depthCameraMode ? BuiltinRenderTextureType.Depth : BuiltinRenderTextureType.DepthNormals;
        int blitPass = mode == depthCameraMode ? depthBlitPass : normalsBlitPass;
        
        buffer.Clear();
        buffer.Blit(source, BuiltinRenderTextureType.CameraTarget, BlitMaterial, blitPass);
    }

    static void RemoveCommandBuffer(Camera cam)
    {
        if (commandBuffers.TryGetValue(cam, out var buffer))
        {
            cam.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
            commandBuffers.Remove(cam);
        }
    }
}

