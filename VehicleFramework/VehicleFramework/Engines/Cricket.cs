using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
    public class CricketEngine : ModVehicleEngine
    {
        protected override float FORWARD_TOP_SPEED => 750;
        protected override float REVERSE_TOP_SPEED => 750;
        protected override float STRAFE_MAX_SPEED => 750;
        protected override float VERT_MAX_SPEED => 750;

        protected override float FORWARD_ACCEL => FORWARD_TOP_SPEED * 3;
        protected override float REVERSE_ACCEL => REVERSE_TOP_SPEED * 3;
        protected override float STRAFE_ACCEL => STRAFE_MAX_SPEED * 3;
        protected override float VERT_ACCEL => VERT_MAX_SPEED * 3;

        protected override float waterDragDecay => 2.5f;
    }
}
