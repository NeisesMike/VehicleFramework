using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;

namespace CricketVehicle
{
    public class CricketContainerManager : MonoBehaviour
    {
        public static CricketContainerManager main
        {
            get
            {
                return Player.main.gameObject.EnsureComponent<CricketContainerManager>();
            }
        }
        public List<CricketContainer> AllCricketContainers = new List<CricketContainer>();
        public CricketContainer FindNearestCricketContainer(Vector3 mount)
        {
            float ComputeDistance(CricketContainer cc)
            {
                try
                {
                    return Vector3.Distance(mount, cc.transform.position);
                }
                catch
                {
                    return 9999;
                }
            }
            CricketContainer nearestContainer = null;
            foreach(CricketContainer cont in AllCricketContainers)
            {
                if (cont is null)
                {
                    continue;
                }
                if (nearestContainer == null || (ComputeDistance(cont) < ComputeDistance(nearestContainer)))
                {
                    nearestContainer = cont;
                }
            }
            //nearestContainer = AllCricketContainers.OrderBy(x => ComputeDistance(x)).FirstOrDefault();
            return nearestContainer;
        }
        public void RegisterCricketContainer(CricketContainer cont)
        {
            AllCricketContainers.Add(cont);
        }
        public void DeregisterCricketContainer(CricketContainer cont)
        {
            AllCricketContainers.Remove(cont);
        }
    }
}
