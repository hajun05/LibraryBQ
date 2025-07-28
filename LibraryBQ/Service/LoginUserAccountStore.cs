using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // 현재 로그인한 사용자 정보를 저장 및 관리하는 싱글톤 서비스 클래스
    // 여러 ViewModel에서 로그인 상태 및 사용자 정보에 쉽게 접근하고 변경사항을 감지하도록 지원
    public partial class LoginUserAccountStore : ObservableObject
    {
        // 필드와 프로퍼티 -------------------------------------------
        private User? _currentUserAccount;
        private bool _isLogin;
        private bool _hasOverdueLoan;
        private string _name;

        public User? CurrentLoginUserAccount
        {
            get { return _currentUserAccount; }
            set
            {
                // 사용자가 로그인 했는지 여부 설정 및 사용자 이름과 연체 대출 여부 갱신
                if (SetProperty(ref _currentUserAccount, value))
                {
                    IsLogin = (_currentUserAccount != null);
                    Name = IsLogin ? _currentUserAccount.Name : "";
                    HasOverdueLoan = IsLogin ? CheckHasOverdueLoan() : false;
                }
            }
        }
        public bool IsLogin
        {
            get => _isLogin;
            set => SetProperty(ref _isLogin, value);
        }
        public bool HasOverdueLoan
        {
            get => _hasOverdueLoan;
            set => SetProperty(ref _hasOverdueLoan, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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
        // 싱글톤 인스턴스 반환 및 로그인 사용자 정보 할당
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
        // 현재 로그인 사용자 정보를 초기화하여 로그아웃 처리
        public void DetachLoginUserAccount()
        {
            if (CurrentLoginUserAccount != null)
            {
                CurrentLoginUserAccount = null;
            }
        }

        // DbContext를 인자로 받아 연체 대출 여부를 확인하고 상태 갱신
        public bool CheckHasOverdueLoan(LibraryBQContext db)
        {
            HasOverdueLoan = false;
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            var CurrentLoans = db.LoanHistories.Include(x => x.User)
                .Where(x => x.ReturnDate == null)
                .Where(x => x.UserId == CurrentLoginUserAccount.Id)
                .Where(x => x.LoanDueDate < today)
                .Select(x => new { x.LoanDueDate }).ToList();

            if (CurrentLoans.Any())
                HasOverdueLoan = true;

            return HasOverdueLoan;
        }

        // 내부에서 DbContext를 생성하여 연체 대출 여부를 확인(오버로딩)
        public bool CheckHasOverdueLoan()
        {
            using (var db = new LibraryBQContext())
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var CurrentLoans = db.LoanHistories.Include(x => x.User)
                    .Where(x => x.ReturnDate == null)
                    .Where(x => x.UserId == CurrentLoginUserAccount.Id)
                    .Where(x => x.LoanDueDate < today)
                    .Select(x => new { x.LoanDueDate }).ToList();

                if (CurrentLoans.Any())
                    return true;

                return false;
            }
        }
    }
}
