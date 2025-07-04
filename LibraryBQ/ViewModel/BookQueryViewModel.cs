using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.ViewModel
{
    public partial class BookQueryViewModel : ObservableObject
    {
        // 필드와 프로퍼티 -----------------------------------------------------------
        private string _inputQueryStr;
        public string InputQueryStr
        {
            get => _inputQueryStr;
            set => SetProperty(ref _inputQueryStr, value);
        }

        // 커멘드 -------------------------------------------------------------------
        [RelayCommand] private void BookQuery()
        {

        }
    }
}
