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

namespace Sunbird
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public State CurrentState { get; set; }
        public Config Config { get; set; }
        public Camera Camera { get; set; }
        public SamplerState SamplerState { get; set; }
        public SpriteFont DefaultFont { get; set; }
        public int Width { get { return graphics.PreferredBackBufferWidth; } }
        public int Height { get { return graphics.PreferredBackBufferHeight; } }
        public bool CleanLoad { get; set; } = false;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
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

            graphics.PreferredBackBufferWidth = Config.WindowWidth;
            graphics.PreferredBackBufferHeight = Config.WindowHeight;
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
                    {0, new CubeMetaData(){Path = "Temp/GrassCube", SheetRows = 1, SheetColumns = 3, FrameCount = 3, AnimState = AnimationState.None} },
                    {1, new CubeMetaData(){Path = "Temp/WaterCube"} },
                    {2, new CubeMetaData(){Path = "Temp/LavaCubeSheet", SheetRows = 1, SheetColumns = 4, FrameCount = 4, FrameSpeed = 0.5f, AnimState = AnimationState.Loop} },
                    {3, new CubeMetaData(){Path = "Temp/StoneCube", SheetRows = 1, SheetColumns = 3, FrameCount = 3, AnimState = AnimationState.None} }
                };
                // There should be at least one cube in the library.
                CubeFactory.CurrentCubeMetaData = CubeFactory.CubeMetaDataLibrary[0];
            }
            else
            {
                CubeFactoryData cubeFactoryData = Serializer.ReadXML<CubeFactoryData>("CubeFactoryData.xml", new Type[] { typeof(CubeMetaData) });
                cubeFactoryData.SyncOut();
            }

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
            spriteBatch = new SpriteBatch(GraphicsDevice);

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
                if (i >= Enum.GetNames(typeof(CameraMode)).Length)
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
            GraphicsDevice.Clear(Color.LightGray);

            if (World.ZoomRatio > 1)
            {
                SamplerState = SamplerState.PointClamp;
            }
            // Primary batch
            spriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            CurrentState.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Overlay batch
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            CurrentState.DrawOverlay(gameTime, spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
       
    }
}
