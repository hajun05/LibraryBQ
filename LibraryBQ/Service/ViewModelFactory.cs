using LibraryBQ.ViewModel;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public interface IBookCopyViewModelFactory
    {
        BookCopyViewModel Create(Book book);
    }

    public class BookCopyViewModelFactory : IBookCopyViewModelFactory
    {
        public BookCopyViewModel Create(Book book)
        {
            return new BookCopyViewModel(book);
        }
    }
}
