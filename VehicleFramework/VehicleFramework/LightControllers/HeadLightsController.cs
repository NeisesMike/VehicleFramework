using UnityEngine;
using VehicleFramework.Interfaces;

namespace VehicleFramework.LightControllers
{
    public class HeadLightsController : BaseLightController
    {
        private ModVehicle MV => GetComponent<ModVehicle>();
        protected override void HandleLighting(bool active)
        {
            MV.HeadLights?.ForEach(x => x.Light.SetActive(active));
            foreach (var component in GetComponentsInChildren<ILightsStatusListener>())
            {
                if (active)
                {
                    component.OnHeadLightsOn();
                }
                else
                {
                    component.OnHeadLightsOff();
                }
            }
        }

        protected override void HandleSound(bool turnOn)
        {
            if(turnOn)
            {
                MV.LightsOnSound?.Stop();
                MV.LightsOnSound?.Play();
            }
            else
            {
                MV.LightsOffSound?.Stop();
                MV.LightsOffSound?.Play();
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
            bool isHeadlightsButtonPressed = Input.GetKeyDown(MainPatcher.NautilusConfig.HeadlightsButton);
            isHeadlightsButtonPressed |= GameInput.GetButtonDown(GameInput.Button.LeftHand) && MainPatcher.NautilusConfig.LeftClickHeadlights;
            isHeadlightsButtonPressed |= GameInput.GetButtonDown(GameInput.Button.RightHand) && MainPatcher.NautilusConfig.RightClickHeadlights;
            if (MV.IsPlayerControlling() && isHeadlightsButtonPressed && !Player.main.GetPDA().isInUse)
            {
                Toggle();
            }
        }
    }
}
