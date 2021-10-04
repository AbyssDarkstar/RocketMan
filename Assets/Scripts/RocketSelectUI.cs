using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.CustomEvents;
using Assets.Scripts.EventHandlers;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class RocketSelectUI : MonoBehaviour
    {
        public static RocketSelectUI Instance { get; private set; }

        private Dictionary<RocketType, Transform> _rocketSelectUiTransforms;

        [SerializeField]
        private Transform _buttonPrefab = default;

        private Button _cargoManagerButton;

        private void Awake()
        {
            Instance = this;

            _rocketSelectUiTransforms = new Dictionary<RocketType, Transform>();
            _cargoManagerButton = transform.Find("CargoManagerButton").GetComponent<Button>();

            _cargoManagerButton.onClick.AddListener(() =>
            {
                MenuUI.Instance.ShowCargoManager(RocketManager.Instance.GetRocketCargoAmounts(), RocketManager.Instance.GetRocketPreburnTime());
            });

            var rocketTypes = Resources.Load<RocketTypeList>(nameof(RocketTypeList));

            var i = 0;
            const float offsetAmount = 130f;

            foreach (var rocketType in rocketTypes.List)
            {
                var buttonTransform = Instantiate(_buttonPrefab, transform);
                buttonTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetAmount * i++, 0);
                buttonTransform.Find("RocketImage").GetComponent<Image>().sprite = rocketType.RocketSprite;
                buttonTransform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (ResourcesManager.Instance.CanAfford(rocketType.RocketCost))
                    {
                        ResourcesManager.Instance.SpendCurrency(rocketType.RocketCost);
                        RocketManager.Instance.SelectRocket(rocketType);
                        foreach (var rocketSelectButtons in _rocketSelectUiTransforms.Where(kp => kp.Key != rocketType))
                        {
                            rocketSelectButtons.Value.GetComponent<Button>().interactable = true;
                        }
                    }
                    else
                    {
                        TooltipUI.Instance.Show("You cannot afford this rocket!", new TooltipUI.TooltipTimer { Timer = 1f });
                    }
                });

                var customMouseEvents = buttonTransform.GetComponent<MouseEvents>();
                customMouseEvents.OnMouseEnter += (sender, args) =>
                {
                    TooltipUI.Instance.Show($"{rocketType.RocketName}\n{rocketType.RocketTooltip()}");
                };

                customMouseEvents.OnMouseExit += (sender, args) =>
                {
                    TooltipUI.Instance.Hide();
                };

                _rocketSelectUiTransforms[rocketType] = buttonTransform;
            }
        }

        private void Start()
        {
            RocketManager.Instance.OnActiveRocketChanged += GameManager_OnActiveRocketChanged;
            RocketManager.Instance.OnActiveRocketLaunched += (sender, args) =>
            {
                foreach (var rocketSelectButtons in _rocketSelectUiTransforms.Values)
                {
                    rocketSelectButtons.GetComponent<Button>().interactable = false;
                }

                _cargoManagerButton.interactable = false;
            };

            RocketManager.Instance.OnActiveRocketDestroyed += (sender, args) =>
            {
                try
                {
                    if (!RocketManager.Instance.HasActiveRocket())
                    {
                        foreach (var rocketSelectButtons in _rocketSelectUiTransforms.Values)
                        {
                            var btn = rocketSelectButtons?.GetComponent<Button>();
                            if (btn != null)
                            {
                                btn.interactable = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //To Resolve annoying error when closing the game.
                }
            };
        }

        private void GameManager_OnActiveRocketChanged(object sender, ActiveRocketChangedEventArgs e)
        {
            try
            {
                if (e.ActiveRocketType != null)
                {
                    _rocketSelectUiTransforms[e.ActiveRocketType].GetComponent<Button>().interactable = false;
                    _cargoManagerButton.interactable = true;
                }
                else
                {
                    _cargoManagerButton.interactable = false;
                }
            }
            catch (Exception)
            {
                //To Resolve annoying error when closing the game.
            }
        }

        private void Update()
        {

        }
    }
}