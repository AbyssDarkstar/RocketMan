using System;
using System.Linq;
using Assets.Scripts.EventHandlers;
using Assets.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MenuUI : MonoBehaviour
    {
        public static MenuUI Instance { get; private set; }

        private Transform _pauseMenu;

        private Button _pauseToAboutButton;
        private Transform _aboutMenu;

        private Button _pauseToSettingsButton;
        private Transform _settingsMenu;
        private Slider _sfxSlider;
        private Slider _musicSlider;
        private Button _closeSettingsButton;

        private Button _mainMenuButton;

        [SerializeField]
        private Transform _cargoItemTemplate = default;

        private Transform _cargoMenu;

        private Button _increasePreburnTime;
        private TextMeshProUGUI _preburnTime;
        private Button _decreasePreburnTime;

        public event EventHandler<float> OnSfxVolumeChanged;

        public event EventHandler<CargoQtyChangedEventArgs> OnCargoQtyChanged;
        public event EventHandler<float> OnPreburnTimeChanged;

        public float SfxVolume { get; private set; }
        public float MusicVolume { get; private set; }

        public void PopulateCargoMenu(CargoAmount[] cargoAmounts, float preburnTime)
        {
            _preburnTime.SetText($"{preburnTime:F1}");
            
            var cargoList = Resources.Load<CargoTypeList>(nameof(CargoTypeList));
            var cargoContainer = _cargoMenu.Find("CargoList");

            foreach (Transform child in cargoContainer)
            {
                Destroy(child.gameObject);
            }

            var i = 0;
            const float offsetAmount = 50f;

            foreach (var cargoType in cargoList.List)
            {
                var citem = Instantiate(_cargoItemTemplate, cargoContainer);
                citem.GetComponent<CargoTypeHolder>().CargoType = cargoType;
                citem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -offsetAmount * i++);
                citem.Find("CargoName").GetComponent<TextMeshProUGUI>().SetText($"{cargoType.CargoName} ({cargoType.CargoCostTooltip()})");

                var cargoAmount = 0;
                if (cargoAmounts != null && cargoAmounts.Any(c => c.CargoType == cargoType))
                {
                    cargoAmount = cargoAmounts.First(c => c.CargoType == cargoType).Amount;
                }

                citem.Find("CargoQty").GetComponent<TextMeshProUGUI>().SetText($"{cargoAmount}");

                citem.Find("CargoAdd").GetComponent<Button>().onClick.AddListener(() =>
                {
                    var qty = 1;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        qty = 10;
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        qty = 100;
                    }

                    var cargoWeightNeeded = cargoType.CargoWeight * qty;
                    var cargoTotalCost = cargoType.CargoCost;
                    
                    if (RocketManager.Instance.RocketHasWeightAvailable(cargoWeightNeeded))
                    {
                        if (ResourcesManager.Instance.CanAfford(cargoTotalCost, qty))
                        {
                            ResourcesManager.Instance.SpendCurrency(cargoTotalCost, qty);
                            var currentAmount = int.Parse(citem.Find("CargoQty").GetComponent<TextMeshProUGUI>().text);
                            var newAmount = currentAmount + qty;
                            citem.Find("CargoQty").GetComponent<TextMeshProUGUI>().SetText($"{newAmount}");

                            OnCargoQtyChanged?.Invoke(this, new CargoQtyChangedEventArgs
                            {
                                CargoType = cargoType,
                                CargoAmount = newAmount
                            });
                        }
                        else
                        {
                            TooltipUI.Instance.Show("You can't afford this cargo.", new TooltipUI.TooltipTimer { Timer = 1f });
                        }
                    }
                    else
                    {
                        TooltipUI.Instance.Show("Adding this item would put the rocket over its weight limit.", new TooltipUI.TooltipTimer {Timer = 1f});
                    }
                });

                citem.Find("CargoRemove").GetComponent<Button>().onClick.AddListener(() =>
                {
                    var currentAmount = int.Parse(citem.Find("CargoQty").GetComponent<TextMeshProUGUI>().text);

                    if (currentAmount == 0) { return; }

                    var qty = 1;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        qty = 10;
                    }
                    else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        qty = 100;
                    }

                    var cargoTotalCost = cargoType.CargoCost;
                    
                    var newAmount = currentAmount - qty;
                    if (newAmount < 0)
                    {
                        qty = currentAmount;
                        newAmount = 0;
                    }
                    citem.Find("CargoQty").GetComponent<TextMeshProUGUI>().SetText($"{newAmount}");

                    ResourcesManager.Instance.AddCurrency(cargoTotalCost, qty);

                    OnCargoQtyChanged?.Invoke(this, new CargoQtyChangedEventArgs
                    {
                        CargoType = cargoType,
                        CargoAmount = newAmount 
                    });
                });
            }
        }

        private void Awake()
        {
            Instance = this;

            _pauseMenu = transform.Find("PauseMenuUI");

            _pauseToSettingsButton = _pauseMenu.Find("SettingsButton").GetComponent<Button>();
            _settingsMenu = transform.Find("SettingsMenuUI");
            _sfxSlider = _settingsMenu.Find("SFXVolumeSlider").GetComponent<Slider>();
            _musicSlider = _settingsMenu.Find("MusicVolumeSlider").GetComponent<Slider>();
            _closeSettingsButton = _settingsMenu.Find("BackButton").GetComponent<Button>();

            _mainMenuButton = _pauseMenu.Find("MainMenuButton").GetComponent<Button>();

            //TODO: Add About EHs if about is added

            _pauseToSettingsButton.onClick.AddListener(() =>
            {
                HidePauseMenu();
                ShowSettingsMenu();
            });

            _sfxSlider.onValueChanged.AddListener(newValue =>
            {
                SfxVolume = newValue;
                PlayerPrefs.SetFloat("sfxVolume", newValue);
                OnSfxVolumeChanged?.Invoke(this, newValue);
            });

            _musicSlider.onValueChanged.AddListener(newValue =>
            {
                MusicVolume = newValue;
                PlayerPrefs.SetFloat("musicVolume", newValue);
                MusicManager.Instance.SetVolume(newValue);
            });

            _closeSettingsButton.onClick.AddListener(() =>
            {
                HideSettingsMenu();
                ShowPauseMenu();
            });

            _mainMenuButton.onClick.AddListener(() =>
            {
                GameSceneManager.Load(GameSceneManager.Scene.MainMenuScene);
            });

            _cargoMenu = transform.Find("CargoManagementUI");

            _cargoMenu.Find("CloseButton").GetComponent<Button>().onClick.AddListener(HideCargoManager);

            var preburn = _cargoMenu.Find("Preburn");
            _increasePreburnTime = preburn.Find("PreburnIncrease").GetComponent<Button>();
            _preburnTime = preburn.Find("PreburnTime").GetComponent<TextMeshProUGUI>();
            _decreasePreburnTime = preburn.Find("PreburnDecrease").GetComponent<Button>();

            _increasePreburnTime.onClick.AddListener(() =>
            {
                var currentPreburn = float.Parse(_preburnTime.text);

                var newPreburn = currentPreburn + 0.1f;

                _preburnTime.SetText($"{newPreburn:F1}");

                OnPreburnTimeChanged?.Invoke(this, newPreburn);
            });

            _decreasePreburnTime.onClick.AddListener(() =>
            {
                var currentPreburn = float.Parse(_preburnTime.text);

                if (currentPreburn == 0.0f) { return; }

                var newPreburn = currentPreburn - 0.1f;

                _preburnTime.SetText($"{newPreburn:F1}");

                OnPreburnTimeChanged?.Invoke(this, newPreburn);
            });
        }

        private void Start()
        {
            SfxVolume = PlayerPrefs.GetFloat("sfxVolume", 5f);
            _sfxSlider.SetValueWithoutNotify(SfxVolume);
            MusicVolume = PlayerPrefs.GetFloat("musicVolume", 5f);
            _musicSlider.SetValueWithoutNotify(MusicVolume);
            MusicManager.Instance.SetVolume(MusicVolume);

            HideAllMenus();
        }

        public void ShowPauseMenu()
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);
            _pauseMenu.gameObject.SetActive(true);
        }

        public void HidePauseMenu()
        {
            _pauseMenu.gameObject.SetActive(false);
        }

        public void ShowSettingsMenu()
        {
            gameObject.SetActive(true);
            _settingsMenu.gameObject.SetActive(true);
        }

        public void HideSettingsMenu()
        {
            _settingsMenu.gameObject.SetActive(false);
        }

        public void HideAllMenus()
        {
            HidePauseMenu();
            HideSettingsMenu();
            HideCargoManager();

            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        public bool MenusOpen()
        {
            return gameObject.activeSelf;
        }

        public void ShowCargoManager(CargoAmount[] cargoAmounts, float preburnTime)
        {
            gameObject.SetActive(true);
            _cargoMenu.gameObject.SetActive(true);
            PopulateCargoMenu(cargoAmounts, preburnTime);
        }

        public void HideCargoManager()
        {
            _cargoMenu.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}