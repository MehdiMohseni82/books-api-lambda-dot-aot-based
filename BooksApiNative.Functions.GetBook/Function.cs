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

namespace BooksApiNative.Functions.GetBook;

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
            options.PropertyNameCaseInsensitive = true;
        }))
            .Build()
            .RunAsync();
    }

    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apiGatewayProxyEvent, ILambdaContext context)
    {
        if (!apiGatewayProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Get.Method))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Only GET allowed",
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
            };
        }

        context.Logger.LogInformation($"Received {apiGatewayProxyEvent}");
        context.Logger.LogLine(JsonSerializer.Serialize(apiGatewayProxyEvent, CustomJsonSerializerContext.Default.APIGatewayHttpApiV2ProxyRequest));

        var id = apiGatewayProxyEvent.PathParameters["id"];

        context.Logger.LogLine($"received Id {id}");

        var book = await _booksRepository.GetById(id);

        context.Logger.LogInformation($"Found book {book}");

        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(book, CustomJsonSerializerContext.Default.Book),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
