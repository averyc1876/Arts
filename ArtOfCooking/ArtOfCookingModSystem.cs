using ArtOfCooking.BlockEntities;
using ArtOfCooking.Blocks;
using ArtOfCooking.Items;
using Vintagestory.API.Common;

namespace ArtOfCooking;
public class ArtOfCooking : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        
        api.RegisterItemClass("AOCItemDough", typeof(AOCItemDough));
        api.RegisterBlockClass("BlockDoughForm", typeof(BlockDoughForm));
        api.RegisterBlockEntityClass("DoughForm", typeof(BlockEntityDoughForm));

        api.RegisterItemClass("AOCItemLavash", typeof(AOCItemLavash));
        api.RegisterBlockClass("AOCBlockShawarma", typeof(AOCBlockShawarma));
        api.RegisterBlockEntityClass("AOCBEShawarma", typeof(AOCBEShawarma));
        
        api.RegisterBlockClass("AOCBlockSpoon", typeof(AOCBlockSpoon));
        api.RegisterBlockClass("AOCBlockEmptySpoon", typeof(AOCBlockEmptySpoon));
        api.RegisterBlockEntityClass("AOCBlockEntitySpoon", typeof(AOCBlockEntitySpoon));
        
        api.RegisterItemClass("AOCItemEgg", typeof(AOCItemEgg));
    }
}
