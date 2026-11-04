using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [Header("UI Toolkit Reference")]
    public UIDocument uiDocument;
    public VisualTreeAsset popupTemplate;

    private VisualElement rootContainer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiDocument != null)
        {
            rootContainer = uiDocument.rootVisualElement;
        }
    }

    public void ShowDamage(Vector3 worldPosition, int damage)
    {
        if (rootContainer == null)
        {
            Debug.LogWarning("DamagePopupManager: Отсутствуют ссылки на UI Document!");
            return;
        }
        if (popupTemplate == null)
        {
            Debug.LogWarning("DamagePopupManager: Отсутствуют ссылки на Template!");
            return;
        }
        if (Camera.main == null)
        {
            Debug.LogWarning("DamagePopupManager: Отсутствуют ссылки на Main Camera!");
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        if (screenPos.z < 0) return; // Объект позади камеры

        float screenX = screenPos.x;
        float screenY = Screen.height - screenPos.y;

        Debug.Log($"Cords: X={screenX}, Y={screenY}");

        // Инстацируем шаблон
        TemplateContainer instance = popupTemplate.Instantiate();
        VisualElement popupElement = instance.Q<VisualElement>("FloatingTextRoot");
        Label damageLabel = instance.Q<Label>("DamageLabel");

        if (damageLabel != null)
        {
            damageLabel.text = $"{damage}";
        }

        if (popupElement != null)
        {
            // Убедимся, что элемент поверх остальных и абсолютный
            popupElement.style.position = Position.Absolute;
            popupElement.style.left = screenX - 50;
            popupElement.style.top = screenY - 20;

            rootContainer.Add(popupElement);
            StartCoroutine(AnimatePopup(popupElement, screenX - 50, screenY - 20));
        }
    }

    IEnumerator AnimatePopup(VisualElement element, float startX, float startY)
    {
        float duration = 0.8f;
        float elapsed = 0f;
        float moveDistance = 50f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            element.style.top = startY - (moveDistance * t);
            element.style.opacity = 1f - t;

            //yield None;
            yield return null;
        }

        element.RemoveFromHierarchy();
    }
}