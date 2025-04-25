using ArtOfCooking.BlockEntities;
using ArtOfCooking.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.Items
{
    internal class AOCItemFlour: Item
    {
        static ItemStack[] tableStacks;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (tableStacks == null)
            {
                List<ItemStack> foundStacks = new List<ItemStack>();
                api.World.Collectibles.ForEach(obj =>
                {
                    if (obj is Block block && block.Attributes?.IsTrue("pieFormingSurface") == true)
                    {
                        foundStacks.Add(new ItemStack(obj));
                    }
                });

                tableStacks = foundStacks.ToArray();
            }
        }
        public override void OnUnloaded(ICoreAPI api)
        {
            tableStacks = null;
        }        
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;

            var block = api.World.BlockAccessor.GetBlock(blockSel.Position);
            bool sneaking = byEntity.Controls.ShiftKey;
            if (block.Attributes?.IsTrue("pieFormingSurface") == true && blockSel.Face == BlockFacing.UP && !sneaking)
            {
                IPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);
                BlockPos placePos = blockSel.Position.AddCopy(blockSel.Face);

                if (!byEntity.World.Claims.TryAccess(player, placePos, EnumBlockAccessFlags.BuildOrBreak))
                {
                    slot.MarkDirty();
                    return;
                }

                IWorldAccessor world = byEntity.World;
                AOCBlockDoughKnead doughknead = world.GetBlock(new AssetLocation("artofcooking:doughknead-flour-" + Variant["type"])) as AOCBlockDoughKnead;
                if (doughknead == null) return;

                BlockPos belowPos = blockSel.Position.AddCopy(blockSel.Face).Down();
                Block belowBlock = world.BlockAccessor.GetBlock(belowPos);

                if (!belowBlock.CanAttachBlockAt(byEntity.World.BlockAccessor, doughknead, belowPos, BlockFacing.UP)) return;


                if (!world.BlockAccessor.GetBlock(placePos).IsReplacableBy(doughknead)) return;

                world.BlockAccessor.SetBlock(doughknead.BlockId, placePos);

                if (doughknead.Sounds != null) world.PlaySoundAt(doughknead.Sounds.Place, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z);
                doughknead.OnCreate (slot, 1);
                handling = EnumHandHandling.PreventDefaultAction;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "artofcooking:heldhelp-placetodoughknead",
                    Itemstacks = tableStacks,
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}
