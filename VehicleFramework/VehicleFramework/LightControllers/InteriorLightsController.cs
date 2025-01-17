using VehicleFramework.VehicleTypes;
using UnityEngine;

namespace VehicleFramework
{
    public class InteriorLightsController : BaseLightController, IPlayerListener
    {
        private Submarine MV => GetComponent<Submarine>();
        protected override void HandleLighting(bool active)
        {
            MV.InteriorLights.ForEach(x => x.enabled = active);
            foreach (var component in GetComponentsInChildren<ILightsStatusListener>())
            {
                if (active)
                {
                    component.OnInteriorLightsOn();
                }
                else
                {
                    component.OnInteriorLightsOff();
                }
            }
        }

        protected override void HandleSound(bool playSound)
        {
            return;
        }
        protected virtual void Awake()
        {
            if (MV.InteriorLights == null || MV.InteriorLights.Count < 1)
            {
                Component.DestroyImmediate(this);
            }
        }

        void IPlayerListener.OnPilotBegin()
        {
        }

        void IPlayerListener.OnPilotEnd()
        {
        }

        void IPlayerListener.OnPlayerEntry()
        {
            if(!IsLightsOn)
            {
                Toggle();
            }
        }

        void IPlayerListener.OnPlayerExit()
        {
            if (IsLightsOn)
            {
                Toggle();
            }
        }
    }
}
