using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace ArtOfGrowing.Items
{
    public class AOGItemInteract : Item
    {
        WorldInteraction[] interactions;
        public string Name => Code.FirstCodePart();

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "flaxClearInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "artofgrowing:heldhelp-interact",
                        MouseButton = EnumMouseButton.Right
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {     
            if (!byEntity.Controls.ShiftKey)
            {
                handling = EnumHandHandling.PreventDefault;
                if (api.World.Side == EnumAppSide.Client)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), byEntity, null, true, 16, 0.5f);
                }
            }
            else
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent,ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.Controls.ShiftKey) return false;
            if (byEntity.World is IClientWorldAccessor)
            {
                byEntity.StartAnimation("squeezehoneycomb");
            }
            return secondsUsed < 2f;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            byEntity.StopAnimation("squeezehoneycomb");

                if (secondsUsed < 1.9f) return;

                    IWorldAccessor world = byEntity.World;

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    if (Name == "flaxbundle") 
                    {
                        ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("artofgrowing:flaxbundle-soft"))); 
                        if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                        {
                            byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
                        } 
                    }   
                    if (Name == "grainbundle") 
                    {
                        ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("game:seeds-sunflower"))); 
                        string size = Variant["size"];
                        stack.Attributes.SetString("size", size);
                        stack.StackSize = 4;
                        if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                        {
                            byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
                        } 
                    }                  

                    return;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            byEntity.StopAnimation("squeezehoneycomb");
            return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}
