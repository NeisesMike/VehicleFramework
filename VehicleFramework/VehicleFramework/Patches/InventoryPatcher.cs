using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework.Patches
{
	/*
    [HarmonyPatch(typeof(Inventory))]
    class InventoryPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddOrSwap", argumentTypes: new Type[] { typeof(InventoryItem), typeof(Equipment), typeof(string) })]
        public static bool AddOrSwap(InventoryItem itemA, Equipment equipmentB, string slotB, ref bool __result)
        {
			if (itemA == null || !itemA.CanDrag(true) || equipmentB == null)
			{
				__result = false;
				return false;
			}
			IItemsContainer container = itemA.container;
			if (container == null)
			{
				__result = false;
				return false;
			}
			Pickupable item = itemA.item;
			if (item == null)
			{
				__result = false;
				return false;
			}
			Equipment equipment = container as Equipment;
			bool flag = equipment != null;
			string empty = string.Empty;
			if (flag && !equipment.GetItemSlot(item, ref empty))
			{
				__result = false;
				return false;
			}
			EquipmentType equipmentType = CraftData.GetEquipmentType(item.GetTechType());
			if (string.IsNullOrEmpty(slotB))
			{
				equipmentB.GetCompatibleSlot(equipmentType, out slotB);
			}
			if (string.IsNullOrEmpty(slotB))
			{
				__result = false;
				return false;
			}
			if (container == equipmentB && empty == slotB)
			{
				__result = false;
				return false;
			}
			if (!Equipment.IsCompatible(equipmentType, Equipment.GetSlotType(slotB)))
			{
				__result = false;
				return false;
			}
			InventoryItem inventoryItem = equipmentB.RemoveItem(slotB, false, true);
			if (inventoryItem == null)
			{
				if (equipmentB.AddItem(slotB, itemA, false))
				{
					__result = true;
					return false;
				}
			}
			else if (equipmentB.AddItem(slotB, itemA, false))
			{
				if ((flag && equipment.AddItem(empty, inventoryItem, false)) || (!flag && container.AddItem(inventoryItem)))
				{
					__result = true;
					return false;
				}
				if (flag)
				{
					equipment.AddItem(empty, itemA, true);
				}
				else
				{
					container.AddItem(itemA);
				}
				equipmentB.AddItem(slotB, inventoryItem, true);
			}
			else
			{
				equipmentB.AddItem(slotB, inventoryItem, true);
			}
			__result = false;
			return false;
		}
    }
	*/
}
