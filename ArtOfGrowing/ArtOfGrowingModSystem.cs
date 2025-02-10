using ArtOfGrowing.BlockBehaviors;
using ArtOfGrowing.BlockEntites;
using ArtOfGrowing.Blocks;
using ArtOfGrowing.Items;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace ArtOfGrowing
{
    public class ArtOfGrowingModSystem : ModSystem
    {   
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("AOGBlockHayStorage", typeof(AOGBlockGroundStorage));
            api.RegisterBlockClass("AOGBlockPumpkin", typeof(AOGBlockPumpkin));
            api.RegisterBlockClass("AOGBlockHayLayer", typeof(AOGBlockHayLayer));
            api.RegisterBlockClass("AOGBlockCrop", typeof(AOGBlockCrop));

            api.RegisterBlockEntityClass("AOGHayStorage", typeof(AOGBlockEntityGroundStorage));
            api.RegisterBlockEntityClass("AOGBlockEntityPumpkinVine", typeof(AOGBlockEntityPumpkinVine));
            api.RegisterBlockEntityClass("AOGTransient", typeof(AOGTransient));

            api.RegisterCollectibleBehaviorClass("AOGHayStorable", typeof(AOGCollectibleBehaviorGroundStorable));

            api.RegisterItemClass("AOGFlaxSoft", typeof(AOGItemFlaxSoft));
            api.RegisterItemClass("AOGItemInteract", typeof(AOGItemInteract));
            api.RegisterItemClass("AOGItemRemap", typeof(AOGItemRemap));
            api.RegisterItemClass("AOGItemDryGrass", typeof(AOGItemDryGrass));
            api.RegisterItemClass("AOGItemPlantableSeed", typeof(AOGItemPlantableSeed));
            api.RegisterItemClass("AOGItemSeedling", typeof(AOGItemSeedling));
            api.RegisterItemClass("AOGHayfork", typeof(AOGItemHayfork));

            api.RegisterCropBehavior("AOGPumpkin", typeof(AOGPumpkinCropBehavior));

            api.RegisterBlockBehaviorClass("AOGPlacing", typeof(AOGBehaviorPlacing));

            ClassRegistry registry = (api as ServerCoreAPI)?.ClassRegistryNative ?? (api as ClientCoreAPI)?.ClassRegistryNative;
            if (registry != null)
            {
                registry.BlockClassToTypeMapping["BlockTallGrass"] = typeof(AOGBlockTallGrass);
                registry.ItemClassToTypeMapping["ItemScythe"] = typeof(AOGItemScythe);
            }

            api.World.Logger.StoryEvent(Lang.Get("It grows..."));        
        }
    }
}
