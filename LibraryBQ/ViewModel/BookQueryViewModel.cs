using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Model;
using LibraryBQ.Service;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    // BookQueryViewModel: 도서 조회/검색 화면의 ViewModel (MVVM, WPF)
    // - 사용자가 도서명 또는 저자명으로 도서 목록을 검색할 수 있게 지원
    // - 검색 결과(BookDetail 리스트) 및 선택 도서(BookDetail)에 대한 상태 관리
    // - "상세 정보" 버튼 등에서 도서 부수(복사본) 정보로 이동할 때 IWindowService를 통해 동적 뷰 연동
    public partial class BookQueryViewModel : ObservableObject
    {
        // 사용자 입력(검색어) 프로퍼티
        private string _inputQueryStr;
        // 검색 결과 도서 리스트(바인딩용)
        private ObservableCollection<BookDetail> _queriedBooks;
        // 현재 선택된 도서 상세 정보(바인딩용)
        private BookDetail _selectedBook;
        // 도서 복사본 상세조회 등 동적 창 띄우기용 서비스(DI 주입)
        private IWindowService _iWindowService;

        public string InputQueryStr
        {
            get => _inputQueryStr;
            set => SetProperty(ref _inputQueryStr, value);
        }
        public ObservableCollection<BookDetail> QueriedBooks
        {
            get { return _queriedBooks; }
            set => SetProperty(ref _queriedBooks, value);
        }
        public BookDetail SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        // 생성자: DI로 IWindowService 주입, 검색결과 초기화, 메시지 등록(외부 커맨드 트리거(UI/다른 VM) 지원)
        public BookQueryViewModel(IWindowService iWindowService)
        {
            _iWindowService = iWindowService;
            _queriedBooks = new ObservableCollection<BookDetail>();

            // CommandMessage 수신 시 BookQueryCommand를 실행(MVVM 메시징 연동)
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => BookQueryCommand.Execute(null));
        }

        // 도서 조회/검색 커맨드: 입력값(제목/저자)으로 Book 리스트를 필터링, 결과를 컬렉션에 채움(실시간 바인딩)
        [RelayCommand]
        private void BookQuery()
        {
            if (_queriedBooks != null)
                _queriedBooks.Clear();
            using (LibraryBQContext db = new LibraryBQContext())
            {
                List<BookDetail> result = new List<BookDetail>();
                if (InputQueryStr.Trim() != "")
                {
                    // 제목이나 저자명에 검색어가 포함된 도서만 조회
                    result = db.Books.Include(x => x.BookCopies)
                        .Where(x => x.Title.Contains(InputQueryStr) || x.Author.Contains(InputQueryStr))
                        .Select(x => new BookDetail(x)).ToList();
                }
                else
                {
                    // 검색어가 없으면 전체 도서 리스트 조회
                    result = db.Books.Include(x => x.BookCopies)
                        .Select(x => new BookDetail(x)).ToList();
                }

                // 결과 리스트를 ObservableCollection에 추가(뷰 즉시 동기화)
                foreach (BookDetail book in result)
                    _queriedBooks.Add(book);
            }
        }

        // "도서 부수 정보" 버튼 클릭 시(혹은 기타 트리거) 선택 도서(Book)에 대한 창 오픈
        [RelayCommand]
        private void BookCopyOpen()
        {
            if (_selectedBook != null)
            {
                using (LibraryBQContext db = new LibraryBQContext())
                {
                    // 최신 상태의 Book 엔티티(복사본 포함) 조회 후 WindowService로 전달
                    Book book = db.Books.Include(x => x.BookCopies).FirstOrDefault(x => x.Id == _selectedBook.Id);
                    _iWindowService.ShowBookCopyWindow(book);
                }
            }
        }

        // 검색 조건 및 결과 초기화
        public void BookQueryClear()
        {
            _inputQueryStr = string.Empty; // 검색어 초기화
            _queriedBooks.Clear();         // 결과 리스트 초기화
        }
    }
}
