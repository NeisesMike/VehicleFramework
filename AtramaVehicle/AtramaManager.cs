using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Reflection;

using SMLHelper;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace AtramaVehicle
{
    public static class AtramaManager
    {
        public static List<Atrama> atramaList = new List<Atrama>();

        public static void addAtrama(Atrama thisAtrama)
        {
            atramaList.Add(thisAtrama);
        }

        public static Atrama getCurrentAtrama()
        {
            foreach(Atrama itAtrama in atramaList)
            {
                if(itAtrama.isPlayerInside)
                {
                    return itAtrama;
                }
            }
            return null;
        }
    }
}
