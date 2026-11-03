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

    // Кнопки
    private Button resumeBtn;
    private Button settingsBtn;
    private Button quitBtn;
    private Button backBtn;
    private Button saveBtn; // Новая кнопка сохранения

    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private Slider sensitivitySlider;

    private Label moneyLabel;

    private VisualElement backgroundOverlay;

    private bool isPaused = false;
    private Resolution[] resolutions;

    // Временные переменные для несохраненных настроек
    private float tempSensitivity;
    private int tempQuality;
    private int tempResolutionIndex;

    public static PauseMenuUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
            else Destroy(gameObject);

        uiDocument = GetComponent<UIDocument>();

        // Если мы находимся в редакторе и игра НЕ запущена — скрываем UI документа
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

        // Находим лейбл денег в UI
        moneyLabel = root.Q<Label>("MoneyLabel");

        pauseContainer = root.Q<VisualElement>("MainPauseContainer");
        settingsContainer = root.Q<VisualElement>("SettingsContainer");

        resumeBtn = root.Q<Button>("ResumeButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        quitBtn = root.Q<Button>("QuitButton");
        backBtn = root.Q<Button>("BackButton");
        saveBtn = root.Q<Button>("SaveButton"); // Ищем кнопку в UXML

        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        qualityDropdown = root.Q<DropdownField>("QualityDropdown");
        sensitivitySlider = root.Q<Slider>("SensitivitySlider");

        backgroundOverlay = root.Q<VisualElement>("BackgroundOverlay");

        // Подписки на события кнопок
        resumeBtn.clicked += Resume;
        settingsBtn.clicked += OpenSettings;
        quitBtn.clicked += QuitGame;
        backBtn.clicked += CloseSettings;

        if (saveBtn != null)
            saveBtn.clicked += SaveSettings;

        // Загружаем сохраненные ранее настройки при старте
        LoadSettings();

        resumeBtn.Focus();
        Resume();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                if (settingsContainer.style.display == DisplayStyle.Flex)
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
    }

    // Метод для обновления денег на экране (вызывайте его, когда игрок уничтожает пачку)
    public void UpdateMoneyDisplay(long currentMoney)
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = $"${currentMoney:N0}"; // Форматирует число с запятыми/пробелами ($1,000,500)
        }
    }

    public void Resume()
    {
        // Скрываем меню и подложку, но HUD (прицел и деньги) остается видимым
        if (backgroundOverlay != null) backgroundOverlay.style.display = DisplayStyle.None;
        pauseContainer.style.display = DisplayStyle.None;
        settingsContainer.style.display = DisplayStyle.None;

        Time.timeScale = 1f;
        isPaused = false;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        if (playerController != null) playerController.enabled = true;
    }

    void Pause()
    {
        // Показываем подложку затемнения и окно паузы
        if (backgroundOverlay != null) backgroundOverlay.style.display = DisplayStyle.Flex;
        pauseContainer.style.display = DisplayStyle.Flex;
        settingsContainer.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
        isPaused = true;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        if (playerController != null) playerController.enabled = false;
    }

    void OpenSettings()
    {
        pauseContainer.style.display = DisplayStyle.None;
        settingsContainer.style.display = DisplayStyle.Flex;
    }

    void CloseSettings()
    {
        // При закрытии без сохранения можно вернуть старые значения из PlayerPrefs
        LoadSettings();
        settingsContainer.style.display = DisplayStyle.None;
        pauseContainer.style.display = DisplayStyle.Flex;
    }

    void LoadSettings()
    {
        // Разрешения
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
        resolutionDropdown.choices = options;

        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentIndex);
        resolutionDropdown.index = savedResIndex;

        // Качество графики
        List<string> qualityNames = new List<string>(QualitySettings.names);
        qualityDropdown.choices = qualityNames;
        int savedQuality = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
        qualityDropdown.index = savedQuality;
        QualitySettings.SetQualityLevel(savedQuality);

        // Чувствительность мыши
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", playerController != null ? playerController.mouseSensitivity : 0.3f);
        sensitivitySlider.value = savedSensitivity;

        sensitivitySlider.RegisterValueChangedCallback(evt => {
            // Округляем значение до ближайших 0.05
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

    public void SaveSettings()
    {
        // Сохраняем чувствительность
        if (playerController != null)
        {
            playerController.mouseSensitivity = sensitivitySlider.value;
            PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);
        }

        // Сохраняем качество графики
        int selectedQuality = qualityDropdown.index;
        QualitySettings.SetQualityLevel(selectedQuality);
        PlayerPrefs.SetInt("QualityIndex", selectedQuality);

        // Сохраняем разрешение
        int selectedRes = resolutionDropdown.index;
        if (selectedRes >= 0 && selectedRes < resolutions.Length)
        {
            Resolution res = resolutions[selectedRes];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", selectedRes);
        }

        PlayerPrefs.Save();
        Debug.Log("Настройки успешно сохранены!");

        // Возвращаемся в главное меню паузы после сохранения
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
}