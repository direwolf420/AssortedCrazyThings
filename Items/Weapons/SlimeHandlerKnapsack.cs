﻿using AssortedCrazyThings.Buffs;
using AssortedCrazyThings.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AssortedCrazyThings.Items.Weapons
{
    public class SlimeHandlerKnapsack : MinionItemBase
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime Handler Knapsack");
            Tooltip.SetDefault("Summons a friendly Slime from your Knapsack to fight for you.");
        }

        public override void SetDefaults()
        {
            //change damage in SlimePackMinion.cs
            item.damage = SlimePackMinion.DefDamage;
            item.summon = true;
            item.mana = 10;
            item.width = 24;
            item.height = 30;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = 4; //4 for life crystal
            item.noMelee = true;
            item.value = Item.sellPrice(0, 0, 75, 0);
            item.rare = -11;
            item.UseSound = SoundID.Item44;
            item.shoot = mod.ProjectileType<SlimePackMinion>();
            item.shootSpeed = 10f;
            item.knockBack = 4f; //same as slime staff x 2
            item.buffType = mod.BuffType<SlimePackMinionBuff>();
            item.buffTime = 3600;
        }

        public override bool AltFunctionUse(Player player)
        {
            return false; //true
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(player.position.X + (player.width / 2) - player.direction * 12f, player.position.Y - 8f, - player.velocity.X, player.velocity.Y - 6f, item.shoot, item.damage, item.knockBack, Main.myPlayer, 0f, 0f);
            return false;
        }

        public override void HoldItem(Player player)
        {
            player.itemLocation.X = 0; //kind of a hack, 
            player.itemLocation.Y = 0;
        }

        public override void AddRecipes()
        {
            //TODO
            //ModRecipe recipe = new ModRecipe(mod);
            //recipe.AddIngredient(ItemID.SlimeCrown, 1);
            //recipe.AddIngredient(ItemID.Gel, 999);
            //recipe.AddIngredient(ItemID.SoulofLight, 10);
            //recipe.AddIngredient(ItemID.SoulofNight, 10);
            //recipe.AddTile(TileID.Anvils);
            //recipe.SetResult(this);
            //recipe.AddRecipe();
        }
    }
}