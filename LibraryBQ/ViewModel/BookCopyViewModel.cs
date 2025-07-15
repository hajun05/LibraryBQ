using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Service;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.ViewModel
{
    public partial class BookCopyViewModel : ObservableObject
    {
        // 필드 및 프로퍼티
        private Book _book;
        public Book Book
        {
            get { return _book; }
            set => SetProperty(ref _book, value);
        }

        // 생성자 --------------------------------------------------------------------
        public BookCopyViewModel(Book selectedBook)
        {
            _book = selectedBook;
        }
    }
}
