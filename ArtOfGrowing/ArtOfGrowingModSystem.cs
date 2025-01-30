using ArtOfGrowing.BlockBehavior;
using ArtOfGrowing.BlockEntites;
using ArtOfGrowing.Blocks;
using ArtOfGrowing.Items;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace ArtOfGrowing
{
    public class ArtOfGrowingModSystem : ModSystem
    {   
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("AOGBlockHayStorage", typeof(AOGBlockGroundStorage));
            api.RegisterBlockEntityClass("AOGHayStorage", typeof(AOGBlockEntityGroundStorage));
            api.RegisterCollectibleBehaviorClass("AOGHayStorable", typeof(AOGCollectibleBehaviorGroundStorable));
            api.RegisterItemClass("AOGFlaxSoft", typeof(AOGItemFlaxSoft));
            api.RegisterItemClass("AOGItemInteract", typeof(AOGItemInteract));
            api.RegisterItemClass("AOGItemRemap", typeof(AOGItemRemap));
            api.RegisterItemClass("AOGItemDryGrass", typeof(AOGItemDryGrass));
            api.RegisterItemClass("AOGItemPlantableSeed", typeof(AOGItemPlantableSeed));
            api.RegisterItemClass("AOGItemSeedling", typeof(AOGItemSeedling));
            api.RegisterBlockClass("AOGBlockPumpkin", typeof(AOGBlockPumpkin));
            api.RegisterCropBehavior("AOGPumpkin", typeof(AOGPumpkinCropBehavior));
            api.RegisterBlockEntityClass("AOGBlockEntityPumpkinVine", typeof(AOGBlockEntityPumpkinVine));
            api.RegisterBlockBehaviorClass("AOGPlacing", typeof(AOGBehaviorPlacing));
            api.World.Logger.StoryEvent(Lang.Get("It grows..."));        
        }
    }
}
