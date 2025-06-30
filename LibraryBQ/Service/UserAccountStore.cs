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
    internal partial class UserAccountStore : ObservableObject
    {
        // 싱글톤 패턴 적용
        static UserAccountStore userAccount;
        private UserAccountStore(User _loginUser)
        {
            CurrentUserAccount = _loginUser;
        }
        public static UserAccountStore Instance(User _loginUser)
        {
            if (userAccount == null)
            {
                userAccount = new UserAccountStore(_loginUser);
            }
            return userAccount;
        }
        public static void DetachInstance()
        {
            if (userAccount != null)
            {
                userAccount = null;
            }
        }

        private User? _currentUserAccount;

        public User? CurrentUserAccount
        {
            get { return _currentUserAccount; }
            set => SetProperty(ref _currentUserAccount, value);
        }
    }
}
