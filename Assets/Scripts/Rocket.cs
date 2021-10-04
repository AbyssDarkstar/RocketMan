using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.EventHandlers;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField]
        private CargoType _fuelCargoType = default;

        private ParticleSystem _backwashParticleSystem;
        private AudioSource _boosterAudio;
        private float _currentTravelSpeed;
        private AudioSource _explosionAudio;
        private ParticleSystem _explosionParticleSystem;
        private float _launchTimer;
        private float _lerpTime;
        private Vector3 _moveDirection = new Vector3(0f, 1f, 0f);
        private bool _peakReported;
        private float _preLaunch;
        private Dictionary<CargoType, int> _rocketCargo;
        private float _rocketFuel;
        private RocketState _rocketState;
        private float _rocketTravelSpeed;
        private RocketType _rocketType;
        private float _rocketWeight;
        private float _rocketWeightMax;
        private SpriteRenderer _spriteRenderer;
        private bool _suppressDestroyedEvent;

        public event EventHandler<CargoUpdatedEventArgs> OnCargoUpdated;
        public event EventHandler OnDestroyed;
        public event EventHandler OnLaunched;
        public event EventHandler<PeakHeightReachedEventArgs> OnPeakHeightReached;

        private enum RocketState
        {
            Idle,
            PreLaunch,
            Launch,
            BurnedOut,
            NoLaunch,
            OrbitAchieved,
            Destroyed
        }

        public void BeingLaunch()
        {
            _launchTimer = _preLaunch;
            _rocketState = RocketState.PreLaunch;
        }

        public void DestroyRocket()
        {
            _rocketState = RocketState.Destroyed;

            _moveDirection.y = 0f;
            _spriteRenderer.enabled = false;

            _boosterAudio.Stop();
            _backwashParticleSystem.Stop();

            _explosionParticleSystem.Play();
            _explosionAudio.Play();

            Destroy(gameObject, _explosionParticleSystem.main.duration);
        }

        public CargoAmount[] GetCargoAmount()
        {
            var cargoAmountList = new List<CargoAmount>();

            foreach (var cargo in _rocketCargo)
            {
                cargoAmountList.Add(new CargoAmount { CargoType = cargo.Key, Amount = cargo.Value });
            }

            return cargoAmountList.ToArray();
        }

        public float GetPreburnTime()
        {
            return _preLaunch;
        }

        public float GetRemainingPreburnTime()
        {
            return _launchTimer;
        }

        public bool HasWeightAvailable(float amount)
        {
            return CalculateRocketCargoWeight() >= amount;
        }

        public bool RocketHasLaunched()
        {
            return _rocketState == RocketState.PreLaunch || _rocketState == RocketState.Launch || _rocketState == RocketState.BurnedOut;
        }

        public void SuppressDestroyedEvent()
        {
            _suppressDestroyedEvent = true;
        }

        private void Awake()
        {
            _rocketState = RocketState.Idle;
            _spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            _backwashParticleSystem = transform.Find("Backwash").GetComponent<ParticleSystem>();
            _explosionParticleSystem = transform.Find("Explosion").GetComponent<ParticleSystem>();
            _boosterAudio = transform.Find("BoosterAudio").GetComponent<AudioSource>();
            _explosionAudio = transform.Find("ExplosionAudio").GetComponent<AudioSource>();

            _rocketCargo = new Dictionary<CargoType, int>();

            _rocketType = GetComponent<RocketTypeHolder>().RocketType;

            foreach (var amount in _rocketType.StartingCargo)
            {
                _rocketCargo[amount.CargoType] = amount.Amount;
            }

            _rocketFuel = _rocketCargo[_fuelCargoType];
            _rocketWeight = CalculateRocketWeight();
            _rocketWeightMax = _rocketType.MaxWeight;
            _rocketTravelSpeed = _rocketType.TravelSpeed;
            _preLaunch = _rocketType.LaunchPrepTime;
            _launchTimer = _preLaunch;
        }

        private float CalculateRocketCargoWeight()
        {
            return _rocketCargo.Sum(c => c.Value * c.Key.CargoWeight);
        }

        private float CalculateRocketWeight()
        {
            return 20 + CalculateRocketCargoWeight();
        }

        private void OnDestroy()
        {
            if (!_suppressDestroyedEvent)
            {
                OnDestroyed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var solidObject = other.GetComponent<SolidObject>();
            var missle = other.GetComponent<Missile>();

            if (solidObject != null || missle != null)
            {
                DestroyRocket();
            }
        }

        private void Start()
        {
            _boosterAudio.volume = MenuUI.Instance.SfxVolume / 10;
            _explosionAudio.volume = MenuUI.Instance.SfxVolume / 10;
            MenuUI.Instance.OnSfxVolumeChanged += (sender, newVolume) =>
            {
                _boosterAudio.volume = newVolume / 10;
                _explosionAudio.volume = newVolume / 10;
            };

            MenuUI.Instance.OnPreburnTimeChanged += (sender, newPreburn) =>
            {
                _preLaunch = newPreburn;
            };

            MenuUI.Instance.OnCargoQtyChanged += (sender, args) =>
            {
                if (args.CargoType == _fuelCargoType)
                {
                    _rocketFuel = args.CargoAmount;
                }

                UpdateCargo(args.CargoType, args.CargoAmount);
            };
        }

        private void UpdateCargo(CargoType cargoType, int cargoAmount)
        {
            if (_rocketCargo.ContainsKey(cargoType))
            {
                _rocketCargo[cargoType] = cargoAmount;

                if (cargoAmount == 0)
                {
                    _rocketCargo.Remove(cargoType);
                }
            }
            else
            {
                _rocketCargo[cargoType] = cargoAmount;
            }

            var cargo = new List<CargoAmount>();

            foreach (var cargoItem in _rocketCargo)
            {
                cargo.Add(new CargoAmount { CargoType = cargoItem.Key, Amount = cargoItem.Value });
            }

            OnCargoUpdated?.Invoke(this, new CargoUpdatedEventArgs
            {
                Cargo = cargo.ToArray(),
                CargoWeightMax = _rocketWeightMax
            });
        }

        private void Update()
        {
            switch (_rocketState)
            {
                case RocketState.PreLaunch:
                    OnLaunched?.Invoke(this, EventArgs.Empty);

                    if (!_backwashParticleSystem.isPlaying)
                    {
                        _backwashParticleSystem.Play();
                        _boosterAudio.Play();
                    }

                    _rocketFuel -= Time.deltaTime * 25f;
                    if (_rocketFuel < 0) { _rocketFuel = 0; }

                    _rocketCargo[_fuelCargoType] = Mathf.FloorToInt(_rocketFuel);

                    _rocketWeight = CalculateRocketWeight();
                    UpdateCargo(_fuelCargoType, _rocketCargo[_fuelCargoType]);

                    if (_rocketFuel < 0f)
                    {
                        _backwashParticleSystem.Stop();
                        _boosterAudio.Stop();
                        _rocketState = RocketState.NoLaunch;
                        return;
                    }

                    _launchTimer -= Time.deltaTime;
                    if (_launchTimer < 0f)
                    {
                        _rocketState = RocketState.Launch;
                    }
                    break;
                case RocketState.Launch:

                    if (_preLaunch * 100 < CalculateRocketCargoWeight())
                    {
                        ResourcesManager.Instance.AddCurrency(_rocketType.RocketCost);

                        StatisticsUI.Instance.SetAlertText("Your preburn was too short,\nthe rocket became unstable and failed to launch.", new StatisticsUI.AlertTimer { Timer = 3f });

                        DestroyRocket();
                        return;
                    }

                    if (_rocketFuel > 0)
                    {
                        _currentTravelSpeed = Mathf.Lerp(0, _rocketTravelSpeed, _lerpTime);

                        _lerpTime += 0.5f * Time.deltaTime;

                        var drag = 1;

                        if (transform.position.y > 200)
                        {
                            drag = 2;
                        }
                        else if (transform.position.y > 400)
                        {
                            drag = 3;
                        }
                        else if (transform.position.y > 600)
                        {
                            drag = 4;
                        }
                        else if (transform.position.y > 800)
                        {
                            drag = 5;
                        }

                        var lift = _currentTravelSpeed - _rocketWeight / drag;
                        if (lift < 0)
                        {
                            lift = 1f;
                        }

                        transform.position += _moveDirection * lift * Time.deltaTime;

                        var fuelUsed = Time.deltaTime * 25f;

                        _rocketFuel -= fuelUsed;
                        if (_rocketFuel < 0) { _rocketFuel = 0; }

                        _rocketCargo[_fuelCargoType] = Mathf.FloorToInt(_rocketFuel);

                        _rocketWeight = CalculateRocketWeight();
                        UpdateCargo(_fuelCargoType, _rocketCargo[_fuelCargoType]);
                    }
                    else
                    {
                        _backwashParticleSystem.Stop();
                        _boosterAudio.Stop();
                        _rocketState = RocketState.BurnedOut;
                    }

                    break;
                case RocketState.BurnedOut:
                    if (_rocketTravelSpeed > 0)
                    {
                        _currentTravelSpeed = Mathf.Lerp(0, _rocketTravelSpeed, _lerpTime);

                        _lerpTime += 1f * Time.deltaTime;

                        var drag = 1;

                        if (transform.position.y > 200)
                        {
                            drag = 2;
                        }
                        else if (transform.position.y > 400)
                        {
                            drag = 3;
                        }
                        else if (transform.position.y > 600)
                        {
                            drag = 4;
                        }
                        else if (transform.position.y > 800)
                        {
                            drag = 5;
                        }

                        var lift = _currentTravelSpeed - _rocketWeight / drag;
                        if (lift < 0)
                        {
                            lift = 0;
                        }

                        transform.position += _moveDirection * lift * Time.deltaTime;
                        _rocketTravelSpeed -= Time.deltaTime * _rocketWeight;
                    }
                    else
                    {
                        var acheivedOrbit = transform.position.y > 1000;

                        if (!_peakReported)
                        {
                            OnPeakHeightReached?.Invoke(this, new PeakHeightReachedEventArgs { MaxHeight = transform.position.y, AchievedOrbit = acheivedOrbit });
                            _peakReported = true;
                        }

                        if (acheivedOrbit)
                        {
                            _rocketState = RocketState.OrbitAchieved;
                            return;
                        }

                        var gravity = 5;

                        if (transform.position.y > 200)
                        {
                            gravity = 4;
                        }
                        else if (transform.position.y > 400)
                        {
                            gravity = 3;
                        }
                        else if (transform.position.y > 600)
                        {
                            gravity = 2;
                        }
                        else if (transform.position.y > 800)
                        {
                            gravity = 1;
                        }

                        transform.position += -_moveDirection * (_rocketWeight * gravity) * Time.deltaTime;
                        if (transform.position.y < -1)
                        {
                            transform.position = new Vector3(0, -1, 0);

                        }
                        if (!_explosionParticleSystem.isPlaying)
                        {
                            transform.Rotate(0f, 0f, Random.Range(0f, 2f));
                        }
                    }

                    break;
                case RocketState.OrbitAchieved:
                    break;
                case RocketState.Destroyed:
                    break;
            }
        }
    }
}