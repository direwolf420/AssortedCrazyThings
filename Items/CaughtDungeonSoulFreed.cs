using AssortedCrazyThings.NPCs.DungeonBird;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Items
{
    //the one actually used in recipes
    [Content(ContentType.Bosses)]
    public class CaughtDungeonSoulFreed : CaughtDungeonSoulBase
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Freed Dungeon Soul");
            Tooltip.SetDefault("Awakened by defeating the " + Harvester.name);
            ItemID.Sets.ItemIconPulse[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SafeSetDefaults()
        {
            frame2CounterCount = 4;
            animatedTextureSelect = 1;
        }

        //hardmode recipe
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(1).AddIngredient(ItemID.BorealWoodCandle, 1).AddIngredient(this, 15).AddTile(TileID.CrystalBall);
            recipe.ReplaceResult(ItemID.WaterCandle);
            recipe.Register();
        }
    }
}
