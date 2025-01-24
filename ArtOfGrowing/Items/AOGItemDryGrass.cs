using System.Numerics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ArtOfGrowing.Items
{
    public class AOGItemDryGrass : ItemDryGrass
    {
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
			if (blockSel == null || (byEntity?.World) == null)
			{
				return;
			}

			EntityPlayer asPlayer = byEntity as EntityPlayer;
			IPlayer byPlayer = byEntity.World.PlayerByUid((asPlayer)?.PlayerUID);
			BlockPos onPos = blockSel.DidOffset ? blockSel.Position : blockSel.Position.AddCopy(blockSel.Face);
			BlockPos position = blockSel.Position;			

            if (!byEntity.World.Claims.TryAccess(byPlayer, onPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }
            
            if (byEntity.Controls.Sneak && byEntity.Controls.Sprint && !(api.World.BlockAccessor.GetBlock(position) is AOGBlockGroundStorage))
			{
				if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
				{
					return;
				}
				BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos);
				if (blockEntity is BlockEntityLabeledChest || blockEntity is BlockEntitySignPost || blockEntity is BlockEntitySign || blockEntity is BlockEntityBloomery || blockEntity is BlockEntityFirepit || blockEntity is BlockEntityForge)
				{
					return;
				}
				if (blockEntity is IBlockEntityItemPile pile && pile.OnPlayerInteract(byPlayer))
				{
					handHandling = EnumHandHandling.PreventDefaultAction;
                    if (((byPlayer != null) ? (byEntity as EntityPlayer).Player : null) is IClientPlayer clientPlayer) clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                    return;
				}
				else
				{
					blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos.AddCopy(blockSel.Face));
					if (blockEntity is IBlockEntityItemPile apile && apile.OnPlayerInteract(byPlayer))
					{
						handHandling = EnumHandHandling.PreventDefaultAction;
                        if (((byEntity is EntityPlayer entityPlayer2) ? entityPlayer2.Player : null) is not IClientPlayer clientPlayer2)
                        {
                            return;
                        }
                        clientPlayer2.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
					else
					{
						AOGBlockGroundStorage blockPile = byEntity.World.GetBlock(new AssetLocation("haystorage")) as AOGBlockGroundStorage;
						if (blockPile == null)
						{
							return;
						}
						BlockPos blockPos1 = position.Copy();
						if (byEntity.World.BlockAccessor.GetBlock(blockPos1).Replaceable < 6000)
						{
							blockPos1.Add(blockSel.Face, 1);
						}
						bool flag = blockPile.CreateStorage(byEntity.World, blockSel, byPlayer);
						Cuboidf[] collisionBoxes = byEntity.World.BlockAccessor.GetBlock(blockPos1).GetCollisionBoxes(byEntity.World.BlockAccessor, blockPos1);
						if (collisionBoxes != null && collisionBoxes.Length != 0 && CollisionTester.AabbIntersect(collisionBoxes[0], (double)blockPos1.X, (double)blockPos1.Y, (double)blockPos1.Z, byPlayer.Entity.CollisionBox, byPlayer.Entity.SidedPos.XYZ))
						{
							byPlayer.Entity.SidedPos.Y += (double)collisionBoxes[0].Y2 - (byPlayer.Entity.SidedPos.Y - (double)((int)byPlayer.Entity.SidedPos.Y));
						}
						if (!flag)
						{
							base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
							return;
						}
						handHandling = EnumHandHandling.PreventDefaultAction;
                        if (((byEntity is EntityPlayer entityPlayer3) ? entityPlayer3.Player : null) is not IClientPlayer clientPlayer3)
                        {
                            return;
                        }
                        clientPlayer3.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
				}
			}

            if (byEntity.Controls.Sneak && api.World.BlockAccessor.GetBlock(position) is AOGBlockGroundStorage)
            {
                return;
            }

			if (itemslot.Itemstack.Item.Code.FirstCodePart() != "thatch")
			{
				base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
				return;
			}

            IWorldAccessor world = byEntity.World;
            Block firepitBlock = world.GetBlock(new AssetLocation("artofgrowing:firepit-construct1"));
            if (firepitBlock == null)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            if (!byEntity.World.Claims.TryAccess(byPlayer, onPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }

            Block block = world.BlockAccessor.GetBlock(onPos.DownCopy());
            Block aimedBlock = world.BlockAccessor.GetBlock(blockSel.Position);
            if (aimedBlock is BlockGroundStorage)
            {
                var bec = world.BlockAccessor.GetBlockEntity<BlockEntityGroundStorage>(blockSel.Position);
                if (bec.Inventory[3].Empty && bec.Inventory[2].Empty && bec.Inventory[1].Empty && bec.Inventory[0].Itemstack.Collectible is ItemFirewood)
                {
                    if (bec.Inventory[0].StackSize == bec.Capacity)
                    {
                        string useless = "";
                        if (!firepitBlock.CanPlaceBlock(world, byPlayer, new BlockSelection() { Position = onPos, Face = BlockFacing.UP }, ref useless)) return;
                        world.BlockAccessor.SetBlock(firepitBlock.BlockId, onPos);
                        if (firepitBlock.Sounds != null) world.PlaySoundAt(firepitBlock.Sounds.Place, blockSel.Position.X, blockSel.Position.InternalY, blockSel.Position.Z, byPlayer);
                        itemslot.Itemstack.StackSize--;

                    }
                    handHandling = EnumHandHandling.PreventDefault;
                    return;
                }

                if (!(aimedBlock is BlockPitkiln))
                {
                    BlockPitkiln blockpk = world.GetBlock(new AssetLocation("pitkiln")) as BlockPitkiln;
                    if (blockpk.TryCreateKiln(world, byPlayer, blockSel.Position))
                    {
                        handHandling = EnumHandHandling.PreventDefault;
                    }
                }
            }
            else
            {

                string useless = "";

                if (!block.CanAttachBlockAt(byEntity.World.BlockAccessor, firepitBlock, onPos.DownCopy(), BlockFacing.UP)) return;
                if (!firepitBlock.CanPlaceBlock(world, byPlayer, new BlockSelection() { Position = onPos, Face = BlockFacing.UP }, ref useless)) return;

                world.BlockAccessor.SetBlock(firepitBlock.BlockId, onPos);

                if (firepitBlock.Sounds != null) world.PlaySoundAt(firepitBlock.Sounds.Place, blockSel.Position.X, blockSel.Position.InternalY, blockSel.Position.Z, byPlayer);

                itemslot.Itemstack.StackSize--;
                handHandling = EnumHandHandling.PreventDefaultAction;
            }

			base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
        }
    }
}
