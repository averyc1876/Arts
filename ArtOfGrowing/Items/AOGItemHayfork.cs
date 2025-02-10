using ArtOfGrowing.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ArtOfGrowing.Items
{
    public class AOGItemHayfork: ItemShears
    {
        public override int MultiBreakQuantity { get { return 5; } }
        public override bool CanMultiBreak(Block block)
        {
            return block is AOGBlockHayLayer;
        }

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
        {
            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);
            if (blockSel == null) return;

            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }

            byEntity.Attributes.SetBool("didBreakBlocks", false);
            byEntity.Attributes.SetBool("didPlayHayforkSound", false);
            handling = EnumHandHandling.PreventDefault;
        }

        public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel)
        {
            performActions(secondsPassed, byEntity, slot, blockSelection);
            if (api.Side == EnumAppSide.Server) return true;

            return secondsPassed < 2f;
        }

        public override void OnHeldAttackStop(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel)
        {
            performActions(secondsPassed, byEntity, slot, blockSelection);
        }

        private void performActions(float secondsPassed, EntityAgent byEntity, ItemSlot slot, BlockSelection blockSelection)
        {
            if (blockSelection == null) return;

            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;

            var canmultibreak = CanMultiBreak(api.World.BlockAccessor.GetBlock(blockSelection.Position));

            if (canmultibreak && secondsPassed > 0.75f && byEntity.Attributes.GetBool("didPlayHayforkSound") == false)
            {
                api.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), byEntity, byPlayer, true, 16);
                byEntity.Attributes.SetBool("didPlayHayforkSound", true);
            }

            if (canmultibreak && secondsPassed > 1.05f && byEntity.Attributes.GetBool("didBreakBlocks") == false)
            {
                if (byEntity.World.Side == EnumAppSide.Server && byEntity.World.Claims.TryAccess(byPlayer, blockSelection.Position, EnumBlockAccessFlags.BuildOrBreak))
                {
                    OnBlockBrokenWith(byEntity.World, byEntity, slot, blockSelection);
                }

                byEntity.Attributes.SetBool("didBreakBlocks", true);
            }
        }
        protected override void breakMultiBlock(BlockPos pos, IPlayer plr)
        {
            var block = api.World.BlockAccessor.GetBlock(pos);            
            var alldrops = block.GetDrops(api.World, pos, plr);
            foreach (var drop in alldrops)
            {
                if (!plr.InventoryManager.TryGiveItemstack(drop, true))
                {
                    api.World.SpawnItemEntity(drop, pos.ToVec3d().AddCopy(0.5, 0.1, 0.5));
                }
            }
            if (block.Variant["overlay"] == "eaten") api.World.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("game:tallgrass-eaten-free")).Id, pos);
            else api.World.BlockAccessor.SetBlock(0, pos);
            api.World.BlockAccessor.MarkBlockDirty(pos);
        }
    }
}
