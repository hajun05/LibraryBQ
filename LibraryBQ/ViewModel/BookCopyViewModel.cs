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
        // 선택한 도서의 부수 목록 조회
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
        // '예약 목록' 삭제시 삭제된 예약과 같은 부수를 예약한 이력 목록의 우선순위 조정
        private void AdjustRangePriorityByDelete(List<ReservationHistory> deleteReservations, LibraryBQContext db)
        {
            if (deleteReservations.Count > 0)
            {
                // 조회한 예약 이력중 부수 번호와 예약 우선 순위 추출
                var adjustCriterias = deleteReservations
                .Select(x => new { x.BookCopyId, x.Priority });

                db.ReservationHistories.RemoveRange(deleteReservations);

                // 삭제한 예약과 같은 도서(부수)를 예약한 이력들의 예약 우선순위 조정
                foreach (var criteria in adjustCriterias)
                {
                    var targets = db.ReservationHistories
                        .Where(x => x.BookCopyId == criteria.BookCopyId && x.Priority > criteria.Priority);

                    foreach (var t in targets)
                        t.Priority -= 1;
                }
            }
        }
        // '단일 예약' 삭제시 삭제된 예약과 같은 부수를 예약한 이력 목록의 우선순위 조정
        private void AdjustPriorityByDelete(ReservationHistory? deleteReservation, LibraryBQContext db)
        {
            if (deleteReservation != null)
            {
                var adjustCriteria = new { deleteReservation.BookCopyId, deleteReservation.Priority };

                db.ReservationHistories.Remove(deleteReservation);

                var targets = db.ReservationHistories
                    .Where(x => x.BookCopyId == adjustCriteria.BookCopyId && x.Priority > adjustCriteria.Priority);

                foreach (var t in targets)
                    t.Priority -= 1;
            }
        }
        // 새 예약 이력 생성
        private ReservationHistory NewReservationHistory(LibraryBQContext db)
        {
            var lastReservationPriority = db.ReservationHistories
                                .Where(x => x.BookCopyId == _selectedBookCopy.BookCopyId).Select(x => new { x.Priority })
                                .OrderByDescending(x => x.Priority).FirstOrDefault();

            ReservationHistory reservationHistory = new ReservationHistory();
            reservationHistory.BookCopyId = _selectedBookCopy.BookCopyId;
            reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
            reservationHistory.ReservationDate = DateOnly.FromDateTime(DateTime.Now);
            reservationHistory.ReservationDueDate = null;
            reservationHistory.Priority = (lastReservationPriority != null) ? (byte)(lastReservationPriority.Priority + 1) : (byte)1;

            return reservationHistory;
        }
        // 새 대출 이력 생성
        private LoanHistory NewLoanHistory(LibraryBQContext db)
        {
            LoanHistory loanHistory = new LoanHistory();
            loanHistory.BookCopyId = _selectedBookCopy.BookCopyId;
            loanHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
            loanHistory.LoanDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
            loanHistory.LoanDueDate = DateOnly.FromDateTime((DateTime)DateTime.Now.AddDays(14));
            loanHistory.ExtensionCount = 0;

            return loanHistory;
        }
        // 대출 및 예약 신청시 다른 대출 가능한 도서의 존재 여부 확인
        private bool IsAvailableOtherBookCopy()
        {
            bool hasAvailableBookCopy = _bookCopies.Any(x => x.CurrentLoanStatusId == 1);
            if (hasAvailableBookCopy)
            {
                MessageBox.Show($"대출 가능한 다른 도서를 선택하십시오.");
            }
            return hasAvailableBookCopy;
        }
        // 예약 신청시 이미 예약한 도서의 존재 여부 확인
        private bool HasReservationBookCopy(LibraryBQContext db)
        {
            bool hasReservation = db.ReservationHistories.Include(x => x.BookCopy)
                            .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id)
                            .Any(x => x.BookCopy.BookId == _selectedBookCopy.BookId);
            if (hasReservation)
            {
                MessageBox.Show("이미 예약하신 도서입니다.");
            }

            return hasReservation;
        }
        // 도서 대출 실행
        private void LoanBookCopy(LibraryBQContext db)
        {
            LoanHistory loanHistory = NewLoanHistory(db);

            // 대출 성공시 대출한 도서 예약 이력 삭제 및 예약 순번 조정
            ReservationHistory? SameBookReservation = db.ReservationHistories.Include(x => x.BookCopy)
                .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id)
                .FirstOrDefault(x => x.BookCopy.BookId == _selectedBookCopy.BookId);
            AdjustPriorityByDelete(SameBookReservation, db);

            db.LoanHistories.Add(loanHistory);

            BookCopy loanBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == _selectedBookCopy.BookCopyId);
            if (loanBookCopy.LoanStatusId == 1)
                MessageBox.Show($"도서가 대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
            else
                MessageBox.Show($"예약하신 도서가 대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
            loanBookCopy.LoanStatusId = 2;

            db.SaveChanges();
        }
        // 도서 예약 실행
        private void ReserveBookCopy(LibraryBQContext db)
        {
            if (HasReservationBookCopy(db))
                return;

            ReservationHistory reservationHistory = NewReservationHistory(db);

            db.ReservationHistories.Add(reservationHistory);
            db.SaveChanges();

            MessageBox.Show($"예약되었습니다.\r\n {reservationHistory.Priority}번째 순번입니다.");
        }

        // 커멘드 ---------------------------------------------------------------------
        [RelayCommand] private void LoanbtnClick()
        {
            if (_selectedBookCopy == null)
            {
                MessageBox.Show("대출 혹은 예약하실 도서를 선택하십시오.");
                return;
            }

            using (LibraryBQContext db = new LibraryBQContext())
            {
                // 현재 사용자의 대출 이력 조회
                var CurrentLoans = db.LoanHistories.Include(x => x.User)
                    .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                    .Select(x => new { x.LoanDueDate, x.BookCopy }).ToList();

                // 대출 및 예약이 불가능한 경우 탐지
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

                if (_selectedBookCopy.CurrentLoanStatusId == 1) // 대출가능 도서
                {
                    LoanBookCopy(db);
                    BookCopiesQuery();
                }
                else if (_selectedBookCopy.CurrentLoanStatusId == 2) // 대출중 도서
                {
                    if (IsAvailableOtherBookCopy())
                        return;

                    if (MessageBox.Show("모든 도서가 대출중입니다.\r\n예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ReserveBookCopy(db);
                        BookCopiesQuery();
                    }
                }
                else // 예약중 도서
                {
                    ReservationHistory? currentUserSeletedBookCopyReservation = _selectedBookCopy.CurrentReservations
                        .FirstOrDefault(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id);

                    if (currentUserSeletedBookCopyReservation != null)
                    {
                        // 현재 사용자가 1순위 예약자일 경우 도서 대출
                        if (currentUserSeletedBookCopyReservation.Priority == 1)
                        {
                            LoanBookCopy(db);
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
                            ReserveBookCopy(db);
                            BookCopiesQuery();
                        }
                    }
                }
            }
        }
    }
}
