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

namespace LibraryBQ.ViewModel
{
    public partial class HistoryViewModel : ObservableObject
    {
        // 필드 및 프로퍼티 ---------------------------------------------
        private ObservableCollection<CurrentLoanHistoryDetail> _currentLoanHistories;
        private ObservableCollection<ReservationHistory> _currentReservationHistories;
        private LoginUserAccountStore _loginUserAccount;

        public ObservableCollection<CurrentLoanHistoryDetail> CurrentLoanHistories
        {
            get { return _currentLoanHistories; }
            set => SetProperty(ref _currentLoanHistories, value);
        }
        public ObservableCollection<ReservationHistory> CurrentReservationHistories
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
            CurrentReservationHistories = new ObservableCollection<ReservationHistory>();
        }

        // 커멘드 ------------------------------------------------------

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

        public void HistoryClear()
        {
            CurrentLoanHistories.Clear();
            CurrentReservationHistories.Clear();
        }
    }
}
