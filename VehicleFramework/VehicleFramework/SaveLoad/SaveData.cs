using System;
using System.Collections.Generic;
using UnityEngine;

using techtype = System.String;
using upgrades = System.Collections.Generic.Dictionary<string, System.String>;
using batteries = System.Collections.Generic.List<System.Tuple<System.String, float>>;
using innateStorages = System.Collections.Generic.List<System.Tuple<UnityEngine.Vector3, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using modularStorages = System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using color = System.Tuple<float, float, float, float>;

/*
 * This file and this save scheme are old news.
 * I've already neutered the serialization methods; they all write empty lists.
 * I'm keeping the deserialization methods intact for now, because that affords VF backwards compatibility.
 * But I'm eager to delete all of this noise in time.
 * The date of publication of the new save scheme is March 9, 2025.
 * After several months, I will relish deleteing so much of this old shitty code.
 * At that time, I'll delete this file and SaveManager.cs and
 * VehicleManager.CreateSaveFileData and VehicleManager.LoadVehicle
 * and the field MainPatcher.SaveFileData
 */

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
        public List<Tuple<Vector3, string>> SubNames { get; set; }
    }
}
