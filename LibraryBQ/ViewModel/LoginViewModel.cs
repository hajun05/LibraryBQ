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
    // 로그인 처리 ViewModel
    public partial class LoginViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------------
        private LoginUserAccountStore accountStore;
        private string _inputUserNo;
        private string _inputPassword;
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

        // 대리자 객체 ------------------------------------------------
        public Action LoginEndAction { get; set; }

        // 커멘드 -----------------------------------------------------
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
                    User? LoginUser = db.Users.AsNoTracking().FirstOrDefault(x => x.UserNo == InputUserNo && x.Password == InputPassword);

                    if (LoginUser == null)
                    {
                        MessageBox.Show($"아이디 혹은 비밀번호가 틀립니다.\r\n다시 한번 확인해 주십시오.");
                    }
                    else
                    {
                        AccountStore = LoginUserAccountStore.Instance(LoginUser);
                        MessageBox.Show($"로그인되었습니다.\r\n환영합니다 {AccountStore.CurrentLoginUserAccount.Name}님.");
                        LoginEndAction.Invoke();
                    }
                }
            }
        }
    }
}
