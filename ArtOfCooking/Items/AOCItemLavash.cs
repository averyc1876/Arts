using ArtOfCooking.Blocks;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.Items
{
    public class AOCItemLavash : Item
    {
        static ItemStack[] tableStacks;
        public string State => Variant["state"];

        public override void OnLoaded(ICoreAPI api)
        {
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
            if (blockSel != null)
            {
                var block = api.World.BlockAccessor.GetBlock(blockSel.Position);
                if (block.Attributes?.IsTrue("pieFormingSurface") == true && blockSel.Face == BlockFacing.UP && State != "raw")
                {
                    AOCBlockShawarma blockform = api.World.GetBlock(new AssetLocation("artofcooking:shawarma-" + State)) as AOCBlockShawarma;
                    blockform.TryPlaceShawarma(byEntity, blockSel);

                    handling = EnumHandHandling.PreventDefault;
                    return;
                }
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "artofgrowing:heldhelp-makeshawarma",
                    Itemstacks = tableStacks,
                    MouseButton = EnumMouseButton.Right,
                    GetMatchingStacks = (wi, bs, es) =>
                    {
                        if (State != "raw")
                        {
                            return wi.Itemstacks;
                        }
                        return null;
                    }
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}
