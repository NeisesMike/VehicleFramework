using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Admin
{
    public interface IGameObjectManager
    {
        void ClearList();
    }
    public class GameObjectManager<T> : IGameObjectManager where T : Component
    {
        public static List<T> AllSuchObjects = new List<T>();
        public static T FindNearestSuch(Vector3 target, Func<T, bool> filter=null)
        {
            float ComputeDistance(T thisObject)
            {
                try
                {
                    return Vector3.Distance(target, thisObject.transform.position);
                }
                catch
                {
                    return 99999;
                }
            }

            List<T> FilteredSuchObjects = AllSuchObjects.Where(x => x != null).ToList();
            if(filter != null)
            {
                FilteredSuchObjects = FilteredSuchObjects.Where(x=>filter(x)).ToList();
            }
            return FilteredSuchObjects.OrderBy(x => ComputeDistance(x)).FirstOrDefault();
        }

        public static void Register(T cont)
        {
            AllSuchObjects.Add(cont);
        }

        public static void Deregister(T cont)
        {
            AllSuchObjects.Remove(cont);
        }

        public void ClearList()
        {
            AllSuchObjects.Clear();
        }
    }

}
