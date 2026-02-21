using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient; // your generated gRPC namespace
using System;
using System.Windows;

namespace GrpcClient
{
    public partial class MainWindow : Window
    {
        private const string Address = "https://localhost:7140";

        private readonly GrpcChannel channel;
        private readonly Service.ServiceClient client;

        private string sessionId = "";
        private string jwttoken = "";
        private bool loggedIn = false;

        public MainWindow()
        {
            InitializeComponent();

            channel = GrpcChannel.ForAddress(Address);
            client = new Service.ServiceClient(channel);

            BlockVisible(false);
        }

        private void BlockVisible(bool isVisible)
        {
            create_button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            update_button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            delete_button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            id_txt.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            district_txt.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            address_txt.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            waste_box.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private Metadata GetAuthHeaders()
        {
            var headers = new Metadata();
            if (!string.IsNullOrEmpty(jwttoken))
            {
                headers.Add("Authorization", $"Bearer {jwttoken}");
            }
            return headers;
        }

        // LOGIN
        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            if (loggedIn)
            {
                login_label.Content = "Already logged in";
                return;
            }

            var user = new User
            {
                Name = username_txt.Text,
                Password = password_txt.Text
            };

            SessionId temp;
            try
            {
                temp = client.Login(user);
            }
            catch (RpcException ex)
            {
                login_label.Content = $"Login error: {ex.Status.Detail}";
                return;
            }

            if (string.IsNullOrEmpty(temp.Id))
            {
                login_label.Content = "Login failed";
                sessionId = null;
                return;
            }

            sessionId = temp.Id;
            jwttoken = temp.Jwtoken;
            login_label.Content = "Logged in :3";
            loggedIn = true;
            BlockVisible(true);
        }

        // LOGOUT
        private void logout_button_Click(object sender, RoutedEventArgs e)
        {
            if (!loggedIn)
            {
                login_label.Content = "Not logged in";
                return;
            }

            var id = new SessionId { Id = sessionId };
            var headers = GetAuthHeaders();

            var result = client.Logout(id, headers);
            login_label.Content = result.Success;
            loggedIn = false;
            BlockVisible(false);
        }

        // CREATE
        private void create_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var req = new Yard
                {
                    Sessionid = sessionId,
                    Id = int.Parse(id_txt.Text),
                    District = district_txt.Text,
                    Address = address_txt.Text,
                    Waste = (waste_box.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? waste_box.Text
                };

                var headers = GetAuthHeaders();
                var result = client.Create(req, headers);
                status_label.Content = result.Success;
            }
            catch
            {
                status_label.Content = "Except, it doesn't work :c";
            }
        }

        // READ
        private void read_button_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();
            var yard = client.Read(new Empty());

            foreach (var i in yard.Yards)
            {
                listBox1.Items.Add($"{i.Id} | {i.District} | {i.Address}");
            }
        }

        // UPDATE
        private void update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var req = new Yard
                {
                    Sessionid = sessionId,
                    Id = int.Parse(id_txt.Text),
                    District = district_txt.Text,
                    Address = address_txt.Text,
                    Waste = (waste_box.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? waste_box.Text
                };

                var headers = GetAuthHeaders();
                var result = client.Update(req, headers);
                status_label.Content = result.Success;
            }
            catch
            {
                status_label.Content = "Except, it doesn't work :c";
            }
        }

        // DELETE
        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var req = new Yard
                {
                    Sessionid = sessionId,
                    Id = int.Parse(id_txt.Text),
                    District = district_txt.Text,
                    Address = address_txt.Text,
                    Waste = (waste_box.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? waste_box.Text
                };

                var headers = GetAuthHeaders();
                var result = client.Delete(req, headers);
                status_label.Content = result.Success;
            }
            catch
            {
                status_label.Content = "Except, it doesn't work :c";
            }
        }

        // QUERY 1
        private void query1_button_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();

            var result = client.BudaPestCount(new Empty());

            listBox1.Items.Add("Number of junkyards:");
            listBox1.Items.Add($"Buda: {result.BudaCount}");
            listBox1.Items.Add($"Pest: {result.PestCount}");
        }

        // QUERY 2
        private void query2_button_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();

            var yard = client.SeveralYards(new Empty());
            listBox1.Items.Add("Several junkyards:");

            foreach (var i in yard.Yards)
            {
                listBox1.Items.Add($"{i.District}");
            }
        }

        // QUERY 3
        private void query3_button_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();

            try
            {
                var req = new Count
                {
                    Count_ = int.Parse(query3_txt.Text)
                };

                var yard = client.WasteType(req);
                foreach (var i in yard.Yards)
                {
                    listBox1.Items.Add($"{i.District}");
                }
            }
            catch
            {
                status_label.Content = "Input number! :c";
            }
        }
    }
}
