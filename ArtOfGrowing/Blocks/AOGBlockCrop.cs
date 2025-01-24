using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ArtOfGrowing.Blocks
{
    internal class AOGBlockCrop: BlockCrop
    {
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            world.BlockAccessor.SetBlock(world.GetBlock(CodeWithVariant("tallgrass", "eaten")).Id, pos);
        }
    }
}
