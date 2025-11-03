using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Interfaces;
using Newtonsoft.Json.Linq;

namespace VehicleFramework.SaveLoad
{
    internal class VFSimpleSaveLoad : MonoBehaviour, ISaveLoadListener
    {
        internal ModVehicle MV => GetComponent<ModVehicle>();
        string ISaveLoadListener.SaveDataKey => "CoreData";
        bool ISaveLoadListener.IsReady()
        {
            return GameStateWatcher.IsWorldLoaded && MV != null;
        }
        void ISaveLoadListener.LoadData(JToken? data)
        {
            if (data == null) return;
            if (data is not JObject _)
                throw new Newtonsoft.Json.JsonException("Expected a JSON object for Dictionary<string,string>.");
            Dictionary<string, string>? loadData = data.ToObject<Dictionary<string, string>>();
            if (loadData != null)
            {
                Admin.SessionManager.StartCoroutine(MV.LoadSimpleData(loadData));
            }
        }
        object? ISaveLoadListener.SaveData()
        {
            return MV.SaveSimpleData();
        }
    }
}
