using System;

namespace VehicleFramework.VehicleComponents
{
    public abstract class ModVehicleDockingBay : DockingBay
    {
        private ModVehicle mv => GetComponent<ModVehicle>();
        protected float rechargePerFrame = 1f;
        protected override void OnFinishedDocking(Vehicle dockingVehicle)
        {
            base.OnFinishedDocking(dockingVehicle);
            if(Player.main.currentMountedVehicle == dockingVehicle)
            {
                mv.PlayerEntry();
            }
        }
        protected override void OnStartedUndocking(bool withPlayer, Vehicle cdVehicle)
        {
            if (withPlayer)
            {
                mv.PlayerExit();
            }
            base.OnStartedUndocking(withPlayer, cdVehicle);
        }
        protected override void TryRechargeDockedVehicle(Vehicle cdVehicle)
        {
            base.TryRechargeDockedVehicle(cdVehicle);
            mv.GetEnergyValues(out float charge, out float _);
            cdVehicle.GetEnergyValues(out float dockedEnergy, out float dockedCapacity);
            float dockDesires = dockedCapacity - dockedEnergy;
            float dockRecharge = Math.Min(1, dockDesires);
            if (charge > dockRecharge && dockRecharge > 0)
            {
                if (mv.powerMan == null)
                {
                    throw Admin.SessionManager.Fatal("MV.powerMan is null! Cannot drain power!");
                }
                float actual = mv.powerMan.TrySpendEnergy(dockRecharge);
                cdVehicle.AddEnergy(actual);
            }
        }
    }
}
