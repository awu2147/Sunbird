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

namespace Sunbird.GUI
{
    public static class GuiHelper
    {
        /// <summary>
        /// Checks if world is in focus. ref bool is set to false if cursor lies on any Overlay sprite.
        /// </summary>
        /// <param name="gui">An object that implements IGui has an Overlay (sprite) list. This list can nest other IGui objects so we do an iterative check.</param>
        public static void CheckFocus(ref bool inFocus, IGui gui)
        {
            foreach (var sprite in gui.Overlay)
            {
                if (sprite.Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()))
                {
                    inFocus = false;
                    break;
                }
                else if (sprite is IGui)
                {
                    CheckFocus(ref inFocus, (IGui)sprite);
                    if (inFocus == false)
                    {
                        break;
                    }
                }
                else
                {
                    inFocus = true;
                }
            }
        }
    }
    public class PartitionArgs
    {
        private ScrollBarContainer ScrollBar { get; set; }
        private Vector2 LocalOffset { get; set; }
        private int ScrollBarSegment { get { return ScrollBar.CurrentSegment - 1; } }

        private Point ColRow { get; set; }
        private int TotalColumns { get { return ColRow.X; } }
        private int TotalRows { get { return ColRow.Y; } }

        private Point ColRowGap { get; set; }
        private int ColumnGap { get { return ColRowGap.X; } }
        private int RowGap { get { return ColRowGap.Y; } }


        private int counter;


        public PartitionArgs(Vector2 localOffset, ScrollBarContainer scrollBar, Point colRow, Point colRowGap)
        {
            LocalOffset = localOffset;
            ScrollBar = scrollBar;
            ColRow = colRow;
            ColRowGap = colRowGap;
        }

        public void NextItemPosition(Sprite item, Vector2 catalogPosition)
        {
            item.Position = catalogPosition + LocalOffset + new Vector2(ColumnGap * (counter % TotalColumns), RowGap * (counter / TotalColumns) - ScrollBarSegment * RowGap);
            if (counter < ScrollBarSegment * TotalColumns || counter >= (ScrollBarSegment + TotalRows) * TotalColumns)
            {
                item.IsHidden = true;
            }
            else
            {
                item.IsHidden = false;
            }
            counter++;
        }

        /// <summary>
        /// This should be called after populating the partition with NextItemPosition calls.
        /// </summary>
        public void RescaleScrollBar()
        {
            if (counter <= TotalColumns * TotalRows)
            {
                ScrollBar.TotalSegments = 1;
            }
            else
            {
                ScrollBar.TotalSegments = ((counter - (TotalColumns * TotalRows)) / TotalColumns) + 2;
            }
        }
    }
}
