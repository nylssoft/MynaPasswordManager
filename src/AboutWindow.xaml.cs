using System.Reflection;
using System.Windows;

namespace PasswordManager
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var productAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            if (productAttribute.Length > 0 && versionAttribute.Length > 0)
            {
                var p = productAttribute[0] as AssemblyProductAttribute;
                var v = versionAttribute[0] as AssemblyFileVersionAttribute;
                if (p != null && v != null)
                {
                    Title = $"{p.Product} Version {v.Version}";
                }
            }
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
