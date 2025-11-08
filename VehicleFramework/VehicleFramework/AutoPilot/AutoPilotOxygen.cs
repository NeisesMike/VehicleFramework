using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;

namespace VehicleFramework.AutoPilot
{
    public class AutoPilotOxygen : MonoBehaviour
    {
        public ModVehicle MV => GetComponent<ModVehicle>();
        public void Update()
        {
            MaybeRefillOxygen();
        }
        private void MaybeRefillOxygen()
        {
            if (MV.energyInterface == null)
            {
                throw Admin.SessionManager.Fatal("AutoPilot.MaybeRefillOxygen: MV.energyInterface is null!");
            }
            float totalPower = MV.energyInterface.TotalCanProvide(out _);
            float totalAIPower = MV.energyInterface.TotalCanProvide(out _);
            if (totalPower < 0.1 && totalAIPower >= 0.1 && MV.IsUnderCommand)
            {
                // The main batteries are out, so the AI will take over life support.
                OxygenManager oxygenMgr = Player.main.oxygenMgr;
                oxygenMgr.GetTotal(out float num, out float num2);
                float amount = Mathf.Min(num2 - num, MV.oxygenPerSecond * Time.deltaTime) * MV.oxygenEnergyCost;
                float? result = (MV.energyInterface.ConsumeEnergy(amount));
                float secondsToAdd = result.Value / MV.oxygenEnergyCost;
                oxygenMgr.AddOxygen(secondsToAdd);
            }
        }
    }
}
