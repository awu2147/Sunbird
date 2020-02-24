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

        private SpriteBatch SpriteBatch;
        private SpriteBatch SpriteBatchShadow;
        private SpriteBatch SpriteBatchLighting;
        private SpriteBatch SpriteBatchLightingStencil;

        private Texture2D GameRender;
        private Texture2D ShadowRender;
        private Texture2D LightingRender;
        private Texture2D LightingStencilRender;

        private BlendState Subtractive = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor,
        };

        private Effect LightingStencil;

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
                string dirCubes = "Cubes/";
                CubeFactory.CubeTopMetaDataLibrary = new List<CubeMetaData>()
                {
                    new CubeMetaData(){Path = $"{dirCubes}GrassCubeTop", SheetRows = 3, SheetColumns = 4, FrameCount = 12, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}DirtCubeTop", SheetRows = 1, SheetColumns = 8, FrameCount = 8, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}LightStoneCubeTop", SheetRows = 1, SheetColumns = 3, FrameCount = 3, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}WaterCubeTop", SheetRows = 1, SheetColumns = 11, FrameCount = 11, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}LavaCubeTop", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.Loop, FrameSpeed = 1.333f},
                    new CubeMetaData(){Path = $"{dirCubes}GraveyardGrassCubeTop", SheetRows = 2, SheetColumns = 4, FrameCount = 8, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}GraveyardDirtCubeTop", SheetRows = 1, SheetColumns = 8, FrameCount = 8, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}SandCubeTop", SheetRows = 2, SheetColumns = 4, FrameCount = 8, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}LightWoodBoardCubeTop", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}WoodBoardCubeTop", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}StoneTileCubeTop", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None},
                };
                CubeFactory.CubeBaseMetaDataLibrary = new List<CubeMetaData>
                {
                    new CubeMetaData(){Path = $"{dirCubes}GrassCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}DirtCubeBase", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}LightStoneCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}WaterCubeBase", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}LavaCubeBase", SheetRows = 4, SheetColumns = 3, FrameCount = 11, AnimState = AnimationState.Loop, },
                    new CubeMetaData(){Path = $"{dirCubes}GraveyardGrassCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None, },
                    new CubeMetaData(){Path = $"{dirCubes}GraveyardDirtCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None},
                    new CubeMetaData(){Path = $"{dirCubes}SandCubeBase", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None},
                };

                // Generate Library Textures from Path.
                // Populate master CubeMetaDataLibrary.
                CubeFactory.CubeMetaDataLibrary = new XDictionary<string, CubeMetaData>();
                foreach (var ctmd in CubeFactory.CubeTopMetaDataLibrary)
                {
                    ctmd.LoadContent(this);
                    if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(ctmd.Path))
                    {
                        CubeFactory.CubeMetaDataLibrary.Add(ctmd.Path, ctmd);
                    }
                }
                foreach (var cbmd in CubeFactory.CubeBaseMetaDataLibrary)
                {
                    cbmd.LoadContent(this);
                    if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(cbmd.Path))
                    {
                        CubeFactory.CubeMetaDataLibrary.Add(cbmd.Path, cbmd);
                    }
                }

                // CurrentCubeMetaData should never be null;
                CubeFactory.CurrentCubeTopMetaData = CubeFactory.CubeTopMetaDataLibrary[0];
                CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeBaseMetaDataLibrary[0];

                string dir1x1 = "Decos/1x1/";
                DecoFactory.DecoMetaDataLibrary1x1 = new List<DecoMetaData>()
                {
                    new DecoMetaData() {Path = $"{dir1x1}3/TreeBas", SheetRows = 2, SheetColumns = 3, FrameCount = 6, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -72), Dimensions = new Dimension(1, 1, 3), TypeName = typeof(Deco).FullName },
                    new DecoMetaData() {Path = $"{dir1x1}3/TreeCon", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -72), Dimensions = new Dimension(1, 1, 3), TypeName = typeof(Deco).FullName },
                    new DecoMetaData() {Path = $"{dir1x1}3/TreeConD", SheetRows = 2, SheetColumns = 2, FrameCount = 4, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -72), Dimensions = new Dimension(1, 1, 3), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}3/TreePal", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -72), Dimensions = new Dimension(1, 1, 3), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/FlowersPWY", SheetRows = 2, SheetColumns = 3, FrameCount = 6, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/MushroomsP", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/Logpile", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/GravestoneS", SheetRows = 2, SheetColumns = 2, FrameCount = 4, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/WallLS", SheetRows = 2, SheetColumns = 3, FrameCount = 6, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/WallDS", SheetRows = 2, SheetColumns = 3, FrameCount = 6, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}1/WallRB", SheetRows = 2, SheetColumns = 3, FrameCount = 6, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, 0), Dimensions = new Dimension(1, 1, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}2/GravestoneM", SheetRows = 2, SheetColumns = 2, FrameCount = 4, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -36), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}2/GravestoneMS", SheetRows = 2, SheetColumns = 2, FrameCount = 4, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -36), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}2/WoodenBarrel", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -36), Dimensions = new Dimension(1, 1, 2), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = $"{dir1x1}3/ObeliskS", SheetRows = 2, SheetColumns = 2, FrameCount = 4, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(0, -72), Dimensions = new Dimension(1, 1, 3), TypeName = typeof(Deco).FullName },
                };
                DecoFactory.DecoMetaDataLibrary2x2 = new List<DecoMetaData>()
                {
                    new DecoMetaData(){Path = "Decos/BloodBowl", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(-36, -36), Dimensions = new Dimension(2, 2, 1), TypeName = typeof(Deco).FullName },
                    new DecoMetaData(){Path = "Decos/MediumRock", SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(-12, -6), Dimensions = new Dimension(2, 2, 2), TypeName = typeof(Deco).FullName },
                };
                DecoFactory.DecoMetaDataLibrary3x3 = new List<DecoMetaData>()
                {
                    new DecoMetaData(){Path = "Temp/House", SheetRows = 1, SheetColumns = 2, FrameCount = 2, AnimState = AnimationState.None,
                    PositionOffset = new Vector2(-87, -99), Dimensions = new Dimension(3, 3, 3), TypeName = typeof(Deco).FullName },
                };

                // Generate Library Textures, AntiShadows, and SelfShadows from Path.
                // Populate master DecoMetaDataLibrary.
                DecoFactory.DecoMetaDataLibrary = new XDictionary<string, DecoMetaData>();
                foreach (var dmd in DecoFactory.DecoMetaDataLibrary1x1)
                {
                    dmd.LoadContent(this);
                    if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                    {
                        DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                    }
                }
                foreach (var dmd in DecoFactory.DecoMetaDataLibrary2x2)
                {
                    dmd.LoadContent(this);
                    if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                    {
                        DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                    }
                }
                foreach (var dmd in DecoFactory.DecoMetaDataLibrary3x3)
                {
                    dmd.LoadContent(this);
                    if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                    {
                        DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                    }
                }

                // Assign current decos. CurrentDecoMetaDataNxN should never be null;
                DecoFactory.CurrentDecoMetaData1x1 = DecoFactory.DecoMetaDataLibrary1x1[0];
                DecoFactory.CurrentDecoMetaData2x2 = DecoFactory.DecoMetaDataLibrary2x2[0];
                DecoFactory.CurrentDecoMetaData3x3 = DecoFactory.DecoMetaDataLibrary3x3[0];
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

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteBatchShadow = new SpriteBatch(GraphicsDevice);
            SpriteBatchLighting = new SpriteBatch(GraphicsDevice);
            SpriteBatchLightingStencil = new SpriteBatch(GraphicsDevice);    

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

            // Game Render Texture
            SpriteBatch.Begin();
            SpriteBatch.Draw(GameRender, Vector2.Zero, Color.White);
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

            // Overlay batch
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            CurrentState.DrawOverlay(gameTime, SpriteBatch);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
       
    }
}
