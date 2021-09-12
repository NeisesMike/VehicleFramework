using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
	/*
	[HarmonyPatch(typeof(ItemsContainer))]
	class ItemsContainerPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("UnsafeAdd")]
		public static bool UnsafeAdd(ItemsContainer __instance, InventoryItem item)
		{
			TechType techType = item.item.GetTechType();
			ItemsContainer.ItemGroup itemGroup;
			if (__instance._items.TryGetValue(techType, out itemGroup))
			{
				itemGroup.items.Add(item);
			}
			else
			{
				Vector2int itemSize = CraftData.GetItemSize(techType);
				itemGroup = new ItemsContainer.ItemGroup((int)techType, itemSize.x, itemSize.y);
				itemGroup.items.Add(item);
				__instance._items.Add(techType, itemGroup);
			}
			item.container = __instance;
			item.item.Reparent(__instance.tr);
			//(item.item as Pickupable).onTechTypeChanged += __instance.UpdateItemTechType;
			int count = __instance.count;
			__instance.count = count + 1;
			__instance.unsorted = true;
			__instance.NotifyAddItem(item);

			return false;
		}
	}
	*/
}
