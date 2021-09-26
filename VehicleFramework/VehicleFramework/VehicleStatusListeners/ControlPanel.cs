using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class ControlPanel : MonoBehaviour, IVehicleStatusListener
    {
        public ModVehicle mv;
        private GameObject button1;
        private GameObject button2;
        private GameObject button3;
        private GameObject button4;

        public void Init()
        {
            // find buttons
            button1 = transform.Find("Button1").gameObject;
            button2 = transform.Find("Button2").gameObject;
            button3 = transform.Find("Button3").gameObject;
            button4 = transform.Find("Button4").gameObject;

            // give buttons their colliders, for touching
            button1.EnsureComponent<BoxCollider>();
            button2.EnsureComponent<BoxCollider>();
            button3.EnsureComponent<BoxCollider>();
            button4.EnsureComponent<BoxCollider>();

            // give buttons their logic, for executing
            button1.EnsureComponent<ControlPanelButton>().Init(HeadlightsClick, HeadlightsLightsHover);
            button2.EnsureComponent<ControlPanelButton>().Init(InteriorLightsClick, InteriorLightsHover);
            button3.EnsureComponent<ControlPanelButton>().Init(PowerClick, PowerHover);
            button4.EnsureComponent<ControlPanelButton>().Init(AutoPilotClick, AutoPilotHover);

            SetButtonLightingActive(button1, false);
            SetButtonLightingActive(button2, false);
            SetButtonLightingActive(button3, false);
            SetButtonLightingActive(button4, false);
        }

        public bool HeadlightsClick()
        {
            mv.headlights.ToggleHeadlights();
            return true;
        }
        public bool HeadlightsLightsHover()
        {
            HandReticle.main.SetInteractText("Toggle Exterior Lighting");
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            return true;
        }
        public bool InteriorLightsClick()
        {
            mv.interiorlights.ToggleInteriorLighting();
            return true;
        }
        public bool InteriorLightsHover()
        {
            HandReticle.main.SetInteractText("Toggle Interior Lighting");
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            return true;
        }
        public bool PowerClick()
        {
            mv.TogglePower();
            return true;
        }
        public bool PowerHover()
        {
            HandReticle.main.SetInteractText("Toggle Power");
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            return true;
        }
        public bool AutoPilotClick()
        {
            // TODO
            return true;
        }
        public bool AutoPilotHover()
        {
            HandReticle.main.SetInteractText("Open Auto-Pilot (to do)");
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            return true;
        }

        public void SetButtonLightingActive(GameObject button, bool active)
        {
            if (active)
            {
                foreach (var renderer in button.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_GlowStrength", 0.1f);
                        mat.SetFloat("_GlowStrengthNight", 0.1f);
                        mat.SetFloat("_EmissionLM", 0.01f);
                        mat.SetFloat("_EmissionLMNight", 0.01f);
                        mat.SetColor("_GlowColor", Color.red);
                        mat.SetColor("_Color", Color.red);
                    }
                }
            }
            else
            {
                foreach (var renderer in button.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.DisableKeyword("MARMO_EMISSION");
                        mat.SetColor("_Color", Color.white);
                    }
                }
            }
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
        }

        void IVehicleStatusListener.OnPilotBegin()
        {
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPowerUp()
        {
            SetButtonLightingActive(button1, false);
            SetButtonLightingActive(button2, false);
            SetButtonLightingActive(button3, false);
            SetButtonLightingActive(button4, false);
        }

        void IVehicleStatusListener.OnPowerDown()
        {
            SetButtonLightingActive(button1, false);
            SetButtonLightingActive(button2, false);
            SetButtonLightingActive(button3, false);
            SetButtonLightingActive(button4, false);
        }

        void IVehicleStatusListener.OnHeadLightsOn()
        {
            SetButtonLightingActive(button1, false);
        }

        void IVehicleStatusListener.OnHeadLightsOff()
        {
            SetButtonLightingActive(button1, true);
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
            SetButtonLightingActive(button2, false);
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
            SetButtonLightingActive(button2, true);
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }

        void IVehicleStatusListener.OnAutoLevel()
        {
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
            SetButtonLightingActive(button4, false);
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
            SetButtonLightingActive(button4, true);
        }

        void IVehicleStatusListener.OnBatteryLow()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryDepletion()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnFloodLightsOn()
        {
        }

        void IVehicleStatusListener.OnFloodLightsOff()
        {
        }
    }
}
