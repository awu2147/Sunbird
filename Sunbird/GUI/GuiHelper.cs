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
}
