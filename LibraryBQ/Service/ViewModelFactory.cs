using LibraryBQ.ViewModel;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // Book의 부수 정보 ViewModel을 생성하는 팩토리 인터페이스 정의
    // (ViewModel 생성 로직을 외부에 위임, 결합도 하락)
    public interface IBookCopyViewModelFactory
    {
        // Book 객체를 받아 BookCopyViewModel 인스턴스 생성
        BookCopyViewModel Create(Book book);
    }

    // BookCopyViewModel의 실제 생성 로직을 담당하는 팩토리 클래스
    // 팩토리 패턴 적용: 직접 생성 대신 팩토리를 통해 객체 생성 책임 분리
    public class BookCopyViewModelFactory : IBookCopyViewModelFactory
    {
        public BookCopyViewModel Create(Book book)
        {
            // 전달받은 Book 정보를 바탕으로 ViewModel 생성
            return new BookCopyViewModel(book);
        }
    }
}
