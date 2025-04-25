using ArtOfCooking.BlockBehaviors;
using ArtOfCooking.BlockEntities;
using ArtOfCooking.Blocks;
using ArtOfCooking.CollectibleBehaviors;
using ArtOfCooking.Items;
using ArtOfCooking.Systems;
using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

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
        
        api.RegisterCollectibleBehaviorClass("AOCEgg", typeof(AOCCollectibleBehaviorEgg));
        api.RegisterBlockBehaviorClass("AOCTable", typeof(AOCBlockBehaviorTable));
        
        api.RegisterItemClass("AOCItemFlour", typeof(AOCItemFlour));
        api.RegisterBlockClass("AOCBlockDoughKnead", typeof(AOCBlockDoughKnead));
        
        api.RegisterItemClass("AOCItemFood", typeof(AOCItemFood));
    }
    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);
        api.GetCookingRecipes().ForEach(recipe =>
        {
            if (!CookingRecipe.NamingRegistry.ContainsKey(recipe.Code))
            {
                CookingRecipe.NamingRegistry[recipe.Code] = new AOCRecipeNames();
            }
        });
    }
}
