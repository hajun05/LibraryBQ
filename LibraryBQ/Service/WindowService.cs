using LibraryBQ.View;
using LibraryBQ.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // 팩토리 패턴 간단 설명
    // -----------------------
    // 팩토리 패턴(Factory Pattern): 객체 생성을 담당하는 별도의 팩토리 클래스를 구현
    // 사용하는 쪽이 구체 인스턴스 생성 방법을 몰라도 되게 추상화하는 디자인 패턴
    // 생성 로직을 캡슐화하고 DI/테스트/유지보수 편의성이 향상
    // -----------------------

    // MVVM 원칙(뷰-뷰모델 분리)을 유지하면서 동적으로 윈도우(뷰)를 여는 서비스 인터페이스
    public interface IWindowService
    {
        // 특정 Book에 대한 부수 정보 윈도우 열기
        void ShowBookCopyWindow(Book book);
    }

    // 동적 Window(뷰) 생성 시 ViewModel을 DI로 주입받아 바인딩하는 서비스 구현체
    // => 팩토리 패턴을 통해 ViewModel을 만들고, DI 컨테이너에서 뷰와 연결
    public class WindowService : IWindowService
    {
        public void ShowBookCopyWindow(Book book) // 도서 부수 정보 열기 메소드
        {
            // ViewModel 생성은 팩토리(IoC/DI 등록)를 통해 관리
            var factory = App.ServiceProvider.GetRequiredService<IBookCopyViewModelFactory>();
            var viewModel = factory.Create(book);

            // View 역시 DI 컨테이너에서 생성 (XAML에서 따로 new 하지 않음)
            var view = App.ServiceProvider.GetRequiredService<BookCopyView>();
            // ViewModel을 DataContext에 바인딩하여 MVVM 구조 유지
            view.DataContext = viewModel;
            // Modal Dialog로 뷰 표시
            view.ShowDialog();
        }
    }
}
