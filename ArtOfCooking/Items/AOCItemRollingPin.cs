using ArtOfCooking.BlockEntities;
using ArtOfCooking.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.Items
{
    public class AOCItemRollingPin : Item
    {
        public bool CanRolling(Block block, BlockSelection blockSel)
        {
            var pos = blockSel?.Position;

            if (pos != null)
            {
                var beg = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityGroundStorage;
                if (beg != null)
                {
                    ItemSlot rollingSlot = beg.GetSlotAt(blockSel);
                    return rollingSlot?.Itemstack?.Attributes["extraNutritionProps"] != null;
                }
            }

            return false;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
            if (block != null && !byEntity.Controls.ShiftKey)
            {

                if (byEntity.World.Side == EnumAppSide.Client)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/woodcreak_4"), byEntity, null, true, 16, 3f);
                }
                handling = EnumHandHandling.PreventDefault;
            }
            else
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel?.Block != null && CanRolling(blockSel.Block, blockSel))
            {
                if (!byEntity.Controls.ShiftKey || slot.Itemstack.Collectible.FirstCodePart() == "eggyolk")
                    return false;
                if (byEntity.World is IClientWorldAccessor)
                {
                    byEntity.StartAnimation("squeezehoneycomb");
                }

                return secondsUsed < 1f;
            }

            return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity,
            BlockSelection blockSel, EntitySelection entitySel)
        {
            byEntity.StopAnimation("squeezehoneycomb");

            if (blockSel != null)
            {
                Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
                if (CanRolling(block, blockSel))
                {
                    if (secondsUsed < 0.9f) return;

                    IWorldAccessor world = byEntity.World;

                    if (!CanRolling(block, blockSel)) return;

                    string source = Variant["source"];
                    ItemStack eggStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggportion-raw-whole")), 99999);
                    ItemStack eggshellStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-" + source)), 2);
                    ItemStack yolkStack = null;
                    float portion = 1;
                    if (byEntity.Controls.CtrlKey && slot.Itemstack.Collectible.FirstCodePart() == "egg")
                    {
                        eggStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggportion-raw-white")), 99999);
                        eggshellStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-" + source)), 1);
                        yolkStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggyolk-" + source)), 1);
                        portion = 1 / 4 * 3;
                    }

                    ILiquidSink blockCnt = block as ILiquidSink;
                    if (blockCnt != null)
                    {
                        if (blockCnt.TryPutLiquid(blockSel.Position, eggStack, portion) == 0) return;
                    }
                    else
                    {
                        var beg = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;
                        if (beg != null)
                        {
                            ItemSlot crackIntoSlot = beg.GetSlotAt(blockSel);

                            if (crackIntoSlot != null && crackIntoSlot?.Itemstack?.Block != null && CanRolling(crackIntoSlot.Itemstack.Block, null))
                            {
                                blockCnt = crackIntoSlot.Itemstack.Block as ILiquidSink;
                                blockCnt.TryPutLiquid(crackIntoSlot.Itemstack, eggStack, portion);
                                beg.MarkDirty(true);
                            }
                        }
                    }

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    if (byPlayer?.InventoryManager.TryGiveItemstack(eggshellStack) == false)
                    {
                        byEntity.World.SpawnItemEntity(eggshellStack, byEntity.SidedPos.XYZ);
                    }
                    if (yolkStack != null && byPlayer?.InventoryManager.TryGiveItemstack(yolkStack) == false)
                    {
                        byEntity.World.SpawnItemEntity(yolkStack, byEntity.SidedPos.XYZ);
                    }

                    if (world.Side == EnumAppSide.Client)
                    {
                        world.PlaySoundAt(new AssetLocation("sounds/effect/squish2"), byEntity, null, true, 16, 0.5f);
                    }

                    return;
                }
            }
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            byEntity.StopAnimation("squeezehoneycomb");
            return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
        }
    }
}
