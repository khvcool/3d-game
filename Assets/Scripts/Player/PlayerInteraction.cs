using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 5f;
    public LayerMask interactableLayer;
    [Header("Computer Interaction")]
    public Key interactionKey = Key.E;

    [Header("Weapon Stats")]
    public string weaponName = "Ножницы";
    public int weaponDamage = 1;
    public float attacksPerSecond = 1.0f;

    [Header("Economy")]
    [SerializeField] private long currentMoney = 0; // Реальный баланс денег игрока

    private float nextAttackTime = 0f;

    // Публичное свойство/метод для проверки баланса из магазина
    public long CurrentMoney => currentMoney;

    void Update()
    {
        // Взаимодействие с компьютером
        if (Time.timeScale > 0 &&
            Keyboard.current != null &&
            Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            TryOpenShop();
        }

        // Атака
        if (Time.timeScale > 0 &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= nextAttackTime)
            {
                ShootRaycast();

                float cooldown = 1f / Mathf.Max(attacksPerSecond, 0.1f);
                nextAttackTime = Time.time + cooldown;
            }
        }
    }

    void TryOpenShop()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.CompareTag("Computer"))
            {
                if (PauseMenuUI.Instance != null)
                {
                    PauseMenuUI.Instance.OpenShop();
                }
            }
        }
    }

    void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            DestructibleObject target =
                hit.collider.GetComponent<DestructibleObject>();

            if (target != null)
            {
                bool damageApplied = target.TakeDamage(
                    weaponDamage,
                    out long earned
                );

                // Показываем цифры только при успешном нанесении урона
                if (damageApplied)
                {
                    if (DamagePopupManager.Instance != null)
                    {
                        DamagePopupManager.Instance.ShowDamage(
                            hit.point + Vector3.up * 0.1f,
                            weaponDamage
                        );
                    }

                    if (earned > 0)
                        AddMoney(earned);
                }
            }
        }
    }
    // void ShootRaycast()
    // {
    //     Ray ray = new Ray(transform.position, transform.forward);
    //     RaycastHit hit;

    //     if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
    //     {
    //         DestructibleObject target = hit.collider.GetComponent<DestructibleObject>();
    //         if (target != null)
    //         {
    //             long earned = target.TakeDamage(weaponDamage);
                
    //             // Спавним цифру урона через UI Toolkit Менеджер
    //             if (DamagePopupManager.Instance != null)
    //             {
    //                 DamagePopupManager.Instance.ShowDamage(hit.point + Vector3.up * 0.1f, weaponDamage);
    //             }

    //             if (earned > 0)
    //             {
    //                 AddMoney(earned);
    //             }
    //         }
    //     }
    // }

    public void AddMoney(long amount)
    {
        currentMoney += amount;
        
        // Обновляем отображение в UI через PauseMenuUI
        if (PauseMenuUI.Instance != null)
        {
            PauseMenuUI.Instance.UpdateMoneyDisplay(currentMoney);
        }
    }

    // Метод списания денег для магазина
    public bool TrySpendMoney(long amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            
            // Обновляем UI
            if (PauseMenuUI.Instance != null)
            {
                PauseMenuUI.Instance.UpdateMoneyDisplay(currentMoney);
            }
            return true;
        }
        return false;
    }

    public void UpgradeWeaponStats(int newDamage, float newSpeed, string newName)
    {
        weaponDamage = newDamage;
        attacksPerSecond = newSpeed;
        weaponName = newName;
        Debug.Log($"Оружие изменено/прокачано: {weaponName} | Урон: {weaponDamage} | Скорость: {attacksPerSecond}/сек");
    }

    public void RefreshWeaponStats(
        int newDamage,
        float newSpeed,
        string newName)
    {
        weaponDamage = newDamage;
        attacksPerSecond = newSpeed;
        weaponName = newName;
    }
}