using System;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts
{
    [Serializable]
    public class CargoAmount
    {
        public CargoType CargoType;
        public int Amount;
    }
}