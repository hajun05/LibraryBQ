using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public partial class BookDetail
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        // Book 클래스는 List<BookCopy> 컬렉션, Count 이외 불필요한 메모리 저장 방지.
        public int BookCopiesCount { get; set; }

        public BookDetail() {}
        public BookDetail(Book book)
        {
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            BookCopiesCount = book.BookCopies.Count;
        }
    }
}
