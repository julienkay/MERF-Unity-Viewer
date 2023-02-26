using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Maps filenames to links.
/// </summary>
public class MERFSources {
    public Dictionary <string, string> Sources { get; set; }

    public string Get(string key) {
        return Sources[key];
    }
}

public class MERFSourcesConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return objectType == typeof(MERFSources);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jObject = JObject.Load(reader);
        MERFSources target = new MERFSources();
        target.Sources = new Dictionary<string, string>();
        foreach (var x in jObject) {
            string key = x.Key;
            JToken value = x.Value;
            target.Sources[key] = value.ToString();
        }
        return target;
    }

    public override bool CanWrite {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}