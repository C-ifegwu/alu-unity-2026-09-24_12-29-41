# Unity - AR Business Card

## Overview
This project is an interactive Augmented Reality (AR) business card built with **Unity** and the **Vuforia Engine SDK**. When a mobile device or webcam points at the target image marker (the seahorse business card logo), an interactive holographic business card anchors to the physical marker, tracking its position, rotation, and distance in real time.

---

## AR Concepts & Theory

### 1. What is Vuforia?
**Vuforia Engine** is an enterprise-grade Augmented Reality software development kit (SDK) that uses computer vision to recognize and track planar images (Image Targets), 3D objects (Model Targets), cylinders, barcodes, and environments.

#### Advantages:
* **Robust Image Tracking:** High tracking accuracy with low latency and jitter.
* **Extended Tracking:** Continues tracking virtual augmentations even when the physical marker is temporarily occluded or moves out of the camera's field of view.
* **Cross-Platform:** Single codebase for Android (ARCore), iOS (ARKit), and Windows/macOS Play Mode webcam.
* **Easy Unity Integration:** Integrates into Unity's XR pipeline via the Vuforia Engine package.

#### Disadvantages:
* **Licensing Requirements:** Free tier displays a small Vuforia watermark in the lower corner and requires paid tiers for commercial distribution.
* **Lighting Sensitivity:** Poor ambient lighting or extreme surface reflections can reduce feature detection.

---

### 2. What is Marker-Based Augmented Reality?
Marker-based AR relies on predefined visual markers (such as high-contrast 2D images, QR codes, or patterns) that act as real-world anchors. The computer vision pipeline searches the incoming camera frames for recognizable optical features (corners, edges, gradients). Once detected, the SDK computes the 6-DoF (Degrees of Freedom: X, Y, Z translation and pitch, yaw, roll rotation) pose of the marker relative to the camera and positions the virtual 3D augmentations accordingly.

---

### 3. Choosing and Optimizing Images for Target Detection
To achieve maximum tracking stability (a 5-star rating in Vuforia Target Manager):
* **High Contrast:** Strong tonal differences between adjacent shapes.
* **Rich Feature Distribution:** Non-repetitive, asymmetrical patterns distributed across the entire surface (corners, edges, varied shapes).
* **Avoid Gradients and Solid Colors:** Smooth color gradients or large untextured solid blocks do not produce feature points.
* **Avoid Repetitive Patterns:** Checkered or striped patterns confuse feature point correspondence.
* **Appropriate Physical Aspect Ratio:** Target dimensions specified in Unity must match physical printed dimensions in meters (e.g., 0.1m for a 10cm card).

---

### 4. UI / UX Design in Augmented Reality
* **World-Space Canvas:** Unlike traditional screen-space UI, AR UI exists within the 3D coordinate space of the physical environment.
* **Physical Scale:** Canvas scale must correspond to real-world ergonomics (0.00025 local scale with 900x450 canvas produces a realistic 22.5cm x 11.2cm business card).
* **Accessibility & Hit Zones:** Interactive buttons are sized at 105x105 pixels to ensure effortless finger tapping on touchscreens.
* **Feedback (Audio & Visual):** Every button press delivers instantaneous dual feedback (smooth bounce animation, color tint, and audible click sound) to confirm interaction in 3D space.

---

## Project Structure & Architecture

```text
unity_ar_business_card/
├── Assets/
│   ├── Audio/
│   │   ├── ButtonClick.wav         # Crisp UI click feedback sound
│   │   └── ButtonClick.wav.meta
│   ├── Editor/
│   │   ├── ARBusinessCardSceneBuilder.cs # Scene automation builder
│   │   └── LayoutScreenshotCapture.cs   # Automated layout capture
│   ├── Resources/
│   │   └── VuforiaConfiguration.asset   # Vuforia license & engine settings
│   ├── Scenes/
│   │   ├── ARBusinessCard.unity    # Main AR scene
│   │   └── ARBusinessCard.unity.meta
│   ├── Scripts/
│   │   ├── ARScreenInteraction.cs  # Mobile touch & mouse raycast interaction
│   │   ├── BusinessCardAnimator.cs # Intro fan-out & idle floating hovering
│   │   └── SocialButton.cs         # URL launcher, audio/visual feedback
│   └── Textures/
│       ├── icon_email.png          # @ Email button icon
│       ├── icon_github.png         # Octocat GitHub button icon
│       ├── icon_linkedin.png       # in LinkedIn button icon
│       ├── icon_twitter.png        # Twitter bird button icon
│       ├── layout_screenshot.png   # 1200x550 layout preview
│       └── TargetMarker.png        # Seahorse AR marker texture
├── Builds/
│   ├── Android/                    # Output directory for ARBusinessCard.apk
│   └── iOS/                        # Output directory for iOS Xcode project
├── 0-layout                        # Task 0 layout screenshot link file
├── 0-layout.png                    # Static layout screenshot
└── README.md                       # Project documentation
```

---

## Tasks Breakdown

### Task 0: Business Card Static Layout
* Scene: `Assets/Scenes/ARBusinessCard.unity`
* Static layout containing:
  * Full Name: **Chibueze Victor Ifegwu** (Bold coral red `#EF4E3E`)
  * Profession: **AR/VR Developer** (Italic soft teal `#9ED4C7`)
  * Target Marker: Seahorse AR marker
  * Four interactive buttons:
    * **Email** (`@`)
    * **Twitter / X**
    * **LinkedIn**
    * **GitHub**
* File: `0-layout` (Contains the URL to the layout screenshot)

### Task 1: Vuforia Image Target Tracking
* `ARCamera` handles video background capture and device tracking.
* `ImageTarget` anchors the business card layout in world space.
* `DefaultObserverEventHandler` automatically enables the card UI when the marker is tracked and hides all elements when the marker leaves camera view.

### Task 2: Dynamic AR Animations
* `BusinessCardAnimator.cs`:
  * **Entrance Animation:** Buttons fan out smoothly from the marker center with an elastic ease-out spring effect, while text slides in.
  * **Idle Hover Animation:** Subtle harmonic sinusoidal floating motion on buttons, providing a holographic 3D appearance.
  * **Reset Behavior:** Automatically resets when target tracking is lost, replayable on re-acquisition.

### Task 3: Interactive Links & Feedback
* `SocialButton.cs` and `ARScreenInteraction.cs`:
  * **Email Link:** Opens `mailto:c.ifegwu@alustudent.com`
  * **Twitter Link:** Opens `https://x.com/chibueze_ifegwu`
  * **LinkedIn Link:** Opens `https://www.linkedin.com/in/chibueze-ifegwu/`
  * **GitHub Link:** Opens `https://github.com/C-ifegwu`
  * **Visual Feedback:** Button punch scale (0.88x) and color tint highlight on press.
  * **Audio Feedback:** Plays `ButtonClick.wav` on press.
* **Documentation Compliance:** All public classes and members have XML documentation tags (`/// <summary>`), and all private members have standard comments (`//`).

---

## Building and Testing Guide (Task 4)

Follow these step-by-step instructions to produce the Android and iOS builds.

### Prerequisites
* **Android Build Support:** Ensure the Android Build Support module (Android SDK & NDK Tools, OpenJDK) is installed in Unity Hub.
* **iOS Build Support (macOS only):** Xcode installed on macOS for building the iOS package.

---

### Step 1: Open the Project in Unity
1. Open **Unity Hub**.
2. Click **Open** and select the `unity_ar_business_card` directory.
3. Open the scene `Assets/Scenes/ARBusinessCard.unity`.

---

### Step 2: Android Build (`ARBusinessCard.apk`)
1. In Unity, go to **File > Build Settings...**
2. In the **Scenes In Build** list, ensure `Assets/Scenes/ARBusinessCard.unity` is checked as Scene `0`.
3. Under **Platform**, select **Android** and click **Switch Platform**.
4. Click **Player Settings...**:
   * **Player > Other Settings > Identification**:
     * Set **Package Name** to: `com.holberton.arbusinesscard`
     * Set **Minimum API Level** to: `Android 8.0 'Oreo' (API Level 26)` or higher.
     * Set **Target API Level** to: `Automatic (highest installed)`.
   * **Player > Other Settings > Configuration**:
     * Set **Scripting Backend** to: `IL2CPP`.
     * Set **Target Architectures**: Check `ARMv7` and `ARM64`.
5. Close Project Settings and return to **Build Settings**.
6. Click **Build**:
   * Choose destination folder: `unity_ar_business_card/Builds/Android/`
   * Set filename: `ARBusinessCard.apk`
   * Click **Save** and wait for the compilation to complete.

---

### Step 3: iOS Build (Xcode Project)
1. In **Build Settings**, select **iOS** and click **Switch Platform**.
2. Click **Player Settings...**:
   * **Player > Other Settings > Identification**:
     * Set **Bundle Identifier** to: `com.holberton.arbusinesscard`
   * **Player > Other Settings > Camera Usage Description**:
     * Enter: `Camera access is required for Vuforia Augmented Reality tracking.`
3. Close Project Settings.
4. Click **Build**:
   * Choose destination folder: `unity_ar_business_card/Builds/iOS/`
   * Unity will export the complete Xcode project files into `Builds/iOS/`.
   * *(If on macOS with Xcode)*: Open the `.xcodeproj` file in Xcode, select your development team under **Signing & Capabilities**, and build/archive to your iOS device.

---

### Step 4: Compressing Builds for Submission
Once builds are generated, create the two zip files required for task submission:

* **iOS Zip:** Compress the contents of `Builds/iOS/` into:
  ```text
  unity-ar_business_card-iOS.zip
  ```
* **Android Zip:** Compress `Builds/Android/ARBusinessCard.apk` into:
  ```text
  unity-ar_business_card-Android.zip
  ```

Upload both `.zip` files to Google Drive or Dropbox and paste the sharing links into your intranet submission form.

---

## Author
* **Chibueze Victor Ifegwu** - AR/VR Developer
* Holberton School / ALU AR/VR Track

