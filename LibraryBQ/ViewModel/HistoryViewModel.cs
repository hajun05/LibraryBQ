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
    public partial class HistoryViewModel : LoanBaseViewModel
    {
        // 필드 및 프로퍼티 ---------------------------------------------
        private ObservableCollection<CurrentLoanHistoryDetail> _currentLoanHistories;
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
        public HistoryViewModel()
        {
            CurrentLoanHistories = new ObservableCollection<CurrentLoanHistoryDetail>();
            CurrentReservationHistories = new ObservableCollection<CurrentReservationHistoryDetail>();
            // LoanBaseViewModel에서 상속받은 프로퍼티
            LoginUserAccount = LoginUserAccountStore.Instance();
            BookCopies = new ObservableCollection<BookCopyDetail>();
        }

        // 커멘드 ------------------------------------------------------
        [RelayCommand] private void ReturnbtnClick(CurrentLoanHistoryDetail selectedLoanHistory)
        {
            if (selectedLoanHistory != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                { 
                    LoanHistory returnLoan = db.LoanHistories.FirstOrDefault(x => x.Id == selectedLoanHistory.CurrentLoanHistoryId);
                    returnLoan.ReturnDate = DateOnly.FromDateTime(DateTime.Now);

                    BookCopy returnBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == selectedLoanHistory.BookCopyId);

                    List<ReservationHistory> reservationHistories = db.ReservationHistories
                        .Where(x => x.BookCopyId == selectedLoanHistory.BookCopyId).OrderBy(x => x.Priority).ToList();
                    if (reservationHistories.Count > 0)
                    {
                        for (int i = 0; i < reservationHistories.Count; i++)
                        {
                            reservationHistories[i].ReservationDueDate = DateOnly.FromDateTime(DateTime.Now).AddDays(i * 3);
                        }
                        returnBookCopy.LoanStatusId = 3;
                    }
                    else
                    {
                        returnBookCopy.LoanStatusId = 1;
                    }

                    CurrentLoanHistories.Remove(selectedLoanHistory);
                    db.SaveChanges();

                    LoginUserAccount.CheckHasOverdueLoan(db);

                    MessageBox.Show("대출 반납이 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("반납할 도서를 선택해 주십시오.");
            }
        }

        [RelayCommand] private void ExtensionbtnClick(CurrentLoanHistoryDetail selectedLoanHistory) // 대출 연장 커멘드
        {
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
                    extensionLoanHistory.LoanDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14));
                    extensionLoanHistory.ExtensionCount += 1;

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

        [RelayCommand] private void CancelbtnClick(CurrentReservationHistoryDetail seletedReservationHistory)
        {
            if (seletedReservationHistory != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    ReservationHistory cancelReservation = db.ReservationHistories
                        .FirstOrDefault(x => x.Id == seletedReservationHistory.CurrentReservationHistoryId);

                    var adjustCriteria = new { cancelReservation.BookCopyId, cancelReservation.Priority };

                    db.ReservationHistories.Remove(cancelReservation);

                    var targets = db.ReservationHistories
                        .Where(x => x.BookCopyId == adjustCriteria.BookCopyId && x.Priority > adjustCriteria.Priority);

                    if (targets.Count() > 0)
                    {
                        foreach (var t in targets)
                            t.Priority -= 1;
                    }
                    else
                    {
                        BookCopy cancelBookCopy = db.BookCopies.FirstOrDefault(x => x.Id == adjustCriteria.BookCopyId);
                        if (cancelBookCopy.LoanStatusId == 3)
                            cancelBookCopy.LoanStatusId = 1;
                    }

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

        [RelayCommand] private void LoanbtnClick(CurrentReservationHistoryDetail seletedReservationHistory)
        {
            if (LoginUserAccount.HasOverdueLoan)
            {
                MessageBox.Show("연체된 도서가 있습니다.\r\n연체된 모든 도서를 반납하셔야 대출 가능합니다.");
                return;
            }
            if (seletedReservationHistory != null)
            {
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
                        LoanBookCopy(db, selectedBookCopy);

                        LoanHistoriesQuery();
                        CurrentReservationHistories.Remove(seletedReservationHistory);
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
        public void LoanHistoriesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<CurrentLoanHistoryDetail> answer = db.LoanHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                    .Select(x => new CurrentLoanHistoryDetail(x)).ToList();

                CurrentLoanHistories.Clear();
                foreach (CurrentLoanHistoryDetail answerItem in answer)
                {
                    CurrentLoanHistories.Add(answerItem);
                }
            }
        }

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

        public void HistoryClear()
        {
            CurrentLoanHistories.Clear();
            CurrentReservationHistories.Clear();
        }
    }
}
