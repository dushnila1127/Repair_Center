using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using RepairCenter.Entities;
using RepairCenter.Security;

namespace RepairCenter
{
    public partial class MainWindow : Window
    {
        private string _page = "Orders";
        private DataGrid _grid;
        private TextBox _search;
        private DatePicker _searchDate;
        private DatePicker _dateFrom;
        private DatePicker _dateTo;
        private TextBlock _summary;
        private List<ReportRow> _reportRows = new List<ReportRow>();
        private bool _reportGenerated;

        private bool IsAdministrator { get { return UserSession.Current != null && UserSession.Current.IsAdministrator; } }

        public MainWindow()
        {
            InitializeComponent();
            var user = UserSession.Current;
            if (user == null) { Close(); return; }
            Title = "Repair center";
            SessionLabel.Text = user.FullName + " · " + (user.IsAdministrator ? "admin" : "employee");
            Loaded += delegate { Navigate("Orders"); };
        }

        private void Orders_Click(object sender, RoutedEventArgs e) { Navigate("Orders"); }
        private void Clients_Click(object sender, RoutedEventArgs e) { Navigate("Clients"); }
        private void Equipment_Click(object sender, RoutedEventArgs e) { Navigate("Equipment"); }
        private void Employees_Click(object sender, RoutedEventArgs e) { Navigate("Employees"); }
        private void Reports_Click(object sender, RoutedEventArgs e) { Navigate("Reports"); }

        private void Navigate(string page)
        {
            _page = page;
            SetNavigationStyle();
            BuildPage();
            if (_page == "Reports")
            {
                _reportRows = new List<ReportRow>();
                _reportGenerated = false;
                _grid.ItemsSource = _reportRows;
                _summary.Text = "Выбери период и нажми «Сформировать отчёт». В отчёт попадут неотменённые заказы с финальным статусом и датой выдачи в выбранном периоде.";
            }
            else LoadPage();
        }

        private void SetNavigationStyle()
        {
            var buttons = new Dictionary<string, Button> { { "Orders", OrdersNav }, { "Clients", ClientsNav }, { "Equipment", EquipmentNav }, { "Employees", EmployeesNav }, { "Reports", ReportsNav } };
            foreach (var pair in buttons)
            {
                pair.Value.Background = pair.Key == _page ? Brush("#314766") : Brushes.Transparent;
                pair.Value.Foreground = Brushes.White;
            }
        }

        private void BuildPage()
        {
            var root = new DockPanel();
            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            header.Children.Add(new TextBlock { Text = PageTitle(), FontSize = 27, FontWeight = FontWeights.Bold, Foreground = Brush("#182338") });
            header.Children.Add(new TextBlock { Text = PageSubtitle(), FontSize = 13, Foreground = Brush("#6D788B"), Margin = new Thickness(0, 5, 0, 0) });
            DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
            _summary = new TextBlock { FontSize = 12, Foreground = Brush("#6D788B"), Margin = new Thickness(0, 12, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(_summary, Dock.Bottom); root.Children.Add(_summary);
            var body = new DockPanel();
            var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 12), VerticalAlignment = VerticalAlignment.Center };
            if (_page == "Reports")
            {
                _dateFrom = new DatePicker { Width = 145, Height = 36, SelectedDate = DateTime.Today.AddDays(-30), Margin = new Thickness(0, 0, 8, 0) };
                _dateTo = new DatePicker { Width = 145, Height = 36, SelectedDate = DateTime.Today, Margin = new Thickness(0, 0, 8, 0) };
                toolbar.Children.Add(new TextBlock { Text = "С даты", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), Foreground = Brush("#566277") });
                toolbar.Children.Add(_dateFrom);
                toolbar.Children.Add(new TextBlock { Text = "По дату", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), Foreground = Brush("#566277") });
                toolbar.Children.Add(_dateTo);
                toolbar.Children.Add(MakeButton("Сформировать отчёт", "#2864D7", Brushes.White, GenerateReport_Click));
                toolbar.Children.Add(MakeButton("Скачать CSV", "#17865C", Brushes.White, DownloadReport_Click));
            }
            else
            {
                _search = new TextBox { Width = 250, Height = 36, Padding = new Thickness(10, 7, 10, 4), Margin = new Thickness(0, 0, 8, 0), VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "Текст для поиска" };
                toolbar.Children.Add(_search);
                if (_page == "Orders")
                {
                    _search.ToolTip = "Номер заказа, клиент или неисправность";
                    _searchDate = new DatePicker { Width = 140, Height = 36, Margin = new Thickness(0, 0, 8, 0), ToolTip = "Дата приёма" };
                    toolbar.Children.Add(_searchDate);
                }
                toolbar.Children.Add(MakeButton("Найти", "#2864D7", Brushes.White, Search_Click));
                toolbar.Children.Add(MakeButton("Сбросить", "#E9EDF4", Brush("#25344B"), Reset_Click));
                if (IsAdministrator)
                {
                    if (_page == "Employees") toolbar.Children.Add(MakeButton("Создать учётную запись", "#2864D7", Brushes.White, CreateEmployeeAccount_Click));
                    toolbar.Children.Add(MakeButton("+ Добавить", "#17865C", Brushes.White, Add_Click));
                    toolbar.Children.Add(MakeButton("Изменить", "#E9EDF4", Brush("#25344B"), Edit_Click));
                    toolbar.Children.Add(MakeButton(_page == "Orders" ? "Отменить" : "Удалить", "#FCE8E8", Brush("#A62929"), Delete_Click));
                }
                else
                {
                    toolbar.Children.Add(new TextBlock { Text = "Режим просмотра", Foreground = Brush("#6D788B"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 8, 0) });
                }
            }
            toolbar.Children.Add(MakeButton("Обновить", "#E9EDF4", Brush("#25344B"), Refresh_Click));
            DockPanel.SetDock(toolbar, Dock.Top); body.Children.Add(toolbar);
            _grid = new DataGrid { AutoGenerateColumns = true, IsReadOnly = true, CanUserAddRows = false, SelectionMode = DataGridSelectionMode.Single, SelectionUnit = DataGridSelectionUnit.FullRow, HeadersVisibility = DataGridHeadersVisibility.Column, GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, BorderThickness = new Thickness(0), RowHeight = 38, FontSize = 12, Background = Brushes.White, BorderBrush = Brush("#E5E9F0"), CanUserSortColumns = true };
            _grid.AutoGeneratingColumn += Grid_AutoGeneratingColumn;
            body.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Child = _grid });
            root.Children.Add(body); PageHost.Content = root;
        }

        private Button MakeButton(string text, string bg, Brush fg, RoutedEventHandler handler)
        {
            var b = new Button
            {
                Content = text,
                Height = 36,
                MinWidth = 82,
                Padding = new Thickness(12, 0, 12, 0),
                Margin = new Thickness(0, 0, 8, 0),
                Background = Brush(bg),
                Foreground = fg,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                IsEnabled = true,
                IsHitTestVisible = true,
                Focusable = true,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            if (handler != null)
            {
                b.Click += delegate(object sender, RoutedEventArgs e)
                {
                    try
                    {
                        handler(sender, e);
                    }
                    catch (Exception ex)
                    {
                        ShowError("Не удалось выполнить действие «" + text + "». Проверь подключение и права доступа к базе данных.", ex);
                    }
                };
            }
            return b;
        }

        private Brush Brush(string value) { return (Brush)new BrushConverter().ConvertFromString(value); }
        private string PageTitle() { if (_page == "Orders") return "Заказы на ремонт"; if (_page == "Clients") return "Клиенты"; if (_page == "Equipment") return "Техника"; if (_page == "Employees") return "Исполнители"; return "Отчёты по ремонту"; }
        private string PageSubtitle()
        {
            var suffix = IsAdministrator ? "Режим администратора: просмотр и изменение данных" : "Режим сотрудника: доступен только просмотр и поиск";
            if (_page == "Orders") return "Заказы на ремонт · " + suffix;
            if (_page == "Clients") return "Клиентская база и контакты · " + suffix;
            if (_page == "Equipment") return "Устройства клиентов, модели и серийные номера · " + suffix;
            if (_page == "Employees") return "Сотрудники и специализации · " + suffix;
            return "Отчёт по завершённым заказам и выгрузка CSV";
        }

        private void Grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var headers = new Dictionary<string, string> { { "RepairOrderID", "ID" }, { "OrderNumber", "Номер заказа" }, { "ClientName", "Клиент" }, { "Equipment", "Устройство" }, { "SerialNumber", "Серийный номер" }, { "AcceptedAt", "Дата приёма" }, { "IssuedAt", "Дата выдачи" }, { "FaultDescription", "Неисправность" }, { "RepairCost", "Стоимость, ₽" }, { "OrderStatus", "Статус" }, { "AssignedEmployees", "Исполнители" }, { "ClientID", "ID клиента" }, { "FullName", "ФИО" }, { "Phone", "Телефон" }, { "Address", "Адрес" }, { "EquipmentID", "ID техники" }, { "EquipmentType", "Тип техники" }, { "Brand", "Марка" }, { "Model", "Модель" }, { "Notes", "Примечание" }, { "EmployeeID", "ID сотрудника" }, { "Specialization", "Специализация" }, { "IsActive", "Активен" }, { "TotalCost", "Общая стоимость, ₽" }, { "Count", "Количество" } };
            if (headers.ContainsKey(e.PropertyName)) e.Column.Header = headers[e.PropertyName];
            var column = e.Column as DataGridBoundColumn;
            var binding = column == null ? null : column.Binding as Binding;
            if (binding != null && (e.PropertyName == "AcceptedAt" || e.PropertyName == "IssuedAt")) binding.StringFormat = "dd.MM.yyyy";
            if (binding != null && (e.PropertyName == "RepairCost" || e.PropertyName == "TotalCost")) binding.StringFormat = "N2";
        }

        private bool LoadPage()
        {
            try
            {
                if (_page == "Orders") LoadOrders();
                else if (_page == "Clients") LoadClients();
                else if (_page == "Equipment") LoadEquipment();
                else if (_page == "Employees") LoadEmployees();
                else LoadReports();
                return true;
            }
            catch (Exception ex) { ShowError("Не удалось загрузить данные. Проверь подключение и схему базы данных.", ex); return false; }
        }

        private void LoadOrders()
        {
            using (var context = App.Context)
            {
                var query = from order in context.Course_project_0502_RepairOrders
                            join equipment in context.Course_project_0502_Equipment on order.EquipmentID equals equipment.EquipmentID
                            join client in context.Course_project_0502_Clients on equipment.ClientID equals client.ClientID
                            join model in context.Course_project_0502_EquipmentModels on equipment.EquipmentModelID equals model.EquipmentModelID
                            join brand in context.Course_project_0502_EquipmentBrands on model.EquipmentBrandID equals brand.EquipmentBrandID
                            join type in context.Course_project_0502_EquipmentTypes on model.EquipmentTypeID equals type.EquipmentTypeID
                            join status in context.Course_project_0502_OrderStatuses on order.OrderStatusID equals status.OrderStatusID
                            where !order.IsCancelled
                            select new OrderRow { RepairOrderID = order.RepairOrderID, OrderNumber = order.OrderNumber, EquipmentID = order.EquipmentID, OrderStatusID = order.OrderStatusID, ClientName = client.FullName, Equipment = type.TypeName + " " + brand.BrandName + " " + model.ModelName, SerialNumber = equipment.SerialNumber, AcceptedAt = order.AcceptedAt, IssuedAt = order.IssuedAt, FaultDescription = order.FaultDescription, RepairCost = order.RepairCost, OrderStatus = status.StatusName };
                string term = _search == null ? string.Empty : _search.Text.Trim();
                if (!string.IsNullOrWhiteSpace(term)) query = query.Where(x => x.OrderNumber.Contains(term) || x.ClientName.Contains(term) || x.FaultDescription.Contains(term));
                if (_searchDate != null && _searchDate.SelectedDate.HasValue) { DateTime fromDate = _searchDate.SelectedDate.Value.Date; DateTime toDate = fromDate.AddDays(1); query = query.Where(x => x.AcceptedAt >= fromDate && x.AcceptedAt < toDate); }
                var rows = query.OrderByDescending(x => x.AcceptedAt).ToList();
                var assignments = (from a in context.Course_project_0502_OrderAssignments join employee in context.Course_project_0502_Employees on a.EmployeeID equals employee.EmployeeID select new { a.RepairOrderID, employee.FullName }).ToList();
                var names = assignments.GroupBy(x => x.RepairOrderID).ToDictionary(x => x.Key, x => string.Join(", ", x.Select(y => y.FullName)));
                var output = rows.Select(x => new OrderRow { RepairOrderID=x.RepairOrderID, OrderNumber=x.OrderNumber, EquipmentID=x.EquipmentID, OrderStatusID=x.OrderStatusID, ClientName=x.ClientName, Equipment=x.Equipment, SerialNumber=x.SerialNumber, AcceptedAt=x.AcceptedAt, IssuedAt=x.IssuedAt, FaultDescription=x.FaultDescription, RepairCost=x.RepairCost, OrderStatus=x.OrderStatus, AssignedEmployees=names.ContainsKey(x.RepairOrderID)?names[x.RepairOrderID]:string.Empty }).ToList();
                _grid.ItemsSource = output; _summary.Text = "Заказов найдено: " + output.Count;
            }
        }

        private void LoadClients()
        {
            using (var context = App.Context)
            {
                var rows = context.Course_project_0502_Clients.Select(x => new ClientRow { ClientID=x.ClientID, FullName=x.FullName, Phone=x.Phone, Address=x.Address }).ToList();
                string term = _search.Text.Trim(); if (!string.IsNullOrWhiteSpace(term)) rows=rows.Where(x=>Contains(x.FullName,term)||Contains(x.Phone,term)||Contains(x.Address,term)).ToList();
                _grid.ItemsSource=rows.OrderBy(x=>x.FullName).ToList(); _summary.Text="Клиентов найдено: "+rows.Count;
            }
        }

        private void LoadEquipment()
        {
            using (var context = App.Context)
            {
                var rows=(from e in context.Course_project_0502_Equipment join c in context.Course_project_0502_Clients on e.ClientID equals c.ClientID join m in context.Course_project_0502_EquipmentModels on e.EquipmentModelID equals m.EquipmentModelID join b in context.Course_project_0502_EquipmentBrands on m.EquipmentBrandID equals b.EquipmentBrandID join t in context.Course_project_0502_EquipmentTypes on m.EquipmentTypeID equals t.EquipmentTypeID select new EquipmentRow { EquipmentID=e.EquipmentID,ClientID=e.ClientID,EquipmentModelID=e.EquipmentModelID,ClientName=c.FullName,EquipmentType=t.TypeName,Brand=b.BrandName,Model=m.ModelName,SerialNumber=e.SerialNumber,Notes=e.Notes }).ToList();
                string term=_search.Text.Trim(); if(!string.IsNullOrWhiteSpace(term)) rows=rows.Where(x=>Contains(x.SerialNumber,term)||Contains(x.ClientName,term)||Contains(x.Brand,term)||Contains(x.Model,term)).ToList();
                _grid.ItemsSource=rows.OrderBy(x=>x.ClientName).ToList(); _summary.Text="Устройств найдено: "+rows.Count;
            }
        }

        private void LoadEmployees()
        {
            using (var context = App.Context)
            {
                var all=context.Course_project_0502_Employees.Include("Course_project_0502_Specializations").ToList();
                var rows=all.Select(x=>new EmployeeRow { EmployeeID=x.EmployeeID,FullName=x.FullName,Phone=x.Phone,IsActive=x.IsActive,Specialization=string.Join(", ",x.Course_project_0502_Specializations.Select(s=>s.SpecializationName)) }).ToList();
                string term=_search.Text.Trim(); if(!string.IsNullOrWhiteSpace(term)) rows=rows.Where(x=>Contains(x.FullName,term)||Contains(x.Phone,term)||Contains(x.Specialization,term)).ToList();
                _grid.ItemsSource=rows.OrderBy(x=>x.FullName).ToList(); _summary.Text="Исполнителей найдено: "+rows.Count;
            }
        }

        private void LoadReports()
        {
            _reportGenerated = false;
            _reportRows = new List<ReportRow>();
            _grid.ItemsSource = _reportRows;
            if (!_dateFrom.SelectedDate.HasValue || !_dateTo.SelectedDate.HasValue) { _summary.Text="Укажи начальную и конечную даты."; return; }
            DateTime startInclusive=_dateFrom.SelectedDate.Value.Date; DateTime endDate=_dateTo.SelectedDate.Value.Date;
            if(endDate<startInclusive) { MessageBox.Show("Дата окончания не может быть раньше даты начала.","Проверка периода",MessageBoxButton.OK,MessageBoxImage.Warning); return; }
            DateTime endExclusive=endDate.AddDays(1);
            using(var context=App.Context)
            {
                var rows=(from o in context.Course_project_0502_RepairOrders join e in context.Course_project_0502_Equipment on o.EquipmentID equals e.EquipmentID join c in context.Course_project_0502_Clients on e.ClientID equals c.ClientID join m in context.Course_project_0502_EquipmentModels on e.EquipmentModelID equals m.EquipmentModelID join b in context.Course_project_0502_EquipmentBrands on m.EquipmentBrandID equals b.EquipmentBrandID join t in context.Course_project_0502_EquipmentTypes on m.EquipmentTypeID equals t.EquipmentTypeID join s in context.Course_project_0502_OrderStatuses on o.OrderStatusID equals s.OrderStatusID where !o.IsCancelled && s.IsFinal && o.IssuedAt.HasValue && o.IssuedAt.Value>=startInclusive && o.IssuedAt.Value<endExclusive orderby o.IssuedAt select new ReportRow {OrderNumber=o.OrderNumber,ClientName=c.FullName,Equipment=t.TypeName+" "+b.BrandName+" "+m.ModelName,IssuedAt=o.IssuedAt.Value,StatusName=s.StatusName,RepairCost=o.RepairCost}).ToList();
                _reportRows=rows; _grid.ItemsSource=rows; _reportGenerated=true; _summary.Text=rows.Count==0?"Отчёт сформирован: за выбранный период завершённых заказов нет.":"Отчёт сформирован. Заказов: "+rows.Count+"; общая стоимость: "+rows.Sum(x=>x.RepairCost).ToString("N2")+" ₽. Нажми «Скачать CSV», чтобы сохранить таблицу.";
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e) { try { LoadReports(); } catch(Exception ex) { ShowError("Не удалось сформировать отчёт.",ex); } }
        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            if(!_reportGenerated) { MessageBox.Show("Сначала выбери период и нажми «Сформировать отчёт».","Скачать отчёт",MessageBoxButton.OK,MessageBoxImage.Information); return; }
            if(_reportRows.Count==0) { MessageBox.Show("В сформированном отчёте нет записей за выбранный период.","Скачать отчёт",MessageBoxButton.OK,MessageBoxImage.Information); return; }
            var dialog=new SaveFileDialog { Title="Сохранить отчёт RepairCenter",Filter="CSV (*.csv)|*.csv",DefaultExt=".csv",AddExtension=true,FileName="RepairCenter_Report_"+DateTime.Now.ToString("yyyyMMdd_HHmm")+".csv" };
            if(dialog.ShowDialog(this)!=true) return;
            try { using(var writer=new StreamWriter(dialog.FileName,false,new UTF8Encoding(true))) { writer.WriteLine("Номер заказа;Клиент;Устройство;Дата выдачи;Статус;Стоимость"); foreach(var x in _reportRows) writer.WriteLine(string.Join(";",new[]{Csv(x.OrderNumber),Csv(x.ClientName),Csv(x.Equipment),Csv(x.IssuedAt.ToString("dd.MM.yyyy")),Csv(x.StatusName),Csv(x.RepairCost.ToString("0.00",CultureInfo.InvariantCulture))})); } MessageBox.Show("Отчёт сохранён: "+dialog.FileName,"Скачать отчёт",MessageBoxButton.OK,MessageBoxImage.Information); }
            catch(Exception ex) { ShowError("Не удалось сохранить CSV.",ex); }
        }

        private string Csv(string value) { return "\""+(value??string.Empty).Replace("\"","\"\"")+"\""; }
        private void Search_Click(object sender, RoutedEventArgs e) { LoadPage(); }
        private void Reset_Click(object sender, RoutedEventArgs e) { if(_search!=null)_search.Clear(); if(_searchDate!=null)_searchDate.SelectedDate=null; LoadPage(); }
        private void Refresh_Click(object sender, RoutedEventArgs e) { LoadPage(); }

        private void CreateEmployeeAccount_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdministrator()) return;
            var window = new RegistrationWindow { Owner = this };
            if (window.ShowDialog() == true) RefreshWithMessage("Учётная запись сотрудника создана.");
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdministrator()) return;
            try
            {
                if (_page == "Clients") SaveClient(null);
                else if (_page == "Equipment") SaveEquipment(null);
                else if (_page == "Employees") SaveEmployee(null);
                else if (_page == "Orders") SaveOrder(null);
                else MessageBox.Show("Добавление для текущего раздела не предусмотрено.", "RepairCenter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError("Не удалось добавить запись.", ex); }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdministrator()) return;
            try
            {
                if (_page == "Clients") { var x = Selected<ClientRow>(); if (x != null) SaveClient(x); }
                else if (_page == "Equipment") { var x = Selected<EquipmentRow>(); if (x != null) SaveEquipment(x); }
                else if (_page == "Employees") { var x = Selected<EmployeeRow>(); if (x != null) SaveEmployee(x); }
                else if (_page == "Orders") { var x = Selected<OrderRow>(); if (x != null) SaveOrder(x); }
                else MessageBox.Show("Редактирование для текущего раздела не предусмотрено.", "RepairCenter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError("Не удалось изменить запись.", ex); }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdministrator()) return;
            try
            {
                if (_page == "Orders") CancelOrder();
                else if (_page == "Clients") DeleteClient();
                else if (_page == "Equipment") DeleteEquipment();
                else if (_page == "Employees") DeleteEmployee();
                else MessageBox.Show("Удаление для текущего раздела не предусмотрено.", "RepairCenter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError("Не удалось удалить или отменить запись. Проверь связи с другими таблицами.", ex); }
        }

        private void SaveClient(ClientRow row)
        {
            if (!EnsureAdministrator()) return;
            var values=ShowForm(row==null?"Новый клиент":"Изменение клиента",new List<FormField>{TextField("FullName","ФИО",row==null?"":row.FullName,150),TextField("Phone","Телефон",row==null?"":row.Phone,20),TextField("Address","Адрес",row==null?"":row.Address,250)});
            if(values==null)return; Require(values,"FullName","Phone","Address");
            using(var c=App.Context) { Course_project_0502_Clients e; if(row==null){e=new Course_project_0502_Clients();c.Course_project_0502_Clients.Add(e);}else{e=c.Course_project_0502_Clients.Find(row.ClientID);if(e==null)return;} e.FullName=Val(values,"FullName");e.Phone=Val(values,"Phone");e.Address=Val(values,"Address");c.SaveChanges(); }
            RefreshWithMessage(row==null?"Клиент добавлен.":"Изменения клиента сохранены.");
        }
        private void DeleteClient() { if (!EnsureAdministrator()) return; var row=Selected<ClientRow>(); if(row==null||!Confirm("Удалить клиента «"+row.FullName+"»?","Удаление клиента"))return; using(var c=App.Context){var e=c.Course_project_0502_Clients.Find(row.ClientID);if(e!=null){c.Course_project_0502_Clients.Remove(e);c.SaveChanges();}} RefreshWithMessage("Клиент удалён."); }

        private void SaveEquipment(EquipmentRow row)
        {
            if (!EnsureAdministrator()) return;
            List<ChoiceItem> clients; List<ChoiceItem> models;
            using(var c=App.Context){clients=c.Course_project_0502_Clients.OrderBy(x=>x.FullName).ToList().Select(x=>new ChoiceItem(x.ClientID,x.FullName)).ToList();var modelRows=(from m in c.Course_project_0502_EquipmentModels join b in c.Course_project_0502_EquipmentBrands on m.EquipmentBrandID equals b.EquipmentBrandID join t in c.Course_project_0502_EquipmentTypes on m.EquipmentTypeID equals t.EquipmentTypeID orderby t.TypeName,b.BrandName,m.ModelName select new { m.EquipmentModelID, DisplayName=t.TypeName+" "+b.BrandName+" "+m.ModelName }).ToList();models=modelRows.Select(x=>new ChoiceItem(x.EquipmentModelID,x.DisplayName)).ToList();}
            if(clients.Count==0||models.Count==0){MessageBox.Show("Сначала добавь клиента и модель оборудования.","Нет справочных данных",MessageBoxButton.OK,MessageBoxImage.Information);return;}
            var fields=new List<FormField>{ChoiceField("ClientID","Клиент",clients,row==null?clients[0].Id:row.ClientID),ChoiceField("EquipmentModelID","Модель",models,row==null?models[0].Id:row.EquipmentModelID),TextField("SerialNumber","Серийный номер",row==null?"":row.SerialNumber,100),TextField("Notes","Примечание",row==null?"":row.Notes,300)};
            var values=ShowForm(row==null?"Новое устройство":"Изменение устройства",fields);if(values==null)return;Require(values,"SerialNumber");
            using(var c=App.Context){Course_project_0502_Equipment e;if(row==null){e=new Course_project_0502_Equipment();c.Course_project_0502_Equipment.Add(e);}else{e=c.Course_project_0502_Equipment.Find(row.EquipmentID);if(e==null)return;}e.ClientID=Num(values,"ClientID");e.EquipmentModelID=Num(values,"EquipmentModelID");e.SerialNumber=Val(values,"SerialNumber");e.Notes=Val(values,"Notes");c.SaveChanges();}
            RefreshWithMessage(row==null?"Техника добавлена.":"Изменения техники сохранены.");
        }
        private void DeleteEquipment(){if (!EnsureAdministrator()) return; var row=Selected<EquipmentRow>();if(row==null||!Confirm("Удалить устройство «"+row.SerialNumber+"»?","Удаление техники"))return;using(var c=App.Context){var e=c.Course_project_0502_Equipment.Find(row.EquipmentID);if(e!=null){c.Course_project_0502_Equipment.Remove(e);c.SaveChanges();}}RefreshWithMessage("Техника удалена.");}

        private void SaveEmployee(EmployeeRow row)
        {
            if (!EnsureAdministrator()) return;
            List<ChoiceItem> specs;using(var c=App.Context){specs=c.Course_project_0502_Specializations.OrderBy(x=>x.SpecializationName).ToList().Select(x=>new ChoiceItem(x.SpecializationID,x.SpecializationName)).ToList();}specs.Insert(0,new ChoiceItem(0,"Не указана"));
            int selectedSpec=0;if(row!=null&&!string.IsNullOrWhiteSpace(row.Specialization)){var item=specs.FirstOrDefault(x=>x.Name==row.Specialization);if(item!=null)selectedSpec=item.Id;}
            var fields=new List<FormField>{TextField("FullName","ФИО",row==null?"":row.FullName,150),TextField("Phone","Телефон",row==null?"":row.Phone,20),ChoiceField("SpecializationID","Специализация",specs,selectedSpec),CheckField("IsActive","Активен",row==null||row.IsActive)};
            var values=ShowForm(row==null?"Новый исполнитель":"Изменение исполнителя",fields);if(values==null)return;Require(values,"FullName","Phone");
            using(var c=App.Context){Course_project_0502_Employees e;if(row==null){e=new Course_project_0502_Employees();c.Course_project_0502_Employees.Add(e);}else{e=c.Course_project_0502_Employees.Include("Course_project_0502_Specializations").FirstOrDefault(x=>x.EmployeeID==row.EmployeeID);if(e==null)return;}e.FullName=Val(values,"FullName");e.Phone=Val(values,"Phone");e.IsActive=Flag(values,"IsActive");e.Course_project_0502_Specializations.Clear();int id=Num(values,"SpecializationID");if(id>0){var sp=c.Course_project_0502_Specializations.Find(id);if(sp!=null)e.Course_project_0502_Specializations.Add(sp);}c.SaveChanges();}
            RefreshWithMessage(row==null?"Исполнитель добавлен.":"Изменения исполнителя сохранены.");
        }
        private void DeleteEmployee(){if (!EnsureAdministrator()) return; var row=Selected<EmployeeRow>();if(row==null||!Confirm("Удалить исполнителя «"+row.FullName+"»?","Удаление исполнителя"))return;using(var c=App.Context){var e=c.Course_project_0502_Employees.Find(row.EmployeeID);if(e!=null){c.Course_project_0502_Employees.Remove(e);c.SaveChanges();}}RefreshWithMessage("Исполнитель удалён.");}

        private void SaveOrder(OrderRow row)
        {
            if (!EnsureAdministrator()) return;
            List<ChoiceItem> equipment;List<ChoiceItem> statuses;List<ChoiceItem> employees;int currentEmployee=0;
            using(var c=App.Context){var equipmentRows=(from e in c.Course_project_0502_Equipment join cl in c.Course_project_0502_Clients on e.ClientID equals cl.ClientID join m in c.Course_project_0502_EquipmentModels on e.EquipmentModelID equals m.EquipmentModelID join b in c.Course_project_0502_EquipmentBrands on m.EquipmentBrandID equals b.EquipmentBrandID join t in c.Course_project_0502_EquipmentTypes on m.EquipmentTypeID equals t.EquipmentTypeID orderby cl.FullName select new { e.EquipmentID, DisplayName=cl.FullName+" — "+t.TypeName+" "+b.BrandName+" "+m.ModelName+" ("+e.SerialNumber+")" }).ToList();equipment=equipmentRows.Select(x=>new ChoiceItem(x.EquipmentID,x.DisplayName)).ToList();statuses=c.Course_project_0502_OrderStatuses.OrderBy(x=>x.OrderStatusID).ToList().Select(x=>new ChoiceItem(x.OrderStatusID,x.StatusName)).ToList();employees=c.Course_project_0502_Employees.Where(x=>x.IsActive).OrderBy(x=>x.FullName).ToList().Select(x=>new ChoiceItem(x.EmployeeID,x.FullName)).ToList();if(row!=null){var a=c.Course_project_0502_OrderAssignments.FirstOrDefault(x=>x.RepairOrderID==row.RepairOrderID);currentEmployee=a==null?0:a.EmployeeID;}}
            if(equipment.Count==0||statuses.Count==0){MessageBox.Show("Для создания заказа требуются устройство и статусы.","Нет данных",MessageBoxButton.OK,MessageBoxImage.Information);return;}employees.Insert(0,new ChoiceItem(0,"Не назначен"));
            var fields=new List<FormField>{ChoiceField("EquipmentID","Устройство",equipment,row==null?equipment[0].Id:row.EquipmentID),DateField("AcceptedAt","Дата приёма",row==null?(DateTime?)DateTime.Today:row.AcceptedAt),DateField("IssuedAt","Дата выдачи",row==null?null:row.IssuedAt),TextField("FaultDescription","Неисправность",row==null?"":row.FaultDescription,1000,true),TextField("RepairCost","Стоимость ремонта",row==null?"0.00":row.RepairCost.ToString("0.00",CultureInfo.InvariantCulture),20),ChoiceField("OrderStatusID","Статус",statuses,row==null?statuses[0].Id:row.OrderStatusID),ChoiceField("EmployeeID","Исполнитель",employees,currentEmployee)};
            var values=ShowForm(row==null?"Новый заказ":"Изменение заказа "+row.OrderNumber,fields);if(values==null)return;Require(values,"AcceptedAt","FaultDescription","RepairCost");decimal cost;if(!decimal.TryParse(Val(values,"RepairCost"),NumberStyles.Number,CultureInfo.CurrentCulture,out cost)&&!decimal.TryParse(Val(values,"RepairCost"),NumberStyles.Number,CultureInfo.InvariantCulture,out cost))throw new InvalidOperationException("Введи корректную стоимость.");if(cost<0)throw new InvalidOperationException("Стоимость не может быть отрицательной.");DateTime accepted=Convert.ToDateTime(values["AcceptedAt"]).Date;DateTime? issued=values["IssuedAt"]==null?(DateTime?)null:Convert.ToDateTime(values["IssuedAt"]).Date;if(issued.HasValue&&issued.Value<accepted)throw new InvalidOperationException("Дата выдачи не может быть раньше даты приёма.");
            using(var c=App.Context){using(var tx=c.Database.BeginTransaction()){Course_project_0502_RepairOrders e;bool isNew=row==null;int priorStatus=0;if(isNew){e=new Course_project_0502_RepairOrders{OrderNumber="RC-"+DateTime.Now.ToString("yyMMddHHmmssfff"),CreatedAt=DateTime.Now,IsCancelled=false};c.Course_project_0502_RepairOrders.Add(e);}else{e=c.Course_project_0502_RepairOrders.Find(row.RepairOrderID);if(e==null)return;priorStatus=e.OrderStatusID;}e.EquipmentID=Num(values,"EquipmentID");e.AcceptedAt=accepted;e.IssuedAt=issued;e.FaultDescription=Val(values,"FaultDescription");e.RepairCost=cost;e.OrderStatusID=Num(values,"OrderStatusID");var st=c.Course_project_0502_OrderStatuses.Find(e.OrderStatusID);if(st==null)throw new InvalidOperationException("Выбранный статус отсутствует в справочнике.");if(st.IsFinal&&!issued.HasValue)throw new InvalidOperationException("Для финального статуса требуется дата выдачи.");c.SaveChanges();if(isNew||priorStatus!=e.OrderStatusID)c.Course_project_0502_RepairOrderStatusHistory.Add(new Course_project_0502_RepairOrderStatusHistory{RepairOrderID=e.RepairOrderID,OrderStatusID=e.OrderStatusID,ChangedAt=DateTime.Now,ChangedByUserID=null,ChangeNote=isNew?"Заказ создан сотрудником":"Статус изменён сотрудником"});var assigned=c.Course_project_0502_OrderAssignments.Where(x=>x.RepairOrderID==e.RepairOrderID).ToList();c.Course_project_0502_OrderAssignments.RemoveRange(assigned);c.SaveChanges();int employeeId=Num(values,"EmployeeID");if(employeeId>0)c.Course_project_0502_OrderAssignments.Add(new Course_project_0502_OrderAssignments{RepairOrderID=e.RepairOrderID,EmployeeID=employeeId,AssignedAt=DateTime.Now,AssignmentNote="Ответственный исполнитель"});c.SaveChanges();tx.Commit();}}
            RefreshWithMessage(row==null?"Заказ создан.":"Изменения заказа сохранены.");
        }

        private void CancelOrder(){if (!EnsureAdministrator()) return; var row=Selected<OrderRow>();if(row==null||!Confirm("Отменить заказ "+row.OrderNumber+"? Запись останется в истории.","Отмена заказа"))return;using(var c=App.Context){var e=c.Course_project_0502_RepairOrders.Find(row.RepairOrderID);if(e==null)return;e.IsCancelled=true;e.CancellationNote="Отменён сотрудником RepairCenter";c.Course_project_0502_RepairOrderStatusHistory.Add(new Course_project_0502_RepairOrderStatusHistory{RepairOrderID=e.RepairOrderID,OrderStatusID=e.OrderStatusID,ChangedAt=DateTime.Now,ChangedByUserID=null,ChangeNote="Заказ отменён"});c.SaveChanges();}RefreshWithMessage("Заказ отменён. История сохранена.");}
        private void DeleteClientOrOtherNotAllowed(){MessageBox.Show("Запись связана с другими данными. Сначала проверь связанные заказы и устройства.","Удаление запрещено",MessageBoxButton.OK,MessageBoxImage.Warning);}
        private T Selected<T>() where T:class {var x=_grid==null?null:_grid.SelectedItem as T;if(x==null)MessageBox.Show("Сначала выбери строку в таблице.","Нет выбора",MessageBoxButton.OK,MessageBoxImage.Information);return x;}
        private bool Confirm(string message,string title){return MessageBox.Show(message,title,MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes;}
        private string Val(Dictionary<string,object> d,string k){return d.ContainsKey(k)?Convert.ToString(d[k])??string.Empty:string.Empty;}
        private int Num(Dictionary<string,object>d,string k){return Convert.ToInt32(d[k]);}
        private bool Flag(Dictionary<string,object>d,string k){return Convert.ToBoolean(d[k]);}
        private void Require(Dictionary<string,object>d,params string[] keys){foreach(var k in keys)if(!d.ContainsKey(k)||d[k]==null||string.IsNullOrWhiteSpace(Convert.ToString(d[k])))throw new InvalidOperationException("Обязательное поле не заполнено: "+k);}
        private bool Contains(string a,string b){return (a??string.Empty).IndexOf(b??string.Empty,StringComparison.OrdinalIgnoreCase)>=0;}
        private void RefreshWithMessage(string message){if(LoadPage())_summary.Text=message;}
        private void ShowError(string message,Exception ex){var b=new StringBuilder(message);for(var x=ex;x!=null;x=x.InnerException)b.AppendLine().Append(x.GetType().Name).Append(": ").Append(x.Message);MessageBox.Show(b.ToString(),"RepairCenter",MessageBoxButton.OK,MessageBoxImage.Error);}

        private bool EnsureAdministrator()
        {
            if (IsAdministrator) return true;
            MessageBox.Show("Изменение данных доступно только администратору.", "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (!Confirm("Выйти из текущей учётной записи?", "Выход")) return;
            UserSession.End();
            Hide();
            var login = new LoginWindow();
            var result = login.ShowDialog();
            if (result == true && UserSession.Current != null)
            {
                var next = new MainWindow();
                Application.Current.MainWindow = next;
                next.Show();
                Close();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void DeleteById<T>(Func<user24_dbEntities1,T> find, Action<user24_dbEntities1,T> remove, int id) where T:class { using(var c=App.Context){var e=find(c);if(e==null)return;remove(c,e);c.SaveChanges();} }
        private FormField TextField(string key,string label,string value,int max,bool multiline=false){return new FormField{Key=key,Label=label,Type=multiline?FieldType.Multiline:FieldType.Text,Value=value,MaxLength=max};}
        private FormField DateField(string key,string label,DateTime? value){return new FormField{Key=key,Label=label,Type=FieldType.Date,Value=value};}
        private FormField ChoiceField(string key,string label,List<ChoiceItem> items,int value){return new FormField{Key=key,Label=label,Type=FieldType.Choice,Choices=items,Value=value};}
        private FormField CheckField(string key,string label,bool value){return new FormField{Key=key,Label=label,Type=FieldType.Check,Value=value};}
        private Dictionary<string,object> ShowForm(string title,List<FormField> fields)
        {
            var result=new Dictionary<string,object>();var win=new Window{Title=title,Width=540,MinWidth=450,SizeToContent=SizeToContent.Height,MaxHeight=720,WindowStartupLocation=WindowStartupLocation.CenterOwner,Owner=this,ResizeMode=ResizeMode.CanResize,Background=Brush("#F3F5F9"),FontFamily=new System.Windows.Media.FontFamily("Segoe UI")};var outer=new DockPanel{Margin=new Thickness(18)};var actions=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,14,0,0)};var save=new Button{Content="Сохранить",Width=110,Height=36,Background=Brush("#2864D7"),Foreground=Brushes.White,BorderThickness=new Thickness(0),Margin=new Thickness(0,0,8,0),IsDefault=true};var cancel=new Button{Content="Отмена",Width=90,Height=36,IsCancel=true};actions.Children.Add(save);actions.Children.Add(cancel);DockPanel.SetDock(actions,Dock.Bottom);outer.Children.Add(actions);var stack=new StackPanel();var controls=new Dictionary<string,Control>();foreach(var f in fields){var block=new StackPanel{Margin=new Thickness(0,0,0,10)};block.Children.Add(new TextBlock{Text=f.Label,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,4),Foreground=Brush("#344054")});Control control;if(f.Type==FieldType.Choice){var combo=new ComboBox{Height=34,ItemsSource=f.Choices,DisplayMemberPath="Name",SelectedValuePath="Id",SelectedValue=f.Value,Padding=new Thickness(7,4,7,4)};if(combo.SelectedItem==null&&f.Choices.Count>0)combo.SelectedIndex=0;control=combo;}else if(f.Type==FieldType.Date)control=new DatePicker{Height=34,SelectedDate=f.Value==null?(DateTime?)null:Convert.ToDateTime(f.Value)};else if(f.Type==FieldType.Check)control=new CheckBox{IsChecked=Convert.ToBoolean(f.Value),Content="Активен",Height=30};else control=new TextBox{Text=Convert.ToString(f.Value)??string.Empty,MaxLength=f.MaxLength,Height=f.Type==FieldType.Multiline?76:34,Padding=new Thickness(8,5,8,5),AcceptsReturn=f.Type==FieldType.Multiline,TextWrapping=f.Type==FieldType.Multiline?TextWrapping.Wrap:TextWrapping.NoWrap,VerticalContentAlignment=f.Type==FieldType.Multiline?VerticalAlignment.Top:VerticalAlignment.Center};controls[f.Key]=control;block.Children.Add(control);stack.Children.Add(block);}outer.Children.Add(new ScrollViewer{Content=stack,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=575});win.Content=outer;bool accepted=false;save.Click+=delegate{var candidate=new Dictionary<string,object>();foreach(var f in fields){var c=controls[f.Key];if(f.Type==FieldType.Choice)candidate[f.Key]=((ComboBox)c).SelectedValue;else if(f.Type==FieldType.Date)candidate[f.Key]=((DatePicker)c).SelectedDate;else if(f.Type==FieldType.Check)candidate[f.Key]=((CheckBox)c).IsChecked==true;else candidate[f.Key]=((TextBox)c).Text.Trim();}try{Require(candidate,fields.Where(x=>x.Key!="IssuedAt"&&x.Key!="Notes"&&x.Key!="EmployeeID"&&x.Key!="SpecializationID").Select(x=>x.Key).ToArray());foreach(var f in fields.Where(x=>x.Type==FieldType.Text||x.Type==FieldType.Multiline)){string v=Val(candidate,f.Key);if(v.Length>f.MaxLength)throw new InvalidOperationException("Слишком длинное значение: "+f.Label);}if(candidate.ContainsKey("Phone")){string phone=Val(candidate,"Phone");if(phone.Length<7)throw new InvalidOperationException("Телефон должен содержать не менее 7 символов.");}if(candidate.ContainsKey("AcceptedAt")&&candidate.ContainsKey("IssuedAt")&&candidate["AcceptedAt"]!=null&&candidate["IssuedAt"]!=null&&Convert.ToDateTime(candidate["IssuedAt"]).Date<Convert.ToDateTime(candidate["AcceptedAt"]).Date)throw new InvalidOperationException("Дата выдачи не может быть раньше даты приёма.");result.Clear();foreach(var pair in candidate)result[pair.Key]=pair.Value;accepted=true;win.DialogResult=true;}catch(Exception ex){MessageBox.Show(ex.Message,"Проверка данных",MessageBoxButton.OK,MessageBoxImage.Warning);}};if(win.ShowDialog()!=true||!accepted)return null;return result;
        }

        public sealed class OrderRow { public int RepairOrderID{get;set;} public string OrderNumber{get;set;} public int EquipmentID{get;set;} public int OrderStatusID{get;set;} public string ClientName{get;set;} public string Equipment{get;set;} public string SerialNumber{get;set;} public DateTime AcceptedAt{get;set;} public DateTime? IssuedAt{get;set;} public string FaultDescription{get;set;} public decimal RepairCost{get;set;} public string OrderStatus{get;set;} public string AssignedEmployees{get;set;} }
        public sealed class ClientRow { public int ClientID{get;set;} public string FullName{get;set;} public string Phone{get;set;} public string Address{get;set;} }
        public sealed class EquipmentRow { public int EquipmentID{get;set;} public int ClientID{get;set;} public int EquipmentModelID{get;set;} public string ClientName{get;set;} public string EquipmentType{get;set;} public string Brand{get;set;} public string Model{get;set;} public string SerialNumber{get;set;} public string Notes{get;set;} }
        public sealed class EmployeeRow { public int EmployeeID{get;set;} public string FullName{get;set;} public string Phone{get;set;} public bool IsActive{get;set;} public string Specialization{get;set;} }
        public sealed class ReportRow { public string OrderNumber{get;set;} public string ClientName{get;set;} public string Equipment{get;set;} public DateTime IssuedAt{get;set;} public string StatusName{get;set;} public decimal RepairCost{get;set;} }
        private enum FieldType { Text, Multiline, Date, Choice, Check }
        private sealed class FormField { public string Key{get;set;} public string Label{get;set;} public FieldType Type{get;set;} public object Value{get;set;} public List<ChoiceItem> Choices{get;set;} public int MaxLength{get;set;} }
        private sealed class ChoiceItem { public int Id{get;set;} public string Name{get;set;} public ChoiceItem(int id,string name){Id=id;Name=name;} public override string ToString(){return Name;} }
    }
}
