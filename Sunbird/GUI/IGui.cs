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
using Sunbird.Decorations;
using Sunbird.Serialization;
using System.ComponentModel;

namespace Sunbird.GUI
{
    public interface IGui
    {
        List<KeyValuePair<Sprite, DeferAction>> DeferredOverlay { get; set; }
    }

    public enum DeferAction
    {
        Add,
        Remove,
    }
}
