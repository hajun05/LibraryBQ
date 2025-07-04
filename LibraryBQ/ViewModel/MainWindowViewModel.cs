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
using static LibraryBQ.ViewModel.HomeViewModel;

namespace LibraryBQ.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        private ObservableObject _currentViewModel;
        private HomeViewModel _homeViewModel;
        private BookQueryViewModel _bookQueryViewModel;
        private LoginViewModel _loginViewModel;
        private LoginUserAccountStore _loginUserAccountStore; // 해당 싱글톤 인스턴스로 IsLogin 프로퍼티 이동. 작동은 잘 되는데 보이는게 이상하다? 겹처보이거나 하나만 보이거나 문제있음.
        private bool _isLogin;

        public ObservableObject CurrentViewModel
        {
            get { return _currentViewModel; }
            set => SetProperty(ref _currentViewModel, value);
        }
        public LoginUserAccountStore LoginUserAccountStore
        {
            get => _loginUserAccountStore;
            set => SetProperty(ref _loginUserAccountStore, value);
        }
        public bool IsLogin
        {
            get => _isLogin;
            set => SetProperty(ref _isLogin, value);
        }

        // 생성자 ----------------------------------------------
        public MainWindowViewModel(HomeViewModel homeViewModel, BookQueryViewModel bookQueryViewModel, LoginViewModel loginViewModel)
        {
            _homeViewModel = homeViewModel;
            _bookQueryViewModel = bookQueryViewModel;
            _loginViewModel = loginViewModel;
            _loginUserAccountStore = LoginUserAccountStore.Instance();

            // 각 하위 ViewModel에서 상위 ViewModel의 상태 변경을 수행할 대리자 초기화
            _homeViewModel.HomeBookQueryAction = () => { _bookQueryViewModel.InputQueryStr = _homeViewModel.InputQueryStr; CurrentViewModel = _bookQueryViewModel; };
            _loginViewModel.LoginEndAction = () => { CurrentViewModel = _homeViewModel; IsLogin = true; };

            // 초기화면
            CurrentViewModel = _homeViewModel;
            IsLogin = false;
        }

        // 커멘드 ----------------------------------------------
        [RelayCommand] private void HomebtnClick() // 홈버튼 클릭 커멘드
        {
            if (CurrentViewModel != _homeViewModel)
            {
                _homeViewModel.InputQueryStr = string.Empty;
                CurrentViewModel = _homeViewModel;
            }
        }

        [RelayCommand] private void BookbtnClick() // 도서조회버튼 클릭 커멘드
        {
            if (CurrentViewModel != _bookQueryViewModel)
                CurrentViewModel = _bookQueryViewModel;
        }

        [RelayCommand] private void LoginbtnClick() // 로그인버튼 클릭 커멘드
        {
            if (!IsLogin)
            {
                if (CurrentViewModel != _loginViewModel)
                {
                    _loginViewModel.InputUserNo = string.Empty;
                    _loginViewModel.InputPassword = string.Empty;
                    CurrentViewModel = _loginViewModel;
                }
            }
            else
            {
                if (MessageBox.Show("로그아웃하시겠습니까?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    LoginUserAccountStore.DetachLoginUserAccount();
                    IsLogin = false;
                }
            }
        }
    }
}
