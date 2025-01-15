using ArtOfCooking.BlockEntities;
using ArtOfCooking.Blocks;
using ArtOfCooking.Systems;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ArtOfCooking.BlockEntities
{
    public class AOCBlockEntitySpoon : BlockEntityMeal
    {
        internal AOCBlockSpoon ownBlock;
        int tickCnt = 0;
        bool wasRotten;
        internal InventoryGeneric inventory;
        MeshData currentMesh;
        public AOCBlockEntitySpoon()
        {
            inventory = new InventoryGeneric(4, null, null);
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            ownBlock = Api.World.BlockAccessor.GetBlock(Pos) as AOCBlockSpoon;

            if (Api.Side == EnumAppSide.Client)
            {
                RegisterGameTickListener(Every100ms, 200);
            }
        }
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            AOCBlockSpoon blockmeal = byItemStack?.Block as AOCBlockSpoon;
            if (blockmeal != null)
            {
                ItemStack[] stacks = blockmeal.GetContents(Api.World, byItemStack);
                for (int i = 0; i < stacks.Length; i++)
                {
                    Inventory[i].Itemstack = stacks[i];
                }

                RecipeCode = blockmeal.GetRecipeCode(Api.World, byItemStack);
                QuantityServings = blockmeal.GetQuantityServings(Api.World, byItemStack);
            }
            
            if (Api.Side == EnumAppSide.Client)
            {
                currentMesh = GenMesh();
                MarkDirty(true);
            }
        }
        
        private void Every100ms(float dt)
        {
            float temp = GetTemperature();
            if (Api.World.Rand.NextDouble() < (temp - 50) / 320)
            {
                BlockCookedContainer.smokeHeld.MinPos = Pos.ToVec3d().AddCopy(0.5 - 0.05, 0.125, 0.5 - 0.05);
                Api.World.SpawnParticles(BlockCookedContainer.smokeHeld);
            }


            if (tickCnt++ % 20 == 0)
            {
                if (!wasRotten && Rotten)
                {
                    currentMesh = GenMesh();
                    MarkDirty(true);
                    wasRotten = true;
                }
            }
        }
        private int GetTemperature()
        {
            ItemStack[] stacks = GetNonEmptyContentStacks(false);
            if (stacks.Length == 0 || stacks[0] == null) return 0;

            return (int)stacks[0].Collectible.GetTemperature(Api.World, stacks[0]);
        }
        internal MeshData GenMesh()
        {
            if (ownBlock == null) return null;
            ItemStack[] stacks = GetNonEmptyContentStacks();
            if (stacks == null || stacks.Length == 0) return null;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            return capi.ModLoader.GetModSystem<AOCMeshCache>().GenMealInContainerMesh(ownBlock, FromRecipe, stacks);
        }
    }
}
