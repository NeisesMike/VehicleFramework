using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;

namespace VehicleFramework.VehicleChildComponents
{
    public class PilotingTrigger : HandTarget, IHandTarget, IScuttleListener, IDockListener
    {
        public required ModVehicle mv;
        public Transform? exit;
        private bool isLive = true;
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if (!mv.GetPilotingMode() && mv.IsPowered() && isLive)
            {
                Submarine? Sub = mv as Submarine;
                if(Sub != null)
                {
                    Sub.thisStopPilotingLocation = exit;
                }
                mv.BeginPiloting();
            }
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            if (!mv.GetPilotingMode() && mv.IsPowered() && isLive)
            {
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("VFStartPiloting"));
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

        internal static void Create(ModVehicle mv)
        {
            switch(mv)
            {
                case Submarine sub:
                    mv.playerPosition = sub.PilotSeat.SitLocation;
                    PilotingTrigger subpt = sub.PilotSeat.Seat.EnsureComponent<PilotingTrigger>();
                    subpt.mv = mv;
                    subpt.exit = sub.PilotSeat.ExitLocation;
                    break;
                case Submersible submer:
                    mv.playerPosition = submer.PilotSeat.SitLocation;
                    PilotingTrigger submerpt = submer.PilotSeat.Seat.EnsureComponent<PilotingTrigger>();
                    submerpt.mv = mv;
                    submerpt.exit = submer.PilotSeat.ExitLocation;
                    break;
                default:
                    return;
            }
        }
    }
}
