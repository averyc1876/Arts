using ArtOfCooking.Blocks;
using ArtOfCooking.Systems;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ArtOfCooking.BlockEntities
{
    public enum EnumShawarmaPartType
    {
        Crust, Filling, Topping
    }
    public class inShawarmaProperties
    {
        public EnumShawarmaPartType PartType;
        public AssetLocation Texture;
    }

    // Idea:
    // AOCBEShawarma is a single slot inventory BE that hold a shawarma item stack
    // that shawarma item stack is a container with always 6 slots:
    // [0] = base dough
    // [1-4] = filling
    // [5] = crust dough
    // 
    // Eliminates the need to convert it to an itemstack once its placed in inventory
    public class AOCBEShawarma : BlockEntityContainer
    {
        InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "shawarma";


        public bool HasAnyFilling
        {
            get
            {
                var shawarmaBlock = inv[0].Itemstack.Block as AOCBlockShawarma;
                ItemStack[] cStacks = shawarmaBlock.GetContents(Api.World, inv[0].Itemstack);
                return cStacks[1] != null || cStacks[2] != null || cStacks[3] != null || cStacks[4] != null;
            }
        }

        public bool HasAllFilling
        {
            get
            {
                var shawarmaBlock = inv[0].Itemstack.Block as AOCBlockShawarma;
                ItemStack[] cStacks = shawarmaBlock.GetContents(Api.World, inv[0].Itemstack);
                return cStacks[1] != null && cStacks[2] != null && cStacks[3] != null && cStacks[4] != null;
            }
        }

        AOCMeshCache ms;
        MeshData mesh;
        ICoreClientAPI capi;

        public AOCBEShawarma() : base()
        {
            inv = new InventoryGeneric(1, null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            ms = api.ModLoader.GetModSystem<AOCMeshCache>();

            capi = api as ICoreClientAPI;

            loadMesh();
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);

            if (inv[0].Itemstack?.Collectible.Code.Path == "rot")
            {
                Api.World.BlockAccessor.SetBlock(0, Pos);
                Api.World.SpawnItemEntity(inv[0].Itemstack, Pos.ToVec3d().Add(0.5, 0.1, 0.5));
            }
            ItemStack slicestack = TakeSlice();
            inv[0].Itemstack = slicestack.Clone();
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            if (byItemStack != null)
            {
                inv[0].Itemstack = byItemStack.Clone();
                inv[0].Itemstack.StackSize = 1;
            }
        }

        public int SlicesLeft
        {
            get
            {
                if (inv[0].Empty) return 0;
                return inv[0].Itemstack.Attributes.GetAsInt("shawarmaParts");
            }
        }

        public ItemStack TakeSlice()
        {
            if (inv[0].Empty) return null;

            MarkDirty(true);

            ItemStack stack = inv[0].Itemstack.Clone();

            if (!stack.Attributes.HasAttribute("quantityServings"))
            {
                stack.Attributes.SetFloat("quantityServings", 1);
            }

            float servingsLeft = stack.Attributes.GetFloat("quantityServings");

            if (servingsLeft == 1)
            {
                stack.Attributes.SetInt("shawarmaParts", 0);
            }
            if (servingsLeft < 1)
            {
                stack.Attributes.SetInt("shawarmaParts", 1);
            }
            if (servingsLeft <= 0.85f)
            {
                stack.Attributes.SetInt("shawarmaParts", 2);
            }
            if (servingsLeft <= 0.7f)
            {
                stack.Attributes.SetInt("shawarmaParts", 3);
            }
            if (servingsLeft <= 0.5f)
            {
                stack.Attributes.SetInt("shawarmaParts", 4);
            }
            if (servingsLeft <= 0.35f)
            {
                stack.Attributes.SetInt("shawarmaParts", 5);
            }
            if (servingsLeft <= 0.15f)
            {
                stack.Attributes.SetInt("shawarmaParts", 6);
            }

            loadMesh();
            MarkDirty(true);

            return stack;
        }

        public void OnPlaced(IPlayer byPlayer)
        {
            ItemStack lavashStack = byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
            if (lavashStack == null) return;

            inv[0].Itemstack = new ItemStack(Block);
            (inv[0].Itemstack.Block as AOCBlockShawarma).SetContents(inv[0].Itemstack, new ItemStack[6] { lavashStack, null, null, null, null, null });
            inv[0].Itemstack.Attributes.SetInt("shawarmaParts", 0);
            inv[0].Itemstack.Attributes.SetBool("finished", false);

            loadMesh();
        }
        public void OnFormed(IPlayer byPlayer)
        {
            string lavashType = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Item.Variant["type"];
            ItemStack lavashStack = new ItemStack(Api.World.GetItem(new AssetLocation("artofcooking:lavash-" + lavashType + "-raw")), 1);
            if (lavashStack == null) return;

            inv[0].Itemstack = new ItemStack(Block);
            (inv[0].Itemstack.Block as AOCBlockShawarma).SetContents(inv[0].Itemstack, new ItemStack[6] { lavashStack, null, null, null, null, null });
            inv[0].Itemstack.Attributes.SetBool("finished", false);

            loadMesh();
        }

        public bool OnInteract(IPlayer byPlayer)
        {
            var shawarmaBlock = inv[0].Itemstack.Block as AOCBlockShawarma;

            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            ItemStack[] cStacks = shawarmaBlock.GetContents(Api.World, inv[0].Itemstack);
            string type = cStacks[0].Item.Variant["type"];
            string state = cStacks[0].Item.Variant["state"];

            bool hasFilling = cStacks[1] != null || cStacks[2] != null || cStacks[3] != null || cStacks[4] != null;
            bool ctrl = byPlayer.Entity.Controls.CtrlKey;

            if (inv[0].Itemstack.Attributes.GetAsBool("wrapped") == true)
            {
                return false;
            }
            if (state == "raw")
            {
                if (Api.Side == EnumAppSide.Server)
                {
                    if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[0]))
                    {
                        Api.World.SpawnItemEntity(cStacks[0], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                    }
                    inv[0].Itemstack = null;
                }

                Api.World.BlockAccessor.SetBlock(0, Pos);
                return true;
            }

            if (!hotbarSlot.Empty)
            {
                bool added = TryAddIngredientFrom(hotbarSlot, byPlayer);
                if (added)
                {
                    loadMesh();
                    MarkDirty(true);
                }

                inv[0].Itemstack.Attributes.SetBool("finished", HasAllFilling);

                return added;
            }
            else
            {
                if (inv[0].Itemstack.Attributes.GetAsBool("finished") == true && ctrl)
                {
                    inv[0].Itemstack.Attributes.SetBool("wrapped", true);
                    inv[0].Itemstack.Attributes.SetFloat("quantityServings", 1);
                    loadMesh();
                    MarkDirty(true);
                    return true;
                }
                if (Api.Side == EnumAppSide.Server)
                {
                    if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[0]))
                    {
                        Api.World.SpawnItemEntity(cStacks[0], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                    }
                    if (cStacks[1] != null) if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[1]))
                        {
                            Api.World.SpawnItemEntity(cStacks[1], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                        }
                    if (cStacks[2] != null) if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[2]))
                        {
                            Api.World.SpawnItemEntity(cStacks[2], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                        }
                    if (cStacks[3] != null) if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[3]))
                        {
                            Api.World.SpawnItemEntity(cStacks[3], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                        }
                    if (cStacks[4] != null) if (!byPlayer.InventoryManager.TryGiveItemstack(cStacks[4]))
                        {
                            Api.World.SpawnItemEntity(cStacks[4], Pos.ToVec3d().Add(0.5, 0.25, 0.5));
                        }
                    inv[0].Itemstack = null;
                }

                Api.World.BlockAccessor.SetBlock(0, Pos);
            }

            return true;
        }

        private bool TryAddIngredientFrom(ItemSlot slot, IPlayer byPlayer = null)
        {
            var shawarmaProps = slot.Itemstack.ItemAttributes?["inShawarmaProperties"]?.AsObject<inShawarmaProperties>(null, slot.Itemstack.Collectible.Code.Domain);
            if (shawarmaProps == null)
            {
                if (byPlayer != null && capi != null) capi.TriggerIngameError(this, "notshawarmaable", Lang.Get("This item can not be added to shawarmas"));
                return false;
            }

            var shawarmaBlock = inv[0].Itemstack.Block as AOCBlockShawarma;
            if (shawarmaBlock == null) return false;

            ItemStack[] cStacks = shawarmaBlock.GetContents(Api.World, inv[0].Itemstack);

            bool isFull = cStacks[1] != null && cStacks[2] != null && cStacks[3] != null && cStacks[4] != null;
            bool hasFilling = cStacks[1] != null || cStacks[2] != null || cStacks[3] != null || cStacks[4] != null;

            if (isFull)
            {
                if (byPlayer != null && capi != null) capi.TriggerIngameError(this, "shawarmafullfilling", Lang.Get("Can't add more filling - already completely filled shawarma"));
                return false;
            }

            if (shawarmaProps.PartType != EnumShawarmaPartType.Filling)
            {
                if (byPlayer != null && capi != null) capi.TriggerIngameError(this, "shawarmaneedsfilling", Lang.Get("Need to add a filling next"));
                return false;
            }


            if (!hasFilling)
            {
                cStacks[1] = slot.TakeOut(1);
                shawarmaBlock.SetContents(inv[0].Itemstack, cStacks);
                return true;
            }

            var foodCats = cStacks.Select(stack => stack?.Collectible.NutritionProps?.FoodCategory ?? stack?.ItemAttributes?["nutritionPropsWhenInMeal"]?.AsObject<FoodNutritionProperties>()?.FoodCategory ?? EnumFoodCategory.Vegetable).ToArray();
            var stackprops = cStacks.Select(stack => stack?.ItemAttributes["inShawarmaProperties"]?.AsObject<inShawarmaProperties>(null, stack.Collectible.Code.Domain)).ToArray();

            ItemStack cstack = slot.Itemstack;
            EnumFoodCategory foodCat = slot.Itemstack?.Collectible.NutritionProps?.FoodCategory ?? slot.Itemstack?.ItemAttributes?["nutritionPropsWhenInMeal"]?.AsObject<FoodNutritionProperties>()?.FoodCategory ?? EnumFoodCategory.Vegetable;


            for (int i = 1; i < cStacks.Length - 1; i++)
            {
                if (cstack == null) continue;
                cstack = cStacks[i];
                foodCat = foodCats[i];
            }

            int emptySlotIndex = 2 + (cStacks[2] != null ? 1 + (cStacks[3] != null ? 1 : 0) : 0);

            cStacks[emptySlotIndex] = slot.TakeOut(1);
            shawarmaBlock.SetContents(inv[0].Itemstack, cStacks);
            return true;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (inv[0].Empty) return true;
            mesher.AddMeshData(mesh);
            return true;
        }

        void loadMesh()
        {
            if (Api == null || Api.Side == EnumAppSide.Server || inv[0].Empty) return;
            mesh = ms.GetShawarmaMesh(inv[0].Itemstack);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            bool isRotten = AOCMeshCache.ContentsRotten(inv);
            if (isRotten)
            {
                dsc.Append(Lang.Get("Rotten"));
            }
            else
            {
                dsc.Append(BlockEntityShelf.PerishableInfoCompact(Api, inv[0], 0, false));
            }
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            if (worldForResolving.Side == EnumAppSide.Client)
            {
                MarkDirty(true);
                loadMesh();
            }
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
        }
    }
}
