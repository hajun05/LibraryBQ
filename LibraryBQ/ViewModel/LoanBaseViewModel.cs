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
    // LoanBaseViewModel: 도서 대출 및 예약 관련 공통 로직을 제공하는 베이스 ViewModel (MVVM, WPF)
    // - 여러 ViewModel이 공통으로 사용할 수 있는 대출/예약 핵심 기능 집합 제공
    // - 예약 우선순위 조정, 대출 이력과 예약 이력 생성, 조건 검증(연체, 대출 한도, 예약 중복 확인), 실제 DB 저장 처리 수행
    // - 로그인 사용자 정보(LoginUserAccountStore)를 활용해 사용자 상태 및 권한 관리
    // - BookCopies 컬렉션과 관련된 비즈니스 로직 추상화 및 재사용 용이성 제공
    // [주요 역할]
    // - 예약 이력 삭제에 따른 우선순위 재조정 기능 제공
    // - 예약 및 대출 이력 객체 생성 및 DB 반영 메서드 제공
    // - 대출 가능 여부, 연체 상태, 대출 한도 초과 여부 등의 검증 기능 포함
    // - 대출 및 예약 수행 시 관련 UI 알림, 상태 업데이트 처리
    public partial class LoanBaseViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 --------------------------------------------------------------
        // 도서 부수(BookCopy) 정보들의 ObservableCollection (자식 뷰모델에서 바인딩/활용)
        private ObservableCollection<BookCopyDetail> _bookCopies;
        // 현재 로그인 사용자 정보 저장 및 관련 상태 확인용
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
        // 예약목록에서 여러 개를 삭제할 때, 같은 부수(BookCopy)에 대한 모든 예약 이력의 우선순위를 밀어줌
        // 삭제건보다 뒤(더 높은 숫자) Priority들은 1씩 감소
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
        // 예약목록에서 하나를 삭제할 때, ""
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
        // 새 예약 이력을 생성 (마지막 우선순위 다음 번호로 할당)
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
        // 새 대출 이력 생성 (대출일 = 오늘, 반납예정일 = 오늘 + 14일)
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
        // 현재 도서 이외에 대출 가능한 부수가 있는지 확인(있으면 true, 없으면 false)
        protected bool IsAvailableOtherBookCopy()
        {
            bool hasAvailableBookCopy = _bookCopies.Any(x => x.CurrentLoanStatusId == 1);
            if (hasAvailableBookCopy)
            {
                MessageBox.Show($"대출 가능한 다른 도서를 선택하십시오.");
            }
            return hasAvailableBookCopy;
        }
        // 똑같은 도서(Book)에 대해 이미 사용자가 예약한 건이 있는지 확인
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
        // 대출/예약 전 금지 조건(연체, 한도초과, 이미 대출 등) 체크 (하나라도 true리턴시 진행 불가)
        protected bool CheckCanLoan(LibraryBQContext db, BookCopyDetail selectedBookCopy)
        {
            if (_loginUserAccount.HasOverdueLoan)
            {
                MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 대출 가능합니다.");
                return false;
            }

            var currentUserLoans = db.LoanHistories.Include(x => x.User).Where(x => x.ReturnDate == null && x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id);

            if (_loginUserAccount.CurrentLoginUserAccount.MaxLoanNum <= currentUserLoans.Count())
            {
                MessageBox.Show("대출 한도를 초과했습니다.");
                return false;
            }

            // 이미 같은 도서를 대출 중인지 확인
            var hasAlready = currentUserLoans.Any(x => x.BookCopy.BookId == selectedBookCopy.BookId);

            if (hasAlready)
            {
                MessageBox.Show("이미 대출하신 도서입니다.");
                return false;
            }

            return true;
        }

        // 도서 대출 로직: 대출 이력 추가, 예약 이력 삭제(있었다면 우선순위 반영), 부수 상태 변경, 메시지 출력
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
            else if (loanBookCopy.LoanStatusId == 3)
                MessageBox.Show($"예약하신 도서가 대출되었습니다.\r\n반납일은 {loanHistory.LoanDueDate}일 입니다.");
            loanBookCopy.LoanStatusId = 2;

            db.SaveChanges();
        }

        // 도서 예약 로직: 이미 같은 도서의 예약이 있으면 중단, 없으면 예약 이력 추가 및 메시지 출력
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
