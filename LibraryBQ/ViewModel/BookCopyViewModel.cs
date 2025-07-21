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
using System.Windows;

namespace LibraryBQ.ViewModel
{
    public partial class BookCopyViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 -----------------------------------------------------------
        private Book _selectedBook;
        private ObservableCollection<BookCopyDetail> _bookCopies;
        private BookCopyDetail _selectedBookCopy;
        private LoginUserAccountStore _loginUserAccount;
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
        public BookCopyDetail SelectedBookCopy
        {
            get { return _selectedBookCopy; }
            set => SetProperty(ref _selectedBookCopy, value);
        }
        public LoginUserAccountStore LoginUserAccount
        {
            get => _loginUserAccount;
            set => SetProperty(ref _loginUserAccount, value);
        }

        // 생성자 --------------------------------------------------------------------
        public BookCopyViewModel(Book selectedBook)
        {
            _selectedBook = selectedBook;
            _bookCopies = new ObservableCollection<BookCopyDetail>();
            _loginUserAccount = LoginUserAccountStore.Instance();
            BookCopiesQuery();
        }

        // 메소드 --------------------------------------------------------------------
        private void BookCopiesQuery()
        {
            _bookCopies.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookCopyDetail> queriedBookCopies = db.BookCopies
                    .Include(x => x.Book).Include(x => x.LoanStatus).Include(x => x.LoanHistories)
                    .Where(x => x.BookId == SelectedBook.Id)
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

        // 커멘드 ---------------------------------------------------------------------
        [RelayCommand] private void LoanbtnClick()
        {
            if (_selectedBookCopy == null)
            {
                MessageBox.Show("대출 혹은 예약하실 도서를 선택하십시오.");
            }
            else
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    var CurrentLoans = db.LoanHistories.Include(x => x.User)
                        .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                        .Select(x => new { x.LoanDueDate, x.BookCopy }).ToList();

                    foreach (var loan in CurrentLoans)
                    {
                        if (loan.LoanDueDate < DateOnly.FromDateTime(DateTime.Now))
                        {
                            MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 대출 가능합니다.");
                            return;
                        }
                        else if (loan.BookCopy.BookId == _selectedBookCopy.BookId)
                        {
                            MessageBox.Show("이미 대출하신 도서입니다.");
                            return;
                        }
                    }

                    if (_selectedBookCopy.CurrentLoanStatusId == 1)
                    {
                        BookCopy loanBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == _selectedBookCopy.BookCopyId);
                        loanBookCopy.LoanStatusId = 2;
                        LoanHistory loanHistory = new LoanHistory();
                        loanHistory.BookCopyId = _selectedBookCopy.BookCopyId;
                        loanHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
                        loanHistory.LoanDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
                        loanHistory.LoanDueDate = DateOnly.FromDateTime((DateTime)DateTime.Now.AddDays(14));
                        loanHistory.ExtensionCount = 0;
                        db.LoanHistories.Add(loanHistory);
                        db.SaveChanges();

                        MessageBox.Show($"대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
                        BookCopiesQuery();
                    }
                    else if (_selectedBookCopy.CurrentLoanStatusId == 2)
                    {
                        if (MessageBox.Show("이미 대출된 도서입니다.\r\n예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            if (db.ReservationHistories.FirstOrDefault(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id) != null)
                            {
                                MessageBox.Show("이미 예약하신 도서입니다.");
                                return;
                            }

                            var preReservations = db.ReservationHistories.Where(x => x.BookCopyId == _selectedBookCopy.BookCopyId)
                                .Select(x => new { x.BookCopyId, x.Priority });

                            ReservationHistory reservationHistory = new ReservationHistory();
                            if (preReservations.Count() > 0)
                            {
                                var lastReservation = preReservations.OrderByDescending(x => x.Priority).First();
                                
                                reservationHistory.BookCopyId = _selectedBookCopy.BookCopyId;
                                reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
                                reservationHistory.ReservationDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
                                reservationHistory.ReservationDueDate = null;
                                reservationHistory.Priority = (byte)(lastReservation.Priority + 1);
                                db.ReservationHistories.Add(reservationHistory);
                            }
                            else
                            {
                                reservationHistory.BookCopyId = _selectedBookCopy.BookCopyId;
                                reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
                                reservationHistory.ReservationDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
                                reservationHistory.ReservationDueDate = null;
                                reservationHistory.Priority = 1;
                                db.ReservationHistories.Add(reservationHistory);
                            }

                            db.SaveChanges();
                            MessageBox.Show($"예약되었습니다.\r\n {reservationHistory.Priority}번째 순번입니다.");
                            BookCopiesQuery();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("이미 예약된 도서입니다.\r\n다음 순서로 예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            if (db.ReservationHistories.FirstOrDefault(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id) != null)
                            {
                                MessageBox.Show("이미 예약하신 도서입니다.");
                                return;
                            }

                            var preReservations = db.ReservationHistories.Where(x => x.BookCopyId == _selectedBookCopy.BookCopyId)
                                .Select(x => new { x.BookCopyId, x.ReservationDueDate, x.Priority });

                            ReservationHistory reservationHistory = new ReservationHistory();

                            if (preReservations.Count() > 0)
                            {
                                var lastReservation = preReservations.OrderByDescending(x => x.Priority).First();
                                
                                reservationHistory.BookCopyId = _selectedBookCopy.BookCopyId;
                                reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
                                reservationHistory.ReservationDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
                                DateOnly lastReservationDueDate = (DateOnly)(lastReservation.ReservationDueDate);
                                reservationHistory.ReservationDueDate = lastReservationDueDate.AddDays(3);
                                reservationHistory.Priority = (byte)(lastReservation.Priority + 1);
                                db.ReservationHistories.Add(reservationHistory);
                            }
                            else
                            {
                                reservationHistory.BookCopyId = _selectedBookCopy.BookCopyId;
                                reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
                                reservationHistory.ReservationDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
                                reservationHistory.ReservationDueDate = reservationHistory.ReservationDate.AddDays(3);
                                reservationHistory.Priority = 1;
                                db.ReservationHistories.Add(reservationHistory);
                            }
                            db.SaveChanges();
                            MessageBox.Show($"예약되었습니다.\r\n {reservationHistory.Priority}번째 순번입니다.");
                            BookCopiesQuery();
                        }
                    }
                }
            }
        }
    }
}
