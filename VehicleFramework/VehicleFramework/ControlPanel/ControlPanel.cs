using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Interfaces;
using VehicleFramework.LightControllers;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.ControlPanel
{
    public class ControlPanel : MonoBehaviour, IVehicleStatusListener, IPowerListener, ILightsStatusListener, IAutoPilotListener
    {
        public Submarine mv = null!;

        private GameObject? buttonHeadLights;
        private GameObject? buttonNavLights;
        private GameObject? buttonAutoPilot;
        private GameObject? buttonInteriorLights;
        private GameObject? button5;
        private GameObject? buttonDefaultColor;
        private GameObject? buttonFloodLights;
        private GameObject? button8;
        private GameObject? buttonPower;

        private void Awake()
        {
            mv = GetComponentInParent<Submarine>();
            if(mv == null)
            {
                throw Admin.SessionManager.Fatal("ControlPanel: could not find parent ModVehicle");
            }

            // find buttons
            buttonHeadLights = transform.Find("1")?.gameObject;
            buttonNavLights = transform.Find("2")?.gameObject;
            buttonAutoPilot = transform.Find("3")?.gameObject;
            buttonInteriorLights = transform.Find("4")?.gameObject;
            button5 = transform.Find("5")?.gameObject;
            buttonDefaultColor = transform.Find("6")?.gameObject;
            buttonFloodLights = transform.Find("7")?.gameObject;
            button8 = transform.Find("8")?.gameObject;
            buttonPower = transform.Find("9")?.gameObject;

            // give buttons their colliders, for touching
            buttonHeadLights?.EnsureComponent<BoxCollider>();
            buttonNavLights?.EnsureComponent<BoxCollider>();
            buttonAutoPilot?.EnsureComponent<BoxCollider>();
            buttonInteriorLights?.EnsureComponent<BoxCollider>();
            button5?.EnsureComponent<BoxCollider>();
            buttonDefaultColor?.EnsureComponent<BoxCollider>();
            buttonFloodLights?.EnsureComponent<BoxCollider>();
            button8?.EnsureComponent<BoxCollider>();
            buttonPower?.EnsureComponent<BoxCollider>();

            // give buttons their logic, for executing
            buttonHeadLights?.EnsureComponent<ControlPanelButton>().Init(HeadlightsClick, HeadLightsHover);
            buttonNavLights?.EnsureComponent<ControlPanelButton>().Init(NavLightsClick, NavLightsHover);
            buttonAutoPilot?.EnsureComponent<ControlPanelButton>().Init(AutoPilotClick, AutoPilotHover);
            buttonInteriorLights?.EnsureComponent<ControlPanelButton>().Init(InteriorLightsClick, InteriorLightsHover);
            button5?.EnsureComponent<ControlPanelButton>().Init(EmptyClick, EmptyHover);
            buttonDefaultColor?.EnsureComponent<ControlPanelButton>().Init(DefaultColorClick, DefaultColorHover);
            buttonFloodLights?.EnsureComponent<ControlPanelButton>().Init(FloodLightsClick, FloodLightsHover);
            button8?.EnsureComponent<ControlPanelButton>().Init(EmptyClick, EmptyHover);
            buttonPower?.EnsureComponent<ControlPanelButton>().Init(PowerClick, PowerHover);

            ResetAllButtonLighting();
        }
        private void ResetAllButtonLighting()
        {
            SetButtonLightingActive(buttonHeadLights, false);
            SetButtonLightingActive(buttonNavLights, false);
            SetButtonLightingActive(buttonAutoPilot, false);
            SetButtonLightingActive(buttonInteriorLights, true);
            SetButtonLightingActive(button5, false);
            SetButtonLightingActive(buttonDefaultColor, false);
            SetButtonLightingActive(buttonFloodLights, false);
            SetButtonLightingActive(button8, false);
            SetButtonLightingActive(buttonPower, true);
        }
        private void AdjustButtonLightingForPowerDown()
        {
            SetButtonLightingActive(buttonHeadLights, false);
            SetButtonLightingActive(buttonNavLights, false);
            SetButtonLightingActive(buttonAutoPilot, false);
            SetButtonLightingActive(buttonInteriorLights, false);
            SetButtonLightingActive(button5, false);
            SetButtonLightingActive(buttonDefaultColor, false);
            SetButtonLightingActive(buttonFloodLights, false);
            SetButtonLightingActive(button8, false);
            SetButtonLightingActive(buttonPower, false);
        }
        public static void EmptyClick()
        {
        }
        public static void EmptyHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFEmptyHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void HeadlightsClick()
        {
            mv.GetComponent<HeadLightsController>()?.Toggle();
        }
        public static void HeadLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFHeadLightsHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void FloodLightsClick()
        {
            mv.GetComponent<FloodLightsController>()?.Toggle();
        }
        public static void FloodLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFFloodLightsHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void NavLightsClick()
        {
            mv.GetComponent<NavigationLightsController>()?.Toggle();
        }
        public static void NavLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFNavLightsHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void InteriorLightsClick()
        {
            mv.GetComponent<InteriorLightsController>()?.Toggle();
        }
        public static void InteriorLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFInteriorLightsHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void DefaultColorClick()
        {
            mv.SetVehicleDefaultStyle(mv.HullName);
        }
        public static void DefaultColorHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFDefaultColorHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void PowerClick()
        {
            mv.energyInterface.GetValues(out float charge, out _);
            if (0 < charge)
            {
                mv.TogglePower();
            }
        }
        public static void PowerHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFPowerHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public static void AutoPilotClick()
        {
            // TODO
        }
        public static void AutoPilotHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFAutoPilotHover"));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public static void SetButtonLightingActive(GameObject? button, bool active)
        {
            if (button == null)
            {
                Logger.Warn("Tried to set control-panel button active, but it was NULL");
                return;
            }
            if (active)
            {
                foreach (var renderer in button.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.EnableKeyword(Admin.Utils.emissionKeyword);
                        mat.SetFloat(Admin.Utils.glowField, 0.1f);
                        mat.SetFloat(Admin.Utils.glowNightField, 0.1f);
                        mat.SetFloat(Admin.Utils.emissionField, 0.01f);
                        mat.SetFloat(Admin.Utils.emissionNightField, 0.01f);
                        mat.SetColor(Admin.Utils.glowColorField, Color.red);
                        mat.SetColor(Admin.Utils.colorField, Color.red);
                    }
                }
            }
            else
            {
                foreach (var renderer in button.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.DisableKeyword(Admin.Utils.emissionKeyword);
                        mat.SetColor(Admin.Utils.colorField, Color.white);
                    }
                }
            }
        }

        void ILightsStatusListener.OnHeadLightsOn()
        {
            SetButtonLightingActive(buttonHeadLights, true);
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
            SetButtonLightingActive(buttonHeadLights, false);
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
            SetButtonLightingActive(buttonInteriorLights, true);
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
            SetButtonLightingActive(buttonInteriorLights, false);
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }
        void IAutoPilotListener.OnAutoLevelBegin()
        {
        }
        void IAutoPilotListener.OnAutoLevelEnd()
        {
        }

        void IAutoPilotListener.OnAutoPilotBegin()
        {
            SetButtonLightingActive(buttonAutoPilot, true);
        }

        void IAutoPilotListener.OnAutoPilotEnd()
        {
            SetButtonLightingActive(buttonAutoPilot, false);
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
            SetButtonLightingActive(buttonFloodLights, true);
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
            SetButtonLightingActive(buttonFloodLights, false);
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
            SetButtonLightingActive(buttonNavLights, true);
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
            SetButtonLightingActive(buttonNavLights, false);
        }

        void IPowerListener.OnBatterySafe()
        {
        }

        void IPowerListener.OnBatteryLow()
        {
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
        }

        void IPowerListener.OnBatteryDepleted()
        {
        }
        void IPowerListener.OnPowerUp()
        {
            ResetAllButtonLighting();
        }
        void IPowerListener.OnPowerDown()
        {
            AdjustButtonLightingForPowerDown();
        }
        void IPowerListener.OnBatteryDead()
        {
            AdjustButtonLightingForPowerDown();
            SetButtonLightingActive(buttonPower, false);
        }
        void IPowerListener.OnBatteryRevive()
        {
            ResetAllButtonLighting();
        }

        void IVehicleStatusListener.OnNearbyLeviathan()
        {
            SetButtonLightingActive(buttonHeadLights, false);
            SetButtonLightingActive(buttonFloodLights, false);
            SetButtonLightingActive(buttonInteriorLights, false);
            SetButtonLightingActive(buttonNavLights, false);
        }
        internal static void Create(Submarine mv)
        {
            if (mv.ControlPanel != null)
            {
                mv.controlPanelLogic = mv.ControlPanel.EnsureComponent<ControlPanel>();
                mv.controlPanelLogic.mv = mv;
                if (mv.transform.Find("Control-Panel-Location") != null)
                {
                    mv.ControlPanel.transform.localPosition = mv.transform.Find("Control-Panel-Location").localPosition;
                    mv.ControlPanel.transform.localRotation = mv.transform.Find("Control-Panel-Location").localRotation;
                    GameObject.Destroy(mv.transform.Find("Control-Panel-Location").gameObject);
                }
            }
        }
    }
}
