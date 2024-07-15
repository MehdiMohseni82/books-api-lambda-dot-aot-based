using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace BooksApiNative.Commons.Models;

public class Book
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Book))]
    public Book()
    {
        this.Id = string.Empty;
        this.Name = string.Empty;
        this.Category = string.Empty;
    }

    public Book(string id, string name, string category)
    {
        this.Id = id;
        this.Name = name;
        this.Category = category;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    public override string ToString()
    {
        return "Product {" +
               " id='" + this.Id + '\'' +
               ", name='" + this.Name + '\'' +
               ", category=" + this.Category +
               '}';
    }
}
