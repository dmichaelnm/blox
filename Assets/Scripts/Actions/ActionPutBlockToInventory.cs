using Blox.UI;

namespace Blox.Actions
{
    public class ActionPutBlockToInventory : Action<int, bool>
    {
        public ActionPutBlockToInventory(int blockTypeId) : base(blockTypeId)
        {
        }

        protected override bool Execute(int blockTypeId)
        {
            var inventory = Inventory.GetInstance();
            return inventory.Put(blockTypeId);
        }
    }
}