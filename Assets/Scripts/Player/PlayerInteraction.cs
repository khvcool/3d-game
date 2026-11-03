using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Максимальная дистанция взаимодействия с деньгами")]
    public float interactionRange = 5f;

    [Tooltip("Слой, на котором находятся пачки денег")]
    public LayerMask moneyLayer;

    private long currentMoney = 0;

    void Update()
    {
        // Проверяем клик левой кнопкой мыши (или тапа)
        // Убедимся, что игра не на паузе (Time.timeScale > 0)
        if (Time.timeScale > 0 && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        // Создаем луч из центра камеры вперед
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Выпускаем луч
        if (Physics.Raycast(ray, out hit, interactionRange, moneyLayer))
        {
            // Проверяем, попали ли мы по объекту с компонентом денег
            MoneyObject money = hit.collider.GetComponent<MoneyObject>();
            if (money != null)
            {
                // Наносим урон / уничтожаем пачку
                long earned = money.TakeDamage();
                AddMoney(earned);
            }
        }
    }

    void AddMoney(long amount)
    {
        currentMoney += amount;

        // Обновляем UI через наш класс PauseMenuUI
        if (PauseMenuUI.Instance != null)
        {
            PauseMenuUI.Instance.UpdateMoneyDisplay(currentMoney);
        }

        Debug.Log($"Заработано! Всего денег: ${currentMoney}");
    }
}