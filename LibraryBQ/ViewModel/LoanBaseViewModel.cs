using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Model;
using LibraryBQ.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    public partial class LoanBaseViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 --------------------------------------------------------------
        private ObservableCollection<BookCopyDetail> _bookCopies;
        private LoginUserAccountStore _loginUserAccount;

        public ObservableCollection<BookCopyDetail> BookCopies
        {
            get { return _bookCopies; }
            set => SetProperty(ref _bookCopies, value);
        }
        public LoginUserAccountStore LoginUserAccount
        {
            get => _loginUserAccount;
            set => SetProperty(ref _loginUserAccount, value);
        }

        // 메소드 -----------------------------------------------------------------------
        // '예약 목록' 삭제시 삭제된 예약과 같은 부수를 예약한 이력 목록의 우선순위 조정
        protected void AdjustRangePriorityByDelete(List<ReservationHistory> deleteReservations, LibraryBQContext db)
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
        protected void AdjustPriorityByDelete(ReservationHistory? deleteReservation, LibraryBQContext db)
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
        protected ReservationHistory NewReservationHistory(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            var lastReservationPriority = db.ReservationHistories
                                .Where(x => x.BookCopyId == selectedBookCopy.BookCopyId).Select(x => new { x.Priority })
                                .OrderByDescending(x => x.Priority).FirstOrDefault();

            ReservationHistory reservationHistory = new ReservationHistory();
            reservationHistory.BookCopyId = selectedBookCopy.BookCopyId;
            reservationHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
            reservationHistory.ReservationDate = DateOnly.FromDateTime(DateTime.Now);
            reservationHistory.ReservationDueDate = null;
            reservationHistory.Priority = (lastReservationPriority != null) ? (byte)(lastReservationPriority.Priority + 1) : (byte)1;

            return reservationHistory;
        }
        // 새 대출 이력 생성
        protected LoanHistory NewLoanHistory(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            LoanHistory loanHistory = new LoanHistory();
            loanHistory.BookCopyId = selectedBookCopy.BookCopyId;
            loanHistory.UserId = _loginUserAccount.CurrentLoginUserAccount.Id;
            loanHistory.LoanDate = DateOnly.FromDateTime((DateTime)DateTime.Now);
            loanHistory.LoanDueDate = DateOnly.FromDateTime((DateTime)DateTime.Now.AddDays(14));
            loanHistory.ExtensionCount = 0;

            return loanHistory;
        }
        // 대출 및 예약 신청시 다른 대출 가능한 도서의 존재 여부 확인
        protected bool IsAvailableOtherBookCopy()
        {
            bool hasAvailableBookCopy = _bookCopies.Any(x => x.CurrentLoanStatusId == 1);
            if (hasAvailableBookCopy)
            {
                MessageBox.Show($"대출 가능한 다른 도서를 선택하십시오.");
            }
            return hasAvailableBookCopy;
        }
        // 예약 신청시 이미 예약한 도서의 존재 여부 확인
        protected bool HasReservationBookCopy(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            bool hasReservation = db.ReservationHistories.Include(x => x.BookCopy)
                            .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id)
                            .Any(x => x.BookCopy.BookId == selectedBookCopy.BookId);
            if (hasReservation)
            {
                MessageBox.Show("이미 예약하신 도서입니다.");
            }

            return hasReservation;
        }
        // 도서 대출 실행
        protected void LoanBookCopy(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            LoanHistory loanHistory = NewLoanHistory(db, selectedBookCopy);

            // 대출 성공시 대출한 도서 예약 이력 삭제 및 예약 순번 조정
            ReservationHistory? SameBookReservation = db.ReservationHistories.Include(x => x.BookCopy)
                .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id)
                .FirstOrDefault(x => x.BookCopy.BookId == selectedBookCopy.BookId);
            AdjustPriorityByDelete(SameBookReservation, db);

            db.LoanHistories.Add(loanHistory);

            BookCopy loanBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == selectedBookCopy.BookCopyId);
            if (loanBookCopy.LoanStatusId == 1)
                MessageBox.Show($"도서가 대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
            else
                MessageBox.Show($"예약하신 도서가 대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
            loanBookCopy.LoanStatusId = 2;

            db.SaveChanges();
        }
        // 도서 예약 실행
        protected void ReserveBookCopy(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            if (HasReservationBookCopy(db, selectedBookCopy))
                return;

            ReservationHistory reservationHistory = NewReservationHistory(db, selectedBookCopy);

            db.ReservationHistories.Add(reservationHistory);
            db.SaveChanges();

            MessageBox.Show($"예약되었습니다.\r\n {reservationHistory.Priority}번째 순번입니다.");
        }

    }
}
