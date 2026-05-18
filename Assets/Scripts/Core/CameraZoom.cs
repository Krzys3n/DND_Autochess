using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f; 
    public float minZoom = 2f;
    public float maxZoom = 10f;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Sprawdzamy, czy mysz jest nad interfejsem użytkownika (UI)
        if (IsPointerOverRealUI())
        {
            return;
        }

        Vector2 scrollValue = Mouse.current.scroll.ReadValue();

        if (scrollValue.y != 0)
        {
            float normalization = scrollValue.y / 120f;
            ZoomCamera(normalization);
        }
    }

    // Nowa, precyzyjna metoda sprawdzająca, czy pod myszką jest faktycznie UI
    private bool IsPointerOverRealUI()
    {
        if (EventSystem.current == null) return false;

        // Standardowe sprawdzenie EventSystemu
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        // Tworzymy paczkę danych o pozycji myszy
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        // Lista na wyniki "strzału" (raycastu)
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Przeszukujemy to, co trafiła myszka
        foreach (RaycastResult result in results)
        {
            // Jeśli trafiony obiekt jest na warstwie "UI", blokujemy zoom
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }

        // Jeśli trafiło kafelki lub jednostki (nie na warstwie UI), pozwalamy na zoom
        return false;
    }

    void ZoomCamera(float increment)
    {
        float newSize = cam.orthographicSize - (increment * zoomSpeed);
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }
}