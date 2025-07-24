using CommunityToolkit.Mvvm.ComponentModel;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public partial class CurrentReservationHistoryDetail : ObservableObject
    {
        // 동적 변동 가능성이 있는 프로퍼티는 별도 필드 구현
        private DateOnly? _currentReservationDueDate;
        private int _priority;
        public int Id { get; set; }
        // 도서 관련 프로퍼티 ---------------------------------------------
        public int BookCopyId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ClassificationNumber { get; set; }

        // 현재 상태 관련 프로퍼티 -----------------------------------------
        public int CurrentLoanStatusId { get; set; }

        // 예약 상태 관련 프로퍼티 -----------------------------------------
        public int CurrentReservationHistoryId { get; set; }
        public DateOnly? CurrentReservationDueDate
        {
            get => _currentReservationDueDate;
            set => SetProperty(ref _currentReservationDueDate, value);
        }
        public int? CurrentReservationUserId { get; set; }
        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        public CurrentReservationHistoryDetail() { }
        public CurrentReservationHistoryDetail(ReservationHistory reservationHistory)
        {
            Id = reservationHistory.Id;
            BookCopyId = reservationHistory.BookCopyId;
            if (reservationHistory.BookCopy != null)
            {
                BookId = reservationHistory.BookCopy.BookId;
                Title = reservationHistory.BookCopy.Book.Title;
                Author = reservationHistory.BookCopy.Book.Author;
                ClassificationNumber = String.Format($"{reservationHistory.BookCopy.BookId}-{reservationHistory.BookCopy.Id}");
                CurrentLoanStatusId = reservationHistory.BookCopy.LoanStatusId;
            }
            CurrentReservationHistoryId = reservationHistory.Id;
            CurrentReservationDueDate = reservationHistory.ReservationDueDate;
            CurrentReservationUserId = reservationHistory.UserId;
            Priority = reservationHistory.Priority;
        }
    }
}
