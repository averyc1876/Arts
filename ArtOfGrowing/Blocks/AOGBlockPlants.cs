using ArtOfGrowing.BlockEntites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Client.Tesselation;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using static Vintagestory.Server.Timer;

namespace ArtOfGrowing.Blocks
{
    internal class AOGBlockCrop: BlockCrop
    {
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {   
            if (byPlayer?.InventoryManager.ActiveTool == EnumTool.Scythe || byPlayer?.InventoryManager.ActiveTool == EnumTool.Knife)
            {
                base.OnBlockBroken(world, pos, byPlayer, 0);
                
                if (Variant["size"] != null && Variant["stage"] != null && Variant["stage"] == CropProps.GrowthStages.ToString())
                    world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:strawlayer-" + Variant["type"] + "-" + Variant["size"] + "-wet-free")).Id, pos);
                if (Variant["stage"] != null && Variant["stage"] == CropProps.GrowthStages.ToString())
                    world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:strawlayer-" + Variant["type"] + "-wild-wet-free")).Id, pos);
                else
                    world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:haylayer-free-veryshort-straw-free")).Id, pos);                               
            }
            else 
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
    }   
    internal class AOGBlockTallGrass : BlockPlant
    {
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            if (byPlayer?.InventoryManager.ActiveTool == EnumTool.Knife && Variant["tallgrass"] != null && Variant["tallgrass"] != "eaten")
            {
                world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:haylayer-eaten-veryshort-grass-free")).Id, pos);
            }
        
            if (byPlayer?.InventoryManager.ActiveTool == EnumTool.Scythe && Variant["tallgrass"] != null && Variant["tallgrass"] != "eaten")
            {
                bool trimMode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetInt("toolMode", 0) == 0;
                if (trimMode) world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:haylayer-eaten-" + Variant["tallgrass"] + "-grass-free")).Id, pos);
                else world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("artofgrowing:haylayer-free-" + Variant["tallgrass"] + "-grass-free")).Id, pos);
            }            
        }
    }    
    internal class AOGBlockHayLayer: Block, IDrawYAdjustable
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            if (Variant["overlay"] == "eaten") world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("game:tallgrass-eaten-free")).Id, pos);
            world.BlockAccessor.MarkBlockDirty(pos);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var alldrops = GetDrops(world, blockSel.Position, byPlayer);
            foreach (var drop in alldrops)
            {
                if (!byPlayer.InventoryManager.TryGiveItemstack(drop, true))
                {
                    world.SpawnItemEntity(drop, blockSel.Position.ToVec3d().AddCopy(0.5, 0.1, 0.5));
                }
            }
            if (Variant["overlay"] == "eaten") world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("game:tallgrass-eaten-free")).Id, blockSel.Position);
            else world.BlockAccessor.SetBlock(0, blockSel.Position);
            world.BlockAccessor.MarkBlockDirty(blockSel.Position);
            return true;
        }
        public float AdjustYPosition(BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
        {
            Block nblock = chunkExtBlocks[extIndex3d + TileSideEnum.MoveIndex[TileSideEnum.Down]];
            return nblock is BlockFarmland ? -0.0625f : 0f;
        }
    }     
    internal class AOGItemScythe: ItemScythe
    {      
        protected override void breakMultiBlock(BlockPos pos, IPlayer plr)
        {
            api.World.BlockAccessor.BreakBlock(pos, plr);
            api.World.BlockAccessor.MarkBlockDirty(pos);
        }
    }   
}
