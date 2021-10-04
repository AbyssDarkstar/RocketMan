using Assets.Scripts.CustomEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class LaunchControlsUI : MonoBehaviour
    {
		public static LaunchControlsUI Instance { get; private set; }

        private Transform _launchButtonTransform;
        private Button _launchButton;

		private void Awake()
        {
            Instance = this;

            _launchButtonTransform = transform.Find("LaunchRocketButton");
            _launchButton = _launchButtonTransform.GetComponent<Button>();
			DisableLaunchButton();
        }
		
		private void Start()
        {
            RocketManager.Instance.OnActiveRocketChanged += (sender, args) =>
            {
                if (args.ActiveRocketType != null)
                {
                    EnableLaunchButton();
                }
            };

			_launchButton.onClick.AddListener(() =>
            {
                DisableLaunchButton();
				RocketManager.Instance.LaunchCurrentRocket();
            });

            var customMouseEvents = _launchButtonTransform.GetComponent<MouseEvents>();
            customMouseEvents.OnMouseEnter += (sender, args) =>
            {
                TooltipUI.Instance.Show("Launch Rocket!");
            };

            customMouseEvents.OnMouseExit += (sender, args) =>
            {
                TooltipUI.Instance.Hide();
            };
        }

        private void Update()
		{
			
		}

        public void EnableLaunchButton()
        {
            _launchButton.interactable = true;
		}

		public void DisableLaunchButton()
        {
            _launchButton.interactable = false;
		}
	}
}