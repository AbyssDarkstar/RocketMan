using System;

namespace Assets.Scripts.EventHandlers
{
    public class CargoUpdatedEventArgs : EventArgs
    {
        public CargoAmount[] Cargo;
        public float CargoWeightMax;
    }
}
