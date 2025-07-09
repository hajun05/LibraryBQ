using LibraryBQ.Model;
using LibraryBQ.View;
using LibraryBQ.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        // DI 컨테이너 프로퍼티 선언
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            // 외부 소스 빌더 인스턴스.설정 파일(appsettings.json) 읽기.IConfiguration 객체 생성;
            Config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();

            // 서비스 컬렉션 생성
            var services = new ServiceCollection();

            // 서비스 컬렉션에 DbContext 풀링 서비스(의존성 객체) 등록
            services.AddDbContextPool<LibraryBQContext>(options =>
                options.UseSqlServer(App.Config.GetConnectionString("MSSQLConnection")));

            // 서비스 컬렉션에 서비스(ViewModel) 및 클라이언트(View) 등록.
            // DI 컨테이너를 이용한 View와 ViewModel 연결. View 생성 시 ViewModel 자동 주입 및 바인딩
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<BookQueryViewModel>();
            services.AddTransient<LoginViewModel>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<BookQueryView>();
            services.AddSingleton<LoginView>();

            // DI 컨테이너 초기화
            ServiceProvider = services.BuildServiceProvider();
        }

        // StartupUri 속성 대체 메소드. WPF가 자동으로 지정된 윈도우를 생성 및 표시. 별도 호출 불필요
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
