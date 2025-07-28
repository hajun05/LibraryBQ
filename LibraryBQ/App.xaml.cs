using LibraryBQ.Service;
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
    // App.xaml.cs: WPF 애플리케이션의 진입점 및 DI(Dependency Injection) 설정 관리
    // - DI 컨테이너를 구성하여 View, ViewModel, Service, DbContext 등의 의존성 객체를 등록하고 생성 관리
    // - IConfiguration을 사용해 appsettings.json의 설정(예: DB 연결 문자열) 불러오기
    // - AddDbContextPool 사용으로 EF Core DbContext 풀링 및 효율적인 DB 연결 관리 지원
    // - Singleton, Transient 라이프사이클로 필요한 객체 타입별 생성 범위 및 재사용 제어
    // - DI 컨테이너를 통해 MainWindow를 생성하고, 내부 의존성(ViewModel 등)을 자동 주입
    //   각 컴포넌트 간 결합도를 낮추고 유지보수/확장/테스트 용이성 향상
    public partial class App : Application
    {
        // 설정 파일의 연결문자열 사용을 위한 구성 객체(IConfiguration) 구현
        // WPF는 기본적으로 DI로 구성 객체가 미등록, 직접 구현 필요
        public static IConfiguration Config { get; private set; }

        // DI 컨테이너 프로퍼티 선언
        // [DI 컨테이너의 역할]
        // 1. 객체 생성 및 생명주기 관리 책임자 (IoC 컨테이너 역할)
        // 2. 의존성 주입을 통한 컴포넌트 간 결합도 감소 및 구현체 교체 유연성 제공
        // 3. MVVM 아키텍처에서 View와 ViewModel, Service를 느슨하게 연결하여 코드 간결성 및 재사용성 확보
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            // 외부_소스_빌더_인스턴스.설정 파일(appsettings.json)_읽기.IConfiguration_객체_생성;
            Config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();

            // DI 컨테이너 초기화
            ServiceProvider = ConfigureServices();
        }

        // StartupUri 속성 대체 메소드. WPF가 자동으로 지정된 윈도우를 생성 및 표시. 별도 호출 불필요
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // DI 컨테이너가 MainWindow를 직접 생성하고 생성자에 의존성을 주입
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private IServiceProvider ConfigureServices()
        {
            // 서비스 컬렉션 생성
            var services = new ServiceCollection();

            // 서비스 컬렉션에 DbContext 풀링 서비스(의존성 객체) 등록
            services.AddDbContextPool<LibraryBQContext>(options =>
                options.UseSqlServer(App.Config.GetConnectionString("MSSQLConnection")));

            // 비즈니스 서비스 및 팩토리 패턴 관련 객체 DI 등록 (인터페이스 → 구현체)
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IBookCopyViewModelFactory, BookCopyViewModelFactory>();

            // ViewModel DI 등록: Transient로 요청할 때마다 새 인스턴스 생성
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<BookQueryViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<BookCopyViewModel>();

            // View DI 등록: 화면 구성 요소 역시 DI로 요청/주입
            services.AddTransient<MainWindow>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<BookQueryView>();
            services.AddTransient<LoginView>();
            services.AddSingleton<HistoryView>();
            services.AddTransient<BookCopyView>(); // UserControl이 아닌 Window는 Transient 등록 권장

            // 서비스 컬렉션 빌드 후 DI 컨테이너(ServiceProvider) 반환
            // 이 컨테이너가 앱 전체에서 View, ViewModel, Service 등 객체 생성/주입 책임
            return services.BuildServiceProvider();
        }
    }
}
