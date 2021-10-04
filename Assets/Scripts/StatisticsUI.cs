using System.Linq;
using Assets.Scripts.EventHandlers;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class StatisticsUI : MonoBehaviour
    {
        public static StatisticsUI Instance { get; private set; }

        private Transform _progressBar;

        private TextMeshProUGUI _peakHeightText;
        private TextMeshProUGUI _rocketThrustText;
        private TextMeshProUGUI _rocketPreburnText;
        private TextMeshProUGUI _rocketCargoLabelText;
        private TextMeshProUGUI _rocketCargoText;

        private TextMeshProUGUI _heightText;

        private TextMeshProUGUI _loopNumberText;
        private TextMeshProUGUI _alertText;

        private AlertTimer _alertTimer;

        private float _heighestPeak;

        private void Awake()
        {
            Instance = this;

            _progressBar = transform.Find("LaunchProgress").Find("Bar");
            _heightText = transform.Find("HeightText").GetComponent<TextMeshProUGUI>();
            _peakHeightText = transform.Find("PeakHeightText").GetComponent<TextMeshProUGUI>();
            _rocketThrustText = transform.Find("RocketThrustText").GetComponent<TextMeshProUGUI>();
            _rocketPreburnText = transform.Find("RocketPreburnText").GetComponent<TextMeshProUGUI>();
            _rocketCargoLabelText = transform.Find("RocketCargoLabelText").GetComponent<TextMeshProUGUI>();
            _rocketCargoText = transform.Find("RocketCargoText").GetComponent<TextMeshProUGUI>();

            _loopNumberText = transform.Find("LoopNumberText").GetComponent<TextMeshProUGUI>();
            _alertText = transform.Find("AlertText").GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            RocketManager.Instance.OnActiveRocketPeakHeightReached += (sender, args) =>
            {
                if (args.MaxHeight > _heighestPeak)
                {
                    _heighestPeak = args.MaxHeight;
                    _peakHeightText.SetText($"{args.MaxHeight:F0}");
                }
            };

            RocketManager.Instance.OnLoopIncreased += (sender, loopNumber) =>
            {
                _loopNumberText.SetText($"Launch Number: {loopNumber}");
            };

            RocketManager.Instance.OnActiveRocketChanged += RocketManager_OnActiveRocketChanged;

            RocketManager.Instance.OnCargoUpdated += RocketManager_OnCargoUpdated;

            MenuUI.Instance.OnPreburnTimeChanged += (sender, newPreburn) =>
            {
                _rocketPreburnText.SetText($"{newPreburn:F1}");
            };
            
            UpdateProgressBar();
        }

        private void RocketManager_OnCargoUpdated(object sender, CargoUpdatedEventArgs e)
        {
            UpdateCargoDisplay(e.Cargo, e.CargoWeightMax);
        }

        private void RocketManager_OnActiveRocketChanged(object sender, ActiveRocketChangedEventArgs e)
        {
            if (e.ActiveRocketType != null)
            {
                _rocketThrustText.SetText($"{e.ActiveRocketType.TravelSpeed}");
                _rocketPreburnText.SetText($"{e.ActiveRocketType.LaunchPrepTime:F1}s");
                UpdateCargoDisplay(e.StartingCargo, e.ActiveRocketType.MaxWeight);
            }
            else
            {
                ResetRocketStatistics();
            }
        }

        private void UpdateCargoDisplay(CargoAmount[] cargo, float maxWeight)
        {
            _rocketCargoLabelText.SetText("Rocket cargo:");
            _rocketCargoText.SetText($"{cargo.Sum(c => c.Amount * c.CargoType.CargoWeight):F2}t / {maxWeight:F2}t");
            foreach (var amount in cargo)
            {
                _rocketCargoLabelText.SetText($"{_rocketCargoLabelText.text}\n * {amount.CargoType.CargoName}");
                _rocketCargoText.SetText($"{_rocketCargoText.text}\n{amount.Amount * amount.CargoType.CargoWeight:F2}t ({amount.Amount})");
            }
        }

        public void SetAlertText(string text, AlertTimer timer = null)
        {
            _alertTimer = timer;
            _alertText.SetText(text);
        }

        private void ResetRocketStatistics()
        {
            _rocketThrustText.SetText("-");
            _rocketPreburnText.SetText("-");
            _rocketCargoLabelText.SetText("Rocket cargo:");
            _rocketCargoText.SetText("- / -");
        }

        private void Update()
        {
            UpdateProgressBar();
            if (RocketManager.Instance.HasRocketLaunched())
            {
                _rocketPreburnText.SetText($"{RocketManager.Instance.GetRemainingRocketPreburnTime():F1}s");
            }

            if (_alertTimer != null)
            {
                _alertTimer.Timer -= Time.deltaTime;
                if (_alertTimer.Timer <= 0)
                {
                    _alertText.SetText("");
                }
            }
        }

        private void UpdateProgressBar()
        {
            _progressBar.localScale = new Vector3(1, RocketManager.Instance.GetRocketHeightNomalized(), 1);

            var height = RocketManager.Instance.GetRocketHeight();
            if (height < 0) { height = 0; }

            _heightText.SetText($"{height:F0}");
        }

        public class AlertTimer
        {
            public float Timer;
        }
    }
}