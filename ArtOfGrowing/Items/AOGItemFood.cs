using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ArtOfGrowing.Items
{
    public class AOGItemFood : Item
    {
        ICoreClientAPI capi;
        WorldInteraction[] interactions;
        public string Type => Variant["type"];
        public string Name => Code.FirstCodePart();
        Dictionary<int, MultiTextureMeshRef> meshrefs => ObjectCacheUtil.GetOrCreate(api, "foodmeshrefs", () => new Dictionary<int, MultiTextureMeshRef>());
        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            capi = api as ICoreClientAPI;

            AddAllTypesToCreativeInventory();

            interactions = ObjectCacheUtil.GetOrCreate(api, "seedInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null || block.EntityClass == null) continue;

                    Type type = api.World.ClassRegistry.GetBlockEntity(block.EntityClass);
                    if (type == typeof(BlockEntityFarmland))
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-plant",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }
        public void AddAllTypesToCreativeInventory()
        {
            List<JsonItemStack> stacks = new List<JsonItemStack>();

            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "small")));
            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "medium")));
            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "decent")));
            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "large")));
            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "hefty")));
            stacks.Add(genJstack(string.Format("{{ size: \"{0}\" }}", "gigantic")));

            this.CreativeInventoryStacks = new CreativeTabAndStackList[]
            {
                new CreativeTabAndStackList() { Stacks = stacks.ToArray(), Tabs = new string[]{ "general", "items", "artofgrowing" } }
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

            jstack.Resolve(api.World, "food size");

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
            var loc = new AssetLocationAndSource("artofgrowing:item/food/" + Name + "/" + size + "/" + Type + ".json");            
            var asset = capi.Assets.TryGet(loc.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/"));            
            if (asset == null) 
                {
                loc = new AssetLocationAndSource("artofgrowing:item/food/clear/" + size + "/" + Type + ".json");            
                asset = capi.Assets.TryGet(loc.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/"));        
                }
            if (asset != null) shape = asset.ToObject<Shape>();
            capi.Tesselator.TesselateShape(this, shape, out mesh, new Vec3f(this.Shape.rotateX, this.Shape.rotateY, this.Shape.rotateZ));
            
            return mesh;
        }
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            EnumHandHandling bhHandHandling = EnumHandHandling.NotHandled;
            if (blockSel != null) 
            { 
                if (Name == "vegetable") 
                { 

                    BlockPos pos = blockSel.Position;

                    string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();
                    string size = itemslot.Itemstack.Attributes.GetString("size");

                    BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
                    if (be is BlockEntityFarmland)
                    {
                        Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crop-seed-" + size + "-" + lastCodePart + "-1"));
                        if (cropBlock != null)
                        { 

                            IPlayer byPlayer = null;
                            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                            bool planted = ((BlockEntityFarmland)be).TryPlant(cropBlock);
                            if (planted)
                            {
                                byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), pos.X, pos.Y, pos.Z, byPlayer);

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
                }
            }            
            WalkBehaviors(
                (CollectibleBehavior bh, ref EnumHandling hd) => bh.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref bhHandHandling, ref hd),
                () => tryEatBegin(itemslot, byEntity, ref bhHandHandling)
            );
            handHandling = bhHandHandling;
        }
        
        void WalkBehaviors(CollectibleBehaviorDelegate onBehavior, Action defaultAction)
        {
            bool executeDefault = true;
            foreach (CollectibleBehavior behavior in CollectibleBehaviors)
            {
                EnumHandling handling = EnumHandling.PassThrough;
                onBehavior(behavior, ref handling);

                if (handling == EnumHandling.PreventSubsequent) return;
                if (handling == EnumHandling.PreventDefault) executeDefault = false;
            }

            if (executeDefault) defaultAction();
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {                      
            string size = inSlot.Itemstack.Attributes.GetString("size");
            CollectibleObject obj = inSlot.Itemstack.Collectible;    
            
            dsc.AppendLine(Lang.Get("artofgrowing:size-food: {0}", Lang.Get("artofgrowing:food-" + size)));            

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            
            if (obj.Attributes?["nutritionPropsWhenInMeal"].Exists == true)
                {
                    EntityPlayer entityPlayer = ((world.Side == EnumAppSide.Client) ? (world as IClientWorldAccessor).Player.Entity : null);              
                    FoodNutritionProperties nutritionProperties = GetNutritionProperties(world, inSlot.Itemstack, entityPlayer);                
                    FoodNutritionProperties nutritionmeal = nutritionProperties.Clone();
                    nutritionmeal.Satiety = nutritionProperties.Satiety * 1.5f;
                    int satietymealint = ((int)nutritionmeal.Satiety);
                    string satietymeal = satietymealint.ToString();
                    dsc.AppendLine(Lang.Get("artofgrowing:nutrition-props-when-in-meal: {0} sat", Lang.Get(satietymeal)));
                }
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {            
            if (Name == "vegetable")
                if (Type == "carrot" || Type == "onion" || Type == "parsnip" || Type == "turnip" ) return interactions.Append(base.GetHeldInteractionHelp(inSlot));
            return base.GetHeldInteractionHelp(inSlot);
        }
        public override FoodNutritionProperties GetNutritionProperties(IWorldAccessor world, ItemStack itemstack, Entity forEntity)
        {            
            string size = itemstack.Attributes.GetString("size");
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
            if (obj.Attributes?["nutritionPropsWhenInMeal"].Exists == true)
            {
                FoodNutritionProperties propsmeal = props.Clone();
                propsmeal.Satiety = props.Satiety * 1.5f;
                int satietymealint = ((int)propsmeal.Satiety);
                string satietymeal = satietymealint.ToString();
                JToken token;
                token = obj.Attributes["nutritionPropsWhenInMeal"].Token;
                if (satietymeal != null) token["satiety"] = JToken.FromObject(satietymeal);
            }
            return props;
        }

    }
}
