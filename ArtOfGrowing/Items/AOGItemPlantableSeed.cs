using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;

namespace ArtOfGrowing.Items
{
    public class AOGItemPlantableSeed : ItemPlantableSeed
    {
        ICoreClientAPI capi;
        WorldInteraction[] interactions;
        public string Type => Variant["type"];
        Dictionary<int, MultiTextureMeshRef> meshrefs => ObjectCacheUtil.GetOrCreate(api, "foodmeshrefs", () => new Dictionary<int, MultiTextureMeshRef>());
        

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            capi = api as ICoreClientAPI; 
            
            AddAllTypesToCreativeInventory();   
            
        }  
        public void AddAllTypesToCreativeInventory()
        {
            List<JsonItemStack> sizestacks = new List<JsonItemStack>();
            
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "wild")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "small")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "medium")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "decent")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "large")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "hefty")));
            sizestacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "gigantic")));

            this.CreativeInventoryStacks = new CreativeTabAndStackList[]
            {
                new CreativeTabAndStackList() { Stacks = sizestacks.ToArray(), Tabs = new string[]{ "general", "items", "artofgrowing" } }
            };
        }

        private JsonItemStack genJstack(string json)
        {
            var jstack = new JsonItemStack()
            {
                Code = this.Code,
                Type = EnumItemClass.Item,
                Attributes = new JsonObject(JToken.Parse(json))
            };

            jstack.Resolve(api.World, "seed size");

            return jstack;
        }  
        public override TransitionState[] UpdateAndGetTransitionStates(IWorldAccessor world, ItemSlot inSlot)
        {
            if (!inSlot.Itemstack.Attributes.HasAttribute("size"))
            {
                inSlot.Itemstack.Attributes.SetString("size", "wild");
            }   

            return base.UpdateAndGetTransitionStates(world, inSlot);
        }       
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            int meshrefid = itemstack.TempAttributes.GetInt("meshRefId");
            if (meshrefid == 0 || !meshrefs.TryGetValue(meshrefid, out renderinfo.ModelRef))
            {
                int id = meshrefs.Count+1;
                var modelref = capi.Render.UploadMultiTextureMesh(GenMesh(itemstack, capi.ItemTextureAtlas));
                renderinfo.ModelRef = meshrefs[id] = modelref;

                itemstack.TempAttributes.SetInt("meshRefId", id);
            }

            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }
        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas)
        {
            MeshData mesh = null;   
            var shape = capi.TesselatorManager.GetCachedShape(this.Shape.Base);

            var size = itemstack.Attributes.GetString("size");
            var loc = new AssetLocationAndSource("artofgrowing:item/seedbag/" + size + ".json");   
            if (Type == "flax" || Type == "spelt" || Type == "rice" || Type == "rye" || Type == "sunflower" || Type == "amaranth" || Type == "pumpkin" ) loc = new AssetLocationAndSource("artofgrowing:item/food/pickledvegetable/" + size + "/cabbage.json");  
            var asset = capi.Assets.TryGet(loc.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/"));
            if (asset != null) shape = asset.ToObject<Shape>();
            capi.Tesselator.TesselateShape(this, shape, out mesh, new Vec3f(this.Shape.rotateX, this.Shape.rotateY, this.Shape.rotateZ));
            return mesh;
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null) return;

            BlockPos pos = blockSel.Position;

            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
            string size = itemslot.Itemstack.Attributes.GetString("size");

            if (lastCodePart == "bellpepper") return;

            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
            if (be is BlockEntityFarmland)
            {
                Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + size + "-" + lastCodePart + "-1"));
                if (lastCodePart == "pumpkin") cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-" + lastCodePart + "-" + size + "-1"));
                if (cropBlock == null) return;

                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                bool planted = ((BlockEntityFarmland)be).TryPlant(cropBlock);
                if (planted)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), pos, 0.4375, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }
                }

                if (planted) handHandling = EnumHandHandling.PreventDefault;
            }
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {            
            string size = inSlot.Itemstack.Attributes.GetString("size");
            Block cropBlock = world.GetBlock(CodeWithPath("crop-" + size + "-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
            if (inSlot.Itemstack.Collectible.LastCodePart() == "pumpkin") cropBlock = world.GetBlock(CodeWithPath("crop-" + inSlot.Itemstack.Collectible.LastCodePart() + "-" + size + "-1"));
            if (cropBlock == null || cropBlock.CropProps == null) return;
            
            if (inSlot.Itemstack.Attributes != null)
            {
                dsc.AppendLine(Lang.Get("artofgrowing:size-food: {0}", Lang.Get("artofgrowing:food-" + size)));
            }

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
        public override FoodNutritionProperties GetNutritionProperties(IWorldAccessor world, ItemStack itemstack, Entity forEntity)
        {            
            string size = itemstack.Attributes.GetString("size");
            if (NutritionProps == null) return NutritionProps;
            FoodNutritionProperties props = NutritionProps.Clone();
            
            CollectibleObject obj = itemstack.Collectible;
            float koef = 1;
            switch (size)
                {
                    case "wild":
                        koef = 0.2f;
                        break;
                    case "small":
                        koef = 0.4f;
                        break;
                    case "medium":
                        koef = 0.6f;
                        break;
                    case "decent":
                        koef = 0.8f;
                        break;
                    case "large":
                        koef = 1;
                        break;
                    case "hefty":
                        koef = 1.5f;
                        break;
                    case "gigantic":
                        koef = 2;
                        break;
                }
            props.Satiety = base.NutritionProps.Satiety * koef;
            return props;
        }
    }
}
