using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MainMenuUI : MonoBehaviour
    {
        private Button _playButton;
        private Button _quitButton;

        private void Awake()
        {
            _playButton = transform.Find("PlayButton").GetComponent<Button>();
            _quitButton = transform.Find("QuitButton").GetComponent<Button>();

            _playButton.onClick.AddListener(() =>
            {
                GameSceneManager.Load(GameSceneManager.Scene.GameScene);
            });

            _quitButton.onClick.AddListener(Application.Quit);
        }
    }
}