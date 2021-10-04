using System;
using Assets.Scripts.EventHandlers;
using Assets.Scripts.ScriptableObjects;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts
{
    public class RocketManager : MonoBehaviour
    {
		public static RocketManager Instance { get; private set; }

        [SerializeField]
        private CinemachineVirtualCamera _gameCamera = default;

        [SerializeField]
        private Transform _origin = default;

        [SerializeField]
        private CurrencyType _rewardCurrencyType = default;

        private Transform _currentRocket;

        public event EventHandler<ActiveRocketChangedEventArgs> OnActiveRocketChanged;
        public event EventHandler OnActiveRocketLaunched;
        public event EventHandler OnActiveRocketDestroyed;
        public event EventHandler<PeakHeightReachedEventArgs> OnActiveRocketPeakHeightReached;
        public event EventHandler<int> OnLoopIncreased;
        public event EventHandler<CargoUpdatedEventArgs> OnCargoUpdated;

        private int _currentLoop = 0;
        
        private void IncreaseLoopNumber()
        {
            _currentLoop++;
            OnLoopIncreased?.Invoke(this, _currentLoop);
        }

		private void Awake()
        {
            Instance = this;
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MenuUI.Instance.MenusOpen())
                {
                    MenuUI.Instance.HideAllMenus();
                }
                else
                {
                    MenuUI.Instance.ShowPauseMenu();
                }
            }
        }
        
        public void LaunchCurrentRocket()
        {
            if (_currentRocket != null)
            {
                IncreaseLoopNumber();
                _currentRocket.GetComponent<Rocket>().BeingLaunch();
            }
        }

        public bool HasRocketLaunched()
        {
            if (_currentRocket != null)
            {
                return _currentRocket.GetComponent<Rocket>().RocketHasLaunched();
            }

            return false;
        }

        public float GetRocketPreburnTime()
        {
            if (_currentRocket != null)
            {
                var rocket = _currentRocket.GetComponent<Rocket>();
                return rocket.GetPreburnTime();
            }

            return 0f;
        }

        public float GetRemainingRocketPreburnTime()
        {
            if (_currentRocket != null)
            {
                var rocket = _currentRocket.GetComponent<Rocket>();
                return rocket.GetRemainingPreburnTime();
            }

            return 0f;
        }

        public void SelectRocket(RocketType rocketType)
        {
            Rocket rocket;
            if (_currentRocket != null)
            {
                rocket = _currentRocket.GetComponent<Rocket>();
                var rocketHolder = _currentRocket.GetComponent<RocketTypeHolder>();

                ResourcesManager.Instance.AddCurrency(rocketHolder.RocketType.RocketCost);

                rocket.SuppressDestroyedEvent();
                Destroy(_currentRocket.gameObject);
            }

            _currentRocket = Instantiate(rocketType.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            _gameCamera.Follow = _currentRocket;

            rocket = _currentRocket.GetComponent<Rocket>();
            rocket.OnLaunched += (sender, args) =>
            {
                OnActiveRocketLaunched?.Invoke(this, EventArgs.Empty);
            };

            rocket.OnDestroyed += (sender, args) =>
            {
                _currentRocket = null;
                OnActiveRocketChanged?.Invoke(this, new ActiveRocketChangedEventArgs());
                OnActiveRocketDestroyed?.Invoke(this, EventArgs.Empty);
            };

            rocket.OnPeakHeightReached += (sender, args) =>
            {
                OnActiveRocketPeakHeightReached?.Invoke(this, args);

                if (args.AchievedOrbit)
                {
                    Destroy(_currentRocket.gameObject);
                    _gameCamera.Follow = _origin;

                    ResourcesManager.Instance.AddCurrency(_rewardCurrencyType, Mathf.FloorToInt(args.MaxHeight / 3));
                }

                ResourcesManager.Instance.AddCurrency(_rewardCurrencyType, Mathf.FloorToInt(args.MaxHeight / 2));
            };

            rocket.OnCargoUpdated += (sender, args) =>
            {
                OnCargoUpdated?.Invoke(this, args);
            };

            OnActiveRocketChanged?.Invoke(this, new ActiveRocketChangedEventArgs {ActiveRocketType = rocketType, StartingCargo = rocket.GetCargoAmount()});
        }

        public bool HasActiveRocket()
        {
            return _currentRocket != null;
        }

        public bool RocketHasWeightAvailable(float amount)
        {
            if (_currentRocket != null)
            {
                return _currentRocket.GetComponent<Rocket>().HasWeightAvailable(amount);
            }

            return false;
        }

        public float GetRocketHeightNomalized()
        {
            if (_currentRocket == null)
            {
                return 0;
            }

            var normalized = _currentRocket.position.y / 1000;
            return Mathf.Clamp01(normalized);
        }

        public CargoAmount[] GetRocketCargoAmounts()
        {
            if (_currentRocket == null)
            {
                return null;
            }

            return _currentRocket.GetComponent<Rocket>().GetCargoAmount();
        }

        public float GetRocketHeight()
        {
            if (_currentRocket == null)
            {
                return 0;
            }

            return _currentRocket.position.y;
        }
    }
}