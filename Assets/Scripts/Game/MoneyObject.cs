using UnityEngine;
using System.Collections;

public class MoneyObject : MonoBehaviour
{
    [Header("Money Settings")]
    public long moneyValue = 100; // Сколько денег дает эта пачка при уничтожении
    public int health = 5;       // Количество кликов для уничтожения

    [Header("Hit Feedback Settings")]
    public float shakeDuration = 0.1f; // Длительность вибрации в секундах
    public float shakeMagnitude = 0.05f; // Сила (амплитуда) вибрации

    private Vector3 originalLocalPosition;
    private bool isShaking = false;

    void Start()
    {
        // Запоминаем исходную позицию пачки относительно родителя
        originalLocalPosition = transform.localPosition;
    }

    // Метод вызывается, когда игрок кликает по пачке
    public long TakeDamage()
    {
        health--;

        // Запускаем эффект вибрации при каждом клике
        StartCoroutine(ShakeEffect());

        if (health <= 0)
        {
            long reward = moneyValue;

            // Уничтожаем объект пачки
            Destroy(gameObject);

            // Возвращаем сумму денег, которую игрок заработал
            return reward;
        }

        return 0; // Если пачка еще не разрушена, денег пока не даем
    }

    IEnumerator ShakeEffect()
    {
        if (isShaking) yield break;
        isShaking = true;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Генерируем случайное смещение в небольтном радиусе
            Vector3 randomPoint = originalLocalPosition + Random.insideUnitSphere * shakeMagnitude;
            transform.localPosition = randomPoint;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Возвращаем объект точно на исходное место
        transform.localPosition = originalLocalPosition;
        isShaking = false;
    }
}