using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        private ObservableObject _currentViewModel;
        private HomeViewModel _homeViewModel;
        private BookQueryViewModel _bookQueryViewModel;

        public ObservableObject CurrentViewModel
        {
            get { return _currentViewModel; }
            set => SetProperty(ref _currentViewModel, value);
        }

        // 생성자 ----------------------------------------------
        public MainWindowViewModel(HomeViewModel homeViewModel, BookQueryViewModel bookQueryViewModel)
        {
            _homeViewModel = homeViewModel;
            _bookQueryViewModel = bookQueryViewModel;

            // 초기화면
            CurrentViewModel = _homeViewModel;
        }

        // 커멘드 ----------------------------------------------
        [RelayCommand] private void HomebtnClick() // 홈버튼 클릭 커멘드
        {
            if (CurrentViewModel != _homeViewModel)
                CurrentViewModel = _homeViewModel;
        }

        [RelayCommand] private void BookbtnClick()
        {
            if (CurrentViewModel != _bookQueryViewModel)
                CurrentViewModel = _bookQueryViewModel;
        }
    }
}
