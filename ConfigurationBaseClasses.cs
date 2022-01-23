﻿using Terraria.ModLoader;

namespace AssortedCrazyThings
{
    //TODO work out some kinda system that detects classes which base is a tml class, and error on mod load
    [Autoload(false)]
    public abstract class AssItem : ModItem
    {

    }

    [Autoload(false)]
    public abstract class AssProjectile : ModProjectile
    {

    }

    [Autoload(false)]
    public abstract class AssBuff : ModBuff
    {

    }

    [Autoload(false)]
    public abstract class AssNPC : ModNPC
    {

    }

    [Autoload(false)]
    public abstract class AssTile : ModTile
    {

    }

    [Autoload(false)]
    public abstract class AssPlayerLayer : PlayerDrawLayer
    {

    }

    [Autoload(false)]
    public abstract class AssPlayerBase : ModPlayer
    {

    }

    [Autoload(false)]
    public abstract class AssSystem : ModSystem
    {

    }

    //ModGore, ModDust

    [Autoload(false)]
    public abstract class AssGlobalNPC : GlobalNPC
    {

    }

    [Autoload(false)]
    public abstract class AssGlobalBuff : GlobalBuff
    {

    }

    [Autoload(false)]
    public abstract class AssGlobalProjectile : GlobalProjectile
    {

    }

    [Autoload(false)]
    public abstract class AssGlobalItem : GlobalItem
    {

    }

    [Autoload(false)]
    public abstract class AssGlobalTile : GlobalTile
    {

    }
}
