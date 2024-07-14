using BooksApiNative.Commons.Models;

namespace BooksApiNative.Commons.Repository;

public interface IBooksRepository
{
    Task Add(Book book);

    Task<Book> GetById(string id);

    Task DeleteById(string id);

    Task<List<Book>> GetAll();
}