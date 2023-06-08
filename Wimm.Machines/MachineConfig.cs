using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wimm.Machines
{
    public class MachineConfig
    {
        private SortedList<string, ConfigItemRegistry> Registries = new SortedList<string, ConfigItemRegistry>();
        private SortedList<string, string> Items = new SortedList<string, string>();
        public void AddRegistries(params ConfigItemRegistry[] items)
        {
            foreach(var item in items)
            {
                Registries.Add(item.Name,item);
            }
        }
        public string? GetValueOrDefault(string name)
        {
            if (Items.TryGetValue(name, out var value)) { return value; }
            if (Registries.TryGetValue(name,out var registry)) { return registry.Default; }
            return null;
        }
        internal MachineConfig(Utf8JsonReader configJsonReader)
        {
            foreach (var item in LoadConfigJson(configJsonReader))
            {
                Items.Add(item.Key, item.Value.Value);
            }
        }
        internal MachineConfig() { }
        // TODO フォーマットをドキュメントにまとめて
        /// <summary>
        /// Json形式のコンフィグを読み込みして配列にして返します。<br></br>
        /// フォーマットは "name","value","default"(これは任意)のプロパティを持つオブジェクトの配列で従わない要素は無視されます
        /// </summary>
        /// <param name="jsonReader">対象となるJsonデータのリーダー</param>
        /// <returns>読み込まれた値、Keyは"name"プロパティに相当します</returns>
        public static KeyValuePair<string,(string Value,string? Default)>[] LoadConfigJson(Utf8JsonReader jsonReader)
        {
            var itemList = new LinkedList<KeyValuePair<string, (string Value, string? Default)>>();
            var json= JsonNode.Parse(ref jsonReader);
            if(json is JsonArray array)
            {
                foreach(var item in array)
                {
                    if(item is JsonObject configItem)
                    {
                        bool correct = false;
                        correct |= configItem.TryGetPropertyValue("name", out var nameObj);
                        correct |= configItem.TryGetPropertyValue("value", out var valueObj);
                        if (!correct) continue;
                        if (
                            nameObj is JsonValue configName && configName.TryGetValue<string>(out var name) &&
                            valueObj is JsonValue configValue && configValue.TryGetValue<string>(out var value)
                        )
                        {
                            if (
                                configItem.TryGetPropertyValue("default", out var defaultObj) &&
                                defaultObj is JsonValue configDefault &&
                                configDefault.TryGetValue<string>(out var defaultValue)
                            )
                            {
                                itemList.AddLast(new KeyValuePair<string, (string Name, string? Default)>(name, (value, defaultValue)));
                            }
                            else
                            {
                                itemList.AddLast(new KeyValuePair<string, (string Name, string? Default)>(name, (value,null)));
                            }
                        }
                    }
                }
            }
            return itemList.ToArray();
        }
        public static void WriteConfigJson(Utf8JsonWriter jsonWriter, KeyValuePair<string, (string Value, string? Default)>[] items)
        {
            var json = new JsonArray();
            foreach(var item in items)
            {
                var jsonItem = new JsonObject();
                jsonItem.Add("name",JsonValue.Create(item.Key));
                jsonItem.Add("value", JsonValue.Create(item.Value.Value));
                if (item.Value.Default is string Default)
                    jsonItem.Add("default", JsonValue.Create(Default));
                json.Add(jsonItem);
            }
            json.WriteTo(jsonWriter);
        }
        public void WriteRegistriesTo(Utf8JsonWriter jsonWriter)
        {
            WriteConfigJson(jsonWriter, Registries.Select(it => new KeyValuePair<string, (string, string?)>(it.Value.Name, (it.Value.Default, it.Value.Default))).ToArray());
        }
    }
    public record ConfigItemRegistry(string Name,string Default);
}
