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
        private SpriteBatch SpriteBatch { get; set; }
        private SpriteBatch SpriteBatchShadow { get; set; }
        private SpriteBatch SpriteBatchLighting { get; set; }
        private SpriteBatch SpriteBatchLightingStencil { get; set; }

        public State CurrentState { get; set; }
        public Config Config { get; set; }
        public Camera Camera { get; set; }
        public SamplerState SamplerState { get; set; }
        public static SpriteFont DefaultFont { get; set; }
        public int Width { get { return Graphics.PreferredBackBufferWidth; } }
        public int Height { get { return Graphics.PreferredBackBufferHeight; } }

        public Texture2D GameRender;
        public Texture2D ShadowRender;
        public Texture2D LightingRender;
        public Texture2D LightingStencilRender;

        public RenderTarget2D GameRenderTarget;
        public RenderTarget2D ShadowRenderTarget; 
        public RenderTarget2D LightingRenderTarget;
        public RenderTarget2D LightingStencilRenderTarget;

        public BlendState Subtractive;

        Effect effect;

        public bool CleanLoad { get; set; } = false;

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
            SamplerState = SamplerState.PointClamp;
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");

            if (CleanLoad == true)
            {
                CubeFactory.CubeMetaDataLibrary = new XDictionary<int, CubeMetaData>()
                {
                    {0, new CubeMetaData(){Path = "Cubes/GrassCubeTop", SheetRows = 3, SheetColumns = 4, FrameCount = 12, AnimState = AnimationState.None} },
                    {1, new CubeMetaData(){Path = "Cubes/DirtCubeTop", SheetRows = 1, SheetColumns = 8, FrameCount = 8, AnimState = AnimationState.None} },
                    {2, new CubeMetaData(){Path = "Cubes/LightStoneCubeTop", SheetRows = 1, SheetColumns = 3, FrameCount = 3, AnimState = AnimationState.None} },
                    {3, new CubeMetaData(){Path = "Cubes/WaterCubeTop", SheetRows = 1, SheetColumns = 11, FrameCount = 11, AnimState = AnimationState.None} },
                    {4, new CubeMetaData(){Path = "Cubes/LavaCubeTop", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.Loop, FrameSpeed = 1.333f} },
                };
                // CurrentCubeMetaData should never be null;
                CubeFactory.CurrentCubeMetaData = CubeFactory.CubeMetaDataLibrary[0];
                // Generate Library Textures from Path;
                foreach (var cMD in CubeFactory.CubeMetaDataLibrary)
                {
                    cMD.Value.LoadContent(this);
                }

                CubeFactory.CubeBaseMetaDataLibrary = new XDictionary<int, CubeBaseMetaData>()
                {
                    {0, new CubeBaseMetaData(){Path = "Cubes/GrassCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                    {1, new CubeBaseMetaData(){Path = "Cubes/DirtCubeBase", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None} },
                    {2, new CubeBaseMetaData(){Path = "Cubes/LightStoneCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None} },
                    {3, new CubeBaseMetaData(){Path = "Cubes/WaterCubeBase", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None} },
                    {4, new CubeBaseMetaData(){Path = "Cubes/LavaCubeBase", SheetRows = 4, SheetColumns = 3, FrameCount = 11, AnimState = AnimationState.Loop, } },
                };
                // CurrentCubeBaseMetaData should never be null;
                CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeBaseMetaDataLibrary[0];
                // Generate Library Textures from Path;
                foreach (var cbMD in CubeFactory.CubeBaseMetaDataLibrary)
                {
                    cbMD.Value.LoadContent(this);
                }

                DecoFactory.DecoMetaDataLibrary = new XDictionary<int, DecoMetaData>()
                {
                    {0, new DecoMetaData(){Path = "Temp/Tree1", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                        PositionOffset = new Vector2(0, -48), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName } },
                    {1, new DecoMetaData(){Path = "Temp/MediumRock", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                        PositionOffset = new Vector2(-12, -6), Dimensions = new Dimension(2, 2, 2), TypeName = typeof(Deco).FullName } },
                    {2, new DecoMetaData(){Path = "Temp/House", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                        PositionOffset = new Vector2(-87, -99), Dimensions = new Dimension(3, 3, 3), TypeName = typeof(Deco).FullName } },
                    {3, new DecoMetaData(){Path = "Temp/Tree2", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                        PositionOffset = new Vector2(0, -36), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName } },
                    {4, new DecoMetaData(){Path = "Temp/Flower1", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                        PositionOffset = new Vector2(0, 36), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName } },
                };
                // CurrentDecoMetaDataNxN should never be null;
                DecoFactory.CurrentDecoMetaData1x1 = DecoFactory.DecoMetaDataLibrary[0];
                DecoFactory.CurrentDecoMetaData2x2 = DecoFactory.DecoMetaDataLibrary[1];
                DecoFactory.CurrentDecoMetaData3x3 = DecoFactory.DecoMetaDataLibrary[2];
                // Generate Library Textures, AntiShadows, and SelfShadows from Path;
                foreach (var dMD in DecoFactory.DecoMetaDataLibrary)
                {
                    dMD.Value.LoadContent(this);
                }
            }
            else
            {
                CubeFactoryData cubeFactoryData = Serializer.ReadXML<CubeFactoryData>(CubeFactoryData.CubeFactoryDataSerializer, "CubeFactoryData.xml");
                cubeFactoryData.SyncOut(this);

                DecoFactoryData decoFactoryData = Serializer.ReadXML<DecoFactoryData>(DecoFactoryData.DecoFactoryDataSerializer, "DecoFactoryData.xml");
                decoFactoryData.SyncOut(this);
            }

            GameRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            ShadowRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            LightingRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);
            LightingStencilRenderTarget = GraphicsHelper.NewRenderTarget2D(GraphicsDevice);

            Subtractive = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceColor,
                //AlphaSourceBlend = Blend.Zero,
                //AlphaDestinationBlend = Blend.InverseSourceColor
            };

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
            // CORE: Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteBatchShadow = new SpriteBatch(GraphicsDevice);
            SpriteBatchLighting = new SpriteBatch(GraphicsDevice);
            SpriteBatchLightingStencil = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("Test");

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

            // Primary batch
            SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            SpriteBatchShadow.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            SpriteBatchLighting.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            SpriteBatchLightingStencil.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            // Game Render
            GraphicsDevice.SetRenderTarget(GameRenderTarget);
            GraphicsDevice.Clear(Color.LightGray);

            CurrentState.Draw(gameTime, SpriteBatch, SpriteBatchShadow, SpriteBatchLighting, SpriteBatchLightingStencil);

            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GameRender = GameRenderTarget;

            // Shadow Render
            GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatchShadow.End();

            GraphicsDevice.SetRenderTarget(null);
            ShadowRender = ShadowRenderTarget;

            // Lighting Render
            GraphicsDevice.SetRenderTarget(LightingRenderTarget);
            GraphicsDevice.Clear(CurrentState.CurrentLightingColor);

            SpriteBatchLighting.End();

            GraphicsDevice.SetRenderTarget(null);
            LightingRender = LightingRenderTarget;

            // Lighting Stencil Render
            GraphicsDevice.SetRenderTarget(LightingStencilRenderTarget);
            GraphicsDevice.Clear(CurrentState.CurrentLightingColor);

            SpriteBatchLightingStencil.End();

            GraphicsDevice.SetRenderTarget(null);
            LightingStencilRender = LightingStencilRenderTarget;

            //GraphicsHelper.ApplyStencil(LightingRender, LightingStencilRender, CurrentState.CurrentLightingColor);

            //// Shadow Render
            //GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
            //GraphicsDevice.Clear(Color.Black);

            //SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            //CurrentState.DrawShadow(gameTime, SpriteBatch);

            //SpriteBatch.End();

            //GraphicsDevice.SetRenderTarget(null);
            //ShadowRender = ShadowRenderTarget;

            //// Lighting Render
            //GraphicsDevice.SetRenderTarget(LightingRenderTarget); 

            //SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            //CurrentState.DrawLighting(gameTime, SpriteBatch);

            //SpriteBatch.End();

            //GraphicsDevice.SetRenderTarget(null);
            //LightingRender = LightingRenderTarget;

            //// Lighting Stencil Render
            //GraphicsDevice.SetRenderTarget(LightingStencilRenderTarget);

            //SpriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);

            //CurrentState.DrawLightingStencil(gameTime, SpriteBatch);

            //SpriteBatch.End();

            //GraphicsDevice.SetRenderTarget(null);
            //LightingStencilRender = LightingStencilRenderTarget;

            //GraphicsHelper.ApplyStencil(LightingRender, LightingStencilRender, CurrentState.CurrentLightingColor);

            // Game Render Texture
            SpriteBatch.Begin();
            SpriteBatch.Draw(GameRender, Vector2.Zero, Color.White);
            SpriteBatch.End();

            // Shadow Render Texture (Subtractive)
            //SpriteBatch.Begin(blendState: Subtractive);
            //SpriteBatch.Draw(ShadowRender, Vector2.Zero, Color.White);
            //SpriteBatch.End();

            effect.Parameters["LightingRender"].SetValue(LightingRender);
            effect.Parameters["LightingStencilRender"].SetValue(LightingStencilRender);
            effect.Parameters["CurrentLighting"].SetValue(CurrentState.CurrentLightingColor.ToVector4());

            // Lighting Render Texture (Subtractive)
            SpriteBatch.Begin(blendState: Subtractive, effect: effect);
            SpriteBatch.Draw(LightingRender, Vector2.Zero, Color.White);
            SpriteBatch.End();

            // Overlay batch
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            CurrentState.DrawOverlay(gameTime, SpriteBatch);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
       
    }
}
