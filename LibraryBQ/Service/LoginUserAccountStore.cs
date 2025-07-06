using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // 현재 로그인한 사용자 정보 저장 서비스
    public partial class LoginUserAccountStore : ObservableObject
    {
        // 필드와 프로퍼티 -------------------------------------------
        private User? _currentUserAccount;
        private bool _isLogin;

        public User? CurrentLoginUserAccount
        {
            get { return _currentUserAccount; }
            set
            {
                if (SetProperty(ref _currentUserAccount, value))
                    IsLogin = (_currentUserAccount != null);
            }
        }
        public bool IsLogin
        {
            get => _isLogin;
            set => SetProperty(ref _isLogin, value);
        }

        // 생성자(싱글톤 패턴 적용) ------------------------------------
        private static LoginUserAccountStore userAccount;
        private LoginUserAccountStore() { }
        public static LoginUserAccountStore Instance()
        {
            if (userAccount == null)
            {
                userAccount = new LoginUserAccountStore();
            }
            return userAccount;
        }
        public static LoginUserAccountStore Instance(User loginUser)
        {
            if (userAccount == null)
            {
                userAccount = new LoginUserAccountStore();
            }
            userAccount.CurrentLoginUserAccount = loginUser;
            return userAccount;
        }

        // 메소드 ----------------------------------------------------
        public void DetachLoginUserAccount()
        {
            if (CurrentLoginUserAccount != null)
            {
                CurrentLoginUserAccount = null;
            }
        }
    }
}
