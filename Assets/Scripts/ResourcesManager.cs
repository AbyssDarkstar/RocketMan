using System;
using System.Collections.Generic;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    public class ResourcesManager : MonoBehaviour
	{
        public static ResourcesManager Instance { get; private set; }

        public event EventHandler OnCurrencyAmountChanged;

        private Dictionary<CurrencyType, int> _currencyAmounts;

        [SerializeField]
        private List<CurrencyAmount> _startingCurrencyAmounts = default;

		private void Awake()
		{
            Instance = this;

			_currencyAmounts = new Dictionary<CurrencyType, int>();

            var currencyTypeList = Resources.Load<CurrencyTypeList>(nameof(CurrencyTypeList));

            foreach (var currency in currencyTypeList.List)
            {
                _currencyAmounts[currency] = 0;
            }

            foreach (var currencyAmount in _startingCurrencyAmounts)
            {
                AddCurrency(currencyAmount.ResourceType, currencyAmount.Amount);
            }
        }

        public void AddCurrency(CurrencyType currencyType, int amount)
        {
            _currencyAmounts[currencyType] += amount;
            OnCurrencyAmountChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddCurrency(CurrencyAmount[] resourceAmounts, int qty = 1)
        {
            foreach (var currencyAmount in resourceAmounts)
            {
                AddCurrency(currencyAmount.ResourceType, currencyAmount.Amount * qty);
            }
        }

        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            return _currencyAmounts[currencyType];
        }

        public bool CanAfford(CurrencyAmount[] resourceAmounts, int qty = 1)
        {
            foreach (var currencyAmount in resourceAmounts)
            {
                if (GetCurrencyAmount(currencyAmount.ResourceType) < currencyAmount.Amount * qty)
                {
                    return false;
                }
            }

            return true;
        }

        public void SpendCurrency(CurrencyAmount[] currencyAmounts, int qty = 1)
        {
            foreach (var resourceAmount in currencyAmounts)
            {
                _currencyAmounts[resourceAmount.ResourceType] -= resourceAmount.Amount * qty;
            }

            OnCurrencyAmountChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}