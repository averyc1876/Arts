using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.Items
{
    public class AOCItemEgg : Item
    {
        public bool CanSqueezeInto(Block block, BlockSelection blockSel)
        {
            var pos = blockSel?.Position;

            if (block is BlockLiquidContainerTopOpened blcto && block.Code.FirstCodePart() == "bowl")
            {
                return pos == null || !blcto.IsFull(pos);
            }
            
            if (block is BlockLiquidContainerTopOpened mblcto && block.Code.FirstCodePart() == "metalbowl")
            {
                return pos == null || !mblcto.IsFull(pos);
            }

            if (pos != null)
            {
                var beg = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityGroundStorage;
                if (beg != null)
                {
                    ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);

                    if (squeezeIntoSlot?.Itemstack?.Block is BlockLiquidContainerTopOpened bowl && bowl.Code.FirstCodePart() == "bowl")
                    {
                        if (bowl.GetCurrentLitres(squeezeIntoSlot.Itemstack) == 0f) return true;
                    }
                    if (squeezeIntoSlot?.Itemstack?.Block is BlockLiquidContainerTopOpened mbowl && mbowl.Code.FirstCodePart() == "metalbowl")
                    {
                        if (mbowl.GetCurrentLitres(squeezeIntoSlot.Itemstack) == 0f) return true;
                    }
                    Block eggBowl = squeezeIntoSlot?.Itemstack?.Block;
                    if (eggBowl != null && eggBowl.Code.FirstCodePart() == "bowl" && eggBowl.Variant["type"] == "egg" && eggBowl.Code.EndVariant() != "4") return true;
                    if (eggBowl != null && eggBowl.Code.FirstCodePart() == "metalbowl" && eggBowl.Variant["type"] == "egg" && eggBowl.Code.EndVariant() != "8") return true;
                }
            }

            if (block != null && block.Code.FirstCodePart() == "bowl" && block.Variant["type"] == "egg" && block.Code.EndVariant() != "4") return true;
            if (block != null && block.Code.FirstCodePart() == "metalbowl" && block.Variant["type"] == "egg" && block.Code.EndVariant() != "8") return true;

            return false;
        }

        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "eggInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null) continue;

                    if (CanSqueezeInto(block, null))
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-eggcrack",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }



        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel) && byEntity.Controls.ShiftKey)
            {
                handling = EnumHandHandling.PreventDefault;
                if (api.World.Side == EnumAppSide.Client)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/player/squeezehoneycomb"), byEntity, null, true, 16, 0.5f);
                }
            }
            else
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent,ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel))
            {
                if (!byEntity.Controls.ShiftKey) return false;
                if (byEntity.World is IClientWorldAccessor)
                {
                    byEntity.StartAnimation("squeezehoneycomb");
                }

                return secondsUsed < 2f;
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
                if (CanSqueezeInto(block, blockSel))
                {
                    if (secondsUsed < 1.9f) return;

                    IWorldAccessor world = byEntity.World;

                    if (!CanSqueezeInto(block, blockSel)) return;

                    BlockLiquidContainerTopOpened blockCnt = block as BlockLiquidContainerTopOpened;
                    if (blockCnt != null)
                    {
                        if (blockCnt.Code.FirstCodePart() == "bowl")
                        {
                            Block blockform = api.World.GetBlock(new AssetLocation("artofcooking:bowl-egg-1"));
                            api.World.BlockAccessor.SetBlock(blockform.BlockId, blockSel.Position);
                        }
                        if (blockCnt.Code.FirstCodePart() == "metalbowl")
                        {
                            Block blockform = api.World.GetBlock(new AssetLocation("artofcooking:metalbowl-" + blockCnt.Variant["metal"] + "-egg-1"));
                            api.World.BlockAccessor.SetBlock(blockform.BlockId, blockSel.Position);
                        }
                    }
                    else
                    {
                        var beg = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;
                        if (beg != null)
                        {
                            ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);

                            if (squeezeIntoSlot != null && squeezeIntoSlot?.Itemstack?.Block != null && CanSqueezeInto(squeezeIntoSlot.Itemstack.Block, null))
                            {
                                blockCnt = squeezeIntoSlot.Itemstack.Block as BlockLiquidContainerTopOpened;
                                ItemStack blockform = squeezeIntoSlot.Itemstack;
                                int slotId = beg.Inventory.GetSlotId(squeezeIntoSlot);
                                if (blockCnt != null)
                                {
                                    if (blockCnt.Code.FirstCodePart() == "bowl")
                                    {
                                        blockform = new ItemStack (api.World.GetBlock(new AssetLocation("artofcooking:bowl-egg-1")),1);
                                    }
                                    if (blockCnt.Code.FirstCodePart() == "metalbowl")
                                    {
                                        blockform = new ItemStack (api.World.GetBlock(new AssetLocation("artofcooking:metalbowl-" + blockCnt.Variant["metal"] + "-egg-1")),1);
                                    }
                                }
                                else
                                {
                                    int quantity = 0;
                                    int.TryParse(blockform.Block.LastCodePart(), out quantity);
                                    string quantityS = (quantity + 1).ToString();                                    
                                    if (blockform.Block.Code.FirstCodePart() == "bowl") blockform = new ItemStack (api.World.GetBlock(new AssetLocation("artofcooking:bowl-egg-" + quantityS)),1);
                                    if (blockform.Block.Code.FirstCodePart() == "metalbowl") blockform = new ItemStack (api.World.GetBlock(new AssetLocation("artofcooking:metalbowl-" + blockform.Block.Variant["metal"] + "-egg-" + quantityS)),1);                                    
                                }
                                beg.Inventory[slotId].TakeOutWhole();
                                beg.Inventory[slotId].MarkDirty();
                                new DummySlot(blockform).TryPutInto(api.World, beg.Inventory[slotId], blockform.StackSize);
                                                             
                            }
                        }
                        beg.MarkDirty(true);    
                    }                               

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-" + Variant["source"])));
                    if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                    {
                        byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
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


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}
