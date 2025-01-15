using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ArtOfCooking.Systems
{
    public class DoughFormingRecipe : LayeredVoxelRecipe<DoughFormingRecipe>, IByteSerializable
    {
        public override int QuantityLayers => 16;
        public override string RecipeCategoryCode => "dough forming";


        /// <summary>
        /// Creates a deep copy
        /// </summary>
        /// <returns></returns>
        public override DoughFormingRecipe Clone()
        {
            DoughFormingRecipe recipe = new DoughFormingRecipe();

            recipe.Pattern = new string[Pattern.Length][];
            for (int i = 0; i < recipe.Pattern.Length; i++)
            {
                recipe.Pattern[i] = (string[])Pattern[i].Clone();
            }

            recipe.Ingredient = Ingredient.Clone();
            recipe.Output = Output.Clone();
            recipe.Name = Name;

            return recipe;
        }

    }
}
