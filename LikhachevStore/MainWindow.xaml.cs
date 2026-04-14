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
        public string Supplier { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; } = "";
        public string ImagePath { get; set; } = "";

        public decimal FinalPrice => Price * (1 - Discount / 100);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class MainWindow : Window
    {
        private string currentUserRole = "";
        private string currentUserName = "";
        private string currentFilterSupplier = "";
        private string currentSearchText = "";
        private string currentSortOrder = "ASC";

        private ObservableCollection<Product> allProducts = new ObservableCollection<Product>();
        private ObservableCollection<Product> filteredProducts = new ObservableCollection<Product>();

        private Product _editingProduct = null;
        private string _selectedImagePath = "";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            LoadTestData();
            LoadSuppliersFromData();
            LoadEditComboBoxes();
        }

        private string GetImagesPath()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesPath = Path.Combine(exePath, "images");
            if (Directory.Exists(imagesPath))
                return imagesPath;

            string projectPath = @"C:\Users\dobva\source\repos\LikhachevStore\LikhachevStore\images";
            if (Directory.Exists(projectPath))
                return projectPath;

            return "images";
        }

        private void LoadTestData()
        {
            allProducts.Clear();
            string imagesPath = GetImagesPath();

            // ВСЕ ТОВАРЫ (11 штук)
            allProducts.Add(new Product { Id = 1, Name = "Кроссовки Air Max", Category = "Кроссовки", Manufacturer = "Nike", Supplier = "ОбувьОпт", Price = 8990, Discount = 10, StockQuantity = 25, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "1.jpg") });
            allProducts.Add(new Product { Id = 2, Name = "Ботинки Timberland", Category = "Ботинки", Manufacturer = "Timberland", Supplier = "ShoeImport", Price = 12990, Discount = 15, StockQuantity = 15, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "2.jpg") });
            allProducts.Add(new Product { Id = 3, Name = "Туфли ECCO", Category = "Туфли", Manufacturer = "ECCO", Supplier = "ОбувьОпт", Price = 7990, Discount = 0, StockQuantity = 8, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "3.jpg") });
            allProducts.Add(new Product { Id = 4, Name = "Сандалии летние", Category = "Сандалии", Manufacturer = "Puma", Supplier = "ShoeImport", Price = 3990, Discount = 5, StockQuantity = 42, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "4.jpg") });
            allProducts.Add(new Product { Id = 5, Name = "Сапоги женские", Category = "Сапоги", Manufacturer = "ECCO", Supplier = "СпортТовары", Price = 15990, Discount = 20, StockQuantity = 12, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "5.jpg") });
            allProducts.Add(new Product { Id = 6, Name = "Кроссовки беговые", Category = "Кроссовки", Manufacturer = "Adidas", Supplier = "ОбувьОпт", Price = 6990, Discount = 0, StockQuantity = 35, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "6.jpg") });
            allProducts.Add(new Product { Id = 7, Name = "Ботинки треккинговые", Category = "Ботинки", Manufacturer = "Timberland", Supplier = "ShoeImport", Price = 11990, Discount = 10, StockQuantity = 5, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "7.jpg") });
            allProducts.Add(new Product { Id = 8, Name = "Туфли летние", Category = "Туфли", Manufacturer = "Reebok", Supplier = "ОбувьОпт", Price = 5490, Discount = 0, StockQuantity = 18, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "8.jpg") });
            allProducts.Add(new Product { Id = 9, Name = "Сандалии детские", Category = "Сандалии", Manufacturer = "Puma", Supplier = "СпортТовары", Price = 2990, Discount = 0, StockQuantity = 30, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "9.jpg") });
            allProducts.Add(new Product { Id = 10, Name = "Сапоги мужские", Category = "Сапоги", Manufacturer = "Timberland", Supplier = "ShoeImport", Price = 18990, Discount = 25, StockQuantity = 7, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "10.jpg") });
            allProducts.Add(new Product { Id = 11, Name = "Кроссовки Puma (НЕТ В НАЛИЧИИ)", Category = "Кроссовки", Manufacturer = "Puma", Supplier = "СпортТовары", Price = 4990, Discount = 0, StockQuantity = 0, Unit = "Пара", ImagePath = Path.Combine(imagesPath, "1.jpg") });

            LoadProducts();
        }

        private void LoadEditComboBoxes()
        {
            var categories = new[] { "Кроссовки", "Ботинки", "Туфли", "Сандалии", "Сапоги" };
            cmbEditCategory.ItemsSource = categories;
            cmbEditCategory.SelectedIndex = 0;

            var manufacturers = new[] { "Nike", "Adidas", "Reebok", "Puma", "ECCO", "Timberland" };
            cmbEditManufacturer.ItemsSource = manufacturers;
            cmbEditManufacturer.SelectedIndex = 0;

            var suppliers = new[] { "ОбувьОпт", "ShoeImport", "СпортТовары" };
            cmbEditSupplier.ItemsSource = suppliers;
            cmbEditSupplier.SelectedIndex = 0;

            var units = new[] { "Штука", "Пара", "Комплект" };
            cmbEditUnit.ItemsSource = units;
            cmbEditUnit.SelectedIndex = 1;
        }

        private void LoadSuppliersFromData()
        {
            var suppliers = allProducts.Select(p => p.Supplier).Distinct().OrderBy(s => s).ToList();
            cmbSupplier.Items.Clear();
            cmbSupplier.Items.Add("Все поставщики");
            foreach (var supplier in suppliers)
                cmbSupplier.Items.Add(supplier);
            cmbSupplier.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            var query = allProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(currentFilterSupplier) && currentFilterSupplier != "Все поставщики")
                query = query.Where(p => p.Supplier == currentFilterSupplier);

            if (!string.IsNullOrEmpty(currentSearchText))
                query = query.Where(p => p.Name.ToLower().Contains(currentSearchText.ToLower()));

            if (currentSortOrder == "ASC")
                query = query.OrderBy(p => p.StockQuantity);
            else
                query = query.OrderByDescending(p => p.StockQuantity);

            filteredProducts.Clear();
            foreach (var product in query)
                filteredProducts.Add(product);

            dgvProducts.ItemsSource = filteredProducts;
            txtStatus.Text = $"Загружено товаров: {filteredProducts.Count}";
        }

        private void dgvProducts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var product = e.Row.Item as Product;
            if (product != null)
            {
                if (product.StockQuantity == 0)
                {
                    e.Row.Background = Brushes.LightBlue;
                    e.Row.Foreground = Brushes.Black;
                }
                else if (product.Discount >= 15)
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

        private void OpenEditForm(Product product = null)
        {
            if (product == null)
            {
                _editingProduct = new Product { Id = allProducts.Count > 0 ? allProducts.Max(p => p.Id) + 1 : 1 };
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                txtEditName.Text = "";
                txtEditPrice.Text = "";
                txtEditDiscount.Text = "";
                txtEditQuantity.Text = "";
                cmbEditCategory.SelectedIndex = 0;
                cmbEditManufacturer.SelectedIndex = 0;
                cmbEditSupplier.SelectedIndex = 0;
                cmbEditUnit.SelectedIndex = 1;
                imgEditPhoto.Source = null;
                _selectedImagePath = "";
            }
            else
            {
                _editingProduct = product;
                btnDeleteProduct.Visibility = Visibility.Visible;
                txtEditName.Text = product.Name;
                txtEditPrice.Text = product.Price.ToString();
                txtEditDiscount.Text = product.Discount.ToString();
                txtEditQuantity.Text = product.StockQuantity.ToString();
                cmbEditCategory.SelectedItem = product.Category;
                cmbEditManufacturer.SelectedItem = product.Manufacturer;
                cmbEditSupplier.SelectedItem = product.Supplier;
                cmbEditUnit.SelectedItem = product.Unit;
                if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
                {
                    imgEditPhoto.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(product.ImagePath));
                    _selectedImagePath = product.ImagePath;
                }
            }

            pnlEditForm.Visibility = Visibility.Visible;
            txtEditError.Text = "";
        }

        private void BtnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                imgEditPhoto.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void BtnSaveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEditName.Text))
            {
                txtEditError.Text = "Введите наименование товара!";
                return;
            }

            if (!decimal.TryParse(txtEditPrice.Text, out decimal price) || price < 0)
            {
                txtEditError.Text = "Введите корректную цену!";
                return;
            }

            if (!decimal.TryParse(txtEditDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                txtEditError.Text = "Введите скидку (0-100)!";
                return;
            }

            if (!int.TryParse(txtEditQuantity.Text, out int quantity) || quantity < 0)
            {
                txtEditError.Text = "Введите корректное количество!";
                return;
            }

            string savedImagePath = _editingProduct.ImagePath;
            if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(_selectedImagePath);
                savedImagePath = Path.Combine(imagesFolder, newFileName);
                File.Copy(_selectedImagePath, savedImagePath, true);
            }

            _editingProduct.Name = txtEditName.Text;
            _editingProduct.Category = cmbEditCategory.SelectedItem?.ToString() ?? "";
            _editingProduct.Manufacturer = cmbEditManufacturer.SelectedItem?.ToString() ?? "";
            _editingProduct.Supplier = cmbEditSupplier.SelectedItem?.ToString() ?? "";
            _editingProduct.Price = price;
            _editingProduct.Discount = discount;
            _editingProduct.StockQuantity = quantity;
            _editingProduct.Unit = cmbEditUnit.SelectedItem?.ToString() ?? "Пара";
            _editingProduct.ImagePath = savedImagePath;

            if (_editingProduct.Id == 0 || !allProducts.Any(p => p.Id == _editingProduct.Id))
            {
                _editingProduct.Id = allProducts.Count > 0 ? allProducts.Max(p => p.Id) + 1 : 1;
                allProducts.Add(_editingProduct);
            }

            pnlEditForm.Visibility = Visibility.Collapsed;
            LoadSuppliersFromData();
            LoadProducts();
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_editingProduct == null || _editingProduct.Id == 0)
                return;

            var result = MessageBox.Show($"Удалить товар \"{_editingProduct.Name}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                allProducts.Remove(_editingProduct);
                pnlEditForm.Visibility = Visibility.Collapsed;
                LoadSuppliersFromData();
                LoadProducts();
            }
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            pnlEditForm.Visibility = Visibility.Collapsed;
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            OpenEditForm(null);
        }

        private void dgvProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может редактировать товары!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedProduct = dgvProducts.SelectedItem as Product;
            if (selectedProduct != null)
            {
                OpenEditForm(selectedProduct);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) { }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (login == "admin" && password == "admin")
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

                LoadProducts();
            }
            else if (login == "manager" && password == "manager")
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

                LoadProducts();
            }
            else if (login == "client" && password == "client")
            {
                currentUserName = "Клиент";
                currentUserRole = "Клиент";

                pnlLogin.Visibility = Visibility.Collapsed;
                pnlProducts.Visibility = Visibility.Visible;
                pnlUserInfo.Visibility = Visibility.Visible;

                txtUserName.Text = currentUserName;
                txtUserRole.Text = currentUserRole;
                pnlSearchFilter.Visibility = Visibility.Collapsed;

                LoadProducts();
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

            LoadProducts();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            pnlLogin.Visibility = Visibility.Visible;
            pnlProducts.Visibility = Visibility.Collapsed;
            pnlUserInfo.Visibility = Visibility.Collapsed;
            pnlEditForm.Visibility = Visibility.Collapsed;
            txtLogin.Clear();
            txtPassword.Clear();
            txtError.Text = "";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearchText = txtSearch.Text;
            LoadProducts();
        }

        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSupplier.SelectedItem != null)
            {
                currentFilterSupplier = cmbSupplier.SelectedItem.ToString() ?? "";
                LoadProducts();
            }
        }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e)
        {
            currentSortOrder = "ASC";
            LoadProducts();
        }

        private void BtnSortDesc_Click(object sender, RoutedEventArgs e)
        {
            currentSortOrder = "DESC";
            LoadProducts();
        }
    }
}