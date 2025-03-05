﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
	public class ModularStorageInput : StorageInput
	{
		public override void OpenFromExternal()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				pda.Open(PDATab.Inventory, null, null);
			}
		}
		protected override void OpenPDA()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			if (storageInSlot == null)
			{
				storageInSlot = gameObject.GetComponent<SeamothStorageContainer>().container;
			}

			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				if (!pda.Open(PDATab.Inventory, this.tr, new PDA.OnClose(this.OnClosePDA)))
				{
					this.OnClosePDA(pda);
					return;
				}
			}
			else
			{
				this.OnClosePDA(null);
			}
		}
		public ItemsContainer GetContainer()
		{
			return mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
		}
		internal static List<ItemsContainer> GetAllModularStorageContainers(ModVehicle mv)
		{
			List<ItemsContainer> result = new List<ItemsContainer>();
			if (mv == null)
			{
				return result;
			}
			var containerList = mv.ModulesRootObject.GetComponentsInChildren<SeamothStorageContainer>(true);
			if (!containerList.Any())
			{
				return result;
			}
			return containerList.Select(x => x.container).ToList();
		}
	}
}
