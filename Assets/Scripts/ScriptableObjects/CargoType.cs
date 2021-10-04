using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Cargo/CargoType")]
	public class CargoType : ScriptableObject
    {
        public string CargoName;
        public float CargoWeight;

        public CurrencyAmount[] CargoCost;
        public int CargoReachedOrbitBonusValue;

        public string CargoCostTooltip()
        {
            var str = string.Empty;

            foreach (var amount in CargoCost)
            {
                str += $"<color=#{amount.ResourceType.ColourHex}>{amount.ResourceType.CurrencyNameShort}{amount.Amount}</color> ";
            }

            return str.Trim();
        }
    }
}