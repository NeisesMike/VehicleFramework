using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public class PairingButton : HandTarget, IHandTarget
    {
        public VehicleFramework.VehicleTypes.Drone drone = null;
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            if ((drone as IDroneInterface).IsInPairingModeAsInitiator())
            {
                (drone as IDroneInterface).FinalizePairingMode();
            }
            else if ((drone as IDroneInterface).IsInPairingModeAsResponder())
            {
                DroneStation.FastenConnection(DroneStation.BroadcastingStation, drone);
                (drone as IDroneInterface).FinalizePairingMode();
            }
            else
            {
                (drone as IDroneInterface).InitiatePairingMode();
            }
        }

        void IHandTarget.OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            if ((drone as IDroneInterface).IsInPairingModeAsInitiator())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Cancel Pairing");
            }
            else if ((drone as IDroneInterface).IsInPairingModeAsResponder())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Confirm Pairing");
            }
            else
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Enter Pairing Mode");
            }
        }
    }
}
