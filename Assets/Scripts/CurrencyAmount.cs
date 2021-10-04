using System;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts
{
    [Serializable]
    public class CurrencyAmount
    {
        public CurrencyType ResourceType;
        public int Amount;
    }
}