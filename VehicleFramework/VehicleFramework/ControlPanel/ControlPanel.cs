﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Localization;

namespace VehicleFramework
{
    public class ControlPanel : MonoBehaviour, IVehicleStatusListener, IPowerListener, ILightsStatusListener, IAutoPilotListener
    {
        public Submarine mv;

        private GameObject buttonHeadLights;
        private GameObject buttonNavLights;
        private GameObject buttonAutoPilot;
        private GameObject buttonInteriorLights;
        private GameObject button5;
        private GameObject buttonDefaultColor;
        private GameObject buttonFloodLights;
        private GameObject button8;
        private GameObject buttonPower;

        public void Init()
        {
            // find buttons
            buttonHeadLights = transform.Find("1").gameObject;
            buttonNavLights = transform.Find("2").gameObject;
            buttonAutoPilot = transform.Find("3").gameObject;
            buttonInteriorLights = transform.Find("4").gameObject;
            button5 = transform.Find("5").gameObject;
            buttonDefaultColor = transform.Find("6").gameObject;
            buttonFloodLights = transform.Find("7").gameObject;
            button8 = transform.Find("8").gameObject;
            buttonPower = transform.Find("9").gameObject;

            // give buttons their colliders, for touching
            buttonHeadLights.EnsureComponent<BoxCollider>();
            buttonNavLights.EnsureComponent<BoxCollider>();
            buttonAutoPilot.EnsureComponent<BoxCollider>();
            buttonInteriorLights.EnsureComponent<BoxCollider>();
            button5.EnsureComponent<BoxCollider>();
            buttonDefaultColor.EnsureComponent<BoxCollider>();
            buttonFloodLights.EnsureComponent<BoxCollider>();
            button8.EnsureComponent<BoxCollider>();
            buttonPower.EnsureComponent<BoxCollider>();

            // give buttons their logic, for executing
            buttonHeadLights.EnsureComponent<ControlPanelButton>().Init(HeadlightsClick, HeadLightsHover);
            buttonNavLights.EnsureComponent<ControlPanelButton>().Init(NavLightsClick, NavLightsHover);
            buttonAutoPilot.EnsureComponent<ControlPanelButton>().Init(AutoPilotClick, AutoPilotHover);
            buttonInteriorLights.EnsureComponent<ControlPanelButton>().Init(InteriorLightsClick, InteriorLightsHover);
            button5.EnsureComponent<ControlPanelButton>().Init(EmptyClick, EmptyHover);
            buttonDefaultColor.EnsureComponent<ControlPanelButton>().Init(DefaultColorClick, DefaultColorHover);
            buttonFloodLights.EnsureComponent<ControlPanelButton>().Init(FloodLightsClick, FloodLightsHover);
            button8.EnsureComponent<ControlPanelButton>().Init(EmptyClick, EmptyHover);
            buttonPower.EnsureComponent<ControlPanelButton>().Init(PowerClick, PowerHover);

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
        public void EmptyClick()
        {
        }
        public void EmptyHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.EmptyHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void HeadlightsClick()
        {
            if (mv.headlights != null)
            {
                mv.headlights.Toggle();
            }
        }
        public void HeadLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.HeadLightsHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void FloodLightsClick()
        {
            if (mv.floodlights != null)
            {
                mv.floodlights.Toggle();
            }
        }
        public void FloodLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.FloodLightsHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void NavLightsClick()
        {
            if (mv.navlights != null)
            {
                mv.navlights.Toggle();
            }
        }
        public void NavLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.NavLightsHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void InteriorLightsClick()
        {
            if(mv.interiorlights != null)
            {
                mv.interiorlights.Toggle();
            }
        }
        public void InteriorLightsHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.InteriorLightsHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void DefaultColorClick()
        {
            mv.PaintVehicleDefaultStyle(mv.GetName());
        }
        public void DefaultColorHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.DefaultColorHover));
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
        public void PowerHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.PowerHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void AutoPilotClick()
        {
            // TODO
        }
        public void AutoPilotHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.AutoPilotHover));
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
        public void SetButtonLightingActive(GameObject button, bool active)
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
    }
}
