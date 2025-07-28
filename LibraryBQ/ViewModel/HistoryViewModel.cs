using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    // HistoryViewModel: 로그인 사용자의 대출 및 예약 이력 화면 ViewModel (MVVM, WPF)
    // - 사용자의 현재 대출 중인 도서 내역과 예약 내역을 ObservableCollection으로 관리하여 UI에 바인딩
    // - 대출 반납, 연장, 예약 취소, 예약 대출 등 사용자 동작과 DB 업데이트 관련 커맨드 구현
    // - 대출 상태 변경 시 예약자 순번 조정 및 도서 상태 반영
    // - 연체 여부, 대출 한도 검사 등 업무 규칙에 따라 UI 동작 제어 및 메시지 출력 책임
    // - 베이스 클래스인 LoanBaseViewModel에서 필수 기능 상속받아 재사용성 극대화
    //
    // [주요 역할]
    // - 대출/예약 내역 조회 및 UI 동기화
    // - 사용자 액션(반납, 연장, 취소, 예약 대출) 처리 및 DB 반영
    // - 복잡한 업무 프로세스 처리 및 사용자 피드백 제공
    public partial class HistoryViewModel : LoanBaseViewModel
    {
        // 필드 및 프로퍼티 ---------------------------------------------
        // UI에 표시할 현재 대출 이력 목록 (사용자 단위)
        private ObservableCollection<CurrentLoanHistoryDetail> _currentLoanHistories;
        // UI에 표시할 현재 예약 이력 목록 (사용자 단위)
        private ObservableCollection<CurrentReservationHistoryDetail> _currentReservationHistories;

        public ObservableCollection<CurrentLoanHistoryDetail> CurrentLoanHistories
        {
            get { return _currentLoanHistories; }
            set => SetProperty(ref _currentLoanHistories, value);
        }
        public ObservableCollection<CurrentReservationHistoryDetail> CurrentReservationHistories
        {
            get { return _currentReservationHistories; }
            set => SetProperty(ref _currentReservationHistories, value);
        }

        // 생성자 ------------------------------------------------------
        // 두 컬렉션 및 사용자 정보, BookCopies 초기화(상속한 베이스와 연동)
        public HistoryViewModel()
        {
            CurrentLoanHistories = new ObservableCollection<CurrentLoanHistoryDetail>();
            CurrentReservationHistories = new ObservableCollection<CurrentReservationHistoryDetail>();
            // LoanBaseViewModel에서 상속받은 프로퍼티 초기화
            LoginUserAccount = LoginUserAccountStore.Instance();
            BookCopies = new ObservableCollection<BookCopyDetail>();
        }

        // 커멘드 ------------------------------------------------------
        // 도서 반납 버튼 클릭 커맨드 (선택 이력 반납 처리)
        [RelayCommand] private void ReturnbtnClick(CurrentLoanHistoryDetail selectedLoanHistory)
        {
            if (selectedLoanHistory != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    // 대출 이력에서 선택된 데이터 조회 및 반납일 지정
                    LoanHistory returnLoan = db.LoanHistories.FirstOrDefault(x => x.Id == selectedLoanHistory.CurrentLoanHistoryId);
                    returnLoan.ReturnDate = DateOnly.FromDateTime(DateTime.Now);

                    // 해당 부수(BookCopy) 정보 및 예약 상태 변경 처리
                    BookCopy returnBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == selectedLoanHistory.BookCopyId);

                    List<ReservationHistory> reservationHistories = db.ReservationHistories
                        .Where(x => x.BookCopyId == selectedLoanHistory.BookCopyId).OrderBy(x => x.Priority).ToList();
                    if (reservationHistories.Count > 0)
                    {
                        // 예약자가 있을 경우, 예약자별 대출가능일(reservationDueDate) 순차 지정
                        for (int i = 0; i < reservationHistories.Count; i++)
                        {
                            reservationHistories[i].ReservationDueDate = DateOnly.FromDateTime(DateTime.Now).AddDays(i * 3);
                        }
                        returnBookCopy.LoanStatusId = 3; // 예약중 상태
                    }
                    else
                    {
                        returnBookCopy.LoanStatusId = 1; // 대출가능 상태
                    }

                    // 뷰 바인딩 데이터 즉시 갱신
                    CurrentLoanHistories.Remove(selectedLoanHistory);
                    db.SaveChanges();

                    // 연체 상태 등 갱신(로그인 정보 갱신)
                    LoginUserAccount.CheckHasOverdueLoan(db);

                    MessageBox.Show("대출 반납이 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("반납할 도서를 선택해 주십시오.");
            }
        }

        // 대출 연장 요청 커맨드
        [RelayCommand] private void ExtensionbtnClick(CurrentLoanHistoryDetail selectedLoanHistory) // 대출 연장 커멘드
        {
            // 연체 사용자는 연장 불가
            if (LoginUserAccount.HasOverdueLoan)
            {
                MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 연장 가능합니다.");
                return;
            }
            if (selectedLoanHistory != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    LoanHistory extensionLoanHistory = db.LoanHistories.FirstOrDefault(x => x.Id == selectedLoanHistory.Id);
                    if (extensionLoanHistory.ExtensionCount > 2)
                    {
                        MessageBox.Show("연장 한도를 초과했습니다.");
                        return;
                    }
                    // 반납 예정일을 2주 연장, 연장 횟수 증가
                    extensionLoanHistory.LoanDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14));
                    extensionLoanHistory.ExtensionCount += 1;

                    // 화면(바인딩) 데이터도 동기화
                    CurrentLoanHistoryDetail extensionCurrentLoan = CurrentLoanHistories.First(x => x.Id == extensionLoanHistory.Id);
                    extensionCurrentLoan.CurrentLoanDueDate = extensionLoanHistory.LoanDueDate;
                    extensionCurrentLoan.ExtensionCount += 1;

                    db.SaveChanges();
                    MessageBox.Show("대출 연장이 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("연장할 도서를 선택해 주십시오.");
            }
        }

        // 예약 취소 커맨드
        [RelayCommand] private void CancelbtnClick(CurrentReservationHistoryDetail seletedReservationHistory)
        {
            if (seletedReservationHistory != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    ReservationHistory cancelReservation = db.ReservationHistories
                        .FirstOrDefault(x => x.Id == seletedReservationHistory.CurrentReservationHistoryId);

                    // 취소할 예약에서 예약내 우선순위 조정에 사용할 기준 추출
                    var adjustCriteria = new { cancelReservation.BookCopyId, cancelReservation.Priority };

                    // 해당 예약 이력 삭제
                    db.ReservationHistories.Remove(cancelReservation);

                    // 동일 도서 부수 예약내 우선순위 조정(삭제된 예약보다 뒤 순위는 -1)
                    var targets = db.ReservationHistories
                        .Where(x => x.BookCopyId == adjustCriteria.BookCopyId && x.Priority > adjustCriteria.Priority);

                    if (targets.Count() > 0)
                    {
                        foreach (var t in targets)
                            t.Priority -= 1;
                    }
                    else
                    {
                        // 예약 없으면 해당 부수는 대출가능 상태로 전환
                        BookCopy cancelBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == adjustCriteria.BookCopyId);
                        if (cancelBookCopy.LoanStatusId == 3)
                            cancelBookCopy.LoanStatusId = 1;
                    }

                    // 바인딩 컬렉션 갱신
                    CurrentReservationHistories.Remove(seletedReservationHistory);
                    db.SaveChanges();
                    MessageBox.Show("예약 취소가 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("취소할 예약을 선택해 주십시오.");
            }
        }

        // 예약 대출(1순위 예약자의 도서 대출 처리) 커맨드
        [RelayCommand] private void LoanbtnClick(CurrentReservationHistoryDetail seletedReservationHistory)
        {
            // 연체 사용자는 대출 불가
            if (LoginUserAccount.HasOverdueLoan)
            {
                MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 대출 가능합니다.");
                return;
            }
            if (seletedReservationHistory != null)
            {
                // 아직 반납되지 않은 도서는 대출 불가
                if (seletedReservationHistory.CurrentLoanStatusId == 2)
                {
                    MessageBox.Show("도서가 아직 반납되지 않았습니다.");
                    return;
                }

                // 현재 사용자가 1순위 예약자일 경우 도서 대출
                if (seletedReservationHistory.Priority == 1)
                {
                    using (LibraryBQContext db = new LibraryBQContext())
                    {
                        BookCopyDetail selectedBookCopy = new BookCopyDetail(db.BookCopies.FirstOrDefault(x => x.Id == seletedReservationHistory.BookCopyId));
                        LoanBookCopy(db, selectedBookCopy); // 도서 대출 실행

                        LoanHistoriesQuery(); // 대출 목록 갱신 (재조회)
                        CurrentReservationHistories.Remove(seletedReservationHistory); // 예약 목록 갱신
                    }
                }
                else
                {
                    MessageBox.Show($"아직 첫번째 차례가 아닙니다.\r\n현재 {seletedReservationHistory.Priority}번째 순서입니다.");
                }
            }
            else
            {
                MessageBox.Show("대출할 예약 도서를 선택해 주십시오.");
            }
        }

        // 메소드 ------------------------------------------------------
        // 로그인된 사용자의 대출 이력(미반납 중) 목록 조회/갱신
        public void LoanHistoriesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<CurrentLoanHistoryDetail> answer = db.LoanHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                    .Select(x => new CurrentLoanHistoryDetail(x)).ToList();

                // 컬렉션 바인딩 값 갱신
                CurrentLoanHistories.Clear();
                foreach (CurrentLoanHistoryDetail answerItem in answer)
                {
                    CurrentLoanHistories.Add(answerItem);
                }
            }
        }

        // 로그인된 사용자의 예약 이력 목록 조회/갱신
        public void ReservationHistoriesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<CurrentReservationHistoryDetail> answer = db.ReservationHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id)
                    .Select(x => new CurrentReservationHistoryDetail(x)).ToList();

                CurrentReservationHistories.Clear();
                foreach (CurrentReservationHistoryDetail answerItem in answer)
                {
                    CurrentReservationHistories.Add(answerItem);
                }
            }
        }

        // 뷰모델 컬렉션 초기화(초기 상태/로그인 변경 시 등)
        public void HistoryClear()
        {
            CurrentLoanHistories.Clear();
            CurrentReservationHistories.Clear();
        }
    }
}
