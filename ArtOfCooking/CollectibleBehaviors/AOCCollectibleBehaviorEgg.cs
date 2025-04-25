using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfCooking.CollectibleBehaviors
{
    public class AOCCollectibleBehaviorEgg : CollectibleBehavior
    {        
        public AOCCollectibleBehaviorEgg(CollectibleObject collObj) : base(collObj)
        {
            this.collObj = collObj;
        }

        WorldInteraction[] interactions;
        WorldInteraction[] interactionsyolk;
        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactionsyolk = ObjectCacheUtil.GetOrCreate(api, "eggyolkInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();
                
                foreach (Block bowl in api.World.Blocks)
                {
                    if (bowl.Code == null) continue;

                    if (bowl is BlockLiquidContainerTopOpened && bowl.Code.FirstCodePart() == "bowl")
                    {
                        stacks.Add(new ItemStack(bowl));
                    }

                    if (bowl is BlockLiquidContainerTopOpened && bowl.Code.FirstCodePart() == "metalbowl")
                    {
                        stacks.Add(new ItemStack(bowl));
                    }
                }     
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "artofcooking:heldhelp-eggyolkinteract",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
            interactions = ObjectCacheUtil.GetOrCreate(api, "eggInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();
                
                foreach (Block bowl in api.World.Blocks)
                {
                    if (bowl.Code == null) continue;

                    if (bowl is BlockLiquidContainerTopOpened && bowl.Code.FirstCodePart() == "bowl")
                    {
                        stacks.Add(new ItemStack(bowl));
                    }

                    if (bowl is BlockLiquidContainerTopOpened && bowl.Code.FirstCodePart() == "metalbowl")
                    {
                        stacks.Add(new ItemStack(bowl));
                    }
                }      
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "artofcooking:heldhelp-egginteract",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "artofcooking:heldhelp-eggwhiteinteract",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "ctrl",
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {     
            BlockLiquidContainerTopOpened bowl = byEntity.LeftHandItemSlot?.Itemstack?.Collectible as BlockLiquidContainerTopOpened;
            if (bowl != null && !bowl.IsFull(byEntity.LeftHandItemSlot.Itemstack) && byEntity.LeftHandItemSlot.Itemstack.StackSize == 1)
            {
                if (slot.Itemstack.Collectible.FirstCodePart() == "eggyolk") 
                {
                    IWorldAccessor world = byEntity.World;
                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                    ItemStack eggStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggportion-raw-yolk")), 99999);
                    ItemStack eggshellStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-chicken")),1);
                    float portion = 0.05f;
                    
                    if (eggStack != null) 
                    {
                        if (bowl.TryPutLiquid(byEntity.LeftHandItemSlot.Itemstack, eggStack, portion) == 0) return;
                        byEntity.LeftHandItemSlot.MarkDirty();
                        slot.TakeOut(1);
                        slot.MarkDirty();
                        if (eggshellStack != null && byPlayer?.InventoryManager.TryGiveItemstack(eggshellStack) == false)
                        {
                            world.SpawnItemEntity(eggshellStack, byEntity.SidedPos.XYZ);
                        }
                        if (byEntity.World.Side == EnumAppSide.Client)
                        {
                            world.PlaySoundAt(new AssetLocation("sounds/effect/squish2"), byEntity, null, true, 16, 0.5f);
                        }
                    }
                }
                else if (byEntity.World.Side == EnumAppSide.Client)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/woodcreak_4"), byEntity, null, true, 16, 3f);
                }                
                handHandling = EnumHandHandling.PreventDefault;
            }
            else
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            BlockLiquidContainerTopOpened bowl = byEntity.LeftHandItemSlot?.Itemstack?.Collectible as BlockLiquidContainerTopOpened;
            if (bowl == null || bowl.IsFull(byEntity.LeftHandItemSlot.Itemstack) 
                || byEntity.LeftHandItemSlot.Itemstack.StackSize != 1 
                || slot.Itemstack.Collectible.FirstCodePart() == "eggyolk") return false;
            if (byEntity.World is IClientWorldAccessor)
            {
                byEntity.StartAnimation("squeezehoneycomb");
            }
            handling = EnumHandling.PreventSubsequent;
            return secondsUsed < 2f;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            byEntity.StopAnimation("squeezehoneycomb");
            

            if (secondsUsed < 1.9f) return;
            
            IWorldAccessor world = byEntity.World;
            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            BlockLiquidContainerTopOpened bowl = byEntity.LeftHandItemSlot?.Itemstack?.Collectible as BlockLiquidContainerTopOpened;
            ItemStack eggStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggportion-raw-whole")), 99999);            
            ItemStack eggshellStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-chicken")),2);
            ItemStack yolkStack = null;
            float portion = 0.25f;
            if (byEntity.Controls.CtrlKey && slot.Itemstack.Collectible.FirstCodePart() == "egg")
            {
                eggStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggportion-raw-white")), 99999);
                eggshellStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggshell-chicken")),1);
                yolkStack = new ItemStack(world.GetItem(new AssetLocation("artofcooking:eggyolk-chicken")),1);
                portion = 0.20f;
            }
            if (bowl != null && !bowl.IsFull(byEntity.LeftHandItemSlot.Itemstack) 
                && byEntity.LeftHandItemSlot.Itemstack.StackSize == 1 && eggStack != null) 
            {
                if (bowl.TryPutLiquid(byEntity.LeftHandItemSlot.Itemstack, eggStack, portion) == 0) return;
                byEntity.LeftHandItemSlot.MarkDirty();
                slot.TakeOut(1);
                slot.MarkDirty();
                if (eggshellStack != null && byPlayer?.InventoryManager.TryGiveItemstack(eggshellStack) == false)
                {
                    world.SpawnItemEntity(eggshellStack, byEntity.SidedPos.XYZ);
                }
                if (yolkStack != null && byPlayer?.InventoryManager.TryGiveItemstack(yolkStack) == false)
                {
                    world.SpawnItemEntity(yolkStack, byEntity.SidedPos.XYZ);
                }
                handling = EnumHandling.PreventDefault;
                if (world.Side == EnumAppSide.Client)
                {
                    world.PlaySoundAt(new AssetLocation("sounds/effect/squish2"), byEntity, null, true, 16, 0.5f);
                }
                return;
            }
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled)
        {
            byEntity.StopAnimation("squeezehoneycomb");
            return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason, ref handled);
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            if (inSlot.Itemstack.Collectible.FirstCodePart() == "eggyolk")
                return interactionsyolk.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
            return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
        }
    }
}
