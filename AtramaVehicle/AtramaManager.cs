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
        public static EquipmentType atramaModuleType = (EquipmentType)625;
        public static EquipmentType atramaArmType    = (EquipmentType)626;
        public static PingType atramaPingType = (PingType)121;
        public static Atlas.Sprite atramaPingSprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(PingType.Exosuit));

        public static GameObject atramaEquipment;

        public static GameObject atramaModule1;
        public static GameObject atramaModule2;
        public static GameObject atramaModule3;
        public static GameObject atramaModule4;
        public static GameObject atramaModule5;
        public static GameObject atramaModule6;
        public static GameObject atramaArmLeft;
        public static GameObject atramaArmRight;

        public static uGUI_EquipmentSlot atramaModuleSlot1;
        public static uGUI_EquipmentSlot atramaModuleSlot2;
        public static uGUI_EquipmentSlot atramaModuleSlot3;
        public static uGUI_EquipmentSlot atramaModuleSlot4;
        public static uGUI_EquipmentSlot atramaModuleSlot5;
        public static uGUI_EquipmentSlot atramaModuleSlot6;
        public static uGUI_EquipmentSlot atramaArmSlotLeft;
        public static uGUI_EquipmentSlot atramaArmSlotRight;

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

        public static string getPingTypeString(CachedEnumString<PingType> cache, PingType inputType)
        {
            if(inputType == atramaPingType)
            {
                return "AtramaPingType";
            }
            else
            {
                return PingManager.sCachedPingTypeStrings.Get(inputType);
            }
        }
        public static Atlas.Sprite getPingTypeSprite(SpriteManager.Group group, string name)
        {
            if (name == "AtramaPingType")
            {
                return atramaPingSprite;
            }
            else
            {
                Logger.Log("ping sprite size is " + SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(PingType.Exosuit)).size.ToString());
                return SpriteManager.Get(SpriteManager.Group.Pings, name);
            }
        }
    }
}
