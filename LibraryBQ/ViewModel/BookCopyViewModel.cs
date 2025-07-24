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
    public partial class BookCopyViewModel : LoanBaseViewModel
    {
        // 필드 및 프로퍼티 -----------------------------------------------------------
        private Book _selectedBook;
        //private BookCopyDetail _selectedBookCopy;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set => SetProperty(ref _selectedBook, value);
        }
        
        //public BookCopyDetail SelectedBookCopy
        //{
        //    get { return _selectedBookCopy; }
        //    set => SetProperty(ref _selectedBookCopy, value);
        //}

        // 생성자 --------------------------------------------------------------------
        public BookCopyViewModel(Book selectedBook)
        {
            _selectedBook = selectedBook;
            // LoanBaseViewModel에서 상속받은 프로퍼티
            BookCopies = new ObservableCollection<BookCopyDetail>();
            LoginUserAccount = LoginUserAccountStore.Instance();
            BookCopiesQuery();
        }

        // 메소드 --------------------------------------------------------------------
        // 선택한 도서의 부수 목록 조회
        private void BookCopiesQuery()
        {
            BookCopies.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookCopyDetail> queriedBookCopies = db.BookCopies
                    .Include(x => x.Book).Include(x => x.LoanHistories).Include(x => x.ReservationHistories)
                    .Where(x => x.BookId == SelectedBook.Id)
                    .Select(x => new BookCopyDetail(x)).ToList();

                foreach (BookCopyDetail bookCopy in queriedBookCopies)
                    BookCopies.Add(bookCopy);
            }
        }

        // 커멘드 ---------------------------------------------------------------------
        [RelayCommand] private void LoanbtnClick(BookCopyDetail? selectedBookCopy)
        {
            if (selectedBookCopy == null)
            {
                MessageBox.Show("대출 혹은 예약하실 도서를 선택하십시오.");
                return;
            }

            using (LibraryBQContext db = new LibraryBQContext())
            {
                // 현재 사용자의 대출 이력 조회
                var CurrentLoans = db.LoanHistories.Include(x => x.User)
                    .Where(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                    .Select(x => new { x.LoanDueDate, x.BookCopy }).ToList();

                // 대출 및 예약이 불가능한 경우 탐지
                foreach (var loan in CurrentLoans)
                {
                    if (loan.LoanDueDate < DateOnly.FromDateTime(DateTime.Now))
                    {
                        MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 대출 가능합니다.");
                        return;
                    }
                    else if (loan.BookCopy.BookId == selectedBookCopy.BookId)
                    {
                        MessageBox.Show("이미 대출하신 도서입니다.");
                        return;
                    }
                }

                if (selectedBookCopy.CurrentLoanStatusId == 1) // 대출가능 도서
                {
                    LoanBookCopy(db, selectedBookCopy);
                    BookCopiesQuery();
                }
                else if (selectedBookCopy.CurrentLoanStatusId == 2) // 대출중 도서
                {
                    if (IsAvailableOtherBookCopy())
                        return;

                    if (MessageBox.Show("모든 도서가 대출중입니다.\r\n예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ReserveBookCopy(db, selectedBookCopy);
                        BookCopiesQuery();
                    }
                }
                else // 예약중 도서
                {
                    ReservationHistory? currentUserSeletedBookCopyReservation = selectedBookCopy.CurrentReservations
                        .FirstOrDefault(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id);

                    if (currentUserSeletedBookCopyReservation != null)
                    {
                        // 현재 사용자가 1순위 예약자일 경우 도서 대출
                        if (currentUserSeletedBookCopyReservation.Priority == 1)
                        {
                            LoanBookCopy(db, selectedBookCopy);
                            BookCopiesQuery();
                        }
                        else
                        {
                            MessageBox.Show($"아직 첫번째 차례가 아닙니다.\r\n현재 {currentUserSeletedBookCopyReservation.Priority}번째 순서입니다.");
                        }
                    }
                    else
                    {
                        if (IsAvailableOtherBookCopy())
                            return;

                        if (MessageBox.Show("이미 예약된 도서입니다.\r\n다음 순서로 예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            ReserveBookCopy(db, selectedBookCopy);
                            BookCopiesQuery();
                        }
                    }
                }
            }
        }
    }
}
