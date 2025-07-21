using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
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
                    _historyViewModel.LoanHistoriesQuery();
                    CurrentViewModel = _historyViewModel;
                }
                else
                    CurrentViewModel = _homeViewModel;
            };

            // 만료된 예약목록 처리
            ExpirationReservation();

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
            {
                _bookQueryViewModel.BookQueryClear();
                CurrentViewModel = _bookQueryViewModel;
            }
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
                if (MessageBox.Show("로그아웃하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    LoginUserAccountStore.DetachLoginUserAccount();
                    if (CurrentViewModel == _historyViewModel)
                    {
                        _historyViewModel.HistoryClear();
                        CurrentViewModel = _homeViewModel;
                    }
                }
            }
        }

        [RelayCommand] private async void MyHistorybtnClick() // 이력조회버튼 클릭 커멘드
        {
            if (!_loginUserAccountStore.IsLogin)
            {
                if (MessageBox.Show("로그인이 필요합니다.\r\n로그인하시겠습니까?", "안내", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                {
                    _historyViewModel.LoanHistoriesQuery();
                    CurrentViewModel = _historyViewModel;
                }
            }
        }

        // 메소드 ----------------------------------------------
        private void ExpirationReservation()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            using (LibraryBQContext db = new LibraryBQContext())
            {
                // 만료할 예약 목록 추출
                List<ReservationHistory> expiredReservations = db.ReservationHistories.Where(x => x.ReservationDueDate < today).ToList();

                if (expiredReservations.Count > 0)
                {
                    // 만료할 예약의 도서 번호와 예약 우선순위 추출
                    var associatedReservations = expiredReservations.Select(x => new { x.BookCopyId, x.Priority }).ToList();

                    // 만료할 에약 목록 삭제
                    db.ReservationHistories.RemoveRange(expiredReservations);

                    // 만료한 예약과 같은 도서를 예약한 이력들의 예약 우선순위 조정
                    foreach (var reservation in associatedReservations)
                    {
                        var targets = db.ReservationHistories.Where(x => x.BookCopyId == reservation.BookCopyId && x.Priority > reservation.Priority);

                        foreach (var t in targets)
                            t.Priority -= 1;
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
