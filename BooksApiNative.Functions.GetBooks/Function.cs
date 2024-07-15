using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using BooksApiNative.Commons;
using BooksApiNative.Commons.Repository;

namespace BooksApiNative.Functions.GetBooks;

public class Function
{
    private static IBooksRepository _booksRepository;
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Function))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyRequest))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyResponse))]
    static Function()
    {
        _booksRepository = new BooksRepository();
    }

    private static async Task Main()
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task<APIGatewayHttpApiV2ProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<CustomJsonSerializerContext>(options =>
        {
            options.PropertyNameCaseInsensitive = false;
        }))
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent, ILambdaContext context)
    {
        if (!apigProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Only GET allowed",
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
            };
        }
        
        context.Logger.LogInformation($"Received {apigProxyEvent}");

        var books = await _booksRepository.GetAll();

        context.Logger.LogInformation($"Found {books.Count} book(s)");
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(books, CustomJsonSerializerContext.Default.ListBook),
            StatusCode = 200,
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }
}
