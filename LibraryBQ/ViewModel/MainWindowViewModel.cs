using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using LibraryBQ.Service;
using System.Windows;

namespace LibraryBQ.ViewModel
{
    // MainWindowViewModel: 애플리케이션 전체 화면 전환 및 상태를 관리하는 최상위 ViewModel (MVVM, WPF)
    // - DI 컨테이너에서 하위 주요 ViewModel(Home, Login, BookQuery, History 등)을 생성자 주입받아 통합 관리
    // - CurrentViewModel 교체를 통해, 메인 윈도우에서 실제로 표시되는 콘텐츠(페이지/화면) 전환을 실현
    // - 각 하위 ViewModel에서의 Action(델리게이트) 연결을 통해, 로그인/검색 등 핵심 이벤트에 맞춰 중앙에서 화면과 상태를 조정
    // - 로그아웃/예약 만료처리 등 공통 로직, 최초 진입 시 초기화, 화면전환 커맨드(Home/Book/Login/History 버튼) 등 중앙 집중 처리
    // - 앱 전체의 '상태 관리, 내비게이션, 전역 로직'이 집약된 MVVM 구조의 컴포지션 루트(MainViewModel) 역할
    public partial class MainWindowViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        private ObservableObject _currentViewModel; // 현재 표시되고 있는 ViewModel (화면 중앙전환에 사용)
        // DI로 주입된 하위 ViewModel
        private HomeViewModel _homeViewModel;
        private BookQueryViewModel _bookQueryViewModel;
        private LoginViewModel _loginViewModel;
        private HistoryViewModel _historyViewModel;
        private LoginUserAccountStore _loginUserAccountStore; // 로그인 사용자 전역 상태 저장소

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
        // DI로 하위 ViewModel들을 받아서 앱 전체 화면 상태·이벤트 연결·상호작용 로직 구성
        public MainWindowViewModel(HomeViewModel homeViewModel, BookQueryViewModel bookQueryViewModel, LoginViewModel loginViewModel, HistoryViewModel historyViewModel)
        {
            _homeViewModel = homeViewModel;
            _bookQueryViewModel = bookQueryViewModel;
            _loginViewModel = loginViewModel;
            _historyViewModel = historyViewModel;
            _loginUserAccountStore = LoginUserAccountStore.Instance();

            // 각 하위 ViewModel에서 상위 ViewModel의 상태 변경을 수행할 대리자 초기화(콜백 연결)
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
                    _historyViewModel.ReservationHistoriesQuery();
                    CurrentViewModel = _historyViewModel;
                }
                else
                {
                    CurrentViewModel = _homeViewModel;
                }
            };

            // 만료된 예약목록 사전 처리
            ExpirationReservation();

            // 초기화면(Home)
            CurrentViewModel = _homeViewModel;
        }

        // 커멘드 ----------------------------------------------
        // 홈버튼 클릭 커멘드: 홈화면으로 전환, 검색어 초기화
        [RelayCommand] private void HomebtnClick() 
        {
            if (CurrentViewModel != _homeViewModel)
            {
                _homeViewModel.InputQueryStr = string.Empty;
                CurrentViewModel = _homeViewModel;
            }
        }

        // 도서조회버튼 클릭 커멘드: 도서검색 화면으로 전환 및 검색 조건 초기화
        [RelayCommand] private void BookbtnClick()
        {
            if (CurrentViewModel != _bookQueryViewModel)
            {
                _bookQueryViewModel.BookQueryClear();
                CurrentViewModel = _bookQueryViewModel;
            }
        }

        // 로그인버튼 클릭 커멘드: 로그인하지 않았으면 로그인화면 전환, 로그인되어 있으면 로그아웃 처리
        [RelayCommand] private void LoginbtnClick()
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
                    _homeViewModel.InputQueryStr = string.Empty;
                    if (CurrentViewModel == _historyViewModel)
                    {
                        _historyViewModel.HistoryClear();
                        CurrentViewModel = _homeViewModel;
                    }
                }
            }
        }

        // 이력조회버튼 클릭 커멘드: 로그인 여부에 따라 로그인 유도 또는 이력화면 전환+데이터 갱신
        [RelayCommand] private async void MyHistorybtnClick() 
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
                    _historyViewModel.ReservationHistoriesQuery();
                    CurrentViewModel = _historyViewModel;
                }
            }
        }

        // 메소드 ----------------------------------------------
        // 만료된 예약 목록 삭제 및 우선순위 자동정렬
        private void ExpirationReservation()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            using (LibraryBQContext db = new LibraryBQContext())
            {
                // 만료할 예약 목록 조회
                var expiredReservations = db.ReservationHistories.Where(x => x.ReservationDueDate < today).ToList();

                if (expiredReservations.Count() > 0)
                {
                    // 만료할 예약의 도서 번호와 예약 우선순위 추출
                    var reservationCriterias = expiredReservations.Select(x => new { x.BookCopyId, x.Priority });

                    // 만료할 에약 목록 삭제
                    db.ReservationHistories.RemoveRange(expiredReservations);

                    // 만료한 예약과 같은 도서를 예약한 이력들의 예약 우선순위 조정
                    foreach (var reservation in reservationCriterias)
                    {
                        List<ReservationHistory> targets = db.ReservationHistories
                            .Where(x => x.BookCopyId == reservation.BookCopyId && x.Priority > reservation.Priority)
                            .ToList();

                        for (int i = 0; i < targets.Count; i++)
                            targets[i].Priority -= 1;
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
