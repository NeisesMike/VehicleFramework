using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.StorageComponents
{
	public class ModularStorageInput : StorageInput
	{
		public override void OpenFromExternal()
        {
            if (mv == null)
            {
                throw Admin.SessionManager.Fatal($"{transform.name} has no ModVehicle component! Please set the ModVehicle before calling OpenPDA.");
            }
            ItemsContainer? storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				pda.Open(PDATab.Inventory, null, null);
			}
		}
		protected override void OpenPDA()
        {
            if (mv == null)
            {
                throw Admin.SessionManager.Fatal($"{transform.name} has no ModVehicle component! Please set the ModVehicle before calling OpenPDA.");
            }
            ItemsContainer? storageInSlot = mv.ModGetStorageInSlot(slotID, TechType.VehicleStorageModule);
			storageInSlot ??= gameObject.GetComponent<SeamothStorageContainer>().container;

			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				if (!pda.Open(PDATab.Inventory, transform, new PDA.OnClose(this.OnClosePDA)))
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
		public static List<ItemsContainer> GetAllModularStorageContainers(ModVehicle mv)
		{
			List<ItemsContainer> result = new();
			if (mv == null)
			{
				return result;
			}
			var containerList = mv.ModulesRootObject.GetComponentsInChildren<SeamothStorageContainer>(true);
			if (containerList.Length == 0)
			{
				return result;
			}
			return containerList.Select(x => x.container).ToList();
		}
	}
}
