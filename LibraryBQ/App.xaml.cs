using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace LibraryBQ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // 설정 파일의 연결문자열 사용을 위한 구성 객체(IConfiguration) 구현
        // WPF는 기본적으로 DI로 구성 객체가 미등록, 직접 구현 필요
        public static IConfiguration Config { get; private set; }

        public App()
        {
            // 외부 소스 빌더 인스턴스.설정 파일(appsettings.json) 읽기.IConfiguration 객체 생성;
            Config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
        }
    }

}
