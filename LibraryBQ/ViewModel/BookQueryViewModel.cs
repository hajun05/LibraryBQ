using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibraryBQ.Service;
using LibraryBQ.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public BookQueryViewModel()
        {
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => BookQueryCommand.Execute(null));
        }

        // 커멘드 -------------------------------------------------------------------
        [RelayCommand] private void BookQuery()
        {
            MessageBox.Show("확인");
        }
    }
}
