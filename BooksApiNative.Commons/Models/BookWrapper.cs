using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BooksApiNative.Commons.Models
{
    public class BookWrapper
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BookWrapper))]
        public BookWrapper()
        {
            this.Books = new List<Book>();
        }

        public BookWrapper(List<Book> books)
        {
            this.Books = books;
        }
        
        public List<Book> Books { get; set; }
    }
}