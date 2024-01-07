using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public class PilotingTrigger : HandTarget, IHandTarget
    {
        public ModVehicle mv;
        public Transform exit;
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if (!mv.GetPilotingMode() && mv.IsPowered())
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
            if (!mv.GetPilotingMode() && mv.IsPowered())
            {
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.StartPiloting));
            }
        }
    }
}
