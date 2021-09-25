using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eitrix
{
    public partial class DefaultDrawTool : IDrawTool
    {
        static class Layers
        {
            public const float GlowLayer = 0.5f;
            public const float FragmentLayer = 0.4f;
        }

        abstract class Animation
        {
            DefaultDrawTool drawTool;
            public Animation(DefaultDrawTool drawTool)
            {
                this.drawTool = drawTool;
                
            }

            public abstract void DrawMe();
            public bool Finished { get; set; }


            /// ---------------------------------------------------------
            /// <summary>
            /// Draws a little growing puff of smoke
            /// </summary>
            /// ---------------------------------------------------------
            public class Puff : Animation
            {
                TimeWatcher timer = new TimeWatcher(.3);
                Vector2 position;
                float blockScale;
                float rotation;
                float rotationMove;

                public Puff(DefaultDrawTool drawTool, Vector2 position, float blockScale)
                    : base(drawTool)
                {
                    this.blockScale = blockScale;
                    this.rotation = (float)Globals.RandomDouble(0,Math.PI * 2);
                    this.rotationMove = (float)Globals.RandomDouble(-.1, .1);
                    this.position = position;
                }

                public override void DrawMe()
                {
                    float delta = 1 - (float)timer.FractionLeft;
                    if (delta >= 1)
                    {
                        Finished = true;
                        return;
                    }

                    Color color = new Color(1, 1, 1, (float)timer.FractionLeft);
                    Rectangle spriteSource = drawTool.GetSpriteArea(Textures.BrickAndOverlay, 10, 5);
                    float scale = blockScale * 3f * delta;
                    rotation += rotationMove;

                    drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, position, spriteSource, color, rotation, new Vector2(50, 50), scale, SpriteEffects.None, Layers.GlowLayer);
                }
            }
            /// ---------------------------------------------------------
            /// <summary>
            /// Draws the Brick in a special color sequence
            /// </summary>
            /// ---------------------------------------------------------
            public class Highlight : Animation
            {
                TimeWatcher timer = new TimeWatcher(.3);
                Vector2 position;
                float blockScale;

                public Highlight(DefaultDrawTool drawTool, Vector2 position, float blockScale)
                    : base(drawTool)
                {
                    this.blockScale = blockScale;
                    this.position = position;
                }

                public override void DrawMe()
                {
                    float delta = 1 - (float)timer.FractionLeft;
                    if (delta >= 1)
                    {
                        Finished = true;
                        return;
                    }

                    Color color = new Color(1, 1, 1, (float)timer.FractionLeft);
                    Rectangle spriteSource = drawTool.GetSpriteArea(Textures.BrickAndOverlay, 10, 2);
                    float scale = blockScale * 1.4f;

                    drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, position, spriteSource, color, 0, new Vector2(50, 50), scale, SpriteEffects.None, Layers.GlowLayer);
                }
            }
            /// ---------------------------------------------------------
            /// <summary>
            /// Draws the explanding glow on a new brick
            /// </summary>
            /// ---------------------------------------------------------
            public class NewbieGlow : Animation
            {
                TimeWatcher timer = new TimeWatcher(.3);
                Vector2 position;

                public NewbieGlow(DefaultDrawTool drawTool, Vector2 position)
                    : base(drawTool)
                {
                    this.position = position;
                }

                public override void DrawMe()
                {
                    float delta = 1 - (float)timer.FractionLeft;
                    if (delta >= 1)
                    {
                        Finished = true;
                        return;
                    }

                    Color color = new Color(1, 1, 1, (float)timer.FractionLeft * .7f);
                    Rectangle spriteSource = drawTool.GetSpriteArea(Textures.BrickAndOverlay, 10, 3);
                    float scale = 1 + delta * 2;

                    drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, position, spriteSource, color, 0, new Vector2(50, 50), scale, SpriteEffects.None, Layers.GlowLayer);
                }
            }

            /// ---------------------------------------------------------
            /// <summary>
            /// Draws an exploding brick
            /// </summary>
            /// ---------------------------------------------------------
            public class ExplodeBrick : Animation
            {
                Vector2 position;
                float scale;
                List<Fragment> fragments = new List<Fragment>();
                Color color;


                public ExplodeBrick(DefaultDrawTool drawTool, Vector2 position, float scale, Color color)
                    : base(drawTool)
                {
                    this.position = position;
                    this.scale = scale;
                    this.color = color;
                    for (int i = 0; i < 30; i++)
                    {
                        double theta = i * Math.PI / 15 + (Globals.rand.NextDouble() - 0.5)/3;

                        Fragment newFragment = new Fragment()
                        {
                            FragmentId = i % 10,
                            Position = position,
                            Velocity = new Vector2((float)Math.Cos(theta) * 2, (float)Math.Sin(theta) - 2) * 3 * (float)Globals.rand.NextDouble() ,
                            RotationVelocity = (float)(Globals.rand.NextDouble() - 0.5)/2
                        };

                        fragments.Add(newFragment);
                    }
                }

                public override void DrawMe()
                {
                    if (Finished) return;
                    int livingFragments = 0;
                    foreach (Fragment fragment in fragments)
                    {
                        fragment.MoveMe();
                        if (fragment.Position.Y > drawTool.Height) continue;

                        livingFragments++;
                        Rectangle spriteSource = drawTool.GetSpriteArea(Textures.BrickAndOverlay, 10, 90 + fragment.FragmentId);
                            
                        drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, fragment.Position, spriteSource, color, fragment.Rotation, new Vector2(50, 50), scale, SpriteEffects.None, Layers.FragmentLayer);

                    }

                    if (livingFragments == 0) Finished = true;

                }

                class Fragment
                {
                    public Vector2 Position;
                    public Vector2 Velocity;
                    public int FragmentId;
                    public float Rotation;
                    public float RotationVelocity;

                    TimeWatcher myTimer = new TimeWatcher();
                    double lastElapsed = 0;

                    public void MoveMe()
                    {
                        Rotation += RotationVelocity;
                        Position += Velocity;
                        float delta = (float)(myTimer.ElapsedSeconds - lastElapsed);
                        Velocity += new Vector2(0, delta * 20);
                        lastElapsed = myTimer.ElapsedSeconds;
                    }
                }
            }

            /// ---------------------------------------------------------
            /// <summary>
            /// Draws an a small shower of sparks
            /// </summary>
            /// ---------------------------------------------------------
            public class SparkShower : Animation
            {
                Vector2 position;
                float scale;
                List<Spark> sparks = new List<Spark>();


                public SparkShower(DefaultDrawTool drawTool, Vector2 position, float scale)
                    : base(drawTool)
                {
                    this.position = position;
                    this.scale = scale;
                    for (int i = 0; i < 50; i++)
                    {
                        double theta = i * Math.PI / 15 + (Globals.rand.NextDouble() - 0.5) / 3;

                        Spark newSpark = new Spark()
                        {
                            Position = position,
                            Velocity = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * 3 * (float)Globals.rand.NextDouble(),
                        };

                        sparks.Add(newSpark);
                    }
                }

                public override void DrawMe()
                {
                    if (Finished) return;
                    int livingSparks = 0;
                    foreach (Spark spark in sparks)
                    {
                        spark.MoveMe();
                        if (spark.Position.Y > drawTool.Height || spark.Timer.Expired) continue;


                        livingSparks++;
                        Rectangle spriteSource = drawTool.GetSpriteArea(Textures.BrickAndOverlay, 10, 6);

                        drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, spark.Position, spriteSource, Color.Yellow, 0, new Vector2(50, 50), (float)(scale * 0.3f * spark.Timer.FractionLeft), SpriteEffects.None, Layers.FragmentLayer);
                        drawTool.spriteBatch.Draw(Textures.BrickAndOverlay, spark.Position, spriteSource, Color.White, 0, new Vector2(50, 50), (float)(scale * 0.1f * spark.Timer.FractionLeft), SpriteEffects.None, Layers.FragmentLayer);

                    }

                    if (livingSparks == 0) Finished = true;

                }

                class Spark
                {
                    public Vector2 Position;
                    public Vector2 Velocity;

                    public TimeWatcher Timer = new TimeWatcher();
                    double lastElapsed = 0;

                    public Spark()
                    {
                        Timer = new TimeWatcher(Globals.RandomDouble(.3, .7));
                    }

                    public void MoveMe()
                    {
                        Position += Velocity;
                        float delta = (float)(Timer.ElapsedSeconds - lastElapsed);
                        Velocity += new Vector2(0, delta * 20);
                        lastElapsed = Timer.ElapsedSeconds;
                    }
                }
            }

        }

    }
}
