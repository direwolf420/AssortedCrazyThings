using AssortedCrazyThings.Buffs;
using AssortedCrazyThings.Projectiles.Pets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Items.Pets
{
    public class CuteSlimeBlackNew : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bottled Black Slime");
            Tooltip.SetDefault("Summons a friendly Cute Black Slime to follow you.");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LizardEgg);
            item.shoot = mod.ProjectileType<CuteSlimeBlackNewPet>();
            item.buffType = mod.BuffType<CuteSlimeBlackNewBuff>();
            item.rare = -11;
        }
		
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod, "CuteSlimeBlack");
			recipe.AddTile(TileID.Solidifier);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override void UseStyle(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(item.buffType, 3600, true);
            }
        }
    }
}