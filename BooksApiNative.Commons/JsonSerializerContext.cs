using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using BooksApiNative.Commons.Models;

namespace BooksApiNative.Commons;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(List<Book>))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}