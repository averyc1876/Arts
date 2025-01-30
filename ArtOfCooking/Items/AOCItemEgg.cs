using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.Items
{
    public class AOCItemEgg : Item
    {
        WorldInteraction[] interactions;
        public string Name => Code.FirstCodePart();

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "eggCrackInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();
                
                foreach (Item items in api.World.Items)
                {
                    if (items.Code == null) continue;

                    if (items.Code.FirstCodePart() == "bowl")
                    {
                        stacks.Add(new ItemStack(items));
                    }
                }
                
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "artofgrowing:heldhelp-interact",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {     
            if (byEntity.Controls.ShiftKey)
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
            if (!byEntity.Controls.ShiftKey) return false;
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

                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    var beg = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;
                    ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);
                    BlockLiquidContainerTopOpened blockCnt = squeezeIntoSlot.Itemstack.Block as BlockLiquidContainerTopOpened;
                    Block nextblock = squeezeIntoSlot.Itemstack.Block.Clone();
                    nextblock = api.World.GetBlock(new AssetLocation("artofcooking:metalbowl-" + blockCnt.Variant["metal"] + "-egg-1"));
                    int slotId = beg.Inventory.GetSlotId(squeezeIntoSlot);
                                squeezeIntoSlot.Itemstack = new ItemStack(nextblock);
                                squeezeIntoSlot.MarkDirty();     
                                beg.updateMeshes();
                                beg.MarkDirty(true);      
                        slot.TakeOut(1);
                        slot.MarkDirty();
                        ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-" + Variant["source"])),1);
                        if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                        {
                            byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
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
