using System.Linq;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public class FloodLightsController : BaseLightController
    {
        private Submarine MV => GetComponent<Submarine>();
        protected override void HandleLighting(bool active)
        {
            MV.FloodLights.ForEach(x => x.Light.SetActive(active));
            if (active)
            {
                MV.FloodLights
                    .Select(x => x.Light.GetComponent<MeshRenderer>())
                    .Where(x => x != null)
                    .SelectMany(x => x.materials)
                    .ForEach(x => Admin.Utils.EnableSimpleEmission(x, 10, 10));
                MV.NotifyStatus(LightsStatus.OnFloodLightsOn);
            }
            else
            {
                MV.FloodLights
                    .Select(x => x.Light.GetComponent<MeshRenderer>())
                    .Where(x => x != null)
                    .SelectMany(x => x.materials)
                    .ForEach(x => Admin.Utils.EnableSimpleEmission(x, 0, 0));
                MV.NotifyStatus(LightsStatus.OnFloodLightsOff);
            }
        }
        protected override void HandleSound(bool playSound)
        {
            if (playSound)
            {
                MV.lightsOnSound.Stop();
                MV.lightsOnSound.Play();
            }
            else
            {
                MV.lightsOffSound.Stop();
                MV.lightsOffSound.Play();
            }
        }
        protected virtual void Awake()
        {
            if (MV.FloodLights == null || MV.FloodLights.Count < 1)
            {
                Component.DestroyImmediate(this);
            }
        }
    }
}
