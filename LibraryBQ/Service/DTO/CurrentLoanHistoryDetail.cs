using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    public partial class CurrentLoanHistoryDetail
    {
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
        public DateOnly? CurrentLoanDueDate { get; set; }
        public int? CurrentLoanUserId { get; set; }
    }
}
