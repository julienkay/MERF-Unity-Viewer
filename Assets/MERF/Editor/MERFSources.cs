using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

/// <summary>
/// Maps filenames to links.
/// </summary>
public class MERFSources {
    public Dictionary <string, Uri> Sources { get; set; }

    public Uri Get(string key) {
        return Sources[key];
    }
    public Uri GetRGBVolumeUrl(int i) {
        string fileName = $"rgba_{i:D3}.png";
        return Get(fileName);
    }
    public Uri GetFeatureVolumeUrl(int i) {
        string fileName = $"feature_{i:D3}.png";
        return Get(fileName);
    }

}

public class MERFSourcesConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return objectType == typeof(MERFSources);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jObject = JObject.Load(reader);
        MERFSources target = new MERFSources();
        target.Sources = new Dictionary<string, Uri>();
        foreach (var x in jObject) {
            string key = x.Key;
            JToken value = x.Value;
            target.Sources[key] = (Uri?)value;
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