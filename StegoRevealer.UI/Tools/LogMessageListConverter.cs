using StegoRevealer.StegoCore.Logger;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StegoRevealer.UI.Tools;

public class LogMessageListConverter : JsonConverter<List<LogMessage>>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(List<LogMessage>);

    public override void Write(Utf8JsonWriter writer, List<LogMessage> value, JsonSerializerOptions options)
    {
        foreach (var logMessage in value)
            writer.WriteStringValue(logMessage.ToString());
    }

    public override List<LogMessage>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
