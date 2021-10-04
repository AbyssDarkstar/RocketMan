using System;

namespace Assets.Scripts.EventHandlers
{
    public class PeakHeightReachedEventArgs : EventArgs
    {
        public float MaxHeight;
        public bool AchievedOrbit;
    }
}
