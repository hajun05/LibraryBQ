using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBQ.Service
{
    // MVVM 메시지 전달용 클래스 (ValueChangedMessage를 상속).
    // 다양한 타입의 값을 payload(메시지 객체에 원하는 데이터를 저장)로 다른 객체에 전달하는데 사용.
    public class CommandMessage : ValueChangedMessage<object>
    {
        // 전달할 데이터를 생성자에서 받아 base 클래스에 전달
        public CommandMessage(object value) : base(value) { }
    }
}
