using AssortedCrazyThings.Items;
using AssortedCrazyThings.Items.Weapons;
using AssortedCrazyThings.Projectiles.Minions;
using AssortedCrazyThings.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using AssortedCrazyThings.UI;
using AssortedCrazyThings.Buffs;
using System.IO;

namespace AssortedCrazyThings
{
    public class AssPlayer : ModPlayer
    {
        public bool everburningCandleBuff = false;
        public bool everburningCursedCandleBuff = false;
        public bool everfrozenCandleBuff = false;
        public bool everburningShadowflameCandleBuff = false;

        public bool teleportHome = false;
        public bool canTeleportHome = false;
        public const short TeleportHomeTimerMax = 600; //in seconds //10 ingame minutes
        public short teleportHomeTimer = 0; //gets saved when you relog so you can't cheese it

        //TECHNICALLY NOT DEFENSE; YOU JUST GET 1 DAMAGE FROM EVERYTHING FOR A CERTAIN DURATION
        public bool getDefense = false;
        public bool canGetDefense = false;
        public const short GetDefenseTimerMax = 600; //in seconds //10 ingame minutes
        private const short GetDefenseDurationMax = 600; //in ticks //10 ingame seconds
        public short getDefenseDuration = 0;
        public short getDefenseTimer = 0; //gets saved when you relog so you can't cheese it

        //soul minion stuff
        public bool soulMinion = false;
        public bool tempSoulMinion = false;
        public SoulType selectedSoulMinionType = SoulType.Dungeon;

        public bool slimePackMinion = false;
        public byte selectedSlimePackMinionType = 0;

        //empowering buff stuff
        public bool empoweringBuff = false;
        private const short empoweringTimerMax = 60; //in seconds //one minute until it caps out (independent of buff duration)
        private short empoweringTimer = 0;
        public static float empoweringTotal = 0.5f; //this gets modified in AssWorld.PreUpdate()
        public float step = 0f;

        //enhanced hunter potion stuff
        public bool enhancedHunterBuff = false;

        //cute slime spawn enable buff
        public bool cuteSlimeSpawnEnable = false;

        public bool soulSaviorArmor = false;

        public bool wyvernCampfire = false;

        public bool droneControllerMinion = false;

        public const byte shieldDroneReductionMax = 50;
        public byte shieldDroneReduction = 0; //percentage * 100

        /// <summary>
        /// Bitfield. Use .HasFlag(DroneType.SomeType) to check if its there or not
        /// </summary>
        public DroneType droneControllerUnlocked = DroneType.BasicLaser;

        /// <summary>
        /// Contains the DroneType value (not 0 to 7, but 2^0 to 2^7)
        /// </summary>
        public DroneType selectedDroneControllerMinionType = DroneType.BasicLaser;

        private bool rightClickPrev = false;
        private bool rightClickPrev2 = false;

        private bool leftClickPrev = false;
        private bool leftClickPrev2 = false;

        public override void ResetEffects()
        {
            everburningCandleBuff = false;
            everburningCursedCandleBuff = false;
            everfrozenCandleBuff = false;
            everburningShadowflameCandleBuff = false;
            teleportHome = false;
            getDefense = false;
            soulMinion = false;
            tempSoulMinion = false;
            slimePackMinion = false;
            empoweringBuff = false;
            enhancedHunterBuff = false;
            cuteSlimeSpawnEnable = false;
            soulSaviorArmor = false;
            droneControllerMinion = false;
        }

        public bool RightClickPressed { get { return rightClickPrev && !rightClickPrev2; } }

        public bool RightClickReleased { get { return !rightClickPrev && rightClickPrev2; } }

        public bool LeftClickPressed { get { return leftClickPrev && !leftClickPrev2; } }

        public bool LeftClickReleased { get { return !leftClickPrev && leftClickPrev2; } }

        public override TagCompound Save()
        {
            return new TagCompound
            {
                {"teleportHomeWhenLowTimer", (int)teleportHomeTimer},
                {"getDefenseTimer", (int)getDefenseTimer},
                {"droneControllerUnlocked", (byte)droneControllerUnlocked}
            };
        }

        public override void Load(TagCompound tag)
        {
            teleportHomeTimer = (short)tag.GetInt("teleportHomeWhenLowTimer");
            getDefenseTimer = (short)tag.GetInt("getDefenseTimer");
            droneControllerUnlocked = (DroneType)tag.GetByte("droneControllerUnlocked");
        }
        public override void clientClone(ModPlayer clientClone)
        {
            AssPlayer clone = clientClone as AssPlayer;
            clone.shieldDroneReduction = shieldDroneReduction;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            AssPlayer clone = clientPlayer as AssPlayer;
            if (clone.shieldDroneReduction != shieldDroneReduction)
            {
                Main.NewText("send packet");
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)AssMessageType.ClientChangesAssPlayer);
                packet.Write((byte)player.whoAmI);
                packet.Write((byte)shieldDroneReduction);
                packet.Send();
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            //from server to clients
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)AssMessageType.SyncAssPlayer);
            packet.Write((byte)player.whoAmI);
            packet.Write((byte)shieldDroneReduction);
            packet.Send(toWho, fromWho);
        }

        public void ResetEmpoweringTimer(bool fromServer = false)
        {
            if (empoweringBuff && !player.HasBuff(BuffID.ShadowDodge))
            {
                for (int i = 0; i < empoweringTimer; i++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 135, new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)) + (new Vector2(Main.rand.Next(-1, 1), Main.rand.Next(-1, 1)) * ((6 * empoweringTimer) / empoweringTimerMax)), 26, Color.White, Main.rand.NextFloat(1.5f, 2.4f));
                    dust.noLight = true;
                    dust.noGravity = true;
                    dust.fadeIn = Main.rand.NextFloat(1f, 2.3f);
                }
                empoweringTimer = 0;

                if (Main.netMode == NetmodeID.MultiplayerClient && !fromServer)
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)AssMessageType.ResetEmpoweringTimerpvp);
                    packet.Write((byte)player.whoAmI);
                    packet.Send(); //send to server
                }
            }
        }

        public void DecreaseDroneShield(ref int damage)
        {
            if(shieldDroneReduction > 0)
            {
                for (int i = 0; i < shieldDroneReduction / 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 135, new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)) + new Vector2(Main.rand.Next(-1, 1), Main.rand.Next(-1, 1)), 26, Color.White, Main.rand.NextFloat(1.5f, 2.4f));
                    dust.noLight = true;
                    dust.noGravity = true;
                    dust.fadeIn = Main.rand.NextFloat(1f, 2.3f);
                }

                //TODO testing shield drone
                Main.NewText("reduction: " + ((100 - shieldDroneReduction) / 100f));
                damage = (int)(damage * ((100 - shieldDroneReduction) / 100f));
                if (Main.netMode != NetmodeID.Server && Main.myPlayer == player.whoAmI) shieldDroneReduction -= 10; //since this is only set clientside by the projectile and synced by packets
            }
        }
        private void PreSync(Projectile proj)
        {
            if (proj.modProjectile != null && proj.modProjectile is CompanionDungeonSoulMinionBase)
            {
                CompanionDungeonSoulMinionBase soul = (CompanionDungeonSoulMinionBase)proj.modProjectile;
                soul.isTemp = true;
            }
        }

        /// <summary>
        /// Spawns the temporary soul when wearing the accessory that allows it
        /// </summary>
        private void SpawnSoulTemp()
        {
            if (tempSoulMinion && player.whoAmI == Main.myPlayer)
            {
                bool checkIfAlive = false;
                int spawnedType = Main.hardMode ? mod.ProjectileType<CompanionDungeonSoulPostWOFMinion>() : mod.ProjectileType<CompanionDungeonSoulPreWOFMinion>();
                int spawnedDamage = Main.hardMode ? (int)(EverhallowedLantern.BaseDmg * 1.1f * 2f) : ((EverhallowedLantern.BaseDmg / 2 - 1) * 2);
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == spawnedType)
                    {
                        if (Main.projectile[i].minionSlots == 0f) //criteria for temp, is set by isTemp
                        {
                            checkIfAlive = true;
                            break;
                        }
                    }
                }
                
                if (!checkIfAlive)
                {
                    AssUtils.NewProjectile(player.Center.X, player.Center.Y, -player.velocity.X, player.velocity.Y - 6f, spawnedType, spawnedDamage, EverhallowedLantern.BaseKB, preSync: PreSync);
                }
            }
        }

        private bool EligibleToRecieveSoulBuff(NPC npc)
        {
            //returns true if isn't in soulbuffblacklist or is a worm body or tail
            
            return Array.BinarySearch(AssortedCrazyThings.soulBuffBlacklist, npc.type) < 0 || AssUtils.IsWormBodyOrTail(npc);
        }

        private void SpawnSoulsWhenHarvesterIsAlive()
        {
            //ALWAYS GENERATE SOULS WHEN ONE IS ALIVE (otherwise he will never eat stuff when you aren't infront of dungeon walls)
            if (Main.time % 30 == 4)
            {
                bool shouldDropSouls = false;
                int index = 200;
                for (short j = 0; j < 200; j++)
                {
                    if (Main.npc[j].active && Array.IndexOf(AssWorld.harvesterTypes, Main.npc[j].type) != -1)
                    {
                        shouldDropSouls = true;
                        index = j;
                        break;
                    }
                }

                if (shouldDropSouls)
                {
                    int distance = (int)(Main.npc[index].Center - player.Center).Length();
                    if (distance < 2880 || player.ZoneDungeon) //one and a half screens or in dungeon
                    {
                        for (short j = 0; j < 200; j++)
                        {
                            if (Main.npc[j].active && Main.npc[j].lifeMax > 5 && !Main.npc[j].friendly && !Main.npc[j].dontTakeDamage && !Main.npc[j].immortal)
                            {
                                if (Array.IndexOf(AssWorld.harvesterTypes, Main.npc[j].type) == -1 && EligibleToRecieveSoulBuff(Main.npc[j]))
                                {
                                    Main.npc[j].AddBuff(mod.BuffType("SoulBuff"), 60, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ConvertInertSoulsInventory()
        {
            //this gets called once on server side for all players, and then each player calls it on itself client side
            int tempStackCount;
            int itemTypeOld = mod.ItemType<CaughtDungeonSoul>();
            int itemTypeNew = mod.ItemType<CaughtDungeonSoulFreed>(); //version that is used in crafting

            Item[][] inventoryArray = { player.inventory, player.bank.item, player.bank2.item, player.bank3.item }; //go though player inv
            for (int y = 0; y < inventoryArray.Length; y++)
            {
                for (int e = 0; e < inventoryArray[y].Length; e++)
                {
                    if (inventoryArray[y][e].type == itemTypeOld) //find inert soul
                    {
                        tempStackCount = inventoryArray[y][e].stack;
                        inventoryArray[y][e].SetDefaults(itemTypeNew); //override with awakened
                        inventoryArray[y][e].stack = tempStackCount;
                    }
                }
            }

            //trash slot
            if (player.trashItem.type == itemTypeOld)
            {
                tempStackCount = player.trashItem.stack;
                player.trashItem.SetDefaults(itemTypeNew); //override with awakened
                player.trashItem.stack = tempStackCount;
            }

            //mouse item
            if (Main.netMode != NetmodeID.Server && Main.mouseItem.type == itemTypeOld)
            {
                tempStackCount = Main.mouseItem.stack;
                Main.mouseItem.SetDefaults(itemTypeNew); //override with awakened
                Main.mouseItem.stack = tempStackCount;
            }
        }

        private void UpdateTeleportHomeWhenLow()
        {
            //this code runs even when the accessory is not equipped
            canTeleportHome = teleportHomeTimer <= 0;

            if (!canTeleportHome && Main.time % 60 == 59)
            {
                teleportHomeTimer--;
            }
        }

        private void UpdateGetDefenseWhenLow()
        {
            //this code runs even when the accessory is not equipped
            canGetDefense = getDefenseTimer <= 0;

            if (!canGetDefense && Main.time % 60 == 59)
            {
                getDefenseTimer--;
            }

            if (getDefenseDuration != 0)
            {
                getDefenseDuration--;
            }
        }

        private void Empower()
        {
            if (empoweringBuff)
            {
                if (Main.time % 60 == 0)
                {
                    if (empoweringTimer < empoweringTimerMax)
                    {
                        empoweringTimer++;
                        step = (empoweringTimer * empoweringTotal) / empoweringTimerMax;
                    }
                }
            }
            else empoweringTimer = 0;
        }

        private bool GetDefense(double damage)
        {
            if (getDefense)
            {
                if (canGetDefense)
                {
                    player.statLife += (int)damage;
                    player.AddBuff(BuffID.RapidHealing, 600);
                    CombatText.NewText(player.getRect(), CombatText.HealLife, "Defense increased");

                    getDefenseTimer = GetDefenseTimerMax;
                    getDefenseDuration = GetDefenseDurationMax;
                    return false;
                }
            }
            return true;
        }

        private bool TeleportHome(double damage)
        {
            if (teleportHome)
            {
                if (canTeleportHome && player.whoAmI == Main.myPlayer)
                {
                    //this part here is from vanilla magic mirror code
                    player.grappling[0] = -1;
                    player.grapCount = 0;
                    for (int i = 0; i < 1000; i++)
                    {
                        //Kill all grappling hooks
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].aiStyle == 7)
                        {
                            Main.projectile[i].Kill();
                        }
                    }

                    //inserted before player.Spawn()
                    player.statLife += (int)damage;

                    player.Spawn();
                    for (int i = 0; i < 70; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, default(Color), 1.5f);
                    }
                    //end

                    player.AddBuff(BuffID.RapidHealing, 300, false);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
                    }

                    teleportHomeTimer = TeleportHomeTimerMax;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the status of right click (one tick delay, used for UI)
        /// </summary>
        private void RightClickStatus()
        {
            if (Main.mouseRight && !rightClickPrev)
            {
                rightClickPrev = true;
                return;
            }
            if (!Main.mouseRight && rightClickPrev)
            {
                rightClickPrev = false;
                return;
            }

            if (rightClickPrev && !rightClickPrev2)
            {
                rightClickPrev2 = true;
            }
            if (!rightClickPrev && rightClickPrev2)
            {
                rightClickPrev2 = false;
            }
        }

        /// <summary>
        /// Updates the status of left click (one tick delay, used for UI)
        /// </summary>
        private void LeftClickStatus()
        {
            if (Main.mouseLeft && !leftClickPrev)
            {
                leftClickPrev = true;
                return;
            }
            if (!Main.mouseLeft && leftClickPrev)
            {
                leftClickPrev = false;
                return;
            }

            if (leftClickPrev && !leftClickPrev2)
            {
                leftClickPrev2 = true;
            }
            if (!leftClickPrev && leftClickPrev2)
            {
                leftClickPrev2 = false;
            }
        }

        private void ApplyCandleDebuffs(Entity victim)
        {
            if (victim is NPC)
            {
                if (everburningCandleBuff) ((NPC)victim).AddBuff(BuffID.OnFire, 120);
                if (everburningCursedCandleBuff) ((NPC)victim).AddBuff(BuffID.CursedInferno, 120);
                if (everfrozenCandleBuff) ((NPC)victim).AddBuff(BuffID.Frostburn, 120);
                if (everburningShadowflameCandleBuff) ((NPC)victim).AddBuff(BuffID.ShadowFlame, 60);
            }
            //else if (victim is Player)
        }

        #region CircleUI

        /// <summary>
        /// Contains a list of CircleUIHandlers that are used in CircleUIStart/End in Mod
        /// </summary>
        public List<CircleUIHandler> CircleUIList;

        public override void Initialize()
        {
            //needs to call new List() since Initialize() is called per player in the player select screen
            CircleUIList = new List<CircleUIHandler>
            {
                new CircleUIHandler(
                triggerItem: AssUtils.Instance.ItemType<EverhallowedLantern>(),
                condition: delegate
                {
                    return true;
                },
                uiConf: delegate
                {
                    List<string> tooltips = new List<string>();
                    List<string> toUnlock = new List<string>();
                    List<Texture2D> textures = new List<Texture2D>();
                    List<bool> unlocked = new List<bool>();

                    foreach (SoulType type in Enum.GetValues(typeof(SoulType)))
                    {
                        if (type != SoulType.None)
                        {
                            SoulData data = EverhallowedLantern.GetSoulData(type);
                            textures.Add(Main.projectileTexture[data.ProjType]);
                            unlocked.Add(data.Unlocked());
                            tooltips.Add(data.Tooltip);
                            toUnlock.Add(data.ToUnlock);
                        }
                    }

                    return new CircleUIConf(8, -1, textures, unlocked, tooltips, toUnlock);
                },
                onUIStart: delegate
                {
                    if (Utils.IsPowerOfTwo((int)selectedSoulMinionType))
                    {
                        return (int)Math.Log((int)selectedSoulMinionType, 2);
                    }
                    return 0;
                },
                onUIEnd: delegate
                {
                    selectedSoulMinionType = (SoulType)(byte)Math.Pow(2, CircleUI.returned);
                    AssortedCrazyThings.UIText("Selected: " + EverhallowedLantern.GetSoulData(selectedSoulMinionType).Name, CombatText.HealLife);
                },
                triggerLeft: false
            ),
                new CircleUIHandler(
                triggerItem: AssUtils.Instance.ItemType<SlimeHandlerKnapsack>(),
                condition: delegate
                {
                    return true;
                },
                uiConf: delegate
                {
                    List<Texture2D> textures = new List<Texture2D>() {
                        AssUtils.Instance.GetTexture("Projectiles/Minions/SlimePackMinions/SlimeMinionPreview"),
                        AssUtils.Instance.GetTexture("Projectiles/Minions/SlimePackMinions/SlimeMinionAssortedPreview"),
                        AssUtils.Instance.GetTexture("Projectiles/Minions/SlimePackMinions/SlimeMinionSpikedPreview") };
                    List<string> tooltips = new List<string>
                    {
                        "Default"
                        + "\nBase Damage: " + SlimePackMinion.DefDamage
                        + "\nBase Knockback: " + SlimePackMinion.DefKnockback,
                        "Assorted"
                        + "\nBase Damage: " + SlimePackMinion.DefDamage
                        + "\nBase Knockback: " + SlimePackMinion.DefKnockback,
                        "Spiked"
                        + "\nBase Damage: " + Math.Round(SlimePackMinion.DefDamage * SlimePackMinion.SpikedIncrease)
                        + "\nBase Knockback: " + Math.Round(SlimePackMinion.DefKnockback * SlimePackMinion.SpikedIncrease, 1)
                        + "\nShoots spikes while fighting"
                    };
                    List<string> toUnlock = new List<string>() { "Default", "Default", "Defeat Plantera" };

                    List<bool> unlocked = new List<bool>()
                    {
                        true,                // 0
                        true,                // 1
                        NPC.downedPlantBoss, // 2
                    };

                    return new CircleUIConf(0, -1, textures, unlocked, tooltips, toUnlock);
                },
                onUIStart: delegate
                {
                    return selectedSlimePackMinionType;
                },
                onUIEnd: delegate
                {
                    selectedSlimePackMinionType = (byte)CircleUI.returned;
                    AssortedCrazyThings.UIText("Selected: " + (selectedSlimePackMinionType == 0 ? "Default" : (selectedSlimePackMinionType == 1 ? "Assorted" : "Spiked")), CombatText.HealLife);
                },
                triggerLeft: false
            ),
                new CircleUIHandler(
                triggerItem: AssUtils.Instance.ItemType<DroneController>(),
                condition: delegate
                {
                    return true;
                },
                uiConf: delegate
                {
                    List<string> tooltips = new List<string>();
                    List<string> toUnlock = new List<string>();
                    List<Texture2D> textures = new List<Texture2D>();
                    List<bool> unlocked = new List<bool>();

                    foreach (DroneType type in Enum.GetValues(typeof(DroneType)))
                    {
                        if (type != DroneType.None)
                        {
                            DroneData data = DroneController.GetDroneData(type);
                            textures.Add(AssUtils.Instance.GetTexture(data.PreviewTextureName));
                            unlocked.Add(droneControllerUnlocked.HasFlag(type));
                            tooltips.Add(data.Tooltip);
                            toUnlock.Add("Craft and use a " + data.Name + " Item");
                        }
                    }

                    return new CircleUIConf(0, -1, textures, unlocked, tooltips, toUnlock);
                },
                onUIStart: delegate
                {
                    if (Utils.IsPowerOfTwo((int)selectedDroneControllerMinionType))
                    {
                        return (int)Math.Log((int)selectedDroneControllerMinionType, 2);
                    }
                    return 0;
                },
                onUIEnd: delegate
                {
                    selectedDroneControllerMinionType = (DroneType)(byte)Math.Pow(2, CircleUI.returned);
                    AssortedCrazyThings.UIText("Selected: " + DroneController.GetDroneData(selectedDroneControllerMinionType).Name, CombatText.HealLife);
                },
                triggerLeft: false
            )
            };

            // after filling the list, set the trigger list
            for (int i = 0; i < CircleUIList.Count; i++)
            {
                CircleUIHandler.AddItemAsTrigger(CircleUIList[i].TriggerItem, CircleUIList[i].TriggerLeft);
            }
        }
        #endregion

        private static SpriteEffects GetSpriteEffects(Player player)
        {
            if (player.gravDir == 1f)
            {
                if (player.direction == 1)
                {
                    return SpriteEffects.None;
                }
                else
                {
                    return SpriteEffects.FlipHorizontally;
                }
            }
            else
            {
                if (player.direction == 1)
                {
                    return SpriteEffects.FlipVertically;
                }
                else
                {
                    return SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                }
            }
        }

        public static readonly PlayerLayer CrazyBundleOfAssortedBalloons = new PlayerLayer("AssortedCrazyThings", "CrazyBundleOfAssortedBalloons", PlayerLayer.BalloonAcc, delegate (PlayerDrawInfo drawInfo)
        {
            //Since it's supposed to replace the Autoload texture, the regular _Balloon is just blank
            if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
            {
                return;
            }
            Player drawPlayer = drawInfo.drawPlayer;
            Mod mod = AssUtils.Instance;
            float mountOffset = drawPlayer.mount.PlayerOffset;

            drawInfo.position.Y += (int)mountOffset / 2;
            if (drawPlayer.balloon > 0)
            {
                Texture2D texture = mod.GetTexture("Items/Accessories/Useful/CrazyBundleOfAssortedBalloons_Balloon_Proper");

                Color color = Lighting.GetColor((int)drawPlayer.Center.X / 16, (int)drawPlayer.Center.Y / 16);

                int frameTimeY = DateTime.Now.Millisecond % 800 / 200;
                Vector2 offsetHand = Main.OffsetsPlayerOffhand[drawPlayer.bodyFrame.Y / 56];
                if (drawPlayer.direction != 1)
                {
                    offsetHand.X = drawPlayer.width - offsetHand.X;
                }
                if (drawPlayer.gravDir != 1f)
                {
                    offsetHand.Y -= drawPlayer.height;
                }
                float drawX = (int)drawInfo.position.X + offsetHand.X - Main.screenPosition.X;
                float drawY = (int)drawInfo.position.Y + offsetHand.Y * drawPlayer.gravDir - Main.screenPosition.Y;

                Vector2 stupidOffset = new Vector2(0f, -drawPlayer.bodyFrame.Height / 3); //works without it, but this makes it higher

                Vector2 drawOrigin = new Vector2(26f + drawPlayer.direction * 4, 28f + drawPlayer.gravDir * 6f);

                DrawData drawData = new DrawData(texture, new Vector2(drawX, drawY) + stupidOffset, new Rectangle(0, texture.Height / 4 * frameTimeY, texture.Width, texture.Height / 4), color, drawPlayer.bodyRotation, drawOrigin, 1f, GetSpriteEffects(drawPlayer), 0)
                {
                    shader = drawInfo.balloonShader
                };
                Main.playerDrawData.Add(drawData);
            }
        });

        public static readonly PlayerLayer SlimeHandlerKnapsack = new PlayerLayer("AssortedCrazyThings", "SlimeHandlerKnapsack", PlayerLayer.MiscEffectsBack, delegate (PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }
            Player drawPlayer = drawInfo.drawPlayer;
            Mod mod = AssUtils.Instance;

            if ((drawPlayer.wings == 0 || drawPlayer.velocity.Y == 0f)/* && (drawPlayer.inventory[drawPlayer.selectedItem].type == mod.ItemType<Items.Weapons.SlimeHandlerKnapsack>())*/)
            {
                Texture2D texture = mod.GetTexture("Items/Weapons/SlimeHandlerKnapsack_Back");
                float drawX = (int)drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X;
                float drawY = (int)drawInfo.position.Y + drawPlayer.height - Main.screenPosition.Y;
                
                Vector2 stupidOffset = new Vector2(0f, - drawPlayer.bodyFrame.Height / 2);
                
                Color color = Lighting.GetColor((int)drawPlayer.Center.X / 16, (int)drawPlayer.Center.Y / 16);

                DrawData drawData = new DrawData(texture, new Vector2(drawX, drawY) + drawPlayer.bodyPosition + stupidOffset, drawPlayer.bodyFrame, color, drawPlayer.bodyRotation, drawInfo.bodyOrigin, 1f, GetSpriteEffects(drawPlayer), 0);
                Main.playerDrawData.Add(drawData);
            }
        });

        public static readonly PlayerLayer HarvesterWings = new PlayerLayer("AssortedCrazyThings", "HarvesterWings", PlayerLayer.Wings, delegate (PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }
            Player drawPlayer = drawInfo.drawPlayer;
            Mod mod = AssUtils.Instance;

            if (drawPlayer.wings == mod.GetEquipSlot("HarvesterWings", EquipType.Wings))
            {
                Texture2D texture = mod.GetTexture("Items/Accessories/Useful/HarvesterWings_Wings_Glowmask");
                float drawX = (int)drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X;
                float drawY = (int)drawInfo.position.Y + drawPlayer.height / 2f - Main.screenPosition.Y;

                Vector2 stupidOffset = new Vector2(-9 * drawPlayer.direction + 0 * drawPlayer.direction, 2f * drawPlayer.gravDir + 0 * drawPlayer.gravDir);

                DrawData drawData = new DrawData(texture, new Vector2(drawX, drawY) + stupidOffset, new Rectangle(0, texture.Height / 4 * drawPlayer.wingFrame, texture.Width, texture.Height / 4), Color.White/* * num51 * (1f - shadow) * 0.5f*/, drawPlayer.bodyRotation, new Vector2(texture.Width / 2, texture.Height / 8), 1f, GetSpriteEffects(drawPlayer), 0)
                {
                    shader = drawInfo.wingShader
                };
                Main.playerDrawData.Add(drawData);

                if (drawPlayer.velocity.Y != 0 && drawPlayer.wingFrame != 0)
                {
                    if (Main.rand.NextBool(3))
                    {
                        int dustOffset = 4;
                        if (drawPlayer.direction == 1)
                        {
                            dustOffset = -40;
                        }
                        int dustIndex = Dust.NewDust(new Vector2(drawPlayer.position.X + (drawPlayer.width / 2) + dustOffset, drawPlayer.position.Y + (drawPlayer.height / 2) - 8f), 30, 26, 135, 0f, 0f, 0, default(Color), 1.5f);
                        Main.dust[dustIndex].noGravity = true;
                        Main.dust[dustIndex].noLight = true;
                        Main.dust[dustIndex].velocity *= 0.3f;
                        if (Main.rand.NextBool(5))
                        {
                            Main.dust[dustIndex].fadeIn = 1f;
                        }
                        Main.dust[dustIndex].shader = GameShaders.Armor.GetSecondaryShader(drawPlayer.cWings, drawPlayer);
                    }
                }
            }
        });
        
        public static readonly PlayerLayer SoulSaviorGlowmask = new PlayerLayer("AssortedCrazyThings", "SoulSaviorGlowmask", PlayerLayer.Body, delegate (PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
            {
                return;
            }
            Player drawPlayer = drawInfo.drawPlayer;
            Mod mod = AssUtils.Instance;
            
            if (drawPlayer.body == mod.GetEquipSlot("SoulSaviorPlate", EquipType.Body))
            {
                Texture2D texture = mod.GetTexture("Items/Armor/SoulSaviorPlate_Glowmask");
                float drawX = (int)drawInfo.position.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2 - Main.screenPosition.X;
                float drawY = (int)drawInfo.position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y;

                Vector2 stupidOffset = new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2);

                DrawData drawData = new DrawData(texture, new Vector2(drawX, drawY) + drawPlayer.bodyPosition + stupidOffset, drawPlayer.bodyFrame, Color.White, drawPlayer.bodyRotation, drawInfo.bodyOrigin, 1f, GetSpriteEffects(drawPlayer), 0)
                {
                    shader = drawInfo.bodyArmorShader
                };
                Main.playerDrawData.Add(drawData);

                //Generate visual dust
                if (Main.rand.NextFloat() < 0.1f)
                {
                    Vector2 position = drawPlayer.Center - new Vector2(8f, 0f) + new Vector2(Main.rand.Next(8), Main.rand.Next(8));
                    if (drawPlayer.direction == 1)
                    {
                        position.X += 8f;
                    }
                    Dust dust = Dust.NewDustPerfect(position, 135, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.3f, -0.1f)), 100, Color.White, 0.6f);
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.fadeIn = Main.rand.NextFloat(0.5f, 0.8f);

                    dust.shader = GameShaders.Armor.GetSecondaryShader(drawInfo.bodyArmorShader, drawPlayer);
                }
            }
        });

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            int wingLayer = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Wings"));
            if (player.inventory[player.selectedItem].type == mod.ItemType<SlimeHandlerKnapsack>())
            {
                if (player.velocity.Y == 0f)
                {
                    layers.RemoveAt(wingLayer);
                }
                layers.Insert(wingLayer + 1, SlimeHandlerKnapsack);
            }
            if (wingLayer != -1)
            {
                layers.Insert(wingLayer + 1, HarvesterWings);
            }

            int bodyLayer = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Body"));
            layers.Insert(bodyLayer + 1, SoulSaviorGlowmask);

            int balloonLayer = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("BalloonAcc"));
            if (player.balloon == mod.GetEquipSlot("CrazyBundleOfAssortedBalloons", EquipType.Balloon)) layers.Insert(balloonLayer + 1, CrazyBundleOfAssortedBalloons);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            DecreaseDroneShield(ref damage);

            ResetEmpoweringTimer();

            SpawnSoulTemp();

            if (wyvernCampfire && proj.type == ProjectileID.HarpyFeather)
            {
                player.noKnockback = true;
            }
        }

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            DecreaseDroneShield(ref damage);

            ResetEmpoweringTimer();

            SpawnSoulTemp();
        }

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            ApplyCandleDebuffs(target);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            ApplyCandleDebuffs(target);
        }

        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            //ApplyCandleDebuffs(target);

            target.GetModPlayer<AssPlayer>().ResetEmpoweringTimer();

            target.GetModPlayer<AssPlayer>().SpawnSoulTemp();
        }

        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            //ApplyCandleDebuffs(target);

            target.GetModPlayer<AssPlayer>().ResetEmpoweringTimer();

            target.GetModPlayer<AssPlayer>().SpawnSoulTemp();
        }

        public override void GetWeaponDamage(Item item, ref int damage)
        {
            if (empoweringBuff && !item.summon && damage > 0) damage += (int)(damage * step); //summon damage gets handled in AssGlobalProj
        }

        public override void GetWeaponCrit(Item item, ref int crit)
        {
            if (empoweringBuff) crit += (int)(10 * step);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            //getDefense before teleportHome (so you don't teleport BEFORE you gain the defense)

            if (!GetDefense(damage)) return false;

            if (!TeleportHome(damage)) return false;

            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (getDefenseDuration != 0) damage = 1;

            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
        }

        public override void PostUpdateBuffs()
        {
            UpdateTeleportHomeWhenLow();

            UpdateGetDefenseWhenLow();

            Empower();
        }

        public override void PreUpdate()
        {
            if (wyvernCampfire) player.AddBuff(mod.BuffType<WyvernCampfireBuff>(), 2); //visual only

            if (Main.netMode != NetmodeID.Server && Main.myPlayer == player.whoAmI &&
                player.ownedProjectileCounts[DroneController.GetDroneData(DroneType.Shield).ProjType] < 1) shieldDroneReduction = 0;

            SpawnSoulsWhenHarvesterIsAlive();

            RightClickStatus();

            LeftClickStatus();
        }
    }
}
