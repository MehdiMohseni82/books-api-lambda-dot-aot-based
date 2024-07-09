using Amazon.DynamoDBv2;
using BooksApiNative.Commons.Models;
using System.Diagnostics.CodeAnalysis;

namespace BooksApiNative.Commons.Repository;

public class BooksRepository : IBooksRepository
{
    private const string BOOKS_TABLE = "dotnet-aot-BooksTable";
    private readonly AmazonDynamoDBClient _dynamoDbClient;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BooksRepository))]
    public BooksRepository()
    {
        this._dynamoDbClient = new AmazonDynamoDBClient();
        this._dynamoDbClient.DescribeTableAsync(BOOKS_TABLE).GetAwaiter().GetResult();
    }

    public Task Add(Book book)
    {
        throw new NotImplementedException();
    }

    public Task<List<Book>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Book> GetById(string id)
    {
        throw new NotImplementedException();
    }
}
