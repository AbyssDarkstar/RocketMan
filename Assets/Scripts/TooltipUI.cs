using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance { get; private set; }

        [SerializeField]
        private RectTransform _canvasRectTransform = default;

        private TextMeshProUGUI _textMeshPro;
        private RectTransform _backgroundRectTransform;
        private RectTransform _rectTransform;

        private TooltipTimer _tooltipTimer;

        private void Awake()
        {
            Instance = this;

            _rectTransform = GetComponent<RectTransform>();
            _textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();

            Hide();
        }

        private void SetText(string tooltipText)
        {
            _textMeshPro.SetText(tooltipText);
            _textMeshPro.ForceMeshUpdate();

            var textSize = _textMeshPro.GetRenderedValues(false);
            var padding = new Vector2(8, 8);
            _backgroundRectTransform.sizeDelta = textSize + padding;
        }

        private void Update()
        {
            HandleFollowMouse();

            if (_tooltipTimer != null)
            {
                _tooltipTimer.Timer -= Time.deltaTime;
                if (_tooltipTimer.Timer <= 0)
                {
                    Hide();
                }
            }
        }

        private void HandleFollowMouse()
        {
            var anchoredPosition = Input.mousePosition / _canvasRectTransform.localScale.x;

            anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, 0, _canvasRectTransform.rect.width - _backgroundRectTransform.rect.width);
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, 0, _canvasRectTransform.rect.height - _backgroundRectTransform.rect.height);

            _rectTransform.anchoredPosition = anchoredPosition;
        }

        public void Show(string tooltipText, TooltipTimer tooltipTimer = null)
        {
            _tooltipTimer = tooltipTimer;
            gameObject.SetActive(true);
            SetText(tooltipText);
            HandleFollowMouse();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public class TooltipTimer
        {
            public float Timer;
        }
    }
}