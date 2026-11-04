using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Object Info")]
    public string objectName = "Бумажная купюра";
    public long moneyReward = 100;

    [Header("Health & Armor")]
    public int maxHealth = 10;     
    private int currentHealth;

    [Tooltip("Минимальный урон оружия, необходимый, чтобы пробить этот объект. Если урон оружия ниже — объект неуязвим.")]
    public int minDamageToHurt = 1;

    [Header("Hit Feedback Settings")]
    public float punchScaleMultiplier = 1.15f; 
    public float punchDuration = 0.12f;      

    private Vector3 originalScale;
    private Coroutine punchRoutine;

    void Start()
    {
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    // Метод получения урона с проверкой порога пробития
    public bool TakeDamage(int weaponDamage, out long earned)
    {
        earned = 0;

        // Проверяем защиту объекта
        if (weaponDamage < minDamageToHurt)
        {
            Debug.Log(
                $"{objectName} слишком прочный! " +
                $"Требуется минимум {minDamageToHurt} урона " +
                $"(у вас: {weaponDamage})."
            );

            return false;
        }

        currentHealth -= weaponDamage;

        // Эффект визуального отклика
        if (punchRoutine != null)
            StopCoroutine(punchRoutine);

        punchRoutine = StartCoroutine(PunchScaleEffect());

        // Проверяем уничтожение объекта
        if (currentHealth <= 0)
        {
            earned = moneyReward;
            Destroy(gameObject);
        }

        return true;
    }
    // public long TakeDamage(int weaponDamage)
    // {
    //     // Проверяем, хватает ли урона оружия для пробития защиты объекта
    //     if (weaponDamage < minDamageToHurt)
    //     {
    //         Debug.Log($"{objectName} слишком прочный! Требуется минимум {minDamageToHurt} урона (у вас: {weaponDamage}).");
    //         // Здесь можно вызвать другой визуальный эффект (например, легкий «звенящий» отскок вместо пульсации)
    //         return 0; 
    //     }

    //     currentHealth -= weaponDamage;

    //     // Эффект визуального отклика (пульс) при успешном ударе
    //     if (punchRoutine != null)
    //     {
    //         StopCoroutine(punchRoutine);
    //     }
    //     punchRoutine = StartCoroutine(PunchScaleEffect());

    //     // Если прочность исчерпана — уничтожаем объект и возвращаем награду
    //     if (currentHealth <= 0)
    //     {
    //         Destroy(gameObject);
    //         return moneyReward;
    //     }

    //     return 0; 
    // }

    IEnumerator PunchScaleEffect()
    {
        float elapsed = 0f;
        Vector3 targetScale = originalScale * punchScaleMultiplier;
        float halfDuration = punchDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        punchRoutine = null;
    }
}