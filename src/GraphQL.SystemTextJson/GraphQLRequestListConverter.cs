using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL.Transports.Json;

namespace GraphQL.SystemTextJson
{
    /// <summary>
    /// A custom JsonConverter for reading or writing a list of <see cref="GraphQLRequest"/> objects.
    /// Will deserialize a single request into a list containing one request.
    /// </summary>
    public class GraphQLRequestListConverter : JsonConverter<IEnumerable<GraphQLRequest>>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            return (
                typeToConvert == typeof(IEnumerable<GraphQLRequest>) ||
                typeToConvert == typeof(ICollection<GraphQLRequest>) ||
                typeToConvert == typeof(IReadOnlyCollection<GraphQLRequest>) ||
                typeToConvert == typeof(IReadOnlyList<GraphQLRequest>) ||
                typeToConvert == typeof(IList<GraphQLRequest>) ||
                typeToConvert == typeof(List<GraphQLRequest>) ||
                typeToConvert == typeof(GraphQLRequest[]));
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IEnumerable<GraphQLRequest> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var request in value)
            {
                JsonSerializer.Serialize(writer, request, options);
            }
            writer.WriteEndArray();
        }

        /// <inheritdoc/>
        public override IEnumerable<GraphQLRequest> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var request = JsonSerializer.Deserialize<GraphQLRequest>(ref reader, options);
                return typeToConvert == typeof(List<GraphQLRequest>)
                    ? new List<GraphQLRequest>(1) { request }
                    : new GraphQLRequest[] { request };
            }

            //unexpected token type
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            var list = new List<GraphQLRequest>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return typeToConvert == typeof(GraphQLRequest[])
                        ? list.ToArray()
                        : list;
                }

                var request = JsonSerializer.Deserialize<GraphQLRequest>(ref reader, options);
                list.Add(request);
            }

            //unexpected end of data
            throw new JsonException();
        }
    }
}