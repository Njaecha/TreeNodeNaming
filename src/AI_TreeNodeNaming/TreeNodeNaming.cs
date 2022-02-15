using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using KKAPI;
using Studio;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using ExtensibleSaveFormat;
using MessagePack;

namespace TreeNodeNaming
{
    [BepInProcess("StudioNEOV2")]
    public partial class TreeNodeNaming : BaseUnityPlugin
    {
    }

}
