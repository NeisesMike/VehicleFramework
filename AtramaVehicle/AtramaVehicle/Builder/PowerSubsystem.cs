using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle.Builder
{
    public static partial class AtramaBuilder
    {
        public static void addPowerSubsystem(Atrama atrama)
        {
            Logger.Log("Add Power Subsystem");
            atrama.vehicle.enabled = false;

            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();

            List<EnergyMixin> atramaEnergyMixins = new List<EnergyMixin>();

            // Setup battery inputs
            List<GameObject> batterySlots = new List<GameObject>();
            batterySlots.Add(atrama.transform.Find("Mechanical-Panel/BatteryInputs/1").gameObject);
            batterySlots.Add(atrama.transform.Find("Mechanical-Panel/BatteryInputs/2").gameObject);
            batterySlots.Add(atrama.transform.Find("Mechanical-Panel/BatteryInputs/3").gameObject);
            batterySlots.Add(atrama.transform.Find("Mechanical-Panel/BatteryInputs/4").gameObject);
            foreach (GameObject slot in batterySlots)
            {
                // Configure energy mixin
                var em = slot.EnsureComponent<EnergyMixin>();
                em.storageRoot = atrama.storageRoot;
                em.defaultBattery = seamothEnergyMixin.defaultBattery;
                em.compatibleBatteries = seamothEnergyMixin.compatibleBatteries;
                em.soundPowerUp = seamothEnergyMixin.soundPowerUp;
                em.soundPowerDown = seamothEnergyMixin.soundPowerDown;
                em.soundBatteryAdd = seamothEnergyMixin.soundBatteryAdd;
                em.soundBatteryRemove = seamothEnergyMixin.soundBatteryRemove;
                em.batteryModels = seamothEnergyMixin.batteryModels;
                //atramaEnergyMixin.capacity = 500; //TODO
                //atramaEnergyMixin.batterySlot = 

                atramaEnergyMixins.Add(em);

                slot.EnsureComponent<AtramaBatteryInput>().mixin = em;
            }

            // Configure energy interface
            atrama.energyInterface = atrama.vehicle.gameObject.EnsureComponent<EnergyInterface>();
            atrama.energyInterface.sources = atramaEnergyMixins.ToArray();

            atrama.vehicle.enabled = true;
        }
    }
}
