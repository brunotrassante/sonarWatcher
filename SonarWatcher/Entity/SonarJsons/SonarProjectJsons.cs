﻿using Newtonsoft.Json;

public class SonarProjectJsons
{
    [JsonProperty("id")]
    public string id { get; set; }
    [JsonProperty("k")]
    public string Key { get; set; }
    [JsonProperty("nm")]
    public string Name { get; set; }
}
