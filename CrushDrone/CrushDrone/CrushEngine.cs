using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Engines;
using UnityEngine;

namespace CrushDrone
{
    public class CrushEngine : CricketEngine
    {
        protected override float FORWARD_TOP_SPEED => 1100;
        protected override float REVERSE_TOP_SPEED => 1100;
        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED * 7;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED * 7;
        protected override float waterDragDecay => 15f;
        protected override float DragThresholdSpeed => 0.2f;

        public override void DrainPower(Vector3 moveDirection)
        {
            /* Rationale for these values
             * Seamoth spends this on Update
             * base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
             * where vector.magnitude in [0,3];
             * instead of enginePowerConsumption, we have upgradeModifier, but they are similar if not identical
             * so the power consumption is similar to that of a seamoth.
             */
            float scalarFactor = 0.04f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.GetComponent<PowerManager>().TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
    }
}
