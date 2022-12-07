using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.StorageComponents
{
    public class BatteryProxy : MonoBehaviour
    {
        public Transform proxy = null;
        public EnergyMixin mixin = null;

        public void Awake()
        {
            if(proxy is null || mixin is null)
            {
                return;
            }
            foreach(Transform tran in proxy)
            {
                GameObject.Destroy(tran.gameObject);
            }
            for (int i = 0; i < mixin.batteryModels.Length; i++)
            {
                mixin.batteryModels[i].model.SetActive(true);
                var model = GameObject.Instantiate(mixin.batteryModels[i].model, proxy);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                if(model.name.ToLower().Contains("ion"))
                {
                    model.transform.localScale *= 0.01f;
                }
                mixin.batteryModels[i].model = model;
            }
        }
        /*
        public void Awake()
        {
            if (battery is null)
            {
                if (proxy.childCount == 0)
                {
                    battery = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    battery.transform.SetParent(proxy);
                    battery.transform.localScale = Vector3.one * 0.001f;
                    battery.transform.localPosition = Vector3.zero;
                    battery.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    battery = proxy.GetChild(0).gameObject;
                }
            }
        }
        public void Register()
        {
            for (int i = 0; i < mixin.batteryModels.Length; i++)
            {
                var model = GameObject.Instantiate(mixin.batteryModels[i].model, proxy);
                battery.transform.localPosition = Vector3.zero;
                battery.transform.localRotation = Quaternion.identity;
                mixin.batteryModels[i].model = model;
            }
        }
        public void ShowBattery()
        {
            battery.SetActive(true);
        }
        public void HideBattery()
        {
            battery.SetActive(false);
        }
        */
    }
}
