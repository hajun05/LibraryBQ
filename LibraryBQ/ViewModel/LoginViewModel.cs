using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    // LoginViewModel: 사용자 로그인(인증) 화면의 ViewModel (MVVM, WPF)
    // - 로그인 화면에서 사용자 입력(UserNo/Password) 상태 관리 및 인증 처리
    // - 로그인 성공 시 싱글톤 AccountStore 갱신, 연체 상황 안내, 상위 화면 이동 등 흐름 제어
    // - Action<bool> 델리게이트(LoginEndAction)로 로그인 완료 후 후속 처리(UI 전환 등) 지원
    public partial class LoginViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------------
        private LoginUserAccountStore accountStore; // 싱글톤 로그인 사용자 정보 저장소(LoginUserAccountStore)
        private string _inputUserNo; // 사용자 입력: 회원 번호(아이디)
        private string _inputPassword; // 사용자 입력: 비밀번호
        public LoginUserAccountStore AccountStore
        {
            get => accountStore;
            set => SetProperty(ref accountStore, value);
        }
        public string InputUserNo 
        {  
            get => _inputUserNo;
            set => SetProperty(ref _inputUserNo, value); 
        }
        public string InputPassword
        {
            get => _inputPassword;
            set => SetProperty(ref _inputPassword, value);
        }
        public bool isLoginByHistory { get; set; }

        // 대리자 객체 ------------------------------------------------
        // 로그인 완료 후 실행할 델리게이트(넘겨받는 쪽에서 화면 전환 등 후처리 가능)
        public Action<bool> LoginEndAction { get; set; }

        // 커멘드 -----------------------------------------------------
        // 로그인 버튼 커맨드 (사용자 입력 체크, 인증, 후속 처리)
        [RelayCommand] private void LoginbtnClick()
        {
            if (InputUserNo.Trim() == "" || InputPassword.Trim() == "")
            {
                MessageBox.Show("필수 정보들을 입력해 주십시오.");
            }
            else
            {
                using (var db = new LibraryBQContext())
                {
                    // 입력된 정보로 사용자 인증
                    User? LoginUser = db.Users.AsNoTracking().FirstOrDefault(x => x.UserNo == InputUserNo && x.Password == InputPassword);

                    if (LoginUser == null)
                    {
                        MessageBox.Show($"아이디 혹은 비밀번호가 틀립니다.\r\n다시 한번 확인해 주십시오.");
                    }
                    else
                    {
                        // 인증 성공: 싱글톤 로그인 사용자 저장소에 사용자 정보 설정
                        AccountStore = LoginUserAccountStore.Instance(LoginUser);

                        // 연체 도서 존재 시 메시지 안내
                        if (AccountStore.HasOverdueLoan)
                            MessageBox.Show($"연체된 도서가 존재합니다.\r\n연체된 모든 도서를 반납하셔야 대출 및 예약이 가능합니다.");

                        // 델리게이트 실행(상위에서 화면 전환 등 후처리)
                        LoginEndAction.Invoke(isLoginByHistory);
                    }
                }
            }
        }

        // 메소드 -----------------------------------------------------
        // 입력값 초기화(로그아웃/화면 전환 등에서 사용)
        public void LoginClear()
        {
            _inputUserNo = string.Empty;
            _inputPassword = string.Empty;
        }
    }
}
