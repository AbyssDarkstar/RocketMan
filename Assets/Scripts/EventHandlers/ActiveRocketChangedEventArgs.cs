using System;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts.EventHandlers
{
    public class ActiveRocketChangedEventArgs : EventArgs
    {
        public RocketType ActiveRocketType;
        public CargoAmount[] StartingCargo;
    }
}
