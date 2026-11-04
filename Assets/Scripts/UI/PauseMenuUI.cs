using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PauseMenuUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;

    private UIDocument uiDocument;
    private VisualElement root;

    private VisualElement pauseContainer;
    private VisualElement settingsContainer;

    // Кнопки паузы и настроек
    private Button resumeBtn;
    private Button settingsBtn;
    private Button quitBtn;
    private Button backBtn;
    private Button saveBtn;

    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private Slider sensitivitySlider;

    private Label moneyLabel;
    private VisualElement backgroundOverlay;

    private bool isPaused = false;
    public bool IsPaused => isPaused;
    private Resolution[] resolutions;

    // --- МАГАЗИН ---

    private VisualElement shopMainContainer;
    private VisualElement weaponShopContainer;
    private VisualElement upgradeShopContainer;

    private Button openWeaponShopButton;
    private Button openUpgradeShopButton;
    private Button closeShopButton;
    private Button backToShopButton;
    private Button backToShopButton2;

    private bool isShopOpen = false;

    public bool IsShopOpen => isShopOpen;

    public static PauseMenuUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        uiDocument = GetComponent<UIDocument>();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
#endif
    }

    void Start()
    {
        root = uiDocument.rootVisualElement;

        // Находим элементы интерфейса
        moneyLabel = root.Q<Label>("MoneyLabel");

        pauseContainer = root.Q<VisualElement>("MainPauseContainer");
        settingsContainer = root.Q<VisualElement>("SettingsContainer");

        resumeBtn = root.Q<Button>("ResumeButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        quitBtn = root.Q<Button>("QuitButton");
        backBtn = root.Q<Button>("BackButton");
        saveBtn = root.Q<Button>("SaveButton");

        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        qualityDropdown = root.Q<DropdownField>("QualityDropdown");
        sensitivitySlider = root.Q<Slider>("SensitivitySlider");

        backgroundOverlay = root.Q<VisualElement>("BackgroundOverlay");

        shopMainContainer = root.Q<VisualElement>("ShopMainContainer");
        weaponShopContainer = root.Q<VisualElement>("WeaponShopContainer");
        upgradeShopContainer = root.Q<VisualElement>("UpgradeShopContainer");

        openWeaponShopButton = root.Q<Button>("OpenWeaponShopButton");
        openUpgradeShopButton = root.Q<Button>("OpenUpgradeShopButton");
        closeShopButton = root.Q<Button>("CloseShopButton");

        backToShopButton = root.Q<Button>("BackToShopButton");
        backToShopButton2 = root.Q<Button>("BackToShopButton2");

        if (openWeaponShopButton != null)
            openWeaponShopButton.clicked += OpenWeaponShop;

        if (openUpgradeShopButton != null)
            openUpgradeShopButton.clicked += OpenUpgradeShop;

        if (closeShopButton != null)
            closeShopButton.clicked += CloseShop;

        if (backToShopButton != null)
            backToShopButton.clicked += BackToShopMain;

        if (backToShopButton2 != null)
            backToShopButton2.clicked += BackToShopMain;

        // Подписки на события кнопок паузы/настроек
        if (resumeBtn != null) resumeBtn.clicked += Resume;
        if (settingsBtn != null) settingsBtn.clicked += OpenSettings;
        if (quitBtn != null) quitBtn.clicked += QuitGame;
        if (backBtn != null) backBtn.clicked += CloseSettings;
        if (saveBtn != null) saveBtn.clicked += SaveSettings;

        LoadSettings();

        if (resumeBtn != null) resumeBtn.Focus();
        Resume();
    }

    void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (isShopOpen)
        {
            if (weaponShopContainer != null &&
                weaponShopContainer.style.display == DisplayStyle.Flex)
            {
                BackToShopMain();
            }
            else if (upgradeShopContainer != null &&
                    upgradeShopContainer.style.display == DisplayStyle.Flex)
            {
                BackToShopMain();
            }
            else
            {
                CloseShop();
            }

            return;
        }

        if (isPaused)
        {
            if (settingsContainer != null &&
                settingsContainer.style.display == DisplayStyle.Flex)
            {
                CloseSettings();
            }
            else
            {
                Resume();
            }
        }
        else
        {
            Pause();
        }
    }

    // --- УПРАВЛЕНИЕ ДЕНЬГАМИ ---

    public void UpdateMoneyDisplay(long currentMoney)
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = $"${currentMoney:N0}";
        }
    }

    // --- СТАНДАРТНОЕ МЕНЮ ПАУЗЫ И НАСТРОЙКИ ---

    public void Resume()
    {
        isShopOpen = false;
        HideShopPages();

        if (backgroundOverlay != null)
            backgroundOverlay.style.display = DisplayStyle.None;

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.None;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;

        Time.timeScale = 1f;
        isPaused = false;

        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;
    }

    void Pause()
    {
        if (isShopOpen)
            return;

        HideShopPages();

        if (backgroundOverlay != null)
            backgroundOverlay.style.display = DisplayStyle.Flex;

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.Flex;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
        isPaused = true;

        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        if (playerController != null)
            playerController.enabled = false;
    }

    void OpenSettings()
    {
        if (pauseContainer != null) pauseContainer.style.display = DisplayStyle.None;
        if (settingsContainer != null) settingsContainer.style.display = DisplayStyle.Flex;
    }

    void CloseSettings()
    {
        LoadSettings();
        if (settingsContainer != null) settingsContainer.style.display = DisplayStyle.None;
        if (pauseContainer != null) pauseContainer.style.display = DisplayStyle.Flex;
    }

    void LoadSettings()
    {
        resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.choices = options;
            int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentIndex);
            resolutionDropdown.index = savedResIndex;
        }

        if (qualityDropdown != null)
        {
            List<string> qualityNames = new List<string>(QualitySettings.names);
            qualityDropdown.choices = qualityNames;
            int savedQuality = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
            qualityDropdown.index = savedQuality;
            QualitySettings.SetQualityLevel(savedQuality);
        }

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", playerController != null ? playerController.mouseSensitivity : 0.2f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.RegisterValueChangedCallback(evt => {
                float snappedValue = Mathf.Round(evt.newValue / 0.05f) * 0.05f;
                if (Mathf.Abs(sensitivitySlider.value - snappedValue) > 0.001f)
                {
                    sensitivitySlider.value = snappedValue;
                }
                if (playerController != null)
                {
                    playerController.mouseSensitivity = snappedValue;
                }
            });
        }
    }

    public void SaveSettings()
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = sensitivitySlider.value;
            PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);
        }

        if (qualityDropdown != null)
        {
            int selectedQuality = qualityDropdown.index;
            QualitySettings.SetQualityLevel(selectedQuality);
            PlayerPrefs.SetInt("QualityIndex", selectedQuality);
        }

        if (resolutionDropdown != null)
        {
            int selectedRes = resolutionDropdown.index;
            if (selectedRes >= 0 && selectedRes < resolutions.Length)
            {
                Resolution res = resolutions[selectedRes];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionIndex", selectedRes);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Настройки успешно сохранены!");
        CloseSettings();
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Открытие главного меню магазина
    public void OpenShop()
    {
        if (isPaused || isShopOpen)
            return;

        isShopOpen = true;

        // Сначала скрываем все страницы
        HideShopPages();

        // Затем показываем главное окно
        if (backgroundOverlay != null)
            backgroundOverlay.style.display = DisplayStyle.Flex;

        if (shopMainContainer != null)
            shopMainContainer.style.display = DisplayStyle.Flex;

        Time.timeScale = 1f;

        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        if (playerController != null)
            playerController.enabled = false;

        Debug.Log("Магазин открыт");
    }

    // Покупка оружия
    private void OpenWeaponShop()
    {
        HideShopPages();

        if (weaponShopContainer != null)
            weaponShopContainer.style.display = DisplayStyle.Flex;
    }

    // Улучшения
    private void OpenUpgradeShop()
    {
        HideShopPages();

        if (upgradeShopContainer != null)
            upgradeShopContainer.style.display = DisplayStyle.Flex;
    }

    // Назад к главному меню магазина
    private void BackToShopMain()
    {
        HideShopPages();

        if (shopMainContainer != null)
            shopMainContainer.style.display = DisplayStyle.Flex;
    }

    // Скрыть все страницы магазина
    private void HideShopPages()
    {
        if (shopMainContainer != null)
            shopMainContainer.style.display = DisplayStyle.None;

        if (weaponShopContainer != null)
            weaponShopContainer.style.display = DisplayStyle.None;

        if (upgradeShopContainer != null)
            upgradeShopContainer.style.display = DisplayStyle.None;
    }

    // Закрыть магазин
    public void CloseShop()
    {
        isShopOpen = false;

        HideShopPages();

        if (backgroundOverlay != null)
            backgroundOverlay.style.display = DisplayStyle.None;

        Resume();
    }
}