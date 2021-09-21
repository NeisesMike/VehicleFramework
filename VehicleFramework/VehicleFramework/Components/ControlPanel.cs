using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class ControlPanel : MonoBehaviour
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
            button1.EnsureComponent<ControlPanelButton>().Init(ExteriorLightsClick, ExteriorLightsHover);
            button2.EnsureComponent<ControlPanelButton>().Init(InteriorLightsClick, InteriorLightsHover);
            button3.EnsureComponent<ControlPanelButton>().Init(PowerClick, PowerHover);
            button4.EnsureComponent<ControlPanelButton>().Init(AutoPilotClick, AutoPilotHover);

        }

        public bool ExteriorLightsClick()
        {
            mv.vLights.ToggleExteriorLighting();
            return true;
        }
        public bool ExteriorLightsHover()
        {
            HandReticle.main.SetInteractText("Toggle Exterior Lighting");
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            return true;
        }
        public bool InteriorLightsClick()
        {
            mv.vLights.ToggleInteriorLighting();
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

    }
}
