using UnityEngine;

namespace VehicleFramework
{
    public abstract class BaseLightController : MonoBehaviour, IPowerChanged, IScuttleListener, IDockListener
    {
        private bool canLightsBeEnabled = true;
        private bool isDocked = false;
        private bool isScuttled = false;
        private bool _isLightsOn = false;
        public bool IsLightsOn
        {
            get
            {
                return _isLightsOn;
            }
            private set
            {
                bool oldValue = _isLightsOn;
                if (canLightsBeEnabled && !isScuttled && !isDocked)
                {
                    _isLightsOn = value;
                }
                else
                {
                    _isLightsOn = false;
                }
                HandleLighting(IsLightsOn);
                if(oldValue != IsLightsOn)
                {
                    HandleSound(IsLightsOn);
                }
            }
        }
        protected abstract void HandleLighting(bool active);
        protected abstract void HandleSound(bool playSound);
        public void Toggle()
        {
            IsLightsOn = !IsLightsOn;
        }
        void IPowerChanged.OnPowerChanged(bool hasBatteryPower, bool isSwitchedOn)
        {
            canLightsBeEnabled = hasBatteryPower && isSwitchedOn;
            IsLightsOn = IsLightsOn;
        }

        void IScuttleListener.OnScuttle()
        {
            isScuttled = true;
            IsLightsOn = IsLightsOn;
        }
        void IScuttleListener.OnUnscuttle()
        {
            isScuttled = false;
            IsLightsOn = IsLightsOn;
        }
        void IDockListener.OnDock()
        {
            isDocked = true;
            IsLightsOn = IsLightsOn;
        }
        void IDockListener.OnUndock()
        {
            isDocked = false;
            IsLightsOn = IsLightsOn;
        }
    }
}
