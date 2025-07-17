using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Service;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LibraryBQ.ViewModel
{
    public partial class BookCopyViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 -----------------------------------------------------------
        private Book _selectedBook;
        private ObservableCollection<BookCopyDetail> _bookCopies;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set => SetProperty(ref _selectedBook, value);
        }
        public ObservableCollection<BookCopyDetail> BookCopies
        {
            get { return _bookCopies; }
            set => SetProperty(ref _bookCopies, value);
        }

        // 생성자 --------------------------------------------------------------------
        public BookCopyViewModel(Book selectedBook)
        {
            _selectedBook = selectedBook;
            _bookCopies = new ObservableCollection<BookCopyDetail>();
            BookCopiesQuery();
        }

        // 메소드 --------------------------------------------------------------------
        private void BookCopiesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookCopyDetail> queriedBookCopies = new List<BookCopyDetail>();
                queriedBookCopies = db.BookCopies.Include(x => x.Book).Include(x => x.LoanStatus)
                    .Include(x => x.LoanHistories).Where(x => x.BookId == SelectedBook.Id)
                    .Select(x => new BookCopyDetail()
                    {
                        BookCopyId = x.Id,
                        BookId = x.BookId,
                        ClassificationNumber = String.Format($"{x.BookId}-{x.Id}"),
                        CurrentLoanStatusId = x.LoanStatusId,
                        CurrentStatusName = BookCopyDetail.ConvertLoanStatus(x.LoanStatusId),
                        CurrentLoanDueDate = (x.LoanStatusId == 2) ? x.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().LoanDueDate : null,
                        CurrentLoanUserId = (x.LoanStatusId == 2) ? x.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().UserId : -1,
                        CurrentReservations = x.ReservationHistories.ToList(),
                    }).ToList();

                foreach(BookCopyDetail bookCopy in queriedBookCopies)
                    _bookCopies.Add(bookCopy);
            }
        }
    }
}
