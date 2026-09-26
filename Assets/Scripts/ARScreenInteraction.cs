using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Facilitates direct screen tap and click interaction with AR business card UI elements.
/// Raycasts from screen coordinates to trigger buttons reliably on mobile and desktop.
/// </summary>
public class ARScreenInteraction : MonoBehaviour
{
    /// <summary>
    /// The camera used for raycasting against the AR scene.
    /// </summary>
    [Tooltip("The AR Camera or Main Camera.")]
    public Camera arCamera;

    /// <summary>
    /// Executes raycasting against 3D colliders and UI graphics at the given screen point.
    /// </summary>
    /// <param name="screenPosition">Screen coordinates of the tap or click.</param>
    public void ProcessScreenPoint(Vector2 screenPosition)
    {
        // Check UI Raycasting first
        if (EventSystem.current != null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                SocialButton socialBtn = result.gameObject.GetComponentInParent<SocialButton>();
                if (socialBtn != null)
                {
                    socialBtn.OpenLink();
                    return;
                }
            }
        }

        // Check 3D Physics Raycast if buttons have colliders
        if (arCamera != null)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                SocialButton socialBtn = hit.collider.GetComponentInParent<SocialButton>();
                if (socialBtn != null)
                {
                    socialBtn.OpenLink();
                }
            }
        }
    }

    // Resolves default camera if none assigned
    private void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    // Polls input for mouse click or mobile touch taps
    private void Update()
    {
        // Handle mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessScreenPoint(touch.position);
            }
            return;
        }

        // Handle desktop mouse click
        if (Input.GetMouseButtonDown(0))
        {
            ProcessScreenPoint(Input.mousePosition);
        }
    }
}
