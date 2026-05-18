using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem current;

    public GameObject tooltipObject;
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public RectTransform rectTransform;

    private void Awake()
    {
        current = this;
        tooltipObject.SetActive(false);
        rectTransform = tooltipObject.GetComponent<RectTransform>();
    }

    public static void Show(string content, string header = "")
    {
        current.headerField.text = header;
        current.contentField.text = content;
        current.tooltipObject.SetActive(true);
        
        // Ukrywamy nagłówek, jeśli jest pusty
        current.headerField.gameObject.SetActive(!string.IsNullOrEmpty(header));
    }

    public static void Hide()
    {
        current.tooltipObject.SetActive(false);
    }

    private void Update()
    {
        if (!tooltipObject.activeSelf) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 1. Obliczamy pivot (to już masz i działa dobrze)
        float pivotX = mousePosition.x / Screen.width;
        float pivotY = mousePosition.y / Screen.height;
        
        float safePivotX = Mathf.Clamp(pivotX, 0.05f, 0.95f);
        float safePivotY = Mathf.Clamp(pivotY, 0.05f, 0.95f);
        rectTransform.pivot = new Vector2(safePivotX, safePivotY);

        // 2. Obliczamy offset (odstęp od kursora)
        float offsetX = (pivotX > 0.5f) ? -20f : 20f;
        float offsetY = (pivotY > 0.5f) ? -20f : 20f;

        // 3. KLUCZOWA ZMIANA: Ustawiamy pozycję bezpośrednio na rectTransform
        // Jeśli TooltipSystem jest na tym samym obiekcie co tło tooltipa:
        rectTransform.position = mousePosition + new Vector2(offsetX, offsetY);
    }
}