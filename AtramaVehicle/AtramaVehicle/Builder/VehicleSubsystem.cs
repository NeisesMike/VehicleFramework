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
        public static void addVehicleSubsystem(Atrama atrama)
        {
            Logger.Log("Add Vehicle Subsystem");

            // Ensure vehicle is a physics object
            var rb = atrama.gameObject.EnsureComponent<Rigidbody>();
            rb.mass = 4000f;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            atrama.vehicle.useRigidbody = rb;

            // Add the engine (physics control)
            atrama.gameObject.EnsureComponent<AtramaEngine>();

            // Ensure vehicle remains in the world always
            atrama.gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

            // Add the hud ping instance
            atrama.pingInstance = atrama.gameObject.EnsureComponent<PingInstance>();
            atrama.pingInstance.origin = atrama.transform;
            atrama.pingInstance.pingType = AtramaManager.atramaPingType;
            atrama.pingInstance.SetLabel("Atrama");

            // add various vehicle things
            atrama.vehicle.stabilizeRoll = true;
            atrama.vehicle.controlSheme = Vehicle.ControlSheme.Submersible;
            atrama.vehicle.mainAnimator = atrama.gameObject.EnsureComponent<Animator>();

            // borrow some things from the seamoth
            atrama.vehicle.crushDamage = CopyComponent<CrushDamage>(seamoth.GetComponent<CrushDamage>(), atrama.vehicle.gameObject);
            atrama.vehicle.crushDamage.kBaseCrushDepth = 300;
            atrama.vehicle.crushDamage.damagePerCrush = 5;
            atrama.vehicle.crushDamage.crushPeriod = 3;
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
            atrama.vehicle.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, atrama.vehicle.gameObject);
            atrama.vehicle.toggleLights = CopyComponent<ToggleLights>(seamoth.GetComponent<SeaMoth>().toggleLights, atrama.vehicle.gameObject);
            atrama.vehicle.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, atrama.vehicle.gameObject);

        }
    }
}
