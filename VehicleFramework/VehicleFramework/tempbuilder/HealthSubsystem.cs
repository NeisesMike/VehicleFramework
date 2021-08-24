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
        public static void addHealthSubsystem(Atrama atrama)
        {
            Logger.Log("Add Health Subsystem");
            // Ensure Atrama can die
            // TODO read this lol
            var liveMixin = atrama.vehicle.gameObject.EnsureComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = false;
            lmData.destroyOnDeath = false;
            lmData.explodeOnDestroy = false;
            lmData.invincibleInCreative = true;
            lmData.weldable = false;
            lmData.minDamageForSound = 20f;
            lmData.maxHealth = 1000;
            liveMixin.data = lmData;
            atrama.vehicle.liveMixin = liveMixin;
        }
    }
}
