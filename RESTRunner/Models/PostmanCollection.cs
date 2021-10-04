using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTRunner.Postman.Models
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Info
    {
        [JsonPropertyName("_postman_id")]
        public string PostmanId;

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("description")]
        public string Description;

        [JsonPropertyName("schema")]
        public string Schema;
    }

    public class Script
    {
        [JsonPropertyName("exec")]
        public List<string> Exec;

        [JsonPropertyName("type")]
        public string Type;
    }

    public class Event
    {
        [JsonPropertyName("listen")]
        public string Listen;

        [JsonPropertyName("script")]
        public Script Script;
    }

    public class Header
    {
        [JsonPropertyName("key")]
        public string Key;

        [JsonPropertyName("value")]
        public string Value;

        [JsonPropertyName("type")]
        public string Type;

        [JsonPropertyName("name")]
        public string Name;
    }

    public class Urlencoded
    {
        [JsonPropertyName("key")]
        public string Key;

        [JsonPropertyName("value")]
        public string Value;

        [JsonPropertyName("description")]
        public string Description;

        [JsonPropertyName("type")]
        public string Type;

        [JsonPropertyName("disabled")]
        public bool? Disabled;
    }

    public class Raw
    {
        [JsonPropertyName("language")]
        public string Language;
    }

    public class Options
    {
        [JsonPropertyName("raw")]
        public Raw Raw;
    }

    public class Body
    {
        [JsonPropertyName("mode")]
        public string Mode;

        [JsonPropertyName("urlencoded")]
        public List<Urlencoded> Urlencoded;

        [JsonPropertyName("raw")]
        public string Raw;

        [JsonPropertyName("options")]
        public Options Options;
    }

    public class Query
    {
        [JsonPropertyName("key")]
        public string Key;

        [JsonPropertyName("value")]
        public string Value;

        [JsonPropertyName("disabled")]
        public bool? Disabled;
    }

    public class Url
    {
        [JsonPropertyName("raw")]
        public string Raw;

        [JsonPropertyName("protocol")]
        public string Protocol;

        [JsonPropertyName("host")]
        public List<string> Host;

        [JsonPropertyName("path")]
        public List<string> Path;

        [JsonPropertyName("query")]
        public List<Query> Query;
    }

    public class Request
    {
        [JsonPropertyName("method")]
        public string Method;

        [JsonPropertyName("header")]
        public List<Header> Header;

        [JsonPropertyName("body")]
        public Body Body;

        [JsonPropertyName("url")]
        public Url Url;

        [JsonPropertyName("description")]
        public string Description;
    }

    public class ProtocolProfileBehavior
    {
        [JsonPropertyName("disableBodyPruning")]
        public bool DisableBodyPruning;
    }

    public class PostmanItem
    {
        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("event")]
        public List<Event> Event;

        [JsonPropertyName("protocolProfileBehavior")]
        public ProtocolProfileBehavior ProtocolProfileBehavior;

        [JsonPropertyName("request")]
        public Request Request;

        [JsonPropertyName("response")]
        public List<object> Response;

        [JsonPropertyName("item")]
        public List<PostmanItem> Item;
    }

    public class Root
    {
        [JsonPropertyName("info")]
        public Info Info;

        [JsonPropertyName("item")]
        public List<PostmanItem> Item;

        [JsonPropertyName("event")]
        public List<Event> Event;
    }

}
