using UnityEngine;

namespace VehicleFramework
{
    public class HeadLightsController : BaseLightController
    {
        private ModVehicle MV => GetComponent<ModVehicle>();
        protected override void HandleLighting(bool active)
        {
            MV.HeadLights.ForEach(x => x.Light.SetActive(active));
            if (active)
            {
                MV.NotifyStatus(LightsStatus.OnHeadLightsOn);
            }
            else
            {
                MV.NotifyStatus(LightsStatus.OnHeadLightsOff);
            }
        }

        protected override void HandleSound(bool turnOn)
        {
            if(turnOn)
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
            if (MV.HeadLights == null || MV.HeadLights.Count < 1)
            {
                Component.DestroyImmediate(this);
            }
        }

        protected virtual void Update()
        {
            bool isHeadlightsButtonPressed = GameInput.GetKeyDown(MainPatcher.VFConfig.headlightsButton);
            isHeadlightsButtonPressed |= GameInput.GetButtonDown(GameInput.Button.LeftHand) && MainPatcher.VFConfig.leftTriggerHeadlights;
            isHeadlightsButtonPressed |= GameInput.GetButtonDown(GameInput.Button.RightHand) && MainPatcher.VFConfig.rightTriggerHeadlights;
            if (MV.IsPlayerControlling() && isHeadlightsButtonPressed && !Player.main.GetPDA().isInUse)
            {
                Toggle();
            }
        }
    }
}
