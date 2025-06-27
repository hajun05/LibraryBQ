using LibraryBQ.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Windows;

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

        // DI 컨테이너를 위한 서비스 제공자 프로퍼티
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            // 외부 소스 빌더 인스턴스.설정 파일(appsettings.json) 읽기.IConfiguration 객체 생성;
            Config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();

            // DI 컨테이너 구성
            var services = new ServiceCollection();

            // DI 컨테이너에 DbContext 풀링을 DI(의존성 주입) 등록
            services.AddDbContextPool<LibraryBQContext>(options =>
                options.UseSqlServer(App.Config.GetConnectionString("MSSQLConnection")));



            // 서비스 제공자에 DI 컨테이너 등록
            ServiceProvider = services.BuildServiceProvider();
        }
    }

}
