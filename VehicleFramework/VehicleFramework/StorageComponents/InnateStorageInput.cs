using VehicleFramework.Admin;

namespace VehicleFramework
{
    public class InnateStorageInput : StorageInput
    {
		public override void OpenFromExternal()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, EnumHelper.GetInnateStorageType());
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				pda.Open(PDATab.Inventory, null, null);
			}
		}
		protected override void OpenPDA()
		{
			ItemsContainer storageInSlot = mv.ModGetStorageInSlot(slotID, EnumHelper.GetInnateStorageType());
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
	}
}
