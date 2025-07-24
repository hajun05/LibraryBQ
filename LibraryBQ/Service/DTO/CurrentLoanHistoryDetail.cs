using CommunityToolkit.Mvvm.ComponentModel;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public partial class CurrentLoanHistoryDetail : ObservableObject
    {
        // 동적 변동 가능성이 있는 프로퍼티는 별도 필드 구현
        private DateOnly? _currentLoanDueDate;
        private byte _extensionCount;

        public int Id { get; set; }
        // 도서 관련 프로퍼티 ---------------------------------------------
        public int BookCopyId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ClassificationNumber { get; set; }

        // 현재 상태 관련 프로퍼티 -----------------------------------------
        public int CurrentLoanStatusId { get; set; }

        // 대출 상태 관련 프로퍼티 -----------------------------------------
        public int CurrentLoanHistoryId { get; set; }
        public DateOnly? CurrentLoanDueDate
        {
            get => _currentLoanDueDate;
            set => SetProperty(ref _currentLoanDueDate, value);
        }
        public int? CurrentLoanUserId { get; set; }
        public byte ExtensionCount
        {
            get => _extensionCount;
            set => SetProperty(ref _extensionCount, value);
        }

        public CurrentLoanHistoryDetail() { }

        public CurrentLoanHistoryDetail(LoanHistory loanHistory)
        {
            Id = loanHistory.Id;
            if (loanHistory.BookCopy != null)
            {
                BookCopyId = loanHistory.BookCopyId;
                BookId = loanHistory.BookCopy.BookId;
                Title = loanHistory.BookCopy.Book.Title;
                Author = loanHistory.BookCopy.Book.Author;
                ClassificationNumber = String.Format($"{loanHistory.BookCopy.BookId}-{loanHistory.BookCopy.Id}");
                CurrentLoanStatusId = loanHistory.BookCopy.LoanStatusId;
            }
            CurrentLoanHistoryId = loanHistory.Id;
            CurrentLoanDueDate = loanHistory.LoanDueDate;
            CurrentLoanUserId = loanHistory.UserId;
            ExtensionCount = loanHistory.ExtensionCount;
        }
    }
}
