using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace AssortedCrazyThings.Items.Weapons
{
    /// <summary>
    /// Item that applies a buff and spawns a single projectile on use
    /// </summary>
    [Content(ContentType.Weapons)]
    public abstract class MinionItemBase : AssItem
    {
        public sealed override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            return SafeShoot(player, source, position, velocity, type, damage, knockback);
        }

        public virtual bool SafeShoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
    }
}
