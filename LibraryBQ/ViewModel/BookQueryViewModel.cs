using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Model;
using LibraryBQ.Service;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
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
        private ObservableCollection<BookDetail> _queriedBooks;
        private BookDetail _selectedBook;
        private IWindowService _iWindowService;
        public string InputQueryStr
        {
            get => _inputQueryStr;
            set => SetProperty(ref _inputQueryStr, value);
        }
        public ObservableCollection<BookDetail> QueriedBooks
        {
            get { return _queriedBooks; }
            set => SetProperty(ref _queriedBooks, value);
        }
        public BookDetail SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        // 생성자 ------------------------------------------------------------------
        public BookQueryViewModel(IWindowService iWindowService)
        {
            _iWindowService = iWindowService;
            _queriedBooks = new ObservableCollection<BookDetail>();
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => BookQueryCommand.Execute(null));
        }

        // 커멘드 -------------------------------------------------------------------
        [RelayCommand] private void BookQuery()
        {
            if (_queriedBooks != null)
                _queriedBooks.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookDetail> result = new List<BookDetail>();
                if (InputQueryStr.Trim() != "")
                {
                    result = db.Books.Include(x => x.BookCopies)
                        .Where(x => x.Title.Contains(InputQueryStr) || x.Author.Contains(InputQueryStr))
                        .Select(x => new BookDetail(x)).ToList();
                }
                else
                {
                    result = db.Books.Include(x => x.BookCopies)
                        .Select(x => new BookDetail(x)).ToList();
                }

                foreach (BookDetail book in result)
                    _queriedBooks.Add(book);
            }
        }

        [RelayCommand] private void BookCopyOpen()
        {
            if (_selectedBook != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    Book book = db.Books.Include(x => x.BookCopies).FirstOrDefault(x => x.Id == _selectedBook.Id);
                    _iWindowService.ShowBookCopyWindow(book);
                }
            }
        }

        // 메소드 --------------------------------------------------------------------
        public void BookQueryClear()
        {
            _inputQueryStr = string.Empty;
            _queriedBooks.Clear();
        }
    }
}
