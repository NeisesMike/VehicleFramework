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
        private static bool isInited = false;
        public static EquipmentType atramaModuleType = (EquipmentType)625;
        public static EquipmentType atramaArmType    = (EquipmentType)626;
        public static PingType atramaPingType = (PingType)121;
        public static Atlas.Sprite atramaPingSprite = SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(PingType.Exosuit));

        public static List<Atrama> atramaList = new List<Atrama>();

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

        public static void Init()
        {
            if (!isInited)
            {
                // add AtramaHUD to uGUI
                var hud = uGUI.main.gameObject.EnsureComponent<uGUI_AtramaHUD>();
                isInited = true;
            }
        }

        public static void reset()
        {
            uGUI_EquipmentSlotPatcher.hasInited = false;

            atramaList.Clear();

            GameObject.Destroy(atramaEquipment);
            GameObject.Destroy(atramaModule1);
            GameObject.Destroy(atramaModule2);
            GameObject.Destroy(atramaModule3);
            GameObject.Destroy(atramaModule4);
            GameObject.Destroy(atramaModule5);
            GameObject.Destroy(atramaModule6);
            GameObject.Destroy(atramaArmLeft);
            GameObject.Destroy(atramaArmRight);

            GameObject.Destroy(atramaModuleSlot1);
            GameObject.Destroy(atramaModuleSlot2);
            GameObject.Destroy(atramaModuleSlot3);
            GameObject.Destroy(atramaModuleSlot4);
            GameObject.Destroy(atramaModuleSlot5);
            GameObject.Destroy(atramaModuleSlot6);
            GameObject.Destroy(atramaArmSlotLeft);
            GameObject.Destroy(atramaArmSlotRight);
        }

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
                return SpriteManager.Get(SpriteManager.Group.Pings, name);
            }
        }
        public static bool isPlayerNotInAtrama()
        {
            return Player.main.currentMountedVehicle.gameObject.GetComponent<Atrama>() == null;
        }
        public static bool isPlayerAtramaPilotNotPDA(bool pop)
        {
            bool notUsingPDA = Player.main.GetPDA() == null || !Player.main.GetPDA().isInUse;
            bool isPlayerPilotingAtrama = false;
            foreach (Atrama itAtrama in atramaList)
            {
                if (itAtrama.isPlayerPiloting)
                {
                    isPlayerPilotingAtrama = true;
                    break;
                }
            }
            return pop || (notUsingPDA && isPlayerPilotingAtrama);
        }
        public static bool isPlayerAtramaPilot()
        {
            bool isPlayerPilotingAtrama = false;
            foreach (Atrama itAtrama in atramaList)
            {
                Logger.Log("Iterating over Atrama");
                if (itAtrama.isPlayerPiloting)
                {
                    isPlayerPilotingAtrama = true;
                    break;
                }
            }
            return isPlayerPilotingAtrama;
        }
    }
}
