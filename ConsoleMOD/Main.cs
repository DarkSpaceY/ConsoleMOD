using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using ModLoader;
using ModLoader.IO;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Variables;
using SFS.World;
using UnityEngine;
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
                            sb.AppendLine($"• Part[{rocket.partHolder.parts.IndexOf(part)}]: {part.name}");
                            sb.AppendLine($"    Mass: {part.mass.Value:F2}");
                            sb.AppendLine($"    Modules: {string.Join(", ", part.GetModules<object>().Select(m => m.GetType().Name))}");
                        }

                        string text = sb.ToString();
                        Console.main.WriteText(text);
                        File.WriteAllText(Path.Combine("ConsoleLogs", "parts.txt"), text);
                        Console.main.WriteText("[Saved to ConsoleLogs/parts.txt]");
                        return true;
                    });

                    Console.commands.Add(input =>
                    {
                        // Check if it starts with "part " (note the space)
                        if (!input.StartsWith("part ")) return false;

                        var rocket = PlayerController.main?.player?.Value as Rocket;
                        if (rocket == null)
                        {
                            Console.main.WriteText("No active rocket found");
                            return true;
                        }

                        // Extract the index part
                        string indexStr = input.Substring(5);
                        if (!int.TryParse(indexStr, out int index))
                        {
                            Console.main.WriteText("Invalid index format, please enter 'part [number]'");
                            return true;
                        }

                        // Get parts array and validate index range
                        var parts = rocket.partHolder.GetArray();
                        if (index < 0 || index >= parts.Length)
                        {
                            Console.main.WriteText($"Index out of range (0-{parts.Length - 1})");
                            return true;
                        }

                        Part targetPart = parts[index];
                        var sb = new StringBuilder();

                        // Part basic information
                        sb.AppendLine($"=== Part [{index}] Details ===");
                        sb.AppendLine($"• Name: {targetPart.name}");
                        sb.AppendLine($"• Mass: {targetPart.mass.Value:F2} kg");
                        sb.AppendLine($"• Position: {targetPart.transform.position}");
                        sb.AppendLine($"• Rotation: {targetPart.transform.rotation.eulerAngles}");
                        sb.AppendLine($"• Module Count: {targetPart.GetModules<object>().Length}");

                        // Iterate through all modules
                        foreach (var module in targetPart.GetModules<object>())
                        {
                            sb.AppendLine($"\n--- Module: {module.GetType().Name} ---");

                            // Get public properties and fields of the module
                            var type = module.GetType();

                            // Output properties
                            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                            {
                                try
                                {
                                    object value = prop.GetValue(module);
                                    sb.AppendLine($"    [Property] {prop.Name}: {FormatValue(value)}");
                                }
                                catch { /* Ignore properties that fail to get */ }
                            }

                            // Output fields
                            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                            {
                                try
                                {
                                    object value = field.GetValue(module);
                                    sb.AppendLine($"    [Field] {field.Name}: {FormatValue(value)}");
                                }
                                catch { /* Ignore fields that fail to get */ }
                            }
                        }

                        // Output the result
                        string text = sb.ToString();
                        Console.main.WriteText(text);

                        return true;
                    });
                    string FormatValue(object value)
                    {
                        if (value == null) return "null";

                        // 处理基础类型
                        if (value is Vector3 vec) return $"({vec.x:F2}, {vec.y:F2}, {vec.z:F2})";
                        if (value is Quaternion quat) return quat.eulerAngles.ToString();
                        if (value is float f) return f.ToString("F4");

                        // 处理 VariableList<T> 及其派生类型
                        if (value is VariableList<double> doubleList) return FormatVariableList(doubleList, "Double", v => v.ToString("F4"));
                        if (value is VariableList<bool> boolList) return FormatVariableList(boolList, "Bool", v => v.ToString());
                        if (value is VariableList<string> stringList) return FormatVariableList(stringList, "String", v => $"\"{v}\"");

                        // 处理 ReferenceVariable<T>
                        if (value is ReferenceVariable<double> doubleRef) return FormatReferenceVariable(doubleRef, "Double", v => v.ToString("F4"));
                        if (value is ReferenceVariable<bool> boolRef) return FormatReferenceVariable(boolRef, "Bool", v => v.ToString());
                        if (value is ReferenceVariable<string> strRef) return FormatReferenceVariable(strRef, "String", v => $"\"{v}\"");

                        // 默认处理
                        return value.ToString();
                    }

                    string FormatReferenceVariable<T>(ReferenceVariable<T> reference, string typeName, Func<T, string> valueFormatter)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{typeName}Reference:");

                        try
                        {
                            // 尝试获取当前值
                            var valueProperty = reference.GetType().GetProperty("Value");
                            if (valueProperty != null)
                            {
                                T val = (T)valueProperty.GetValue(reference);
                                sb.AppendLine($"        Current Value: {valueFormatter(val)}");
                            }

                            // 显示变量列表信息
                            var list = reference.GetVariableList();
                            if (list != null)
                            {
                                sb.AppendLine($"        From List: {list.GetType().Name} (Count: {list.saves.Count})");
                            }
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"        [ERROR READING REFERENCE: {ex.Message}]");
                        }

                        return sb.ToString();
                    }

                    string FormatVariableList<T>(VariableList<T> list, string typeName, Func<T, string> valueFormatter)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{typeName}VariableList (Count: {list.saves.Count})");

                        foreach (var save in list.saves)
                        {
                            var variable = list.GetVariable(save.name);
                            if (variable == null)
                            {
                                sb.AppendLine($"            [{save.name}] (INVALID VARIABLE)");
                                continue;
                            }

                            sb.AppendLine($"            [{save.name}]");
                            sb.AppendLine($"                Value: {valueFormatter(variable.Value)}");
                            sb.AppendLine($"                Saved: {save.save}");

                            // 显示回调信息
                            int callbackCount = 0;
                            callbackCount += variable.onValueChange?.GetInvocationList().Length ?? 0;
                            callbackCount += variable.onValueChangeNew?.GetInvocationList().Length ?? 0;
                            callbackCount += variable.onValueChangeOldNew?.GetInvocationList().Length ?? 0;

                            sb.AppendLine($"                Callbacks: {callbackCount}");

                            // 显示原始数据（如果有）
                            if (save.data != null && save.data.Length > 0)
                            {
                                sb.AppendLine($"                DataLength: {save.data.Length} bytes");
                            }
                        }

                        return sb.ToString();
                    }

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
                        Console.main.WriteText($"Collsion Info: {entercollsioninfo}");
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
