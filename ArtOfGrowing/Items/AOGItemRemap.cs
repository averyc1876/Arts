using Vintagestory.API.Common;

namespace ArtOfGrowing.Items
{
    public class AOGItemRemap : Item
    {
        public string Type => Variant["type"];
        public string Name => Code.FirstCodePart();
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }      
        public override TransitionState[] UpdateAndGetTransitionStates(IWorldAccessor world, ItemSlot inSlot)
        {
            if (inSlot != null && !inSlot.Itemstack.Attributes.HasAttribute("size"))
            {
                inSlot.Itemstack.Attributes.SetString("size", "wild");
            }     
            string size = inSlot.Itemstack.Attributes.GetAsString("size");
            Item item = world.GetItem(new AssetLocation("artofgrowing:" + Name + "-" + size + "-" + Type));
            if (item != null)
            {
                ItemStack stack = new ItemStack(item,inSlot.Itemstack.StackSize); 
                inSlot.Itemstack = stack;
            }
            return base.UpdateAndGetTransitionStates(world, inSlot);
        }     

    }
}
