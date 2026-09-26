using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;
using Vuforia;

/// <summary>
/// Editor script that automates the construction of the ARBusinessCard scene,
/// configures texture import settings, sets up Vuforia Image Target tracking,
/// builds the UI layout, and configures EditorBuildSettings.
/// </summary>
public static class ARBusinessCardSceneBuilder
{
    /// <summary>
    /// Master build method that constructs the complete ARBusinessCard scene.
    /// </summary>
    [MenuItem("AR/Build Business Card Scene")]
    public static void BuildScene()
    {
        Debug.Log("Starting ARBusinessCard Scene Build...");

        // 1. Configure Texture Import Settings
        ConfigureTextureImporter("Assets/Textures/TargetMarker.png", TextureImporterType.Default, false, true);
        ConfigureTextureImporter("Assets/Textures/card_background.png", TextureImporterType.Sprite, true, true);
        ConfigureTextureImporter("Assets/Textures/icon_email.png", TextureImporterType.Sprite, true, true);
        ConfigureTextureImporter("Assets/Textures/icon_twitter.png", TextureImporterType.Sprite, true, true);
        ConfigureTextureImporter("Assets/Textures/icon_linkedin.png", TextureImporterType.Sprite, true, true);
        ConfigureTextureImporter("Assets/Textures/icon_github.png", TextureImporterType.Sprite, true, true);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // 2. Load Assets
        Texture2D markerTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/TargetMarker.png");
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/card_background.png");
        Sprite emailSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/icon_email.png");
        Sprite twitterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/icon_twitter.png");
        Sprite linkedinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/icon_linkedin.png");
        Sprite githubSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/icon_github.png");
        AudioClip clickAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ButtonClick.wav");

        // Standard Font
        Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (standardFont == null)
        {
            standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // 3. Create New Empty Scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4. Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = Color.white;
        dirLight.intensity = 1.0f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 5. EventSystem
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        
        var inputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModuleType != null)
        {
            eventSystemObj.AddComponent(inputModuleType);
        }
        else
        {
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        // 6. Vuforia AR Camera
        GameObject arCameraObj = Vuforia.EditorClasses.GameObjectFactory.CreateARCamera();
        arCameraObj.name = "ARCamera";
        arCameraObj.tag = "MainCamera";
        Camera cam = arCameraObj.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        // Add ARScreenInteraction to camera
        ARScreenInteraction screenInteraction = arCameraObj.AddComponent<ARScreenInteraction>();
        screenInteraction.arCamera = cam;

        // 7. Vuforia Image Target
        GameObject imageTargetObj = Vuforia.EditorClasses.GameObjectFactory.CreateImageTarget();
        imageTargetObj.name = "ImageTarget";
        imageTargetObj.transform.position = Vector3.zero;
        imageTargetObj.transform.rotation = Quaternion.identity;

        ImageTargetBehaviour itb = imageTargetObj.GetComponent<ImageTargetBehaviour>();
        if (itb != null)
        {
            SetImageTargetProperties(itb, markerTex, 0.1f);
        }

        // 8. Business Card Root (Child of Image Target)
        GameObject cardRootObj = new GameObject("BusinessCardLayout");
        cardRootObj.transform.SetParent(imageTargetObj.transform, false);
        // Lie flat on the marker (facing camera looking down +Y) with slight elevation
        cardRootObj.transform.localPosition = new Vector3(0f, 0.002f, 0f);
        cardRootObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // World Space Canvas
        Canvas canvas = cardRootObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        RectTransform canvasRect = cardRootObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(960f, 480f);
        canvasRect.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);

        cardRootObj.AddComponent<CanvasScaler>();
        cardRootObj.AddComponent<GraphicRaycaster>();

        // Glassmorphic Backing Plate
        if (bgSprite != null)
        {
            GameObject bgObj = new GameObject("CardBacking");
            bgObj.transform.SetParent(cardRootObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(25f, 0f);
            bgRect.sizeDelta = new Vector2(940f, 440f);
            UIImage bgImg = bgObj.AddComponent<UIImage>();
            bgImg.sprite = bgSprite;
            bgImg.type = UIImage.Type.Sliced;
            bgImg.color = new Color(1f, 1f, 1f, 0.92f);
            bgImg.raycastTarget = false;
        }

        // Marker visual plane
        GameObject markerPlane = new GameObject("MarkerPlane");
        markerPlane.transform.SetParent(cardRootObj.transform, false);
        RectTransform planeRect = markerPlane.AddComponent<RectTransform>();
        planeRect.anchoredPosition = new Vector2(-180f, 0f);
        planeRect.sizeDelta = new Vector2(240f, 240f);
        UIImage markerImg = markerPlane.AddComponent<UIImage>();
        if (markerTex != null)
        {
            Sprite markerSprite = Sprite.Create(markerTex, new Rect(0, 0, markerTex.width, markerTex.height), new Vector2(0.5f, 0.5f));
            markerImg.sprite = markerSprite;
        }
        markerImg.color = new Color(1f, 1f, 1f, 0.98f);
        markerImg.raycastTarget = false;

        // Text Elements
        // Name Text: "Chibueze Victor Ifegwu"
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(cardRootObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchoredPosition = new Vector2(170f, 40f);
        nameRect.sizeDelta = new Vector2(560f, 75f);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = standardFont;
        nameText.text = "Chibueze Victor Ifegwu";
        nameText.fontSize = 42;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = new Color(0.937f, 0.306f, 0.243f, 1f); // #EF4E3E Coral Red
        nameText.raycastTarget = false;

        // Title Text: "AR/VR Developer"
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(cardRootObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(170f, -28f);
        titleRect.sizeDelta = new Vector2(560f, 60f);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = standardFont;
        titleText.text = "AR/VR Developer";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Italic;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.color = new Color(0.40f, 0.90f, 0.82f, 1f); // Vibrant Teal/Cyan #66E6D1
        titleText.raycastTarget = false;

        // Create the 4 Social Buttons
        List<Transform> buttonTransforms = new List<Transform>();

        // 1. Email Button (Top)
        GameObject emailBtnObj = CreateButton(cardRootObj.transform, "EmailButton", new Vector2(-180f, 155f), emailSprite, "mailto:c.ifegwu@alustudent.com", clickAudio);
        buttonTransforms.Add(emailBtnObj.transform);

        // 2. Twitter / X Button (Upper-Left)
        GameObject twitterBtnObj = CreateButton(cardRootObj.transform, "TwitterButton", new Vector2(-325f, 78f), twitterSprite, "https://x.com/chibueze_ifegwu", clickAudio);
        buttonTransforms.Add(twitterBtnObj.transform);

        // 3. LinkedIn Button (Lower-Left)
        GameObject linkedinBtnObj = CreateButton(cardRootObj.transform, "LinkedInButton", new Vector2(-325f, -78f), linkedinSprite, "https://www.linkedin.com/in/chibueze-ifegwu/", clickAudio);
        buttonTransforms.Add(linkedinBtnObj.transform);

        // 4. GitHub Button (Bottom)
        GameObject githubBtnObj = CreateButton(cardRootObj.transform, "GitHubButton", new Vector2(-180f, -155f), githubSprite, "https://github.com/C-ifegwu", clickAudio);
        buttonTransforms.Add(githubBtnObj.transform);

        // 9. Add BusinessCardAnimator
        BusinessCardAnimator cardAnimator = cardRootObj.AddComponent<BusinessCardAnimator>();
        cardAnimator.buttonTransforms = buttonTransforms;
        cardAnimator.nameTransform = nameObj.transform;
        cardAnimator.titleTransform = titleObj.transform;
        cardAnimator.entranceDuration = 0.8f;
        cardAnimator.hoverAmplitude = 0.005f;
        cardAnimator.hoverFrequency = 2.0f;

        // 10. Hook animator to Vuforia's DefaultObserverEventHandler
        DefaultObserverEventHandler observerHandler = imageTargetObj.GetComponent<DefaultObserverEventHandler>();
        if (observerHandler != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(observerHandler.OnTargetFound, cardAnimator.PlayIntroAnimation);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(observerHandler.OnTargetLost, cardAnimator.ResetAnimation);
        }

        // 11. Save Scene
        string scenePath = "Assets/Scenes/ARBusinessCard.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Scene saved successfully at: " + scenePath);

        // 12. Configure EditorBuildSettings
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("Configured EditorBuildSettings scenes: " + scenePath);

        Debug.Log("ARBusinessCard Scene Build COMPLETED successfully!");
    }

    // Helper method to instantiate and configure a social button
    private static GameObject CreateButton(Transform parent, string name, Vector2 anchoredPosition, Sprite sprite, string url, AudioClip clickSound)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(108f, 108f);

        UIImage img = btnObj.AddComponent<UIImage>();
        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.88f, 0.88f, 1f);
        colors.pressedColor = new Color(0.9f, 0.55f, 0.55f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        SocialButton social = btnObj.AddComponent<SocialButton>();
        social.targetUrl = url;
        social.clickSound = clickSound;
        social.pressScaleFactor = 0.88f;

        // Add BoxCollider for physics raycasting fallback
        BoxCollider boxCol = btnObj.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(108f, 108f, 2f);

        return btnObj;
    }

    // Helper method to configure asset import parameters
    private static void ConfigureTextureImporter(string path, TextureImporterType type, bool isSprite, bool isReadable)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = type;
            if (isSprite)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
            }
            importer.isReadable = isReadable;
            importer.SaveAndReimport();
        }
    }

    // Helper method to assign Vuforia image target runtime properties
    private static void SetImageTargetProperties(ImageTargetBehaviour itb, Texture2D tex, float width)
    {
        try
        {
            var propType = typeof(ImageTargetBehaviour).GetProperty("ImageTargetType");
            if (propType != null && propType.CanWrite)
            {
                propType.SetValue(itb, ImageTargetType.INSTANT);
            }
            else
            {
                var fieldType = typeof(ImageTargetBehaviour).GetField("mImageTargetType", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fieldType != null)
                {
                    fieldType.SetValue(itb, ImageTargetType.INSTANT);
                }
            }

            var fieldTex = typeof(ImageTargetBehaviour).GetField("mRuntimeTexture", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldTex != null)
            {
                fieldTex.SetValue(itb, tex);
            }

            itb.SetWidth(width);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SetImageTargetProperties exception: " + ex.Message);
        }
    }
}
