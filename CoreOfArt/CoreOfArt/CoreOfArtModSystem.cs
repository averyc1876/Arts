using CoreOfArts.Blocks;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace CoreOfArts
{
    public class CoreOfArtsModSystem : ModSystem
    {
        private readonly Harmony _harmony = new("coapatch");     
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("COABlockCookingContainer", typeof(COABlockCookingContainer));

            PatchGame();
            api.World.Logger.StoryEvent(Lang.Get("It changes..."));       
        }    
        public override void Dispose()
        {
            var harmony = new Harmony("coapatch");
            harmony.UnpatchAll("coapatch");
        }
        private void PatchGame()
        {
            Mod.Logger.Event("Applying Harmony patches");
            var harmony = new Harmony("coapatch");
            harmony.PatchAll();
        }
    
        [HarmonyPatch]   
        class BlockBarrelPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(BlockBarrel), "getContentMeshFromAttributes")]        
            static void Patch_getContentMeshFromAttributes(ItemStack contentStack, ref ItemStack liquidContentStack, BlockPos forBlockPos)
            {
                liquidContentStack = contentStack;
            }
        }         
    }
}
