using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Studio;

namespace TreeNodeNaming
{
    class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.AddNode), typeof(string), typeof(TreeNodeObject)) ]
        private static void AddNodePostfix(TreeNodeObject __result)
        {
            List<string> allNames = TreeNodeNaming.getAllNodeTextNames();
            string name = __result.textName;
            if (allNames.FindAll(s => s.Equals(name)).Count > 1 && TreeNodeNaming.autoRenaming.Value)
            {
                int i = 0;
                bool exists = true;
                while (exists)
                {
                    i++;
                    if (!allNames.Contains(name + " " + i)) exists = false;
                }
                __result.textName += $" {i}";
            }
        }
    }
}
