using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.ViewModel
{
    // HomeViewModel: 홈(메인) 화면에서 도서 검색 입력/트리거를 제어하는 ViewModel (MVVM, WPF)
    // - 메인 페이지의 도서 검색 입력창에 바인딩되어 사용자의 검색어 상태를 관리
    // - View 또는 하위 ViewModel에 검색 실행을 지시하기 위한 델리게이트(Action)와 메시지 전송 기능 포함
    // - 입력된 검색어로 검색 요청시 HomeBookQueryAction 및 MVVM 메시징(CommandMessage) 활용
    public partial class HomeViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        // 검색어 입력 상태를 보관 (뷰의 검색 입력란과 바인딩)
        private string _inputQueryStr;
        public string InputQueryStr
        {
            get { return _inputQueryStr; }
            set => SetProperty(ref _inputQueryStr, value);
        }

        // 대리자(하위 ViewModel에서 상위 ViewModel 제어) --------
        // 홈 화면에서 하위(또는 상위) ViewModel의 실행 메서드를 위임/호출할 수 있도록 하는 Action 델리게이트
        public Action HomeBookQueryAction { get; set; }

        // 커멘드 ----------------------------------------------
        // 도서 검색 버튼 클릭 커맨드
        [RelayCommand] private void HomeBookQuery()
        {
            // 검색어가 공백이 아니면 검색 실행을 트리거
            if (InputQueryStr.Trim() != "")
            {
                // 위임된 검색 실행 메서드 호출(뷰 또는 하위 VM의 동작)
                HomeBookQueryAction?.Invoke();
                // MVVM 메시징을 통해 다른 ViewModel(BookQueryViewModel)에 CommandMessage 신호 전달, 도서 검색 커멘드 실행
                WeakReferenceMessenger.Default.Send(new CommandMessage(null));
            }
        }
    }
}
