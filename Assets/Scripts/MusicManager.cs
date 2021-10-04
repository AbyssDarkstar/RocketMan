using UnityEngine;

namespace Assets.Scripts
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        private AudioSource _audioSource;
        private float _volume = 0.5f;

        private void Awake()
        {
            Instance = this;

            _audioSource = GetComponent<AudioSource>();
            _volume = _audioSource.volume;
        }

        public int GetVolumeLevel()
        {
            return Mathf.RoundToInt(_volume * 10);
        }

        public void SetVolume(float volume)
        {
            _volume = volume / 10;
            _volume = Mathf.Clamp01(_volume);

            _audioSource.volume = _volume;
        }
    }
}