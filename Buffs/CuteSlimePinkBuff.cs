using Terraria;
using Terraria.ModLoader;
using AssortedCrazyThings.Projectiles.Pets;

namespace AssortedCrazyThings.Buffs
{
	public class CuteSlimePinkBuff : ModBuff
		{
			public override void SetDefaults()
				{
					DisplayName.SetDefault("Cute Slime");
					Description.SetDefault("A cute pink slime girl is following you.");
					Main.buffNoTimeDisplay[Type] = true;
					Main.vanityPet[Type] = true;
				}
			public override void Update(Player player, ref int buffIndex)
				{
					player.buffTime[buffIndex] = 18000;
					player.GetModPlayer<PetPlayer>(mod).CuteSlimePink = true;
					bool petProjectileNotSpawned = player.ownedProjectileCounts[mod.ProjectileType<CuteSlimePinkPet>()] <= 0;
					if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
						{
							Projectile.NewProjectile(player.position.X + (float)(player.width / 2), player.position.Y, 0f, 0f, mod.ProjectileType<CuteSlimePinkPet>(), 0, 0f, player.whoAmI, 0f, 0f);
						}
				}
		}
}