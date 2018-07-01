using System.Windows;
using System.Windows.Controls;

namespace SimpleNotes
{
    public class FiButton : Button
    {
        static FiButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FiButton), new FrameworkPropertyMetadata(typeof(FiButton)));
        }
    }
}
