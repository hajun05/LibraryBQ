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
                    MessageBox.Show("대출 반납이 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("반납할 도서를 선택해 주십시오.");
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
            if (seletedReservationHistory != null)
            {
                // 현재 사용자가 1순위 예약자일 경우 도서 대출
                if (seletedReservationHistory.Priority == 1)
                {
                    using (LibraryBQContext db = new LibraryBQContext())
                    {
                        BookCopyDetail selectedBookCopy = new BookCopyDetail(db.BookCopies.FirstOrDefault(x => x.Id == seletedReservationHistory.BookCopyId));
                        LoanBookCopy(db, selectedBookCopy);
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
                List<string> overdueBooks = new List<string>();
                List<CurrentLoanHistoryDetail> answer = db.LoanHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
                    .Select(x => new CurrentLoanHistoryDetail()
                    {
                        BookCopyId = x.BookCopyId,
                        BookId = x.BookCopy.BookId,
                        Title = x.BookCopy.Book.Title,
                        Author = x.BookCopy.Book.Author,
                        ClassificationNumber = String.Format($"{x.BookCopy.BookId}-{x.BookCopy.Id}"),
                        CurrentLoanHistoryId = x.Id,
                        CurrentLoanStatusId = x.BookCopy.LoanStatusId,
                        CurrentLoanDueDate = x.LoanDueDate,
                        CurrentLoanUserId = x.UserId,
                    }).ToList();

                CurrentLoanHistories.Clear();
                foreach (CurrentLoanHistoryDetail answerItem in answer)
                {
                    CurrentLoanHistories.Add(answerItem);
                    if (answerItem.CurrentLoanDueDate > DateOnly.FromDateTime((DateTime.Now)))
                        overdueBooks.Add(answerItem.Title);
                }

                if (overdueBooks.Count > 0)
                {
                    string overdueString = "연체되신 도서가 있습니다.\r\n";
                    foreach (string title in overdueBooks)
                        overdueString += String.Format($" {title}\r\n");
                    MessageBox.Show($"{overdueString}", "안내", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    .Select(x => new CurrentReservationHistoryDetail()
                    {
                        BookCopyId = x.BookCopyId,
                        BookId = x.BookCopy.BookId,
                        Title = x.BookCopy.Book.Title,
                        Author = x.BookCopy.Book.Author,
                        ClassificationNumber = String.Format($"{x.BookCopy.BookId}-{x.BookCopy.Id}"),
                        CurrentReservationHistoryId = x.Id,
                        CurrentLoanStatusId = x.BookCopy.LoanStatusId,
                        CurrentReservationDueDate = x.ReservationDueDate,
                        CurrentReservationUserId = x.UserId,
                        Priority = x.Priority,
                    }).ToList();

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
