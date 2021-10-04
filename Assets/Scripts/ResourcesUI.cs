using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.CustomEvents;
using Assets.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class ResourcesUI : MonoBehaviour
	{
        private CurrencyTypeList _currencyTypeList;
        private Dictionary<CurrencyType, Transform> _currencyDisplayTransforms;

        [SerializeField]
        private Transform _currencyPrefab = default;

        private void Awake()
		{
            _currencyTypeList = Resources.Load<CurrencyTypeList>(nameof(CurrencyTypeList));
            _currencyDisplayTransforms = new Dictionary<CurrencyType, Transform>();
            
            var i = 0;
            foreach (var currencyType in _currencyTypeList.List)
            {
                var currencyTransform = Instantiate(_currencyPrefab, transform);
                currencyTransform.gameObject.SetActive(true);
                const float offsetAmount = -160f;
                currencyTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetAmount * i++, 0);

                currencyTransform.Find("Image").GetComponent<Image>().sprite = currencyType.CurrencySprite;

                var mouseEnterExitEvents = currencyTransform.Find("Image").GetComponent<MouseEvents>();
                mouseEnterExitEvents.OnMouseEnter += (sender, args) =>
                {
                    TooltipUI.Instance.Show(currencyType.CurrencyName);
                };

                mouseEnterExitEvents.OnMouseExit += (sender, args) =>
                {
                    TooltipUI.Instance.Hide();
                };

                _currencyDisplayTransforms[currencyType] = currencyTransform;
            }
        }
		
		private void Start()
		{
            ResourcesManager.Instance.OnCurrencyAmountChanged += ResourcesManager_OnCurrencyAmountChanged;

            UpdateResourceAmounts();
		}

        private void ResourcesManager_OnCurrencyAmountChanged(object sender, EventArgs e)
        {
            UpdateResourceAmounts();
        }

        private void UpdateResourceAmounts()
        {
            foreach (var currencyType in _currencyTypeList.List)
            {
                var currencyAmount = ResourcesManager.Instance.GetCurrencyAmount(currencyType);
                _currencyDisplayTransforms[currencyType].Find("Text").GetComponent<TextMeshProUGUI>().SetText($"{currencyAmount}");
            }
        }
	}
}