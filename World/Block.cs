using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eitrix
{
    ///------------------------------------------------------------------------------
    /// <summary>
    /// Different animations for blocks
    /// </summary>
    ///------------------------------------------------------------------------------
    public enum AnimationType
    {
        None,
        Newbie,
        ExplodeMe,
        Highlight,
    }

    ///------------------------------------------------------------------------------
    /// <summary>
    /// Code for blocks
    /// </summary>
    ///------------------------------------------------------------------------------
    public class Block
    {
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int ColorIndex { get; set; }
        public bool NeedsPuff { get; set; }
        public Vector2 FreezeDriedOffset { get; set; }

        private AnimationType animationType;
        public AnimationType AnimationType 
        {
            get { return animationType; }
            set
            {
                animationType = value;
                AnimationStarted = false;
            }
        }
        public bool AnimationStarted { get; set; }

        private SpecialType specialType;
        public SpecialType SpecialType
        {
            get 
            {
                if (SpecialTimer.Expired) specialType = SpecialType.None;
                return specialType; 
            }
            set
            {
                specialType = value;
                SpecialTimer = new TimeWatcher(Globals.Options.SpecialLifetimeSeconds);
            }
        }

        public TimeWatcher SpecialTimer { get; set; }

        const double clearingTimeSeconds = .4;
        TimeWatcher clearingTimer;

        private bool clearing;
        public bool Clearing
        {
            get { return clearing; }
            set
            {
                if (value && clearing != value)
                {
                    clearingTimer = new TimeWatcher(clearingTimeSeconds);
                }
                clearing = value;
            }
        }


        /// <summary>
        /// When the block is clearing, this represent how much is has cleared
        /// </summary>
        public double FractionLeft
        {
            get
            {
                return clearingTimer.FractionLeft;
            }           
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        public Block(int offsetX, int offsetY, int colorIndex)
        {
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            ColorIndex = colorIndex;
            SpecialType = SpecialType.None;
            AnimationType = AnimationType.None;
            AnimationStarted = false;
            this.FreezeDriedOffset = new Vector2((float)Globals.RandomDouble(.1, .5), (float)Globals.RandomDouble(.1, .5));
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        public void ActivateSpecial(Player owner, World world)
        {
            Special.ActivateSpecial(SpecialType, owner, world);
        }
    }
}
