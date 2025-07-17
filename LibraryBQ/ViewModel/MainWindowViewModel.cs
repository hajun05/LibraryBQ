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
    public partial class MainWindowViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        private ObservableObject _currentViewModel;
        private HomeViewModel _homeViewModel;
        private BookQueryViewModel _bookQueryViewModel;
        private LoginViewModel _loginViewModel;
        private HistoryViewModel _historyViewModel;
        private LoginUserAccountStore _loginUserAccountStore;

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

        // 생성자 ----------------------------------------------
        public MainWindowViewModel(HomeViewModel homeViewModel, BookQueryViewModel bookQueryViewModel, LoginViewModel loginViewModel, HistoryViewModel historyViewModel)
        {
            _homeViewModel = homeViewModel;
            _bookQueryViewModel = bookQueryViewModel;
            _loginViewModel = loginViewModel;
            _historyViewModel = historyViewModel;
            _loginUserAccountStore = LoginUserAccountStore.Instance();

            // 각 하위 ViewModel에서 상위 ViewModel의 상태 변경을 수행할 대리자 초기화
            _homeViewModel.HomeBookQueryAction = () =>
            {
                _bookQueryViewModel.InputQueryStr = _homeViewModel.InputQueryStr;
                CurrentViewModel = _bookQueryViewModel;
            };
            _loginViewModel.LoginEndAction = (bool IsLoginByHistory) =>
            {
                if (IsLoginByHistory)
                {
                    CurrentViewModel = _historyViewModel;
                }
                else
                    CurrentViewModel = _homeViewModel;
            };

            // 초기화면
            CurrentViewModel = _homeViewModel;
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
            if (!_loginUserAccountStore.IsLogin)
            {
                if (CurrentViewModel != _loginViewModel)
                {
                    _loginViewModel.LoginClear();
                    _loginViewModel.isLoginByHistory = false;
                    CurrentViewModel = _loginViewModel;
                }
            }
            else
            {
                if (MessageBox.Show("로그아웃하시겠습니까?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    LoginUserAccountStore.DetachLoginUserAccount();
                }
            }
        }

        [RelayCommand] private async void MyHistorybtnClick() // 이력조회버튼 클릭 커멘드
        {
            if (!_loginUserAccountStore.IsLogin)
            {
                if (MessageBox.Show("로그인이 필요합니다.\r\n로그인하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (CurrentViewModel != _loginViewModel)
                    {
                        _loginViewModel.LoginClear();
                        _loginViewModel.isLoginByHistory = true;
                        CurrentViewModel = _loginViewModel;
                    }
                }
            }
            else
            {
                if (CurrentViewModel != _historyViewModel)
                    CurrentViewModel = _historyViewModel;
            }
        }

        // 메소드 ----------------------------------------------

    }
}
