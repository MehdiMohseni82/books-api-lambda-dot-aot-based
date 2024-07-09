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
using BooksApiNative.Commons.Models;

namespace BooksApiNative.Functions.GetBooks;

public class Function
{
    //static ProductsDAO dataAccess;
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Function))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyRequest))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(APIGatewayHttpApiV2ProxyResponse))]
    static Function()
    {
        //dataAccess = new DynamoDbProducts();
    }

    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
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

        var books = new BookWrapper(new List<Book>
        {
            new("1", "To Kill a Mockingbird", "Fiction"),
            new("2", "1984", "Dystopian"),
            new("3", "The Great Gatsby", "Classic"),
            new("4", "The Catcher in the Rye", "Classic"),
            new("5", "Moby Dick", "Adventure"),
            new("6", "Pride and Prejudice", "Romance"),
            new("7", "The Lord of the Rings", "Fantasy"),
            new("8", "Harry Potter and the Sorcerer's Stone", "Fantasy"),
            new("9", "The Chronicles of Narnia", "Fantasy"),
            new("10", "The Hobbit", "Fantasy")
        });

        context.Logger.LogInformation($"Found 10 product(s)");
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            Body = JsonSerializer.Serialize(books, CustomJsonSerializerContext.Default.BookWrapper),
            StatusCode = 200,
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }
}
