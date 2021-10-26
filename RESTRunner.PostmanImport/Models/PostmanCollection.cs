using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTRunner.Postman.Models
{
    public record Info(
        [property: JsonPropertyName("_postman_id")] string PostmanId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("schema")] string Schema);

    public record Script(
        [property: JsonPropertyName("exec")] List<string> Exec,
        [property: JsonPropertyName("type")] string Type);

    public record Event(
        [property: JsonPropertyName("listen")] string Listen,
        [property: JsonPropertyName("script")] Script Script);

    public record Header(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("value")] string Value,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("name")] string Name);

    public record Urlencoded(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("value")] string Value,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("disabled")] bool? Disabled);

    public record Raw([property: JsonPropertyName("language")] string Language);

    public record Options([property: JsonPropertyName("raw")] Raw Raw);

    public record Body(
        [property: JsonPropertyName("mode")] string Mode,
        [property: JsonPropertyName("urlencoded")] List<Urlencoded> Urlencoded,
        [property: JsonPropertyName("raw")] string Raw,
        [property: JsonPropertyName("options")] Options Options);

    public record Query(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("value")] string Value,
        [property: JsonPropertyName("disabled")] bool? Disabled);

    public record Url(
        [property: JsonPropertyName("raw")] string Raw,
        [property: JsonPropertyName("protocol")] string Protocol,
        [property: JsonPropertyName("host")] List<string> Host,
        [property: JsonPropertyName("path")] List<string> Path,
        [property: JsonPropertyName("query")] List<Query> Queries
        );

    public record Request(
        [property: JsonPropertyName("method")] string Method,
        [property: JsonPropertyName("header")] List<Header> Header,
        [property: JsonPropertyName("body")] Body Body,
        [property: JsonPropertyName("url")] Url Url,
        [property: JsonPropertyName("description")] string Description);

    public record ProtocolProfileBehavior(
        [property: JsonPropertyName("disableBodyPruning")] bool DisableBodyPruning);

    public record PostmanItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("event")] List<Event> Event,
        [property: JsonPropertyName("protocolProfileBehavior")] ProtocolProfileBehavior ProtocolProfileBehavior,
        [property: JsonPropertyName("request")] Request Request,
        [property: JsonPropertyName("response")] List<object> Response,
        [property: JsonPropertyName("item")] List<PostmanItem> Item);


    public record Root(
        [property: JsonPropertyName("info")] Info Info,
        [property: JsonPropertyName("item")] List<PostmanItem> Item,
        [property: JsonPropertyName("event")] List<Event> Event);

}