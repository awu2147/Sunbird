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
using Sunbird.States;
using Sunbird.Core;
using Sunbird.External;
using Sunbird.Controllers;
using Sunbird.Serialization;
using System.Reflection;
using Sunbird.GUI;
using Sunbird.Decorations;

namespace Sunbird
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Game
    {
        private GraphicsDeviceManager Graphics { get; set; }

        private RenderTarget2D GameRenderTarget;
        private RenderTarget2D ShadowRenderTarget;
        private RenderTarget2D LightingRenderTarget;
        private RenderTarget2D LightingStencilRenderTarget;
        public RenderTarget2D WaterRenderTarget;
        public RenderTarget2D WaterStencilRenderTarget;

        private SpriteBatch SpriteBatch;
        private SpriteBatch SpriteBatchShadow;
        private SpriteBatch SpriteBatchLighting;
        private SpriteBatch SpriteBatchLightingStencil;
        public SpriteBatch SpriteBatchWater;
        public SpriteBatch SpriteBatchWaterStencil;

        private Texture2D GameRender;
        private Texture2D ShadowRender;
        private Texture2D LightingRender;
        private Texture2D LightingStencilRender;
        public Texture2D WaterRender;
        public Texture2D WaterStencilRender;

        private readonly BlendState Subtractive = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor,
        };

        private Effect LightingStencil;
        private Effect WaterStencil;

        private State _CurrentState;
        public State CurrentState
        {
            get { return _CurrentState; }
            set
            {
                bool changed = false;
                if (_CurrentState != value)
                {
                    changed = true;
                }
                _CurrentState = value;
                if (changed)
                {
                    _CurrentState.OnStateChanged();
                }
            }
        }
        public Config Config { get; set; }
        public Camera Camera { get; set; }
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;
        public static SpriteFont DefaultFont { get; set; }
        public int Width { get { return Graphics.PreferredBackBufferWidth; } }
        public int Height { get { return Graphics.PreferredBackBufferHeight; } }

        public bool CleanLoad { get; set; } = false;

        public float Brightness = 0.6f;

        public MainGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Serializer.ExtraTypes = new Type[]
            {
                typeof(Player),
                typeof(Cube),
                typeof(Deco),
                typeof(GhostMarker),
                typeof(Button),
                typeof(House),
            };

            if (CleanLoad == true)
            {
                Config = new Config(this);
            }
            else
            {
                Config = Serializer.ReadXML<Config>(Config.ConfigSerializer, "Config.xml");
                Config.LoadContent(this);
            }

            Graphics.PreferredBackBufferWidth = Config.WindowWidth;
            Graphics.PreferredBackBufferHeight = Config.WindowHeight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Templates.InitializeTemplates();

            Camera = new Camera(this);

            if (CleanLoad == true)
            {
                AssetLibraries.RebuildLibraries(this);
            }
            else
            {
                AssetLibraries.ImportLibraries(this);
            }          

            GameRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            ShadowRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            LightingRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            LightingStencilRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            WaterRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            WaterStencilRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteBatchShadow = new SpriteBatch(GraphicsDevice);
            SpriteBatchLighting = new SpriteBatch(GraphicsDevice);
            SpriteBatchLightingStencil = new SpriteBatch(GraphicsDevice);
            SpriteBatchWater = new SpriteBatch(GraphicsDevice);
            SpriteBatchWaterStencil = new SpriteBatch(GraphicsDevice);

            Exiting += MainGame_Exiting;

            base.Initialize();

        }

        private void MainGame_Exiting(object sender, EventArgs e)
        {
            var cubeFactoryData = new CubeFactoryData();
            cubeFactoryData.SyncIn();
            cubeFactoryData.Serialize();

            var decoFactoryData = new DecoFactoryData();
            decoFactoryData.SyncIn();
            decoFactoryData.Serialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");
            LightingStencil = Content.Load<Effect>("Effects/LightingStencil");
            WaterStencil = Content.Load<Effect>("Effects/WaterStencil");

            CurrentState = new MapBuilder(this, GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Peripherals.PreUpdate();

            CurrentState.Update(gameTime);

            if (Peripherals.KeyTapped(Keys.C) && IsActive)
            {
                var i = (int)Camera.CurrentMode + 1;
                if (i >= Enum.GetNames(typeof(CameraMode)).Length - 1)
                {
                    i = 0;
                }
                Camera.CurrentMode = (CameraMode)(i);
            }
            Camera.Update();

            Peripherals.PostUpdate();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (World.ZoomRatio > 1) { SamplerState = SamplerState.PointClamp; }

            if (CurrentState is ILoadingScreen)
            {
                SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

                GraphicsDevice.SetRenderTarget(null);

                CurrentState.Draw(gameTime, SpriteBatch, SpriteBatchShadow, SpriteBatchLighting, SpriteBatchLightingStencil);

                SpriteBatch.End();
            }
            else
            {
                // Primary batch
                SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
                SpriteBatchShadow.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
                SpriteBatchLighting.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
                SpriteBatchLightingStencil.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
                SpriteBatchWater.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
                SpriteBatchWaterStencil.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

                // Game Render
                GraphicsDevice.SetRenderTarget(GameRenderTarget);
                GraphicsDevice.Clear(Color.LightGray);

                CurrentState.Draw(gameTime, SpriteBatch, SpriteBatchShadow, SpriteBatchLighting, SpriteBatchLightingStencil);

                SpriteBatch.End();

                GameRender = GameRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Shadow Render
                GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
                GraphicsDevice.Clear(Color.Black);

                SpriteBatchShadow.End();

                ShadowRender = ShadowRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Lighting Render
                GraphicsDevice.SetRenderTarget(LightingRenderTarget);
                GraphicsDevice.Clear(CurrentState.CurrentLightingColor);

                SpriteBatchLighting.End();

                LightingRender = LightingRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Lighting Stencil Render
                GraphicsDevice.SetRenderTarget(LightingStencilRenderTarget);
                GraphicsDevice.Clear(CurrentState.CurrentLightingColor);

                SpriteBatchLightingStencil.End();

                LightingStencilRender = LightingStencilRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Water Render
                GraphicsDevice.SetRenderTarget(WaterRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                SpriteBatchWater.End();

                WaterRender = WaterRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Water Stencil Render
                GraphicsDevice.SetRenderTarget(WaterStencilRenderTarget);
                GraphicsDevice.Clear(Color.Black);

                SpriteBatchWaterStencil.End();

                WaterStencilRender = WaterStencilRenderTarget;
                GraphicsDevice.SetRenderTarget(null);

                // Game Render Texture
                SpriteBatch.Begin();
                SpriteBatch.Draw(GameRender, Vector2.Zero, Color.White);
                SpriteBatch.End();

                // Water Render Texture (Subtractive)
                WaterStencil.Parameters["WaterRender"].SetValue(WaterRender);
                WaterStencil.Parameters["WaterStencilRender"].SetValue(WaterStencilRender);
                WaterStencil.Parameters["Brightness"].SetValue(Brightness);

                SpriteBatch.Begin(blendState: Subtractive, effect: WaterStencil);
                SpriteBatch.Draw(WaterRender, Vector2.Zero, Color.White);
                SpriteBatch.End();

                // Shadow Render Texture (Subtractive)
                SpriteBatch.Begin(blendState: Subtractive);
                SpriteBatch.Draw(ShadowRender, Vector2.Zero, Color.White);
                SpriteBatch.End();

                LightingStencil.Parameters["LightingRender"].SetValue(LightingRender);
                LightingStencil.Parameters["LightingStencilRender"].SetValue(LightingStencilRender);
                LightingStencil.Parameters["CurrentLighting"].SetValue(CurrentState.CurrentLightingColor.ToVector4());

                // Lighting Render Texture (Subtractive)
                SpriteBatch.Begin(blendState: Subtractive, effect: LightingStencil);
                SpriteBatch.Draw(LightingRender, Vector2.Zero, Color.White);
                SpriteBatch.End();
            }

            // Overlay batch
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            CurrentState.DrawOverlay(gameTime, SpriteBatch);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
       
    }
}
