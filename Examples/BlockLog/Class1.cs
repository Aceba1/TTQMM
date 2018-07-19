using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace ClassLibrary2
{
    internal class GuiClass : MonoBehaviour
    {
        public void Append(string str)
        {
            this.log = this.log + str + "\n";
            this.logLines++;
        }
        public GuiClass()
        {
            GuiClass._inst = this;
        }
        private void OnGUI()
        {
            if (this.ShowGUI)
            {
                GUI.Window(0, this.Window, new GUI.WindowFunction(this.GUIWindow), "Block Spawn Log");
                return;
            }
            GUI.Button(new Rect(this.Window.width - 15f, 0f, 15f, 15f), "O");
        }
        private void GUIWindow(int ID)
        {
            int num = Mathf.Max(20 * this.logLines - 400, 66);
            this.scroll = GUI.BeginScrollView(new Rect(0f, 15f, 400f, 65f), this.scroll, new Rect(0f, 0f, 380f, (float)num));
            GUI.TextArea(new Rect(0f, 0f, 385f, (float)num), this.log);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(this.Window.width - 15f, 0f, 15f, 15f), "X"))
            {
                this.ShowGUI = false;
            }
        }
        private Rect Window = new Rect(0f, 0f, 400f, 80f);
        private Vector2 scroll = Vector2.zero;
        public string log = "";
        public int logLines = 0;
        public static GuiClass _inst;
        private bool ShowGUI;
    }
    [HarmonyPatch("OnSpawn")]
    [HarmonyPatch(typeof(TankBlock))]
    internal class Patch1
    {
        private static void Postfix(TankBlock __instance)
        {
            GuiClass._inst.Append(__instance.BlockType.ToString() + " spawned");
        }
    }
    internal class QPatch
    {
        public static void Main()
        {
            HarmonyInstance.Create("ttqmm.examples.blocklog").PatchAll(Assembly.GetExecutingAssembly());
            new GameObject("ModInfo Object", new Type[]
            {
                typeof(GuiClass)
            });
            GuiClass._inst.Append("Mod was applied");
        }
    }
}