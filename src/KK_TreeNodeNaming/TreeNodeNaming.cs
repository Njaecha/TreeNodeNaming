using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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
    public class TreeNodeNaming: BaseUnityPlugin
    {
        public const string PluginName = "KK_TreeNodeNaming";
        public const string GUID = "org.njaecha.plugins.treenodenaming";
        public const string Version = "1.0.0";

        internal new static ManualLogSource Logger;

        internal static bool ui = false;
        internal static bool uiActive = true;

        private int uiX;
        private int uiY;
        private Rect windowRect = new Rect(100, 100, 200, 80);
        private string inputString = "";
        
        internal static TreeNodeCtrl tnc = new TreeNodeCtrl();

        private ConfigEntry<KeyboardShortcut> hotkey;

        public void Awake()
        {
            KKAPI.Studio.StudioAPI.StudioLoadedChanged += registerTreeNodeCtrl;
            TreeNodeNaming.Logger = base.Logger;
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(GUID);
            KeyboardShortcut defaultShortcut = new KeyboardShortcut(KeyCode.L);
            hotkey = Config.Bind("General", "Hotkey", defaultShortcut, "Press this key to open UI");
        }
        public void registerTreeNodeCtrl(object sender, EventArgs e)
        {
            tnc = Singleton<Studio.Studio>.Instance.treeNodeCtrl;
            if (tnc)
            {
                tnc.onSelect += delegate { setUi(true); };
                tnc.onDeselect += delegate { setUi(false); };
                tnc.onDelete += delegate { setUi(false); };
            }
        }
        public void OnGUI()
        {
            if (ui)
            {
                windowRect = GUI.Window(77, windowRect, WindowFunction, "Rename Tree Node");
                KKAPI.Utilities.IMGUIUtils.EatInputInRect(windowRect);
            }
        }
        void Update()
        {
            if (hotkey.Value.IsDown())
            {
                uiActive = !uiActive;
                setUi(uiActive);
            }
        }
        public void setUi(bool state)
        {
            if (!state)
            {
                ui = false;
                return;
            }

            IEnumerable<ObjectCtrlInfo> selectedObjects = KKAPI.Studio.StudioAPI.GetSelectedObjects();
            bool renambaleSeclected = false;
            uiX = (int)(Screen.width * 0.805 - 200);
            uiY = (int)(Screen.height * 0.075);
            windowRect.x = uiX;
            windowRect.y = uiY;
            foreach (ObjectCtrlInfo oci in selectedObjects)
            {
                if (oci is OCIItem || oci is OCIChar)
                {
                    renambaleSeclected = true;
                    break;
                }
            }
            if (uiActive)
                ui = renambaleSeclected;

        }
        private void WindowFunction(int WindowID)
        {
            inputString = GUI.TextField(new Rect(10, 20, 180, 20), inputString);
            if (GUI.Button(new Rect(10, 50, 180, 20), "Rename"))
            {

                IEnumerable<ObjectCtrlInfo> selectedObjects = KKAPI.Studio.StudioAPI.GetSelectedObjects();
                foreach (ObjectCtrlInfo oci in selectedObjects)
                {
                    renameItem(oci.treeNodeObject, inputString);
                }
            }
            //GUI.DragWindow();
        }
        public static void renameItem(TreeNodeObject tno, string name)
        {
            tno.textName = name;
        }
    }

    public class SceneController: SceneCustomFunctionController
    {
        protected override void OnSceneSave()
        {
            var data = new PluginData();

            Dictionary<int, ObjectCtrlInfo> idObjectPairs = Studio.Studio.Instance.dicObjectCtrl;
            Dictionary<int, string> idNamePairs = new Dictionary<int, string>();

            foreach(int id in idObjectPairs.Keys)
            {
                idNamePairs[id] = idObjectPairs[id].treeNodeObject.textName;
            }

            if (idNamePairs.Count > 0)
            {
                data.data.Add("names", MessagePackSerializer.Serialize(idNamePairs));
            }
            else data.data.Add("names", null);

            SetExtendedData(data);
        }

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            TreeNodeNaming.ui = false;

            if (operation == SceneOperationKind.Clear) return;

            var data = GetExtendedData();

            if (data?.data == null) return;

            Dictionary<int, string> idNamePairs = new Dictionary<int, string>();
            if (data.data.TryGetValue("names", out var temp) && temp != null)
            {
                idNamePairs = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])temp);
                foreach (int id in idNamePairs.Keys)
                    TreeNodeNaming.renameItem(loadedItems[id].treeNodeObject, idNamePairs[id]);
            }
            else TreeNodeNaming.Logger.LogError("failed to obtain pluginData from OnLoad event");
        }
        protected override void OnObjectsCopied(ReadOnlyDictionary<Int32, ObjectCtrlInfo> copiedItems)
        {
            Dictionary<int, ObjectCtrlInfo> sceneObjects = Studio.Studio.Instance.dicObjectCtrl;
            foreach (int id in copiedItems.Keys)
            {
                if (copiedItems[id] is OCIItem || copiedItems[id] is OCIChar)
                    copiedItems[id].treeNodeObject.textName = sceneObjects[id].treeNodeObject.textName;
            }
        }
    }
}
