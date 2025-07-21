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
    public partial class HistoryViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 ---------------------------------------------
        private ObservableCollection<CurrentLoanHistoryDetail> _currentLoanHistories;
        private ObservableCollection<CurrentReservationHistoryDetail> _currentReservationHistories;
        private LoginUserAccountStore _loginUserAccount;

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
        public LoginUserAccountStore LoginUserAccount
        {
            get => _loginUserAccount;
            set => SetProperty(ref _loginUserAccount, value);
        }

        // 생성자 ------------------------------------------------------
        public HistoryViewModel()
        {
            _loginUserAccount = LoginUserAccountStore.Instance();
            CurrentLoanHistories = new ObservableCollection<CurrentLoanHistoryDetail>();
            CurrentReservationHistories = new ObservableCollection<CurrentReservationHistoryDetail>();
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
                    MessageBox.Show("반납이 완료되었습니다.");
                }
            }
            else
            {
                MessageBox.Show("반납할 도서를 선택해 주십시오.");
            }
        }

        // 메소드 ------------------------------------------------------
        public void LoanHistoriesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<CurrentLoanHistoryDetail> answer = db.LoanHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id && x.ReturnDate == null)
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
                }
            }
        }

        public void ReservationHistoriesQuery()
        {
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<CurrentReservationHistoryDetail> answer = db.ReservationHistories
                    .Include(x => x.BookCopy).ThenInclude(x => x.Book)
                    .Where(x => x.UserId == _loginUserAccount.CurrentLoginUserAccount.Id)
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
