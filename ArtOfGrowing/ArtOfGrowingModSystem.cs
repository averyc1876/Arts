using ArtOfGrowing.Items;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

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
            api.RegisterItemClass("AOGItemRidge", typeof(AOGItemRidge));
            api.RegisterItemClass("AOGDryGrass", typeof(AOGItemDryGrass));
            api.RegisterItemClass("AOGItemFood", typeof(AOGItemFood));
            api.RegisterItemClass("AOGItemPlantableSeed", typeof(AOGItemPlantableSeed));
            api.World.Logger.StoryEvent(Lang.Get("It grows..."));        
        }
    }
}
