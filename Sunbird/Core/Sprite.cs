using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using System.Xml.Schema;

namespace Sunbird.Core
{
    public enum Alignment
    {
        TopLeft,
        TopRight,
        Center,
        BottomLeft,
        BottomRight
    }

    [Serializable]
    public class Sprite
    {
        public Animator Animator { get; set; }
        public float Alpha { get; set; } = 1f;
        public Vector2 Position { get; set; }
        public Vector2 PositionOffset { get; set; }
        public Coord Coords { get; set; }
        public int Altitude { get; set; }
        public int DrawAltitude { get { return Altitude + DrawPriority; } }
        public int DrawPriority { get; set; }
        public bool IsHidden { get; set; }

        [XmlIgnore]
        public Texture2D Shadow { get; set; }
        public string ShadowPath { get; set; }

        [XmlIgnore]
        public Texture2D AntiShadow { get; set; }
        public string AntiShadowPath { get; set; }

        [XmlIgnore]
        public Texture2D SelfShadow { get; set; }


        public Sprite() { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet) : this(mainGame, spriteSheet, Vector2.Zero) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position) : this(mainGame, spriteSheet, position, Alignment.TopLeft, null) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment) : this(mainGame, spriteSheet, position, alignment, null) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, AnimArgs animArgs) : this(mainGame, spriteSheet, Vector2.Zero, Alignment.TopLeft, animArgs) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment, AnimArgs animArgs)
        {
            Animator = new Animator(this, spriteSheet);
            if (animArgs != null)
            {
                ReconfigureAnimator(animArgs);
            }
            GenerateShadowTextures(mainGame, Animator);
            if (alignment == Alignment.TopLeft)
            {
                Position = position;
            }
            else if (alignment == Alignment.TopRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, 0);
            }
            else if (alignment == Alignment.Center)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width / 2, -spriteSheet.Texture.Height / 2);
            }
            else if (alignment == Alignment.BottomLeft)
            {
                Position = position + new Vector2(0, -spriteSheet.Texture.Height);
            }
            else if (alignment == Alignment.BottomRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, -spriteSheet.Texture.Height);
            }
        }

        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            if (Animator != null)
            {
                Animator.LoadContent(mainGame, graphicsDevice, content);
                Animator.Owner = this;
                GenerateShadowTextures(mainGame, Animator);
            }

            if (ShadowPath != null)
            {
                Shadow = content.Load<Texture2D>(ShadowPath);
            }
            if (AntiShadowPath != null)
            {
                AntiShadow = content.Load<Texture2D>(AntiShadowPath);
            }
        }

        /// <summary>
        /// Generate AntiShadow and SelfShadow textures from an Animator.
        /// </summary>
        /// <param name="mainGame"></param>
        public void GenerateShadowTextures(MainGame mainGame, Animator animator)
        {
            AntiShadow = GetAntiShadow(mainGame, animator);
            SelfShadow = GetShadow(mainGame, animator);
        }

        /// <summary>
        /// Replace the SpriteSheet of the default Sprite Animator. This is usually followed by a ReconfigureAnimator method call.
        /// </summary>
        /// <param name="newSheet"></param>
        public void ReplaceSpriteSheet(SpriteSheet newSheet)
        {
            ReplaceSpriteSheet(newSheet, Animator);
        }

        /// <summary>
        /// Replace the SpriteSheet of a specified Animator. This is usually followed by a ReconfigureAnimator method call.
        /// </summary>
        /// <param name="newSheet"></param>
        /// <param name="animator"></param>
        public void ReplaceSpriteSheet(SpriteSheet newSheet, Animator animator)
        {
            animator.SpriteSheet = newSheet;
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator using Current frame = Start frame.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            ReconfigureAnimator(startFrame, startFrame, frameCount, frameSpeed, animState, Animator);
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator. 
        /// </summary>
        public void ReconfigureAnimator(AnimArgs args)
        {
            ReconfigureAnimator(args.StartFrame, args.CurrentFrame, args.FramesInLoop, args.FrameSpeed, args.AnimState);
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator. 
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            ReconfigureAnimator(startFrame, currentFrame, frameCount, frameSpeed, animState, Animator);
        }

        /// <summary>
        /// Reconfigure the specified Animator using Current frame = Start frame.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int frameCount, float frameSpeed, AnimationState animState, Animator animator)
        {
            ReconfigureAnimator(startFrame, startFrame, frameCount, frameSpeed, animState, animator);
        }

        /// <summary>
        /// Reconfigure the specified Animator.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState, Animator animator)
        {
            animator.StartFrame = startFrame;
            animator.CurrentFrame = currentFrame;
            animator.FrameCounter = currentFrame - startFrame;
            animator.FramesInLoop = frameCount;
            animator.FrameSpeed = frameSpeed;
            animator.AnimState = animState;
            animator.Timer.Reset();
        }

        private Texture2D GetMask(MainGame mainGame, Animator animator, Color color)
        {
            var totalPixels = animator.SpriteSheet.Texture.Width * animator.SpriteSheet.Texture.Height;
            Color[] maskPixels = new Color[totalPixels];
            animator.SpriteSheet.Texture.GetData(maskPixels);

            #if DEBUG
            Debug.Assert(maskPixels.Length == totalPixels);
            #endif

            for (int i = 0; i < maskPixels.Length; i++)
            {
                if (maskPixels[i].A != 0)
                {
                    maskPixels[i] = color;
                }
            }
            var mask = new Texture2D(mainGame.GraphicsDevice, animator.SpriteSheet.Texture.Width, animator.SpriteSheet.Texture.Height);
            mask.SetData(maskPixels);
            return mask;
        }

        public Texture2D GetAntiShadow(MainGame mainGame, Animator animator)
        {
            return GetMask(mainGame, animator, Color.Black);
        }

        public Texture2D GetShadow(MainGame mainGame, Animator animator)
        {
            return GetMask(mainGame, animator, new Color(109, 117, 141));
        }

        public virtual void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                Animator.Draw(gameTime, spriteBatch, Alpha);
            }
        }
    }
}
