using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using Sunbird.Serialization;
using Sunbird.GUI;

namespace Sunbird.External
{
    public static class AssetLibraries
    {
        private static string dirCubes = "Cubes/";
        private static string dir1x1 = "Decos/1x1/";

        public static void RebuildLibraries(MainGame mainGame)
        {
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

            // Generate library Textures from Path and populate CubeMetaDataLibrary (Dictionary).
            CubeFactory.CubeMetaDataLibrary = new XDictionary<string, CubeMetaData>();
            foreach (var ctmd in CubeFactory.CubeTopMetaDataLibrary)
            {
                ctmd.LoadContent(mainGame);
                if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(ctmd.Path))
                {
                    CubeFactory.CubeMetaDataLibrary.Add(ctmd.Path, ctmd);
                }
            }
            foreach (var cbmd in CubeFactory.CubeBaseMetaDataLibrary)
            {
                cbmd.LoadContent(mainGame);
                if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(cbmd.Path))
                {
                    CubeFactory.CubeMetaDataLibrary.Add(cbmd.Path, cbmd);
                }
            }

            // Set current CubeMetaDatas.
            CubeFactory.CurrentCubeTopMetaData = CubeFactory.CubeTopMetaDataLibrary[0];
            CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeBaseMetaDataLibrary[0];

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

            // Generate library Textures, AntiShadows, and SelfShadows from Path and populate master DecoMetaDataLibrary (Dictionary).
            DecoFactory.DecoMetaDataLibrary = new XDictionary<string, DecoMetaData>();
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary1x1)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary2x2)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary3x3)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }

            // Set current DecoMetaDatas.
            DecoFactory.CurrentDecoMetaData1x1 = DecoFactory.DecoMetaDataLibrary1x1[0];
            DecoFactory.CurrentDecoMetaData2x2 = DecoFactory.DecoMetaDataLibrary2x2[0];
            DecoFactory.CurrentDecoMetaData3x3 = DecoFactory.DecoMetaDataLibrary3x3[0];
        }

        /// <summary>
        /// Import library data from XML files.
        /// </summary>
        /// <param name="mainGame"></param>
        public static void ImportLibraries(MainGame mainGame)
        {
            CubeFactoryData cubeFactoryData = Serializer.ReadXML<CubeFactoryData>(CubeFactoryData.CubeFactoryDataSerializer, "CubeFactoryData.xml");
            cubeFactoryData.SyncOut(mainGame);

            DecoFactoryData decoFactoryData = Serializer.ReadXML<DecoFactoryData>(DecoFactoryData.DecoFactoryDataSerializer, "DecoFactoryData.xml");
            decoFactoryData.SyncOut(mainGame);
        }

    }
}
