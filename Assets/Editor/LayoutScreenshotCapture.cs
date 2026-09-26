using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor utility that captures a clean screenshot of the AR business card layout.
/// </summary>
public static class LayoutScreenshotCapture
{
    /// <summary>
    /// Captures and saves the business card layout screenshot.
    /// </summary>
    [MenuItem("AR/Capture Layout Screenshot")]
    public static void Capture()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ARBusinessCard.unity");

        GameObject camGo = new GameObject("ScreenshotCam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        
        camGo.transform.position = new Vector3(0.01f, 0.35f, 0f);
        camGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.orthographic = true;
        cam.orthographicSize = 0.065f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1f;

        int width = 1200;
        int height = 550;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camGo);

        byte[] bytes = screenShot.EncodeToPNG();
        string outPath1 = "Assets/Textures/layout_screenshot.png";
        string outPath2 = "0-layout.png";
        File.WriteAllBytes(outPath1, bytes);
        File.WriteAllBytes(outPath2, bytes);
        Debug.Log("Screenshot saved to: " + outPath1 + " and " + outPath2);

        AssetDatabase.Refresh();
    }
}
