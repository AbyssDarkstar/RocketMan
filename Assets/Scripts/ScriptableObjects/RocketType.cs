using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Rockets/RocketType")]
	public class RocketType : ScriptableObject
    {
        public string RocketName;
        public Sprite RocketSprite;
        public Transform Prefab;

        public float LaunchPrepTime;
        public float TravelSpeed;
        public float MaxWeight;

        public CurrencyAmount[] RocketCost;

        public CargoAmount[] StartingCargo;

        public string RocketTooltip()
        {
            var str = string.Empty;

            str += $"\nMax Weight: {MaxWeight}\nStarting Cargo:\n";

            foreach (var amount in StartingCargo)
            {
                str += $" * {amount.CargoType.CargoName}: {amount.Amount * amount.CargoType.CargoWeight} ({amount.Amount})\n";
            }

            str += "\nCost: ";
            foreach (var amount in RocketCost)
            {
                str += $"<color=#{amount.ResourceType.ColourHex}>{amount.ResourceType.CurrencyNameShort}{amount.Amount}</color> ";
            }

            return str.TrimEnd(' ', '\n', '\r');

        }
    }
}