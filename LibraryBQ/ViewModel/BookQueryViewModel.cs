using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Model;
using LibraryBQ.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Book> _queriedBooks;
        private Book? _selectedBook;
        private IWindowService _iWindowService;
        public string InputQueryStr
        {
            get => _inputQueryStr;
            set => SetProperty(ref _inputQueryStr, value);
        }
        public ObservableCollection<Book> QueriedBooks
        {
            get { return _queriedBooks; }
            set => SetProperty(ref _queriedBooks, value);
        }
        public Book? SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        // 생성자 ------------------------------------------------------------------
        public BookQueryViewModel(IWindowService iWindowService)
        {
            _iWindowService = iWindowService;
            _queriedBooks = new ObservableCollection<Book>();
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => BookQueryCommand.Execute(null));
        }

        // 커멘드 -------------------------------------------------------------------
        [RelayCommand] private void BookQuery()
        {
            if (_queriedBooks != null)
                _queriedBooks.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<Book> result = new List<Book>();
                if (InputQueryStr.Trim() != "")
                {
                    result = db.Books.Where(x => x.Title.Contains(InputQueryStr)).ToList();
                    result.AddRange(db.Books.Where(x => x.Author.Contains(InputQueryStr)).ToList());
                }
                else
                {
                    result = db.Books.ToList();
                    result.AddRange(db.Books.ToList());
                }

                foreach (Book book in result)
                    _queriedBooks.Add(book);
            }
        }

        [RelayCommand] private void BookCopyOpen()
        {
            if (_selectedBook != null)
                _iWindowService.ShowBookCopyWindow(_selectedBook);
        }

        // 메소드 --------------------------------------------------------------------
        public void BookQueryClear()
        {
            _inputQueryStr = string.Empty;
            _queriedBooks.Clear();
        }
    }
}
