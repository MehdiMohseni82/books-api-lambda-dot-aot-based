using Amazon.DynamoDBv2;
using BooksApiNative.Commons.Models;
using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2.Model;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace BooksApiNative.Commons.Repository;

public class BooksRepository : IBooksRepository
{
    private const string BOOKS_TABLE = "dotnet-aot-BooksTable";
    private const string COLUMN_PK = "PK";
    private const string COLUMN_SK = "SK";
    private const string COLUMN_NAME = "NAME";
    private const string COLUMN_TYPE = "TYPE";

    private readonly AmazonDynamoDBClient _dynamoDbClient;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BooksRepository))]
    public BooksRepository()
    {
        this._dynamoDbClient = new AmazonDynamoDBClient();
        this._dynamoDbClient.DescribeTableAsync(BOOKS_TABLE).GetAwaiter().GetResult();
    }

    public async Task Add(Book book)
    {
        var dicToAdd = MapToDynamoDb(book);
        await _dynamoDbClient.PutItemAsync(BOOKS_TABLE, dicToAdd);
    }

    public async Task DeleteById(string id)
    {
        var book = await GetById(id);
        var key = new Dictionary<string, AttributeValue>
        {
            [COLUMN_PK] = new AttributeValue { S = id },
            [COLUMN_SK] = new AttributeValue { S = book.Category },
        };

        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = BOOKS_TABLE,
            Key = key
        };

        await _dynamoDbClient.DeleteItemAsync(deleteItemRequest);
    }

    public async Task<Book> GetById(string id)
    {
        var queryResponse = await this._dynamoDbClient.QueryAsync(
            new QueryRequest
            {
                TableName = BOOKS_TABLE,
                KeyConditions = new Dictionary<string, Condition>()
                {
                    {
                        COLUMN_PK,
                        new Condition()
                        {
                            ComparisonOperator = ComparisonOperator.EQ,
                            AttributeValueList = [new AttributeValue(id)]
                        }
                    }
                }
            });

        return queryResponse.Items.Any() ? MapToDto(queryResponse.Items.First()) : null;
    }

    public async Task<List<Book>> GetAll()
    {
        var queryRequest = new QueryRequest
        {
            TableName = BOOKS_TABLE,
            IndexName = "TypeIndex",
            KeyConditionExpression = "#type = :typeValue",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#type", COLUMN_TYPE }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":typeValue", new AttributeValue { S = "_books" } }
            }
        };

        var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

        return queryResponse.Items.Any() ? queryResponse.Items.Select(MapToDto).ToList() : new List<Book>();
    }

    private static Book MapToDto(Dictionary<String, AttributeValue> items)
    {
        return new Book(items[COLUMN_PK].S, items[COLUMN_NAME].S, items[COLUMN_SK].S);
    }

    public static Dictionary<string, AttributeValue> MapToDynamoDb(Book book)
    {
        var item = new Dictionary<string, AttributeValue>(4)
        {
            { COLUMN_PK, new AttributeValue($"{book.Id}") },
            { COLUMN_SK, new AttributeValue($"{book.Category}") },
            { COLUMN_NAME, new AttributeValue(book.Name) },
            { COLUMN_TYPE, new AttributeValue("_books") }
        };

        return item;
    }
}
