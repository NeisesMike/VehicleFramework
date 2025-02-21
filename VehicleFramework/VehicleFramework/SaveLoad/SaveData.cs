using System;
using System.Collections.Generic;
using UnityEngine;

using techtype = System.String;
using upgrades = System.Collections.Generic.Dictionary<string, System.String>;
using batteries = System.Collections.Generic.List<System.Tuple<System.String, float>>;
using innateStorages = System.Collections.Generic.List<System.Tuple<UnityEngine.Vector3, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using modularStorages = System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using color = System.Tuple<float, float, float, float>;

namespace VehicleFramework.SaveLoad
{
    [Nautilus.Json.Attributes.FileName("vehicle_storage")]
    internal class SaveData : Nautilus.Json.SaveDataCache
    {
        public List<Tuple<Vector3, bool>> IsPlayerInside { get; set; }
        public List<Tuple<Vector3, bool>> IsPlayerControlling { get; set; }
        public List<Tuple<Vector3, upgrades>> UpgradeLists { get; set; }
        public List<Tuple<Vector3, innateStorages>> InnateStorages { get; set; }
        public List<Tuple<Vector3, modularStorages>> ModularStorages { get; set; }
        public List<Tuple<Vector3, batteries>> Batteries { get; set; }
        public List<Tuple<Vector3, batteries>> BackupBatteries { get; set; }
        public List<Tuple<Vector3, string, color, color, color, color, bool>> AllVehiclesAesthetics { get; set; }
        // todo: maybe this?
        // save a few lines in the output json?
        public List<Tuple<Vector3, Tuple<upgrades, innateStorages, modularStorages, batteries>>> AllVehiclesStorages { get; set; }
        public List<Tuple<Vector3, string>> SubNames { get; set; }
        public List<string> HasVehicleTechTypes { get; set; }
    }
}
