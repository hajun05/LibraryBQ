using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Service;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    // BookCopyViewModel: 특정 도서의 부수(복사본) 정보 관리 ViewModel (MVVM, WPF)
    // - View와 바인딩 되는 선택 도서(Book) 정보를 보유하며, 해당 도서의 복수 부수 목록을 조회 및 관리
    // - 도서 부수들의 대출/예약 상태 및 이벤트 처리(대출 버튼 클릭시 로직 등)를 담당
    // - 로그인한 사용자 정보(LoginUserAccountStore)를 참조하여 대출 가능성, 예약 절차 등을 제어
    // - BookCopies(ObservableCollection)을 통해 UI와 데이터를 즉시 동기화
    // [주요 역할]
    // - 선택한 Book 정보를 기반으로 관련 부수 정보 DB 조회 후 화면에 리스트 표시
    // - 대출 가능/대출중/예약중 상태 분기 처리 및 그에 따른 대출/예약 호출
    // - MVVM RelayCommand를 활용해 UI 이벤트 대응
    public partial class BookCopyViewModel : LoanBaseViewModel
    {
        // 필드 및 프로퍼티 -----------------------------------------------------------
        // 뷰에서 선택한 Book 데이터 저장 (뷰에서 바인딩)
        private Book _selectedBook;
        // DataGrid에서 선택한 레코드 바인딩 (현재는 미사용, CommandParameter로 변경)
        //private BookCopyDetail _selectedBookCopy;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set => SetProperty(ref _selectedBook, value);
        }

        //public BookCopyDetail SelectedBookCopy
        //{
        //    get { return _selectedBookCopy; }
        //    set => SetProperty(ref _selectedBookCopy, value);
        //}

        // 생성자 --------------------------------------------------------------------
        // 선택한 도서 정보를 받아 상태 초기화
        public BookCopyViewModel(Book selectedBook)
        {
            _selectedBook = selectedBook;
            // LoanBaseViewModel에서 상속받은 BookCopies(ObservableCollection)와 로그인 사용자 정보 사용
            BookCopies = new ObservableCollection<BookCopyDetail>();
            LoginUserAccount = LoginUserAccountStore.Instance(); // 로그인 사용자 정보 싱글톤 참조
            BookCopiesQuery(); // 선택도서의 부수 목록 즉시 불러와 BookCopies 컬렉션에 대입
        }

        // 메소드 --------------------------------------------------------------------
        // 선택한 도서의 부수(BookCopy) 목록을 데이터베이스에서 조회하여 BookCopies에 반영
        private void BookCopiesQuery()
        {
            BookCopies.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookCopyDetail> queriedBookCopies = db.BookCopies
                    .Include(x => x.Book).Include(x => x.LoanHistories).Include(x => x.ReservationHistories)
                    .Where(x => x.BookId == SelectedBook.Id)
                    .Select(x => new BookCopyDetail(x)) // 각 BookCopy 엔티티를 BookCopyDetail DTO로 변환
                    .ToList();

                // ObservableCollection에 추가 → UI 자동 갱신
                foreach (BookCopyDetail bookCopy in queriedBookCopies)
                    BookCopies.Add(bookCopy);
            }
        }

        // 커멘드 ---------------------------------------------------------------------
        // 대출/예약 버튼 커맨드 핸들러. 도서 부수(BookCopyDetail) 선택 상황과 상태에 따라 대출 또는 예약 로직 처리
        [RelayCommand] private void LoanbtnClick(BookCopyDetail? selectedBookCopy)
        {
            if (selectedBookCopy == null)
            {
                MessageBox.Show("대출 혹은 예약하실 도서를 선택하십시오.");
                return;
            }

            using (LibraryBQContext db = new LibraryBQContext())
            {
                // 대출 가능 조건 체크(연체 여부, 대출 한도, 사전 대출 여부 등)
                if (!CheckCanLoan(db, selectedBookCopy))
                    return;

                if (selectedBookCopy.CurrentLoanStatusId == 1) // 대출가능 도서
                {
                    LoanBookCopy(db, selectedBookCopy); // 대출 로직 실행
                    BookCopiesQuery(); // 상태 갱신, 화면 갱신
                }
                else if (selectedBookCopy.CurrentLoanStatusId == 2) // 대출중 도서
                {
                    // 다른 부수가 대출 가능하면 안내 후 중단
                    if (IsAvailableOtherBookCopy())
                        return;

                    // 모든 부수가 대출중이라면 예약 여부 확인 후 예약 처리
                    if (MessageBox.Show("모든 도서가 대출중입니다.\r\n예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ReserveBookCopy(db, selectedBookCopy);
                        BookCopiesQuery();
                    }
                }
                else // 예약중 도서
                {
                    // 해당 도서 예약 내역에서 현재 사용자의 예약 내역 검색
                    ReservationHistory? currentUserSeletedBookCopyReservation = selectedBookCopy.CurrentReservations
                        .FirstOrDefault(x => x.UserId == LoginUserAccount.CurrentLoginUserAccount.Id);

                    if (currentUserSeletedBookCopyReservation != null)
                    {
                        // 현재 사용자가 1순위 예약자일 경우 도서 대출
                        if (currentUserSeletedBookCopyReservation.Priority == 1)
                        {
                            LoanBookCopy(db, selectedBookCopy);
                            BookCopiesQuery();
                        }
                        // 1순위가 아니라면 예약 순서 메시지 안내
                        else
                        {
                            MessageBox.Show($"아직 첫번째 차례가 아닙니다.\r\n현재 {currentUserSeletedBookCopyReservation.Priority}번째 순서입니다.");
                        }
                    }
                    else
                    {
                        // 이미 예약된 도서라면 다른 부수 대출 가능 여부 확인
                        if (IsAvailableOtherBookCopy())
                            return;

                        // 다음 순서로 예약할 것인지 사용자에게 확인 후 예약 처리
                        if (MessageBox.Show("이미 예약된 도서입니다.\r\n다음 순서로 예약하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            ReserveBookCopy(db, selectedBookCopy);
                            BookCopiesQuery();
                        }
                    }
                }
            }
        }
    }
}
