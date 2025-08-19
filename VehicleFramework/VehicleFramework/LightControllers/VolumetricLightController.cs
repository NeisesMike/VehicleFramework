using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;

namespace VehicleFramework.LightControllers
{
    public class VolumetricLightController : MonoBehaviour, IPlayerListener, ILightsStatusListener
    {
        private ModVehicle MV => GetComponent<ModVehicle>();
        protected virtual void Awake()
        {
            if (MV.volumetricLights.Count < 1)
            {
                Component.DestroyImmediate(this);
            }
        }

        private void SetVolumetricLights(bool active)
        {
            MV.volumetricLights.ForEach(x => x.SetActive(active));
        }

        void IPlayerListener.OnPilotBegin()
        {
            return;
        }

        void IPlayerListener.OnPilotEnd()
        {
            return;
        }

        void IPlayerListener.OnPlayerEntry()
        {
            SetVolumetricLights(false);
        }

        void IPlayerListener.OnPlayerExit()
        {
            SetVolumetricLights(true);
        }

        void ILightsStatusListener.OnHeadLightsOn()
        {
            if(MV.IsUnderCommand)
            {
                SetVolumetricLights(false);
            }
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
            if (MV.IsUnderCommand)
            {
                SetVolumetricLights(false);
            }
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
        }
    }
}
