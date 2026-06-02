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
using System.Windows.Shapes;

namespace projectName.user
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private int currentAdminId;
        private string currentAdminName;
        public UserWindow(int id, string name)
        {
            InitializeComponent();
            this.currentAdminId = id;
            this.currentAdminName = name;
            this.Title = $"Панель пользователя | Вы зашли как: {currentAdminName} (ID: {currentAdminId})";
        }
    }
}
