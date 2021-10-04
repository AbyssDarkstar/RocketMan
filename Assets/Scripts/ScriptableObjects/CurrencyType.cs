using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Currency/CurrencyType")]
    public class CurrencyType : ScriptableObject
    {
        public string ColourHex;
        public string CurrencyName;
        public string CurrencyNameShort;
        public Sprite CurrencySprite;
	}
}