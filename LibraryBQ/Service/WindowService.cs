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
    // 동적으로 열리는 Window에서 MVVM 원칙을 지키기 위한 Service 패턴 구현
    // 동적 Window View - ViewModel DI 컨테이너에 등록
    public interface IWindowService
    {
        void ShowBookCopyWindow(Book book);
    }

    public class WindowService : IWindowService
    {
        public void ShowBookCopyWindow(Book book) // 도서 부수 정보 열기 메소드
        {
            var factory = App.ServiceProvider.GetRequiredService<IBookCopyViewModelFactory>();
            var viewModel = factory.Create(book);

            var view = App.ServiceProvider.GetRequiredService<BookCopyView>();
            view.DataContext = viewModel;
            view.ShowDialog();
        }
    }
}
