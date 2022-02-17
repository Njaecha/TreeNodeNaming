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
    [BepInDependency(ExtendedSave.GUID, ExtendedSave.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("CharaStudio")]
    public class TreeNodeNaming : BaseUnityPlugin
    {
        public const string PluginName = "TreeNodeNaming";
        public const string GUID = "org.njaecha.plugins.treenodenaming";
        public const string Version = "1.1.1";

        internal new static ManualLogSource Logger;

        internal static bool uiActive = false;

        private StringBuilder inputStringBuilder = new StringBuilder();

        private Dictionary<TreeNodeObject, string> oldTnoNames = new Dictionary<TreeNodeObject, string>();
        private string cursor = " ";
        private int cursorPosition;

        private ConfigEntry<KeyboardShortcut> hotkey;

        private TreeNodeCtrl treeNodeCtrl;


        void Awake()
        {

            TreeNodeNaming.Logger = base.Logger;
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(GUID);
            KeyboardShortcut defaultShortcut = new KeyboardShortcut(KeyCode.R, KeyCode.LeftShift);
            hotkey = Config.Bind("General", "Hotkey", defaultShortcut, "Press this key to rename selected objects");
            KKAPI.Studio.StudioAPI.StudioLoadedChanged += registerCtrls;
        }

        private void registerCtrls(object sender, EventArgs e)
        {
            treeNodeCtrl = Singleton<Studio.Studio>.Instance.treeNodeCtrl;
        }
        void OnGUI()
        {
            if (uiActive)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Escape:
                            CancelInvoke();
                            foreach (TreeNodeObject tno in oldTnoNames.Keys)
                                tno.textName = oldTnoNames[tno];
                            uiActive = false;
                            break;
                        case KeyCode.Return:
                            CancelInvoke();
                            renameCurrentItems(inputStringBuilder.ToString());
                            uiActive = false;
                            break;
                        case KeyCode.Backspace:
                            if (inputStringBuilder.Length > 0)
                            {
                                inputStringBuilder.Remove(cursorPosition - 1, 1);
                                cursorPosition--;
                                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
                            }
                            break;
                        case KeyCode.Delete:
                            if (inputStringBuilder.Length > 0 && cursorPosition != inputStringBuilder.Length)
                            {
                                inputStringBuilder.Remove(cursorPosition, 1);
                                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
                            }
                            break;
                        case KeyCode.LeftArrow:
                            if (cursorPosition > 0)
                            {
                                cursorPosition--;
                                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
                            }
                            break;
                        case KeyCode.RightArrow:
                            if (cursorPosition < inputStringBuilder.Length)
                            {
                                cursorPosition++;
                                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
                            }
                            break;
                        default:
                            int a = inputStringBuilder.Length;
                            inputStringBuilder.Insert(cursorPosition, Input.inputString);
                            if (a != inputStringBuilder.Length)
                            {
                                cursorPosition++;
                                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
                            }
                            break;
                    }
                }
                Input.ResetInputAxes();
            }
        }

        void Update()
        {
            if (hotkey.Value.IsDown())
            {
                if (treeNodeCtrl.selectNode == null) return;
                inputStringBuilder.Remove(0, inputStringBuilder.Length);
                inputStringBuilder.Append(treeNodeCtrl.selectNode.textName);
                cursorPosition = inputStringBuilder.Length;
                oldTnoNames.Clear();
                foreach (ObjectCtrlInfo oci in KKAPI.Studio.StudioAPI.GetSelectedObjects())
                {
                    oldTnoNames[oci.treeNodeObject] = oci.treeNodeObject.textName;
                }

                uiActive = !uiActive;
                InvokeRepeating("cursorBlinking", 0f, 0.5f);
            }
        }

        private void cursorBlinking()
        {
            if (uiActive)
            {
                if (cursor == "¦") cursor = "|";
                else cursor = "¦";
                renameCurrentItems(inputStringBuilder.ToString().Insert(cursorPosition, cursor));
            }
        }

        public static void renameCurrentItems(string name)
        {
            foreach (ObjectCtrlInfo oci in KKAPI.Studio.StudioAPI.GetSelectedObjects())
            {
                switch (oci.kind)
                {
                    case 3:
                        ((OCIFolder)oci).name = name;
                        break;
                    case 4:
                        ((OCIRoute)oci).name = name;
                        break;
                    case 5:
                        ((OCICamera)oci).name = name;
                        break;
                    default:
                        oci.treeNodeObject.textName = name;
                        break;
                }
            }
        }
        public static void renameItem(ObjectCtrlInfo oci, string name)
        {
            switch (oci.kind)
            {
                case 3:
                    ((OCIFolder)oci).name = name;
                    break;
                case 4:
                    ((OCIRoute)oci).name = name;
                    break;
                case 5:
                    ((OCICamera)oci).name = name;
                    break;
                default:
                    oci.treeNodeObject.textName = name;
                    break;
            }
        }
    }

}
