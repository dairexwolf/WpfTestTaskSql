using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace WpfSqlLite
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Контекст бд
        ApplicationContext db;
        // Переменная для смены категорий
        static ObservableCollection<string> oc;
        public MainWindow()
        {
            InitializeComponent();
            LoadDB();
            Balance.DataContext = db;
        }
        // Загрузка данных из БД
        private void LoadDB()
        {
            db = new ApplicationContext();
            db.Operations.Load();
            this.DataContext = db.Operations.Local.ToBindingList();
        }
        // Добавление данных пользователем
        private void AddData(object sender, RoutedEventArgs e)
        {
            float sum;
            // Проверка введенной пользователем суммы
            if (float.TryParse(AddSum.Text, out sum)&&(sum>0))
            {
                string _type = AddType.Text;
                string _category = AddCategory.Text;
                // Проверка выбранных пользователем типом и категорий, чтобы не было нулевых
                if ((_type != null && _type != "")&&(_category != null && _category != ""))
                {
                    // После - добавление данных в БД
                    Operation op = new Operation { Date = DateTime.Now.ToShortDateString(), Sum = sum, Type = _type, Category = _category, Comment = AddComment.Text };
                    // db.Operations.Add(new Operation { Date=DateTime.Now.ToShortDateString(), Category="Деньга", Sum=1000, Type="Доход"} );
                    db.Operations.Add(op);
                    db.SaveChanges();
                    db.UpdateSum();
                }
            }
        }
        // Метод изменения категории в зависимости от типа
        private void AddType_Selected(object sender, RoutedEventArgs e)
        {
            // Конвентирование типа объекта для дальнейшей работы с ним
            ComboBox comboBox = (ComboBox)sender;
            // Узнаем, что пользователь выбрал. Проверку можно опустить, т.к. данный метод вызывается событием
            TextBlock selectedItem = (TextBlock)comboBox.SelectedItem;
            string name = selectedItem.Text.ToString();
            // В зависимости от того, что выбрал пользователь, меняются коллекции категорий
            switch (name)
            {
                case "Доход":
                    oc = new ObservableCollection<string> { "ЗП", "Возврат долга", "Дивиденды", "Прочее" };
                    AddCategory.ItemsSource = oc;
                    break;
                case "Расход":
                    oc = new ObservableCollection<string> { "Развлечения", "Еда", "Транспорт", "Прочее" };
                    AddCategory.ItemsSource = oc;
                    break;
                default:
                    break;
            }
            
        }
    }
    // Класс, представляющий данные. Реализует INotifyPropertyChanged, чтобы сообщать оболочке о своем изменении
    public class Operation : INotifyPropertyChanged
    {
        private string date;
        private float sum;
        private string type;
        private string category;
        private string comment;
        public int ID { get; set; }
        public string Date
        {
            get { return date; }
            set{ date = value;
                OnPropertyChanged("Date");
            }
        }
        public float Sum
        {
            get { return sum; }
            set
            {
                sum = value;
                OnPropertyChanged("Sum");
            }
        }
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }
        public string Category
        {
            get { return category; }
            set
            {
                category = value;
                OnPropertyChanged("Category");
            }
        }
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                OnPropertyChanged("Comment");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop="")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    // Контекст БД. Содержит в себе коллекцию операций из БД, а также функциональность из EF для установки связи и работы с БД. Реализует INotifyPropertyChanged,
    // чтобы обновлять баланс
    public class ApplicationContext : DbContext, INotifyPropertyChanged
    {
        private float sum;
        // В App.config путь установлен как DefaultConnection
        public ApplicationContext() : base("DefaultConnection")
        {
        }
        // набор данных из БД
        public DbSet<Operation> Operations { get; set; }
        // Свойство суммы всех операций. Нужен, чтобы отображать баланс
        public float Sum
        {
            get
            {
                sum = 0;
                foreach (Operation op in Operations)
                {
                    if (op.Type.Equals("Доход"))
                        sum += op.Sum;
                    if (op.Type.Equals("Расход"))
                        sum -= op.Sum;
                }
                
                return sum;
            }
        }
        // Перерасчет суммы наблюдается при добавлении новой операции, а также сообщается оболочке
        public void UpdateSum()
        {
            sum = this.Sum;
            OnPropertyChanged("Sum");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


}
