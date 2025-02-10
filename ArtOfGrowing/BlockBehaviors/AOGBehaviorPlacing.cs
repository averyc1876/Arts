using Vintagestory.API.Common;

namespace Vintagestory.GameContent
{
    public class AOGBehaviorPlacing : BlockBehavior
    {
        public AOGBehaviorPlacing(Block block) : base(block)
        {
        }

        public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            if (byPlayer.Entity.Controls.ShiftKey)
            {
                handling = EnumHandling.PreventDefault;
                failureCode = "onlywhensneaking";
                return false;
            }

            return base.CanPlaceBlock(world, byPlayer, blockSel, ref handling, ref failureCode);
        }
        
    }
}
