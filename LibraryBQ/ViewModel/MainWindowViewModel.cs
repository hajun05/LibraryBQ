using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ObservableObject CurrentViewModel
        {
            get { return _currentViewModel; }
            set => SetProperty(ref _currentViewModel, value);
        }
        public HomeViewModel HomeViewModel
        {
            get => _homeViewModel;
        }

        // 생성자 ----------------------------------------------
        public MainWindowViewModel(HomeViewModel homeViewModel, BookQueryViewModel bookQueryViewModel, LoginViewModel loginViewModel)
        {
            _homeViewModel = homeViewModel;
            _bookQueryViewModel = bookQueryViewModel;
            _loginViewModel = loginViewModel;

            // 각 하위 ViewModel에서 상위 ViewModel의 상태 변경을 수행할 대리자 초기화
            _homeViewModel.HomeBookQueryAction = () => { CurrentViewModel = _bookQueryViewModel; };

            // 초기화면
            CurrentViewModel = _homeViewModel;
            _loginViewModel = loginViewModel;
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
            if (CurrentViewModel != _loginViewModel)
                CurrentViewModel = _loginViewModel;
        }
    }
}
