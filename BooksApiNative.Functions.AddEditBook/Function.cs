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
using Amazon.Runtime.Endpoints;
using BooksApiNative.Commons;
using BooksApiNative.Commons.Models;
using BooksApiNative.Commons.Repository;

namespace BooksApiNative.Functions.AddEditBook;

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
        if (!apiGatewayProxyEvent.RequestContext.Http.Method.Equals(HttpMethod.Post.Method))
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Only Post allowed",
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
            };
        }

        try
        {
            context.Logger.LogLine(JsonSerializer.Serialize(apiGatewayProxyEvent, CustomJsonSerializerContext.Default.APIGatewayHttpApiV2ProxyRequest));

            var id = apiGatewayProxyEvent.PathParameters["id"];

            context.Logger.LogLine($"received Id {id}");

            var cleanedBody = apiGatewayProxyEvent.Body.Replace("\r\n", "").Trim();
            var book = JsonSerializer.Deserialize(cleanedBody, CustomJsonSerializerContext.Default.Book);

            context.Logger.LogLine($"received Book {book}");

            await _booksRepository.Add(book);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = $"Created book with id {book.Id}"
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error creating book {e.Message} {e.StackTrace}");

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = $"Error creating book {e.Message} {e.StackTrace}",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };
        }
    }
}
