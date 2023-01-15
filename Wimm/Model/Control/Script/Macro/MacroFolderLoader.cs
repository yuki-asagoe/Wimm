using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Wimm.Model.Control.Script.Macro
{
    //もうちょっとエラーロギングあってもええんちゃうか
    public class MacroFolderLoader
    {
        public ImmutableArray<MacroInfo> Macros { get; init; }
        public MacroFolderLoader(DirectoryInfo macroDirectory,Lua lua)
        {
            Macros = ImmutableArray<MacroInfo>.Empty;
            var jsonFile = new FileInfo(macroDirectory + "/macros.json");
            if (!jsonFile.Exists) { return; }
            JsonNode? json = null;
            using(var stream = jsonFile.OpenRead())
            {
                json=JsonNode.Parse(stream);
            }
            if(json is null) { return; }
            var listBuilder = new LinkedList<MacroInfo>();
            for(int i=1;i<=100;i++)//さすがに100個もマクロ作るやつおらんやろ
            {
                var macroObj=json[i.ToString()];
                if(macroObj is not JsonObject) { break; }
                var macroFile = new FileInfo($"{macroDirectory.FullName}/{i}.neo.lua");
                var nameObj = macroObj["name"] as JsonValue;
                var lifetimeObj = macroObj["lifetime"] as JsonValue;
                if(nameObj is null || lifetimeObj is null || !macroFile.Exists) { break; }
                var chunk=lua.CompileChunk(macroFile.FullName,new LuaCompileOptions());
                if (chunk is null) { break; }
                if(nameObj.TryGetValue<string>(out var name)&&lifetimeObj.TryGetValue<double>(out var lifetime))
                {
                    listBuilder.AddLast(new MacroInfo(name, lifetime, chunk));
                }
                else
                {
                    break;
                }
            }
            Macros = listBuilder.ToImmutableArray();
        }
    }
}
