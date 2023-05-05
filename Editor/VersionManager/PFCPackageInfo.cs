using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class PFCPackageInfo {

    [JsonProperty("name")]
    public string Name = "";

    [JsonProperty("version")]
    public string Version = "";

    [JsonProperty("description")]
    public string Description = "";

    [JsonProperty("displayName")]
    public string DisplayName = "";

    [JsonProperty("author")]
    public VRCAuthor Author = new VRCAuthor();

    [JsonProperty("dependencies")]
    public Dictionary<string, string> Dependencies = new Dictionary<string, string>();

    [JsonProperty("gitDependencies")]
    public Dictionary<string, string> GitDependencies = new Dictionary<string, string>();

    [JsonProperty("vpmDependencies")]
    public Dictionary<string, string> VPMDependencies = new Dictionary<string, string>();

    [JsonProperty("legacyFolders")]
    public Dictionary<string, string> LegacyFolders = new Dictionary<string, string>();

    [JsonProperty("legacyFiles")]
    public Dictionary<string, string> LegacyFiles = new Dictionary<string, string>();

    [JsonProperty("url")]
    public string Url = "";

    [JsonProperty("localPath")]
    public string LocalPath = "";

    [JsonProperty("repo")]
    public string Repo = "";

    [JsonProperty("repoTokenID")]
    public string TokenID = "";

    [JsonProperty("zipSHA256")]
    public string ZipSHA256 = "";

    [JsonProperty("headers")]
    public Dictionary<string, string> Headers = new Dictionary<string, string>();

}

[Serializable]
public class VRCAuthor {
    public string name = "";
    public string email = "";
    public string url = "";
}

[Serializable]
public class dependencyData {
    public string url;
    public string version;
}