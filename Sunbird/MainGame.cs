﻿using System;
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

namespace Sunbird
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Game
    {
        private GraphicsDeviceManager Graphics { get; set; }
        private SpriteBatch SpriteBatch { get; set; }

        public State CurrentState { get; set; }
        public Config Config { get; set; }
        public Camera Camera { get; set; }
        public SamplerState SamplerState { get; set; }
        public static SpriteFont DefaultFont { get; set; }
        public int Width { get { return Graphics.PreferredBackBufferWidth; } }
        public int Height { get { return Graphics.PreferredBackBufferHeight; } }

        public Texture2D LightingRender { get; set; }
        public RenderTarget2D LightingRenderTarget { get; set; }

        public Texture2D ShadowRender { get; set; }
        public RenderTarget2D ShadowRenderTarget { get; set; }

        public Texture2D GameRender { get; set; }
        public RenderTarget2D GameRenderTarget { get; set; }

        public BlendState Subtractive { get; set; }

        public bool CleanLoad { get; set; } = false;

        public MainGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            if (CleanLoad == true)
            {
                Config = new Config(this);
            }
            else
            {
                Config = Serializer.ReadXML<Config>("Config.xml");
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
            SamplerState = SamplerState.PointClamp;
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");

            if (CleanLoad == true)
            {
                CubeFactory.CubeMetaDataLibrary = new XDictionary<int, CubeMetaData>()
                {
                    {0, new CubeMetaData(){Path = "Temp/GrassCubeTop", SheetRows = 3, SheetColumns = 4, FrameCount = 12, AnimState = AnimationState.None} },
                    {1, new CubeMetaData(){Path = "Temp/LightStoneCubeTop", SheetRows = 1, SheetColumns = 3, FrameCount = 3, AnimState = AnimationState.None} },
                    {2, new CubeMetaData(){Path = "Temp/WaterCubeTop", SheetRows = 1, SheetColumns = 11, FrameCount = 11, AnimState = AnimationState.None} },
                };
                CubeFactory.CubeBaseMetaDataLibrary = new XDictionary<int, CubeBaseMetaData>()
                {
                    {0, new CubeBaseMetaData(){Path = "Temp/EmptyCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                    {1, new CubeBaseMetaData(){Path = "Temp/GrassCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                    {2, new CubeBaseMetaData(){Path = "Temp/LightStoneCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                    {3, new CubeBaseMetaData(){Path = "Temp/WaterCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                };
                // There should be at least one cube in the library.
                CubeFactory.CurrentCubeMetaData = CubeFactory.CubeMetaDataLibrary[0];
                CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeBaseMetaDataLibrary[0];
            }
            else
            {
                CubeFactoryData cubeFactoryData = Serializer.ReadXML<CubeFactoryData>("CubeFactoryData.xml", new Type[] { typeof(CubeMetaData), typeof(CubeBaseMetaData) });
                cubeFactoryData.SyncOut();
            }

            GameRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            true,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);

            ShadowRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            true,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);

            Subtractive = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceColor,

                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.InverseSourceColor
            };

            Exiting += MainGame_Exiting;

            base.Initialize();
        }

        private void MainGame_Exiting(object sender, EventArgs e)
        {
            var cubeFactoryData = new CubeFactoryData();
            cubeFactoryData.SyncIn();
            cubeFactoryData.Serialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // CORE: Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

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

            if (Peripherals.KeyTapped(Keys.C))
            {
                var i = (int)Camera.CurrentMode + 1;
                if (i >= Enum.GetNames(typeof(CameraMode)).Length - 1)
                {
                    i = 0;
                }
                Camera.CurrentMode = (CameraMode)(i);
                Debug.Print(Camera.CurrentMode.ToString());
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
            if (World.ZoomRatio > 1)
            {
                SamplerState = SamplerState.PointClamp;
            }

            // Game Render
            GraphicsDevice.SetRenderTarget(GameRenderTarget);
            GraphicsDevice.Clear(Color.LightGray);

            // Primary batch
            SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            CurrentState.Draw(gameTime, SpriteBatch);

            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GameRender = GameRenderTarget;

            // Shadow Render
            GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            CurrentState.DrawShadow(gameTime, SpriteBatch);

            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            ShadowRender = ShadowRenderTarget;

            // Game Render Texture
            SpriteBatch.Begin();
            SpriteBatch.Draw(GameRender, Vector2.Zero, Color.White);
            SpriteBatch.End();

            // Shadow Render Texture
            SpriteBatch.Begin(blendState: Subtractive);
            SpriteBatch.Draw(ShadowRender, Vector2.Zero, Color.White);
            SpriteBatch.End();

            // Overlay batch
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            CurrentState.DrawOverlay(gameTime, SpriteBatch);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
       
    }
}
