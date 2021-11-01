using Blox.UI;

namespace Blox.Actions
{
    public class ActionRemoveBlockFromInventory : Action<int, bool>
    {
        public ActionRemoveBlockFromInventory(int input) : base(input)
        {
        }

        protected override bool Execute(int blockTypeId)
        {
            var inventory = Inventory.GetInstance();
            return inventory.Remove(blockTypeId);
        }
    }
}