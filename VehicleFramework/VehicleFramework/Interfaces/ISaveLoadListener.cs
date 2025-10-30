
using Newtonsoft.Json.Linq;
using System;

namespace VehicleFramework.Interfaces
{
    public interface ISaveLoadListener
    {
        bool IsReady();
        string SaveDataKey { get; } // unique key for this listener's save data
        object? SaveData();
        void LoadData(JToken? data);
    }
}
