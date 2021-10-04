using System;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts.EventHandlers
{
    public class CargoQtyChangedEventArgs : EventArgs
    {
        public CargoType CargoType;
        public int CargoAmount;
    }
}