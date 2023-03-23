using System.Text.Json.Serialization;

namespace RESTRunner.Postman.Models;
/// <summary>
/// 
/// </summary>
/// <param name="PostmanId"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Schema"></param>
public record Info(
    [property: JsonPropertyName("_postman_id")] string PostmanId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("schema")] string Schema);


/// <summary>
/// 
/// </summary>
/// <param name="Exec"></param>
/// <param name="Type"></param>
public record Script(
    [property: JsonPropertyName("exec")] List<string> Exec,
    [property: JsonPropertyName("type")] string Type);

/// <summary>
/// 
/// </summary>
/// <param name="Listen"></param>
/// <param name="Script"></param>
public record Event(
    [property: JsonPropertyName("listen")] string Listen,
    [property: JsonPropertyName("script")] Script Script);

/// <summary>
/// 
/// </summary>
/// <param name="Key"></param>
/// <param name="Value"></param>
/// <param name="Type"></param>
/// <param name="Name"></param>
public record Header(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name);

/// <summary>
/// 
/// </summary>
/// <param name="Key"></param>
/// <param name="Value"></param>
/// <param name="Description"></param>
/// <param name="Type"></param>
/// <param name="Disabled"></param>
public record Urlencoded(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("disabled")] bool? Disabled);

/// <summary>
/// 
/// </summary>
/// <param name="Language"></param>
public record Raw([property: JsonPropertyName("language")] string Language);

/// <summary>
/// 
/// </summary>
/// <param name="Raw"></param>
public record Options([property: JsonPropertyName("raw")] Raw Raw);

/// <summary>
/// 
/// </summary>
/// <param name="Mode"></param>
/// <param name="Urlencoded"></param>
/// <param name="Raw"></param>
/// <param name="Options"></param>
public record Body(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("urlencoded")] List<Urlencoded> Urlencoded,
    [property: JsonPropertyName("raw")] string Raw,
    [property: JsonPropertyName("options")] Options Options);

/// <summary>
/// 
/// </summary>
/// <param name="Key"></param>
/// <param name="Value"></param>
/// <param name="Disabled"></param>
public record Query(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("disabled")] bool? Disabled);

/// <summary>
/// 
/// </summary>
/// <param name="Raw"></param>
/// <param name="Protocol"></param>
/// <param name="Host"></param>
/// <param name="Path"></param>
/// <param name="Queries"></param>
public record Url(
    [property: JsonPropertyName("raw")] string Raw,
    [property: JsonPropertyName("protocol")] string Protocol,
    [property: JsonPropertyName("host")] List<string> Host,
    [property: JsonPropertyName("path")] List<string> Path,
    [property: JsonPropertyName("query")] List<Query> Queries
    );

/// <summary>
/// 
/// </summary>
/// <param name="Method"></param>
/// <param name="Header"></param>
/// <param name="Body"></param>
/// <param name="Url"></param>
/// <param name="Description"></param>
public record Request(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("header")] List<Header> Header,
    [property: JsonPropertyName("body")] Body Body,
    [property: JsonPropertyName("url")] Url Url,
    [property: JsonPropertyName("description")] string Description);

/// <summary>
/// 
/// </summary>
/// <param name="DisableBodyPruning"></param>
public record ProtocolProfileBehavior(
    [property: JsonPropertyName("disableBodyPruning")] bool DisableBodyPruning);

/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="Event"></param>
/// <param name="ProtocolProfileBehavior"></param>
/// <param name="Request"></param>
/// <param name="Response"></param>
/// <param name="Item"></param>
public record PostmanItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("event")] List<Event> Event,
    [property: JsonPropertyName("protocolProfileBehavior")] ProtocolProfileBehavior ProtocolProfileBehavior,
    [property: JsonPropertyName("request")] Request Request,
    [property: JsonPropertyName("response")] List<object> Response,
    [property: JsonPropertyName("item")] List<PostmanItem> Item);

/// <summary>
/// 
/// </summary>
/// <param name="Info"></param>
/// <param name="Item"></param>
/// <param name="Event"></param>
public record Root(
    [property: JsonPropertyName("info")] Info Info,
    [property: JsonPropertyName("item")] List<PostmanItem> Item,
    [property: JsonPropertyName("event")] List<Event> Event);
