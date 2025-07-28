using CommunityToolkit.Mvvm.ComponentModel;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // DTO: Data Transfer Object의 약자, 데이터 전송 객체를 의미.
    // 주로 애플리케이션의 서로 다른 계층(예: DB Model ↔ DTO ↔ ViewModel)이나 프로세스 간 데이터 전달에 사용.
    // 데이터를 담아 포장하는 객체, 비즈니스 로직이나 처리 로직은 미포함.
    public partial class BookCopyDetail : ObservableObject
    {
        // 도서 관련 프로퍼티 ---------------------------------------------
        public int BookCopyId { get; set; }
        public int BookId { get; set; }
        public string ClassificationNumber { get; set; }

        // 현재 상태 관련 프로퍼티 -----------------------------------------
        public int CurrentLoanStatusId { get; set; }
        public string CurrentStatusName { get; set; } = null!;

        // 대출 상태 관련 프로퍼티 -----------------------------------------
        public DateOnly? CurrentLoanDueDate { get; set; }
        public int? CurrentLoanUserId { get; set; }

        // 예약 상태 관련 프로퍼티 -----------------------------------------
        public List<ReservationHistory>? CurrentReservations { get; set; }

        // 메소드 --------------------------------------------------------
        public string ConvertLoanStatus(int _currentLoanStatusId)
        {
            string answer = string.Empty;
            switch (_currentLoanStatusId)
            {
                case 1:
                    answer = "대출가능"; break;
                case 2:
                    answer = "대출중"; break;
                case 3:
                    answer = "예약중"; break;
            }
            return answer;
        }

        // 생성자 --------------------------------------------------------
        public BookCopyDetail() { }
        public BookCopyDetail(BookCopy bookCopy)
        {
            BookCopyId = bookCopy.Id;
            BookId = bookCopy.BookId;
            ClassificationNumber = String.Format($"{bookCopy.BookId}-{bookCopy.Id}");
            CurrentLoanStatusId = bookCopy.LoanStatusId;
            CurrentStatusName = ConvertLoanStatus(bookCopy.LoanStatusId);
            if (bookCopy.LoanHistories.Count > 0)
            {
                CurrentLoanDueDate = (bookCopy.LoanStatusId == 2) ? bookCopy.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().LoanDueDate : null;
                CurrentLoanUserId = (bookCopy.LoanStatusId == 2) ? bookCopy.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().UserId : -1;
            }
            if (bookCopy.ReservationHistories.Count > 0)
                CurrentReservations = bookCopy.ReservationHistories.ToList();
        }
    }
}
