using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Localization;

namespace VehicleFramework
{
    public class PilotingTrigger : HandTarget, IHandTarget, IScuttleListener, IDockListener
    {
        public ModVehicle mv;
        public Transform exit;
        private bool isLive = true;
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if (!mv.GetPilotingMode() && mv.IsPowered() && isLive)
            {
                // TODO multiplayer?
                if(mv as Submarine != null)
                {
                    (mv as Submarine).thisStopPilotingLocation = exit;
                }
                mv.BeginPiloting();
            }
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            if (!mv.GetPilotingMode() && mv.IsPowered() && isLive)
            {
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.StartPiloting));
            }
        }

        void IScuttleListener.OnScuttle()
        {
            isLive = false;
        }

        void IScuttleListener.OnUnscuttle()
        {
            isLive = true;
        }

        void IDockListener.OnDock()
        {
            isLive = false;
        }

        void IDockListener.OnUndock()
        {
            isLive = true;
        }
    }
}
