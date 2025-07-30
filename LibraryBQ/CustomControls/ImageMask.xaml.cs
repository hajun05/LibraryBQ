using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// 특정 컨트롤 형식을 메소드처럼 등록, 인수를 전달하며 재사용할 수 있는 가장 표준적인 접근법.
// 의존 프로퍼티가 매개변수 역할 담당. 제작 컨트롤 호출시 값 입력.
// 동적 프로퍼티 값 지정에 한계가 있는 스타일 등록을 보완.
namespace LibraryBQ.CustomControls
{
    // User Control 사용. 이미지 출력과 색상 마스킹
    public partial class ImageMask : UserControl
    {
        public ImageMask()
        {
            InitializeComponent();
        }

        public Brush MaskingColor // 마스킹할 색상 의존 프로퍼티
        {
            get { return (Brush)GetValue(RectangleFillProperty); }
            set { SetValue(RectangleFillProperty, value); }
        }
        public static readonly DependencyProperty RectangleFillProperty =
            DependencyProperty.Register("RectangleFill", typeof(Brush), typeof(ImageMask));

        public ImageSource MaskImageSource // 이미지 경로 의존 프로퍼티
        {
            get { return (ImageSource)GetValue(MaskImageSourceProperty); }
            set { SetValue(MaskImageSourceProperty, value); }
        }
        public static readonly DependencyProperty MaskImageSourceProperty =
            DependencyProperty.Register("MaskImageSource", typeof(ImageSource), typeof(ImageMask));
    }
}
