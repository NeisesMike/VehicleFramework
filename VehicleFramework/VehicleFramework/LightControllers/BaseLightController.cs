using Newtonsoft.Json.Linq;
using UnityEngine;
using VehicleFramework.Interfaces;

namespace VehicleFramework.LightControllers
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
                BumpLights(value);
            }
        }
        private void BumpLights(bool value)
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
            if (oldValue != IsLightsOn)
            {
                HandleSound(IsLightsOn);
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
            BumpLights(IsLightsOn);
        }

        void IScuttleListener.OnScuttle()
        {
            isScuttled = true;
            BumpLights(IsLightsOn);
        }
        void IScuttleListener.OnUnscuttle()
        {
            isScuttled = false;
            BumpLights(IsLightsOn);
        }
        void IDockListener.OnDock()
        {
            isDocked = true;
            BumpLights(IsLightsOn);
        }
        void IDockListener.OnUndock()
        {
            isDocked = false;
            BumpLights(IsLightsOn);
        }
    }
}
