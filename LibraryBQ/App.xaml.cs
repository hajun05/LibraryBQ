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
        // 설정 파일의 연결문자열 사용을 위한 구성 객체 구현
        public static IConfiguration Config { get; private set; }

        public App()
        {
            Config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
        }
    }

}
