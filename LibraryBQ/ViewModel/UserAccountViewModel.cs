using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    // 로그인 처리 ViewModel
    internal partial class UserAccountViewModel : ObservableObject
    {
        private UserAccountStore accountStore;

        public UserAccountStore AccountStore
        {
            get => accountStore;
            set => SetProperty(ref accountStore, value);
        }

        [RelayCommand] private void Login(string userNo, string password)
        {
            if (userNo.Trim() == "" || password.Trim() == "")
            {
                MessageBox.Show("필수 정보들을 입력해 주십시오.");
            }
            else
            {
                using (var db = new LibraryBQContext())
                {
                    User? LoginUser = db.Users.FirstOrDefault(x => x.UserNo == userNo && x.Password.Equals(password));
                    if (LoginUser == null)
                    {
                        MessageBox.Show($"아이디 혹은 비밀번호가 틀립니다.\r\n다시 한번 확인해 주십시오.");
                    }
                    else
                    {
                        AccountStore = UserAccountStore.Instance(LoginUser);
                    }
                }
            }
        }

        [RelayCommand] private void Logout()
        {
            UserAccountStore.DetachInstance();
        }
    }
}
