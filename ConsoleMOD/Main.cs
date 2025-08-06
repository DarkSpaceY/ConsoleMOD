using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using ModLoader;
using ModLoader.IO;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.World;
using static SFS.Parsers.Ini.IniDataEnv;
using Console = ModLoader.IO.Console;

namespace ConsoleMOD
{
    public class Main : Mod
    {
        public override string ModNameID => "ConsoleMOD";
        public override string DisplayName => "ConsoleMOD";
        public override string Author => "DarkSpace";
        public override string MinimumGameVersionNecessary => "1.5.9.8";
        public override string ModVersion => "v1.0.0";
        public override string Description => "More commands in console!";

        static Harmony patcher;

        public override void Early_Load()
        {
            patcher = new Harmony("ConsoleMOD.darkspace");
            patcher.PatchAll();
        }

        public static bool entercollsioninfo = false;
        public override void Load()
        {
            if (ModLoader.IO.Console.main != null)
            {
                ModLoader.IO.Console.main.WriteText("[ConsoleMOD] Loaded successfully.");

                try
                {
                    ModLoader.IO.Console.commands.Add((string input) =>
                    {
                        if (input != "loc")
                        {
                            return false;
                        }
                        Console.main.WriteText($"Current Local Location:[{SFS.World.PlayerController.main.player.Value.location.position.Value.x}, {SFS.World.PlayerController.main.player.Value.location.position.Value.y}]");
                        return true;
                    });

                    Console.commands.Add(input =>
                    {
                        if (input != "parts") return false;

                        var rocket = PlayerController.main?.player?.Value as Rocket;
                        if (rocket == null)
                        {
                            Console.main.WriteText("No active Rocket found.");
                            return true;
                        }

                        var sb = new StringBuilder();
                        sb.AppendLine("=== Rocket Parts Info ===");
                        foreach (var part in rocket.partHolder.GetArray())
                        {
                            sb.AppendLine($"• Part: {part.name}");
                            sb.AppendLine($"    Mass: {part.mass.Value:F2}");
                            sb.AppendLine($"    Modules: {string.Join(", ", part.GetModules<object>().Select(m => m.GetType().Name))}");
                        }

                        string text = sb.ToString();
                        Console.main.WriteText(text);
                        File.WriteAllText(Path.Combine("ConsoleLogs", "parts.txt"), text);
                        Console.main.WriteText("[Saved to ConsoleLogs/parts.txt]");
                        return true;
                    });

                    // 2) joints：显示并保存关节连接树
                    ModLoader.IO.Console.commands.Add((string input) =>
                    {
                        if (input != "joints")
                            return false;

                        var rocket = PlayerController.main?.player?.Value as Rocket;
                        if (rocket == null)
                        {
                            Console.main.WriteText("No rocket found.");
                            return true;
                        }

                        var jointsGroup = rocket.jointsGroup;
                        var parts = rocket.partHolder.parts;
                        if (jointsGroup?.joints == null || parts == null)
                        {
                            Console.main.WriteText("No joints or parts found.");
                            return true;
                        }

                        Console.main.WriteText($"[joints] Total: {jointsGroup.joints.Count}");
                        var i = 0;
                        foreach (var joint in jointsGroup.joints)
                        {
                            try
                            {
                                var partA = joint?.a;
                                var partB = joint?.b;
                                int indexA = parts.IndexOf(partA);
                                int indexB = parts.IndexOf(partB);
                                string nameA = partA?.name ?? "null";
                                string nameB = partB?.name ?? "null";

                                string strA = indexA >= 0 ? $"[{indexA}] {nameA}" : "null";
                                string strB = indexB >= 0 ? $"[{indexB}] {nameB}" : "null";
                                Console.main.WriteText($"[{i}] {strA} → {strB}");

                                i += 1;
                            }
                            catch (Exception ex)
                            {
                                Console.main.WriteText($"Error: {ex.Message}");
                            }
                        }

                        return true;
                    });


                    ModLoader.IO.Console.commands.Add((string input) =>
                    {
                        if (input != "collsion")
                            return false;
                        entercollsioninfo = !entercollsioninfo;
                        return true;
                    });



                    Console.main.WriteText("[ConsoleMOD] Registered commands: parts, joints, resources.");
                }
                catch (System.Exception ex)
                {
                    ModLoader.IO.Console.main.WriteText($"[ConsoleMOD] Error load commands:{ex}");
                }

                ModLoader.IO.Console.main.WriteText("[ConsoleMOD] Current registered commands:");
                foreach (var commandDelegate in ModLoader.IO.Console.commands)
                {
                    ModLoader.IO.Console.main.WriteText($"[ConsoleMOD] - Successfully load {ModLoader.IO.Console.commands.Count} Command!");
                }
            }
        }
    }
}
