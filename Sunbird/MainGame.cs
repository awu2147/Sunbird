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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public State CurrentState { get; set; }
        public Config Config { get; set; }
        public Peripherals Peripherals { get; set; }
        public Camera Camera { get; set; }
        public SamplerState SamplerState { get; set; }
        public SpriteFont DefaultFont { get; set; }
        public int Width { get { return graphics.PreferredBackBufferWidth; } }
        public int Height { get { return graphics.PreferredBackBufferHeight; } }

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            //IsFixedTimeStep = true;

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            Templates.InitializeTemplates();
            //Config = new Config(this);
            Config = Serializer.ReadXML<Config>("Config.xml");
            Config.LoadContent(this);

            Peripherals = new Peripherals();
            Camera = new Camera(this);
            SamplerState = SamplerState.PointClamp;
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //CurrentState = new LoadingScreen(this, GraphicsDevice, Content);
            CurrentState = new GameState1(this, GraphicsDevice, Content);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            // TODO: Add your update logic here
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

            spriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            // TODO: Add your drawing code here
            CurrentState.Draw(gameTime, spriteBatch);

            spriteBatch.End();
            if (Camera.CurrentMode == CameraMode.Follow)
            {
                spriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState.PointClamp);
                if (CurrentState is GameState1)
                {
                    var a = CurrentState as GameState1;
                    spriteBatch.DrawString(DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(Camera).ToString()}", Peripherals.GetCornerWorldPosition(Camera, 10, 10).ToVector2(), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Player World Position {a.Player.Position.ToString()}", Peripherals.GetCornerWorldPosition(Camera, 10, 30).ToVector2(), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Normalized Position {World.TopFace_NormalizedPoint(Peripherals.GetMouseWorldPosition(Camera))}", Peripherals.GetCornerWorldPosition(Camera, 10, 50).ToVector2(), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Grid Coords {World.TopFace_PointToGridCoord(Peripherals.GetMouseWorldPosition(Camera))}", Peripherals.GetCornerWorldPosition(Camera, 10, 70).ToVector2(), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Coords {World.TopFace_PointToCoord(Peripherals.GetMouseWorldPosition(Camera))}", Peripherals.GetCornerWorldPosition(Camera, 10, 90).ToVector2(), Color.Black);
                }
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                if (CurrentState is GameState1)
                {
                    var a = CurrentState as GameState1;
                    spriteBatch.DrawString(DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(Camera).ToString()}", new Vector2(10,10), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Player World Position {a.Player.Position.ToString()}", new Vector2(10, 30), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Normalized Position {World.TopFace_NormalizedPoint(Peripherals.GetMouseWorldPosition(Camera))}", new Vector2(10, 50), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Grid Coords {World.TopFace_PointToGridCoord(Peripherals.GetMouseWorldPosition(Camera))}", new Vector2(10, 70), Color.Black);
                    spriteBatch.DrawString(DefaultFont, $"Coords {World.TopFace_PointToCoord(Peripherals.GetMouseWorldPosition(Camera))}", new Vector2(10, 90), Color.Black);
                }
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
       
    }
}
