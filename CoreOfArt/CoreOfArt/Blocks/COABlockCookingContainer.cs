using CoreOfArts.BlockEntityRenderer;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace CoreOfArts.Blocks
{
public class COABlockCookingContainer : BlockCookingContainer, IInFirepitRendererSupplier
{  
    new public IInFirepitRenderer GetRendererWhenInFirepit(ItemStack stack, BlockEntityFirepit firepit, bool forOutputSlot)
    {
        return new COAPotInFirepitRenderer(api as ICoreClientAPI, stack, firepit.Pos, forOutputSlot);
    }
        
    public override void DoSmelt(IWorldAccessor world, ISlotProvider cookingSlotsProvider, ItemSlot inputSlot, ItemSlot outputSlot)
    {
        ItemStack[] array = GetCookingStacks(cookingSlotsProvider);
        CookingRecipe matchingCookingRecipe = GetMatchingCookingRecipe(world, array);
        Block block = world.GetBlock(CodeWithVariant("type", "cooked"));
        if (matchingCookingRecipe == null)
        {
            return;
        }

        int quantityServings = matchingCookingRecipe.GetQuantityServings(array);
        if (matchingCookingRecipe.CooksInto != null)
        {
            ItemStack itemStack = matchingCookingRecipe.CooksInto.ResolvedItemstack?.Clone();
            if (itemStack != null)
            {
                itemStack.StackSize *= quantityServings;
                array = new ItemStack[1] { itemStack };
                if (itemStack.Attributes.HasAttribute("notDirtied"))
                { 
                    Block block1 = world.GetBlock(new AssetLocation(Attributes["mealBlockCode"].AsString()));
                    block = world.GetBlock(new AssetLocation(block1.Attributes["emptiedBlockCode"].AsString()));
                }
                if (!itemStack.Attributes.HasAttribute("notDirtied")) block = world.GetBlock(new AssetLocation(Attributes["dirtiedBlockCode"].AsString()));
            }
        }
        else
        {
            for (int i = 0; i < array.Length; i++)
            {
                ItemStack itemStack2 = matchingCookingRecipe.GetIngrendientFor(array[i]).GetMatchingStack(array[i])?.CookedStack?.ResolvedItemstack.Clone();
                if (itemStack2 != null)
                {
                    array[i] = itemStack2;
                }
            }
        }

        ItemStack itemStack3 = new ItemStack(block);
        itemStack3.Collectible.SetTemperature(world, itemStack3, GetIngredientsTemperature(world, array));
        TransitionableProperties transitionableProperties = matchingCookingRecipe.PerishableProps.Clone();
        transitionableProperties.TransitionedStack.Resolve(world, "cooking container perished stack");
        CollectibleObject.CarryOverFreshness(api, cookingSlotsProvider.Slots, array, transitionableProperties);
        if (matchingCookingRecipe.CooksInto != null)
        {
            for (int j = 0; j < cookingSlotsProvider.Slots.Length; j++)
            {
                cookingSlotsProvider.Slots[j].Itemstack = ((j == 0) ? array[0] : null);
            }

            inputSlot.Itemstack = itemStack3;
            return;
        }

        for (int k = 0; k < cookingSlotsProvider.Slots.Length; k++)
        {
            cookingSlotsProvider.Slots[k].Itemstack = null;
        }

        ((BlockCookedContainer)block).SetContents(matchingCookingRecipe.Code, quantityServings, itemStack3, array);
        outputSlot.Itemstack = itemStack3;
        inputSlot.Itemstack = null;
    }
}
}
