using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Service;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    public partial class BookQueryViewModel : ObservableObject
    {
        // 필드와 프로퍼티 -----------------------------------------------------------
        private string _inputQueryStr;
        private List<Book> _books;
        private Book? _selectedBook;
        public string InputQueryStr
        {
            get => _inputQueryStr;
            set => SetProperty(ref _inputQueryStr, value);
        }
        public List<Book> Books
        {
            get { return _books; }
            set => SetProperty(ref _books, value);
        }
        public Book? SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        public BookQueryViewModel()
        {
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => BookQueryCommand.Execute(null));
        }

        // 커멘드 -------------------------------------------------------------------
        [RelayCommand] private void BookQuery()
        {
            if (Books != null)
                Books.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                if (InputQueryStr.Trim() != "")
                {
                    Books = db.Books.Where(x => x.Title.Contains(InputQueryStr)).ToList();
                    Books.AddRange(db.Books.Where(x => x.Author.Contains(InputQueryStr)).ToList());
                }
                else
                {
                    Books = db.Books.ToList();
                    Books.AddRange(db.Books.ToList());
                }
            }
        }
    }
}
