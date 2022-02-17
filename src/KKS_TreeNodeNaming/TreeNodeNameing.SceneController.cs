using System;
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
    public class SceneController : SceneCustomFunctionController
    {
        protected override void OnSceneSave()
        {
            var data = new PluginData();

            Dictionary<int, ObjectCtrlInfo> idObjectPairs = Studio.Studio.Instance.dicObjectCtrl;
            Dictionary<int, string> idNamePairs = new Dictionary<int, string>();

            foreach (int id in idObjectPairs.Keys)
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
            TreeNodeNaming.uiActive = false;

            if (operation == SceneOperationKind.Clear) return;

            var data = GetExtendedData();

            if (data?.data == null) return;

            Dictionary<int, string> idNamePairs = new Dictionary<int, string>();
            if (data.data.TryGetValue("names", out var temp) && temp != null)
            {
                idNamePairs = MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])temp);
                foreach (int id in idNamePairs.Keys)
                    TreeNodeNaming.renameItem(loadedItems[id], idNamePairs[id]);
            }
            else TreeNodeNaming.Logger.LogError("failed to obtain pluginData from OnLoad event");
        }
        protected override void OnObjectsCopied(ReadOnlyDictionary<Int32, ObjectCtrlInfo> copiedItems)
        {
            Dictionary<int, ObjectCtrlInfo> sceneObjects = Studio.Studio.Instance.dicObjectCtrl;
            foreach (int id in copiedItems.Keys)
            {
                if (copiedItems[id].kind != 6)
                    TreeNodeNaming.renameItem(copiedItems[id], sceneObjects[id].treeNodeObject.textName);
            }
        }
    }
}
