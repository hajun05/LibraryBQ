using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.ViewModel
{
    public partial class HomeViewModel : ObservableObject
    {
        // 필드와 프로퍼티 --------------------------------------
        private string _inputQueryStr;
        public string InputQueryStr
        {
            get { return _inputQueryStr; }
            set => SetProperty(ref _inputQueryStr, value);
        }

        // 대리자(하위 ViewModel에서 상위 ViewModel 제어) --------
        public Action HomeBookQueryAction { get; set; }

        // 커멘드 ----------------------------------------------
        [RelayCommand] private void HomeBookQuery()
        {
            HomeBookQueryAction?.Invoke();
        }
    }
}
