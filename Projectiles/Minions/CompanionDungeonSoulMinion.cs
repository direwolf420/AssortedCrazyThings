﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AssortedCrazyThings.Projectiles.Minions
{
    public class CompanionDungeonSoulMinion : ModProjectile
    {
        //change damage here, reminder that there are three minions so you are effectively tripling the damage
        public static int Damage = 7;
        private int sincounter;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Companion Soul");
            Main.projFrames[projectile.type] = 4; //4
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = false;
            ProjectileID.Sets.Homing[projectile.type] = true;
            //ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.Spazmamini);
            projectile.width = 16;
            projectile.height = 28;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.minion = true; //only determines the damage type
            projectile.minionSlots = 0f;
            projectile.penetrate = -1;
        }

        public override bool PreAI()
        {
            Player player = Main.player[projectile.owner];
            return true;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public void Draw()
        {
            if(projectile.ai[0] == 2f)
            {
                projectile.rotation = projectile.velocity.X * 0.05f;
            }
            else
            {
                //projectile.rotation = projectile.velocity.ToRotation() + 3.14159274f;
                //+= projectile.velocity.X * 0.05f; makes it rotate around itself faster depending on its velo.x
                projectile.rotation = projectile.velocity.X * -0.05f;
            }

            projectile.frameCounter++;
            if (projectile.frameCounter >= 4)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
            }
            if (projectile.frame > 3)
            {
                projectile.frame = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float sinY = -10f;
            if (Main.hasFocus)  //here since we override the AI, we can use the projectiles own frame and frameCounter in Draw()
            {
                Draw();
                sincounter = sincounter > 120 ? 0 : sincounter + 1;
                sinY = (float)((Math.Sin((sincounter / 120f) * 2 * Math.PI) - 1) * 10);
            }

            Lighting.AddLight(projectile.Center, new Vector3(0.15f, 0.15f, 0.35f));

            SpriteEffects effects = SpriteEffects.None;
            Texture2D image = Main.projectileTexture[projectile.type];
            Rectangle bounds = new Rectangle
            {
                X = 0,
                Y = projectile.frame,
                Width = image.Bounds.Width,
                Height = (int)(image.Bounds.Height / 4)
            };
            bounds.Y *= bounds.Height; //cause proj.frame only contains the frame number

            //Generate visual dust
            if (Main.rand.NextFloat() < 0.015f)
            {
                Vector2 position = new Vector2(projectile.position.X + projectile.width / 2, projectile.position.Y);
                Dust dust = Dust.NewDustPerfect(position, 135, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -1f)), 100, new Color(255, 255, 255), 1f);
                dust.noGravity = false;
                dust.noLight = true;
                dust.fadeIn = Main.rand.NextFloat(0.8f, 1.1f);
            }

            Vector2 stupidOffset = new Vector2(projectile.width / 2, (projectile.height - 10f) + sinY);

            spriteBatch.Draw(image, projectile.position - Main.screenPosition + stupidOffset, bounds, Color.White * 0.78f, projectile.rotation, bounds.Size() / 2, projectile.scale, effects, 0f);
        }

        public override void AI()
        {
            //projectile.ai[0] == 0 : no targets found
            //projectile.ai[0] == 1 : noclipping to player
            //projectile.ai[0] == 2 : target found, attacking

            Player player = Main.player[projectile.owner];
            AssPlayer modPlayer = player.GetModPlayer<AssPlayer>(mod);
            if (player.dead)
            {
                modPlayer.soulArmorMinions = false;
            }
            if (modPlayer.soulArmorMinions)
            {
                projectile.timeLeft = 2;
            }

            //if(projectile.localAI[0] == 0)
            //{
            //    sincounter = (projectile.whoAmI * 37) % 180;
            //    projectile.localAI[0]++;
            //}

            float distance1 = 700f;
            float distance2playerfaraway = 800f;
            float distance2playerfarawayWhenHasTarget = 1200f;
            float num633 = 150f;

            float overlapVelo = 0.04f; //0.05
            int someIndex;
            for (int i = 0; i < 1000; i++)
            {
                //fix overlap with other minions
                if (((i != projectile.whoAmI && Main.projectile[i].active && Main.projectile[i].owner == projectile.owner) & true) && Math.Abs(projectile.position.X - Main.projectile[i].position.X) + Math.Abs(projectile.position.Y - Main.projectile[i].position.Y) < (float)projectile.width)
                {
                    if (projectile.position.X < Main.projectile[i].position.X)
                    {
                        projectile.velocity.X = projectile.velocity.X - overlapVelo;
                    }
                    else
                    {
                        projectile.velocity.X = projectile.velocity.X + overlapVelo;
                    }
                    if (projectile.position.Y < Main.projectile[i].position.Y)
                    {
                        projectile.velocity.Y = projectile.velocity.Y - overlapVelo;
                    }
                    else
                    {
                        projectile.velocity.Y = projectile.velocity.Y + overlapVelo;
                    }
                }
                someIndex = i;
            }
            bool flag23 = false;
            if (projectile.ai[0] == 2f) //attack mode

            {
                projectile.ai[1] += 1f;
                projectile.extraUpdates = 1;

                if (projectile.ai[1] > 40f)
                {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] = 0f;
                    projectile.extraUpdates = 0;
                    projectile.numUpdates = 0;
                    projectile.netUpdate = true;
                }
                else
                {
                    flag23 = true;
                }
            }
            if (!flag23)
            {
                Vector2 vector40 = projectile.position;
                bool foundTarget = false;
                if (projectile.ai[0] != 1f)
                {
                    projectile.tileCollide = true;
                }
                if (projectile.tileCollide && WorldGen.SolidTile(Framing.GetTileSafely((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16)))
                {
                    projectile.tileCollide = false;
                }
                NPC ownerMinionAttackTargetNPC3 = projectile.OwnerMinionAttackTargetNPC;
                if (ownerMinionAttackTargetNPC3 != null && ownerMinionAttackTargetNPC3.CanBeChasedBy(this))
                {
                    float between = Vector2.Distance(ownerMinionAttackTargetNPC3.Center, projectile.Center);
                    if (((Vector2.Distance(projectile.Center, vector40) > between && between < distance1) || !foundTarget) && Collision.CanHitLine(projectile.position, projectile.width, projectile.height, ownerMinionAttackTargetNPC3.position, ownerMinionAttackTargetNPC3.width, ownerMinionAttackTargetNPC3.height))
                    {
                        distance1 = between;
                        vector40 = ownerMinionAttackTargetNPC3.Center;
                        foundTarget = true;
                    }
                }
                if (!foundTarget)
                {
                    for (int j = 0; j < 200; j = someIndex + 1)
                    {
                        NPC nPC2 = Main.npc[j];
                        if (nPC2.CanBeChasedBy(this))
                        {
                            float num644 = Vector2.Distance(nPC2.Center, projectile.Center);
                            if (((Vector2.Distance(projectile.Center, vector40) > num644 && num644 < distance1) || !foundTarget) && Collision.CanHitLine(projectile.position, projectile.width, projectile.height, nPC2.position, nPC2.width, nPC2.height))
                            {
                                distance1 = num644;
                                vector40 = nPC2.Center;
                                foundTarget = true;
                            }
                        }
                        someIndex = j;
                    }
                }
                float distanceNoclip = distance2playerfaraway;
                if (foundTarget)
                {
                    distanceNoclip = distance2playerfarawayWhenHasTarget;
                }
                if (Vector2.Distance(player.Center, projectile.Center) > distanceNoclip)
                {
                    projectile.ai[0] = 1f;
                    projectile.tileCollide = false;
                    projectile.netUpdate = true;
                }
                if (foundTarget && projectile.ai[0] == 0f)//idek
                {
                    Vector2 value16 = vector40 - projectile.Center;
                    float num646 = value16.Length();
                    value16.Normalize();
                    if (num646 > 20f) //200f //approach distance to enemy
                    {
                        //if its far away from it
                        //Main.NewText("first " + Main.time);
                        float scaleFactor2 = 6f; //8f
                        float acc1 = 16f; //41f
                        value16 *= scaleFactor2;
                        projectile.velocity = (projectile.velocity * (acc1 - 1) + value16) / acc1;
                    }
                    else //slowdown after a dash
                    {
                        //if its close to the enemy
                        //Main.NewText("second " + Main.time);
                        float scaleFactor3 = 8; //4f
                        float acc2 = 41; //41f
                        value16 *= 0f - scaleFactor3;
                        projectile.velocity = (projectile.velocity * (acc2 - 1) + value16) / acc2;
                    }
                }
                else //!(foundTarget && projectile.ai[0] == 0f)
                {
                    bool isNoclipping = false;
                    if (!isNoclipping)
                    {
                        isNoclipping = projectile.ai[0] == 1f;
                    }

                    float velocityFactor1 = 1f; //6f
                    if (isNoclipping)
                    {
                        velocityFactor1 = 12f; //15f
                    }
                    Vector2 center2 = projectile.Center;
                    Vector2 value17 = player.Center - center2 + new Vector2(0f, -60f);
                    float num649 = value17.Length();
                    if (num649 > 200f && velocityFactor1 < 8f) //8f
                    {
                        velocityFactor1 = 8f; //8f
                    }
                    if (num649 < num633 && isNoclipping && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
                    {
                        projectile.ai[0] = 0f;
                        projectile.netUpdate = true;
                    }
                    if (num649 > 2000f)
                    {
                        projectile.position.X = player.Center.X - (float)(projectile.width / 2);
                        projectile.position.Y = player.Center.Y - (float)(projectile.height / 2);
                        projectile.netUpdate = true;
                    }
                    if (num649 > 70f) //the immediate range around the player (when it passively floats about)
                    {
                        value17.Normalize();
                        value17 *= velocityFactor1;
                        float acc3 = 100f; //41f
                        projectile.velocity = (projectile.velocity * (acc3 - 1) + value17) / acc3;
                    }
                    else if (projectile.velocity.X == 0f && projectile.velocity.Y == 0f)
                    {
                        projectile.velocity.X = -0.15f;
                        projectile.velocity.Y = -0.05f;
                    }
                }

                if (projectile.ai[1] > 0f)
                {
                    projectile.ai[1] += (float)Main.rand.Next(1, 4);
                }
                if (projectile.ai[1] > 40f)
                {
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
                if (projectile.ai[0] == 0f)
                {
                    if ((projectile.ai[1] == 0f & foundTarget) && distance1 < 30f) //500f
                    {
                        projectile.ai[1] += 1f;
                        if (Main.myPlayer == projectile.owner)
                        {
                            //Main.NewText("dash " + Main.time);
                            projectile.ai[0] = 2f;
                            Vector2 value20 = vector40 - projectile.Center;
                            value20.Normalize();
                            projectile.velocity = value20 * 3f; //8f
                            projectile.netUpdate = true;
                        }
                    }
                }
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = true;
            return true;
        }
    }
}