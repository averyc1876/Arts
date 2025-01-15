using ArtOfCooking.BlockEntities;
using ArtOfCooking.Items;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.Client.NoObf;
using System.Collections;
using Vintagestory.API.Common.Entities;
using Cairo;

namespace ArtOfCooking.Blocks
{
    public class AOCBlockEmptySpoon : Block
    {        
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel?.Position == null)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);            
            if (block is BlockGroundStorage)
            {
                var begs = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;
                ItemSlot gsslot = begs.GetSlotAt(blockSel);
                if (gsslot == null || gsslot.Empty) return;
                var bowlcont = (gsslot.Itemstack.Block as IBlockMealContainer);

                if (bowlcont != null)
                {
                    float quantityServings = (float)gsslot.Itemstack.Attributes.GetDecimal("quantityServings");
                    if (quantityServings > 0)
                    {
                        ServeIntoStack(slot, gsslot, byEntity.World);
                        slot.MarkDirty();
                        begs.updateMeshes();
                        begs.MarkDirty(true);
                    }

                    handHandling = EnumHandHandling.PreventDefault;
                    return;
                }
            }
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
        }    
        
        public void ServeIntoStack(ItemSlot spoonSlot, ItemSlot bowlSlot, IWorldAccessor world)
        {
            if (world.Side == EnumAppSide.Client) return;
            
            var bowlcont = (bowlSlot.Itemstack.Block as IBlockMealContainer);
            float quantityServings = bowlcont.GetQuantityServings(world, bowlSlot.Itemstack);
            string ownRecipeCode = bowlcont.GetRecipeCode(world, bowlSlot.Itemstack);
            float servingCapacity = spoonSlot.Itemstack.Block.Attributes["servingCapacity"].AsFloat(1);

            
            ItemStack[] stacks = bowlcont.GetContents(api.World, bowlSlot.Itemstack);
            string code = spoonSlot.Itemstack.Block.Attributes["mealBlockCode"].AsString();
            if (code == null) return;
            Block mealblock = api.World.GetBlock(new AssetLocation(code));

            float servingsToTransfer = Math.Min(quantityServings, servingCapacity);

            ItemStack stack = new ItemStack(mealblock);
            (mealblock as IBlockMealContainer).SetContents(ownRecipeCode, stack, stacks, servingsToTransfer);

            bowlcont.SetQuantityServings(world, bowlSlot.Itemstack, quantityServings - servingsToTransfer);

            bowlSlot.MarkDirty();

            spoonSlot.Itemstack = stack;
            spoonSlot.MarkDirty();
            return;
        }   
    }
}
