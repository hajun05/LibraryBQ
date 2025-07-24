using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public partial class BookCopyDetail
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
            CurrentLoanDueDate = (bookCopy.LoanStatusId == 2) ? bookCopy.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().LoanDueDate : null;
            CurrentLoanUserId = (bookCopy.LoanStatusId == 2) ? bookCopy.LoanHistories.OrderByDescending(lh => lh.Id).FirstOrDefault().UserId : -1;
            CurrentReservations = bookCopy.ReservationHistories.ToList();
        }
    }
}
