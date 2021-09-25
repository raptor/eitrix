using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eitrix
{
    public enum SpecialType
    {
        None = -1,
        Speedup = 0,
        Escalator,
        SlowDown,
        Jumble,
        Psycho,
        Antidote,
        TheWall,
        SeeShadows,
        Bridge,
        EvilPieces,
        CrazyIvan,
        Shackle,
        TowerOfEit,
        SwitchScreens,
        FreezeDried,
        Transparency,
        NumberOfSpecials
    }

    ///------------------------------------------------------------------------------
    /// <summary>
    /// All specials are here
    /// </summary>
    ///------------------------------------------------------------------------------
    public abstract class Special
    {
        public abstract SpecialType SpecialType { get; }
        public TimeWatcher Time = new TimeWatcher(Globals.Options.PowerLifetimeSeconds);

        bool firstTime = true;
        bool FirstTime
        {
            get 
            {
                bool returnMe = firstTime;
                firstTime = false;
                return returnMe; 
            }
        }

        Player Victim
        {
            get { return world.Players[owner.VictimId]; }
        }

        Player CooperativeVictim
        {
            get { return Globals.Options.CooperativePlay ? Victim : owner; }
        }

        Player afflictedVictim;
        Player owner;
        World world;
        public virtual bool Finished { get; set; }
        public Special(Player owner, World world)
        {
            Finished = false;
            this.owner = owner;
            this.world = world;
        }

        public abstract void Update();

        internal static void ActivateSpecial(SpecialType SpecialType, Player owner, World world)
        {
            if (SpecialType == SpecialType.None) return;

            Player victim = world.Players[owner.VictimId];
            Special newSpecial = null;

            switch (SpecialType)
            {
                case SpecialType.Speedup: newSpecial = new Special.Speedup(owner, world); break;
                case SpecialType.Escalator: newSpecial = new Special.Escalator(owner, world); break;
                case SpecialType.SlowDown: newSpecial = new Special.Slowdown(owner, world); break;
                case SpecialType.Jumble: newSpecial = new Special.Jumble(owner, world); break;
                case SpecialType.Psycho: newSpecial = new Special.Psycho(owner, world); break;
                case SpecialType.Antidote: newSpecial = new Special.Antidote(owner, world); break;
                case SpecialType.TheWall: newSpecial = new Special.TheWall(owner, world); break;
                case SpecialType.SeeShadows: newSpecial = new Special.SeeShadows(owner, world); break;
                case SpecialType.Bridge: newSpecial = new Special.Bridge(owner, world); break;
                case SpecialType.EvilPieces: newSpecial = new Special.EvilPieces(owner, world); break;
                case SpecialType.CrazyIvan: newSpecial = new Special.CrazyIvan(owner, world); break;
                case SpecialType.Shackle: newSpecial = new Special.Shackle(owner, world); break;
                case SpecialType.TowerOfEit: newSpecial = new Special.TowerOfEit(owner, world); break;
                case SpecialType.SwitchScreens: newSpecial = new Special.SwitchScreens(owner, world); break;
                case SpecialType.FreezeDried: newSpecial = new Special.FreezeDried(owner, world); break;
                case SpecialType.Transparency: newSpecial = new Special.Transparency(owner, world); break;
                default: newSpecial = null; break;
            }

            if (newSpecial == null) return;
            newSpecial.AddToPlayer();

        }

        internal virtual void AddToPlayer()
        {
            AddAttackToPlayer();
        }

        internal virtual void RemoveFromPlayer()
        {
            
        }

        public void AddAttackToPlayer()
        {
            if (Victim != owner && Victim != null) Victim.AttackWith(this);
            owner.LastAttackTimer = new TimeWatcher(1);
        }

        public delegate void GeneralAction();
        public void AddAfflictionToPlayer(GeneralAction doAction)
        {
            if (Victim == null) return;
            if (Victim == owner) Victim.Cure(this);
            else
            {
                Victim.AfflictWith(this);
                doAction();
                owner.LastAttackTimer = new TimeWatcher(1);
            }
        }

        /// ##########################################################################################################
        /// ##########################################################################################################
        /// ##########################################################################################################
        /// ##########################################################################################################

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Crazy Ivan
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Transparency : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.Transparency; } }

            public Transparency(Player owner, World world)
                : base(owner, world)
            {

            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                AddAfflictionToPlayer(() =>
                {
                    world.AudioTool.PlaySound(SoundEffectType.Attack09);
                    afflictedVictim = Victim;
                    afflictedVictim.Transparency = true;
                });
            }

            internal override void RemoveFromPlayer()
            {
                afflictedVictim.Transparency = false;
            }

        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Crazy Ivan
        /// </summary>
        ///------------------------------------------------------------------------------
        public class FreezeDried : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.FreezeDried; } }

            public FreezeDried(Player owner, World world)
                : base(owner, world)
            {

            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                AddAfflictionToPlayer(() =>
                {
                    world.AudioTool.PlaySound(SoundEffectType.Attack07);
                    afflictedVictim = Victim;
                    afflictedVictim.FreezeDried = true;
                });
            }

            internal override void RemoveFromPlayer()
            {
                afflictedVictim.FreezeDried = false;
            }

        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Switch Screens
        /// </summary>
        ///------------------------------------------------------------------------------
        public  class SwitchScreens : Special
        {
            TimeWatcher brickTimer;
            protected int x;
            protected double secondsPerRow = 0.1;

            public override SpecialType SpecialType { get { return SpecialType.SwitchScreens; } }

            public SwitchScreens(Player owner, World world)
                : base(owner, world)
            {
                x = 0;
                brickTimer = new TimeWatcher(0);
            }

            public override void Update()
            {
                if (brickTimer.Expired && !Finished)
                {
                    for (int y = 0; y < Victim.Grid.Height; y++)
                    {
                        Block victimBlock = Victim.Grid[x, y];
                        Block ownerBlock = owner.Grid[x, y];

                        Victim.Grid[x, y] = ownerBlock;
                        owner.Grid[x, y] = victimBlock;
                        if (ownerBlock != null) ownerBlock.AnimationType = AnimationType.Highlight;
                        if (victimBlock != null) victimBlock.AnimationType = AnimationType.Highlight;
                    }

                    brickTimer = new TimeWatcher(secondsPerRow);

                    world.AudioTool.PlaySound(SoundEffectType.Dot);

                    x++;
                    if (x >= Victim.Grid.Width ) Finished = true;
                }
            }
        }



        ///------------------------------------------------------------------------------
        /// <summary>
        /// Crazy Ivan
        /// </summary>
        ///------------------------------------------------------------------------------
        public class CrazyIvan : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.CrazyIvan; } }

            public CrazyIvan(Player owner, World world)
                : base(owner, world)
            {

            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                AddAfflictionToPlayer(() =>
                    {
                        world.AudioTool.PlaySound(SoundEffectType.Attack13_CrazyLaugh);
                        afflictedVictim = Victim;
                        afflictedVictim.CrazyIvan = true;

                    });
            }

            internal override void RemoveFromPlayer()
            {
                afflictedVictim.CrazyIvan = false;
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// EvilPieces
        /// </summary>
        ///------------------------------------------------------------------------------
        public class EvilPieces : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.EvilPieces; } }

            public EvilPieces(Player owner, World world)
                : base(owner, world)
            {

            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                AddAfflictionToPlayer(() =>
                    {
                        world.AudioTool.PlaySound(SoundEffectType.Attack11_Trombone);
                        afflictedVictim = Victim;
                        afflictedVictim.EvilPieces = true;

                    });
            }

            internal override void RemoveFromPlayer()
            {
                afflictedVictim.EvilPieces = false;
                Victim.ClearNextPieceBuffer();
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Bridge
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Bridge : Special
        {
            int topY;
            int x;
            int[] skipx = new int[2];
            TimeWatcher brickTimer;
            int blockType;

            public override SpecialType SpecialType { get { return SpecialType.Bridge; } }

            public Bridge(Player owner, World world)
                : base(owner, world)
            {
                brickTimer = new TimeWatcher(0);
                blockType = owner.Number;
            }

            public override void Update()
            {
                if (FirstTime)
                {
                    int j = 0;
                    bool foundTop = false;
                    for (; j < Victim.Grid.Height - 1; j++)
                    {
                        for (int i = 0; i < Victim.Grid.Width; i++)
                        {
                            if (Victim.Grid[i, j] != null)
                            {
                                foundTop = true;
                                break;
                            }
                        }
                        if (foundTop) break;
                    }
                    topY = j - 1;
                    skipx[0] = Globals.rand.Next(Victim.Grid.Width);
                    skipx[1] = Globals.rand.Next(Victim.Grid.Width);
                }

                if (brickTimer.Expired && !Finished)
                {
                    brickTimer = new TimeWatcher(0.12);

                    for (int j = 0; j < 2; j++ )
                    {
                        int y = topY - j;
                        if (y < 0) break;
                        
                        if (x == skipx[j])
                        {
                            if (Victim.Grid[x, y] != null) Victim.Grid[x, y].AnimationType = AnimationType.ExplodeMe;
                        }
                        else
                        {
                            Victim.Grid[x, y] = new Block(x, y, blockType);
                            Victim.Grid[x, y].AnimationType = AnimationType.Newbie;
                        }
                    }


                    world.AudioTool.PlaySound(SoundEffectType.Smack01, 1, 0, 0);
                    x++;
                    if (x >= Victim.Grid.Width) Finished = true;
                }
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// See Shadows
        /// </summary>
        ///------------------------------------------------------------------------------
        public class SeeShadows : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.SeeShadows; } }

            public SeeShadows(Player owner, World world)
                : base(owner, world)
            {
            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                CooperativeVictim.SeeShadows = true;
                world.AudioTool.PlaySound(SoundEffectType.Trans01);
                Finished = true;
            }
        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// The Wall
        /// </summary>
        ///------------------------------------------------------------------------------
        public class TheWall : ShapeDraw
        {
            List<string> shape;

            protected override string[] Shape { get { return shape.ToArray(); } }
            public override SpecialType SpecialType { get { return SpecialType.TheWall; } }

            public TheWall(Player owner, World world)
                : base(owner, world)
            {
                shape = new List<string>();
                for (int i = 0; i < 8; i++)
                {
                    char[] line = new char[Victim.Grid.Width];
                    for(int x = 0; x < Victim.Grid.Width; x++) line[x] = '#';
                    line[Globals.rand.Next(Victim.Grid.Width)] = '-';
                    shape.Add(new string(line));
                }
            }

            protected override void PlaySound()
            {
                world.AudioTool.PlaySound(SoundEffectType.Attack04, 1, 0, 0);
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Speedup
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Speedup : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.Speedup; } }

            public Speedup(Player owner, World world)
                : base(owner, world)
            {

            }

            public override void Update()
            {
                Victim.DropTickIntervalSeconds *= 0.6;
                world.AudioTool.PlaySound(SoundEffectType.Speedup);
                Finished = true;
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Slowdown
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Slowdown : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.SlowDown; } }

            public Slowdown(Player owner, World world)
                : base(owner, world)
            {
            }

            public override void Update()
            {
            }

            internal override void AddToPlayer()
            {
                CooperativeVictim.DropTickIntervalSeconds *= 1.3;
                world.AudioTool.PlaySound(SoundEffectType.Slowdown);
                Finished = true;
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// TowerOfEit
        /// </summary>
        ///------------------------------------------------------------------------------
        public class TowerOfEit : ShapeDraw
        {
            string[] shape = new string[]
            {
                "-#-#--#-#-",
                "-########-",
                "-########-",
                "--##-###--",
                "--##-###--",
                "--####-#--",
                "--####-#--",
                "--#-####--",
                "--#-####--",
                "--###-##--",
                "--###-##--",
                "--######--",
            };

            protected override string[] Shape { get { return shape; } }
            public override SpecialType SpecialType { get { return SpecialType.TowerOfEit; } }

            public TowerOfEit(Player owner, World world)
                : base(owner, world)
            {

            }

            internal override void AddToPlayer()
            {
                blockType = 18;
                base.AddToPlayer();
            }
            protected override void PlaySound()
            {
                world.AudioTool.PlaySound(SoundEffectType.Smack07_Slam, 1, 0, 0);
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Shackle
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Shackle : ShapeDraw
        {
            string[] shape = new string[]
            {
                "..........",
                "..-####-..",
                "..#----#..",
                ".#-....-#.",
                "#-......-#",
                "#-......-#",
                "#-......-#",
                "#-......-#",
                ".#-....-#.",
                "..#----#..",
                "..-####-..",
            };

            protected override string[] Shape { get { return shape; } }
            public override SpecialType SpecialType { get { return SpecialType.Shackle; } }

            public Shackle(Player owner, World world)
                : base(owner, world)
            {

            }

            protected override void PlaySound()
            {
                world.AudioTool.PlaySound(SoundEffectType.Smack03a, 1, 0 + (float)(Math.Abs(y) * .05), 0);
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Escalator
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Escalator : ShapeDraw
        {
            string[] shape = new string[]
            {
                ".........#",
                "........#-",
                ".......#-.",
                "......#-..",
                ".....#-...",
                "....#-....",
                "...#-.....",
                "..#-......",
                ".#-.......",
                "#-........",
            };

            protected override string[] Shape { get { return shape; } }
            public override SpecialType SpecialType { get { return SpecialType.Escalator; } }

            public Escalator(Player owner, World world)
                : base(owner, world)
            {

            }

            protected override void PlaySound()
            {
                world.AudioTool.PlaySound(SoundEffectType.Smack00, 1, 0 + (float)(Math.Abs(y) * .05), 0);
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Generic shape drawing attack
        /// </summary>
        ///------------------------------------------------------------------------------
        public abstract class ShapeDraw : Special
        {
            TimeWatcher brickTimer;
            protected int y;
            protected int blockType;
            protected double secondsPerRow = 0.1;
            protected bool reverse;

            protected abstract string[] Shape {get;}

            public ShapeDraw(Player owner, World world)
                : base(owner, world)
            {
                y = 0;
                brickTimer = new TimeWatcher(0);
                blockType = owner.Number;
                reverse = Globals.rand.Next(2) == 0;
            }

            protected abstract void PlaySound();

            public override void Update()
            {
                if (brickTimer.Expired && !Finished)
                {
                    int gridY = Victim.Grid.Height - 1 - y;
                    string line = Shape[Shape.Length - 1 - y];
                    int bricksSet = 0;
                    for (int x = 0; x < Victim.Grid.Width; x++)
                    {
                        int shapeSourceX = x;
                        if (reverse) shapeSourceX = line.Length - 1 - x;
                        if (x >= line.Length) break;
                        switch (line[shapeSourceX])
                        {
                            case '#':
                                Victim.Grid[x, gridY] = new Block(x, y, blockType);
                                Victim.Grid[x, gridY].AnimationType = AnimationType.Newbie;
                                bricksSet++;
                                break;
                            case '-':
                                if (Victim.Grid[x, gridY] != null) Victim.Grid[x, gridY].AnimationType = AnimationType.ExplodeMe;
                                break;
                        }

                    }

                    brickTimer = new TimeWatcher(secondsPerRow);

                    if (bricksSet > 0)
                    {
                        PlaySound();
                    }

                    y++;
                    if (y >= Shape.Length) Finished = true;
                }
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Jumble
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Jumble : Special
        {
            int x, y;
            int tryCount;
            TimeWatcher brickTimer;

            public override SpecialType SpecialType { get { return SpecialType.Jumble; } }

            public Jumble(Player owner, World world)
                : base(owner, world)
            {
                x = 0;
                y = 0;
                brickTimer = new TimeWatcher(0);
            }

            public override void Update()
            {
                
                if (brickTimer.Expired && !Finished)
                {
                    tryCount++;
                    brickTimer = new TimeWatcher(0.015);
                    x = Globals.rand.Next(Victim.Grid.Width);
                    y = Globals.rand.Next(Victim.Grid.Height);
                    if (Victim.Grid[x, y] != null)
                    {
                        List<IntPoint> emptyPoints = new List<IntPoint>();
                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                int xx = x + i;
                                int yy = y + j;
                                if (xx >= 0 && xx < Victim.Grid.Width &&
                                    yy >= 0 && yy < Victim.Grid.Height &&
                                    Victim.Grid[xx, yy] == null)
                                {
                                    emptyPoints.Add(new IntPoint(xx, yy));
                                }
                            }
                        }

                        if (emptyPoints.Count > 0)
                        {
                            IntPoint point = emptyPoints[Globals.rand.Next(emptyPoints.Count)];
                            Victim.Grid[x, y].AnimationType = AnimationType.Highlight;
                            Victim.Grid[point.X, point.Y] = Victim.Grid[x, y];
                            Victim.Grid[x, y] = null;
                            world.AudioTool.PlaySound(SoundEffectType.Dot02, 1, 0 + (float)(Globals.RandomPitch(0.2)), 0);
                        }
                    }



                    tryCount++;
                    if (tryCount > 200) Finished = true;
                }
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Psycho- Visual screw up the colors as the play progresses
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Psycho : Special
        {
            Piece lastPiece = null;
            public Psycho(Player owner, World world) : base(owner, world) {}

            public override SpecialType SpecialType { get { return SpecialType.Psycho; } }

            public override bool Finished
            {
                get
                {
                    return base.Finished;
                }
                set
                {
                    if (value)
                    {
                        Victim.PsychoColors = null;
                        Victim.PsychoOverlayGrid = null;
                    }

                    base.Finished = value;
                }
            }

            public override void Update()
            {
                if (Victim.CurrentPiece == null) return;
                if (lastPiece != Victim.CurrentPiece)
                {
                    lastPiece = Victim.CurrentPiece;
                    Victim.PsychoColors = new List<object>();

                    if (Victim.PsychoOverlayGrid == null)
                    {
                        Victim.PsychoOverlayGrid = new int[Victim.Grid.Width, Victim.Grid.Height];
                    }

                    int randomSkew = Globals.rand.Next(Globals.PsychoColorCount);
                    for (int x = 0; x < Victim.Grid.Width; x++)
                    {
                        for (int y = 0; y < Victim.Grid.Height; y++)
                        {
                            Victim.PsychoOverlayGrid[x, y] ^= randomSkew;
                        }
                    }
                }

                Victim.CurrentPiece.ProcessBlocks((block, realx, realy) =>
                    {
                        if (realx >= 0 && realy >= 0 && realx < Victim.Grid.Width && realy <= Victim.Grid.Height)
                        {
                            Victim.PsychoOverlayGrid[realx, realy] = block.ColorIndex;
                        }
                    });
            }

            internal override void AddToPlayer()
            {
                AddAfflictionToPlayer(() =>
                    {
                        world.AudioTool.PlaySound(SoundEffectType.Attack10_Yell, 1, .5f, 0);
                    });
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Antidote Cure everything
        /// </summary>
        ///------------------------------------------------------------------------------
        public class Antidote : Special
        {
            public override SpecialType SpecialType { get { return SpecialType.Antidote; } }

            public Antidote(Player owner, World world)
                : base(owner, world)
            {
            }

            public override void Update()
            {
                if (FirstTime)
                {
                    Time.Reset();
                    world.AudioTool.PlaySound(SoundEffectType.Wooahh02_Chior_oohs);
                }
                
                owner.Cure(null);
                
                if(Time.Expired) Finished = true;
            }

            internal override void AddToPlayer()
            {
                CooperativeVictim.AddWeapon(this);
                world.AudioTool.PlaySound(SoundEffectType.Trans03_Chimes);
            }
        }
    }
}
