using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace LikhachevStore
{
    public class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public decimal FinalPrice => Price * (1 - Discount / 100);
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class MainWindow : Window
    {
        private string currentUserRole = "";
        private string currentUserName = "";
        private string currentFilterCategory = "";
        private string currentSearchText = "";
        private string currentSortOrder = "ASC";

        private ObservableCollection<Product> allProducts = new ObservableCollection<Product>();

        public MainWindow()
        {
            InitializeComponent();
            LoadTestData();
            LoadCategories();
            FilterProducts();
        }

        private string GetImagesPath()
        {
            // СНАЧАЛА ИЩЕТ ПАПКУ РЯДОМ С EXE
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesPath = Path.Combine(exePath, "images");

            if (Directory.Exists(imagesPath))
                return imagesPath;

            // ЕСЛИ НЕТ - ИЩЕТ В ПАПКЕ ПРОЕКТА (ДЛЯ ОТЛАДКИ)
            string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\images"));

            if (Directory.Exists(projectPath))
                return projectPath;

            // ЕСЛИ ВСЕ ЕЩЕ НЕТ - СОЗДАЁТ ПАПКУ РЯДОМ С EXE
            Directory.CreateDirectory(imagesPath);
            return imagesPath;
        }

        private void LoadTestData()
        {
            allProducts.Clear();
            string img = GetImagesPath();

            allProducts.Add(new Product { Id = 1, Name = "Кроссовки Air Max", Category = "Кроссовки", Manufacturer = "Nike", Price = 8990, Discount = 10, StockQuantity = 25, Unit = "Пара", ImagePath = Path.Combine(img, "1.jpg") });
            allProducts.Add(new Product { Id = 2, Name = "Ботинки Timberland", Category = "Ботинки", Manufacturer = "Timberland", Price = 12990, Discount = 15, StockQuantity = 15, Unit = "Пара", ImagePath = Path.Combine(img, "2.jpg") });
            allProducts.Add(new Product { Id = 3, Name = "Туфли ECCO", Category = "Туфли", Manufacturer = "ECCO", Price = 7990, Discount = 0, StockQuantity = 8, Unit = "Пара", ImagePath = Path.Combine(img, "3.jpg") });
            allProducts.Add(new Product { Id = 4, Name = "Сандалии летние", Category = "Сандалии", Manufacturer = "Puma", Price = 3990, Discount = 5, StockQuantity = 42, Unit = "Пара", ImagePath = Path.Combine(img, "4.jpg") });
            allProducts.Add(new Product { Id = 5, Name = "Сапоги женские", Category = "Сапоги", Manufacturer = "ECCO", Price = 15990, Discount = 20, StockQuantity = 12, Unit = "Пара", ImagePath = Path.Combine(img, "5.jpg") });
            allProducts.Add(new Product { Id = 6, Name = "Кроссовки беговые", Category = "Кроссовки", Manufacturer = "Adidas", Price = 6990, Discount = 0, StockQuantity = 35, Unit = "Пара", ImagePath = Path.Combine(img, "6.jpg") });
            allProducts.Add(new Product { Id = 7, Name = "Ботинки треккинговые", Category = "Ботинки", Manufacturer = "Timberland", Price = 11990, Discount = 10, StockQuantity = 5, Unit = "Пара", ImagePath = Path.Combine(img, "7.jpg") });
            allProducts.Add(new Product { Id = 8, Name = "Туфли летние", Category = "Туфли", Manufacturer = "Reebok", Price = 5490, Discount = 0, StockQuantity = 18, Unit = "Пара", ImagePath = Path.Combine(img, "8.jpg") });
            allProducts.Add(new Product { Id = 9, Name = "Сандалии детские", Category = "Сандалии", Manufacturer = "Puma", Price = 2990, Discount = 0, StockQuantity = 30, Unit = "Пара", ImagePath = Path.Combine(img, "9.jpg") });
            allProducts.Add(new Product { Id = 10, Name = "Сапоги мужские", Category = "Сапоги", Manufacturer = "Timberland", Price = 18990, Discount = 25, StockQuantity = 7, Unit = "Пара", ImagePath = Path.Combine(img, "10.jpg") });
            allProducts.Add(new Product { Id = 11, Name = "Кроссовки Puma (НЕТ В НАЛИЧИИ)", Category = "Кроссовки", Manufacturer = "Puma", Price = 4990, Discount = 0, StockQuantity = 0, Unit = "Пара", ImagePath = Path.Combine(img, "1.jpg") });
        }

        private void LoadCategories()
        {
            var categories = allProducts.Select(p => p.Category).Distinct().OrderBy(s => s).ToList();
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("Все категории");
            foreach (var c in categories) cmbCategory.Items.Add(c);
            cmbCategory.SelectedIndex = 0;
        }

        private void FilterProducts()
        {
            var query = allProducts.AsEnumerable();
            if (!string.IsNullOrEmpty(currentFilterCategory) && currentFilterCategory != "Все категории")
                query = query.Where(p => p.Category == currentFilterCategory);
            if (!string.IsNullOrEmpty(currentSearchText))
                query = query.Where(p => p.Name.ToLower().Contains(currentSearchText.ToLower()));
            if (currentSortOrder == "ASC")
                query = query.OrderBy(p => p.StockQuantity);
            else
                query = query.OrderByDescending(p => p.StockQuantity);

            dgvProducts.ItemsSource = query.ToList();
            txtStatus.Text = $"Товаров: {query.Count()}";
        }

        private void dgvProducts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var p = e.Row.Item as Product;
            if (p != null)
            {
                if (p.StockQuantity == 0)
                {
                    e.Row.Background = Brushes.LightBlue;
                    e.Row.Foreground = Brushes.Black;
                }
                else if (p.Discount > 15)
                {
                    e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57"));
                    e.Row.Foreground = Brushes.White;
                }
                else
                {
                    e.Row.Background = Brushes.White;
                    e.Row.Foreground = Brushes.Black;
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может редактировать товары!");
                return;
            }

            var product = (sender as Button)?.Tag as Product;
            if (product == null) return;

            var dialog = new Window
            {
                Title = "Редактирование товара",
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.White
            };

            var stack = new StackPanel { Margin = new Thickness(10) };

            stack.Children.Add(new TextBlock { Text = "Наименование:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 10, 0, 5) });
            var txtName = new TextBox { Text = product.Name, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtName);

            stack.Children.Add(new TextBlock { Text = "Категория:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbCat = new ComboBox { ItemsSource = new[] { "Кроссовки", "Ботинки", "Туфли", "Сандалии", "Сапоги" }, SelectedItem = product.Category, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbCat);

            stack.Children.Add(new TextBlock { Text = "Производитель:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbMan = new ComboBox { ItemsSource = new[] { "Nike", "Adidas", "Reebok", "Puma", "ECCO", "Timberland" }, SelectedItem = product.Manufacturer, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbMan);

            stack.Children.Add(new TextBlock { Text = "Цена:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtPrice = new TextBox { Text = product.Price.ToString(), FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtPrice);

            stack.Children.Add(new TextBlock { Text = "Скидка (%):", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtDiscount = new TextBox { Text = product.Discount.ToString(), FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtDiscount);

            stack.Children.Add(new TextBlock { Text = "Количество:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtQuantity = new TextBox { Text = product.StockQuantity.ToString(), FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtQuantity);

            stack.Children.Add(new TextBlock { Text = "Ед. изм.:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbUnit = new ComboBox { ItemsSource = new[] { "Штука", "Пара", "Комплект" }, SelectedItem = product.Unit, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbUnit);

            var btnSave = new Button { Content = "Сохранить", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FA9A")), Padding = new Thickness(10), Margin = new Thickness(0, 10, 0, 5), FontFamily = new FontFamily("Times New Roman"), FontWeight = FontWeights.Bold };
            var btnCancel = new Button { Content = "Отмена", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFF00")), Padding = new Thickness(10), Margin = new Thickness(0, 5, 0, 0), FontFamily = new FontFamily("Times New Roman"), FontWeight = FontWeights.Bold };

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);

            dialog.Content = stack;

            btnSave.Click += (s, args) =>
            {
                if (decimal.TryParse(txtPrice.Text, out decimal price) && price >= 0 &&
                    decimal.TryParse(txtDiscount.Text, out decimal discount) && discount >= 0 && discount <= 100 &&
                    int.TryParse(txtQuantity.Text, out int qty) && qty >= 0)
                {
                    product.Name = txtName.Text;
                    product.Category = cmbCat.SelectedItem?.ToString() ?? "";
                    product.Manufacturer = cmbMan.SelectedItem?.ToString() ?? "";
                    product.Price = price;
                    product.Discount = discount;
                    product.StockQuantity = qty;
                    product.Unit = cmbUnit.SelectedItem?.ToString() ?? "Пара";
                    FilterProducts();
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Проверьте введенные данные!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            btnCancel.Click += (s, args) => dialog.Close();

            dialog.ShowDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может удалять товары!");
                return;
            }

            var product = (sender as Button)?.Tag as Product;
            if (product != null)
            {
                if (MessageBox.Show($"Удалить товар \"{product.Name}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    allProducts.Remove(product);
                    FilterProducts();
                }
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может добавлять товары!");
                return;
            }

            var dialog = new Window
            {
                Title = "Добавление товара",
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.White
            };

            var stack = new StackPanel { Margin = new Thickness(10) };

            stack.Children.Add(new TextBlock { Text = "Наименование:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 10, 0, 5) });
            var txtName = new TextBox { FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtName);

            stack.Children.Add(new TextBlock { Text = "Категория:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbCat = new ComboBox { ItemsSource = new[] { "Кроссовки", "Ботинки", "Туфли", "Сандалии", "Сапоги" }, SelectedIndex = 0, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbCat);

            stack.Children.Add(new TextBlock { Text = "Производитель:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbMan = new ComboBox { ItemsSource = new[] { "Nike", "Adidas", "Reebok", "Puma", "ECCO", "Timberland" }, SelectedIndex = 0, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbMan);

            stack.Children.Add(new TextBlock { Text = "Цена:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtPrice = new TextBox { FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtPrice);

            stack.Children.Add(new TextBlock { Text = "Скидка (%):", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtDiscount = new TextBox { Text = "0", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtDiscount);

            stack.Children.Add(new TextBlock { Text = "Количество:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtQuantity = new TextBox { Text = "0", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtQuantity);

            stack.Children.Add(new TextBlock { Text = "Ед. изм.:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var cmbUnit = new ComboBox { ItemsSource = new[] { "Штука", "Пара", "Комплект" }, SelectedIndex = 1, FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(cmbUnit);

            stack.Children.Add(new TextBlock { Text = "Путь к фото:", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 5, 0, 5) });
            var txtImage = new TextBox { Text = "1.jpg", FontFamily = new FontFamily("Times New Roman"), Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(txtImage);

            var btnSave = new Button { Content = "Сохранить", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FA9A")), Padding = new Thickness(10), Margin = new Thickness(0, 10, 0, 5), FontFamily = new FontFamily("Times New Roman"), FontWeight = FontWeights.Bold };
            var btnCancel = new Button { Content = "Отмена", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFF00")), Padding = new Thickness(10), Margin = new Thickness(0, 5, 0, 0), FontFamily = new FontFamily("Times New Roman"), FontWeight = FontWeights.Bold };

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);

            dialog.Content = stack;

            btnSave.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Неверная цена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Скидка должна быть от 0 до 100!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
                {
                    MessageBox.Show("Неверное количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int newId = allProducts.Count > 0 ? allProducts.Max(x => x.Id) + 1 : 1;
                string imgPath = Path.Combine(GetImagesPath(), txtImage.Text);

                allProducts.Add(new Product
                {
                    Id = newId,
                    Name = txtName.Text,
                    Category = cmbCat.SelectedItem?.ToString() ?? "",
                    Manufacturer = cmbMan.SelectedItem?.ToString() ?? "",
                    Price = price,
                    Discount = discount,
                    StockQuantity = qty,
                    Unit = cmbUnit.SelectedItem?.ToString() ?? "Пара",
                    ImagePath = imgPath
                });

                LoadCategories();
                FilterProducts();
                dialog.Close();
            };

            btnCancel.Click += (s, args) => dialog.Close();

            dialog.ShowDialog();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string pass = txtPassword.Password;

            if (login == "admin" && pass == "admin")
            {
                currentUserName = "Администратор";
                currentUserRole = "Администратор";
                pnlLogin.Visibility = Visibility.Collapsed;
                pnlProducts.Visibility = Visibility.Visible;
                pnlUserInfo.Visibility = Visibility.Visible;
                txtUserName.Text = currentUserName;
                txtUserRole.Text = currentUserRole;
                pnlSearchFilter.Visibility = Visibility.Visible;
                btnAddProduct.Visibility = Visibility.Visible;
                FilterProducts();
            }
            else if (login == "manager" && pass == "manager")
            {
                currentUserName = "Менеджер";
                currentUserRole = "Менеджер";
                pnlLogin.Visibility = Visibility.Collapsed;
                pnlProducts.Visibility = Visibility.Visible;
                pnlUserInfo.Visibility = Visibility.Visible;
                txtUserName.Text = currentUserName;
                txtUserRole.Text = currentUserRole;
                pnlSearchFilter.Visibility = Visibility.Visible;
                btnAddProduct.Visibility = Visibility.Collapsed;
                FilterProducts();
            }
            else if (login == "client" && pass == "client")
            {
                currentUserName = "Клиент";
                currentUserRole = "Клиент";
                pnlLogin.Visibility = Visibility.Collapsed;
                pnlProducts.Visibility = Visibility.Visible;
                pnlUserInfo.Visibility = Visibility.Visible;
                txtUserName.Text = currentUserName;
                txtUserRole.Text = currentUserRole;
                pnlSearchFilter.Visibility = Visibility.Collapsed;
                FilterProducts();
            }
            else
            {
                txtError.Text = "Неверный логин или пароль!";
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            currentUserName = "Гость";
            currentUserRole = "Гость";
            pnlLogin.Visibility = Visibility.Collapsed;
            pnlProducts.Visibility = Visibility.Visible;
            pnlUserInfo.Visibility = Visibility.Visible;
            txtUserName.Text = "Гость";
            txtUserRole.Text = "Гость";
            pnlSearchFilter.Visibility = Visibility.Collapsed;
            FilterProducts();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            pnlLogin.Visibility = Visibility.Visible;
            pnlProducts.Visibility = Visibility.Collapsed;
            pnlUserInfo.Visibility = Visibility.Collapsed;
            txtLogin.Clear();
            txtPassword.Clear();
            txtError.Text = "";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearchText = txtSearch.Text;
            FilterProducts();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem != null)
            {
                currentFilterCategory = cmbCategory.SelectedItem.ToString();
                FilterProducts();
            }
        }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e)
        {
            currentSortOrder = "ASC";
            FilterProducts();
        }

        private void BtnSortDesc_Click(object sender, RoutedEventArgs e)
        {
            currentSortOrder = "DESC";
            FilterProducts();
        }
    }
}