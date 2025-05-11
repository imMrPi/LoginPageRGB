using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LoginPageRGB
{
    public partial class Login : Window
    {
        private string Connection = "Server=localhost;DataBase=university;Uid=root;Pwd=1111;";
        private DispatcherTimer timer;
        private double hueAngle = 0;
        private BitmapImage bitmap;
        private Color initialGradientColor1;
        private Color initialGradientColor2;
        private LinearGradientBrush normalBrush;
        private LinearGradientBrush hoverBrush;

        public Login()
        {
            InitializeComponent();

            initialGradientColor1 = (Color)ColorConverter.ConvertFromString("#FF23F4FC");
            initialGradientColor2 = (Color)ColorConverter.ConvertFromString("#FFF529D7");

            string relativePath = @"../../Images/bg.jpg";
            string filePath = Path.GetFullPath(relativePath);
            StartHueRotation(filePath);
            Intialize();


        }
        public void Intialize()
        {
        
            normalBrush = new LinearGradientBrush();
            normalBrush.StartPoint = new Point(0, 0);
            normalBrush.EndPoint = new Point(1, 1);
            normalBrush.GradientStops.Add(new GradientStop(initialGradientColor1, 0));
            normalBrush.GradientStops.Add(new GradientStop(initialGradientColor2, 1));

            hoverBrush = new LinearGradientBrush();
            hoverBrush.StartPoint = new Point(0, 0);
            hoverBrush.EndPoint = new Point(1, 1);
            hoverBrush.GradientStops.Add(new GradientStop(initialGradientColor2, 0));
            hoverBrush.GradientStops.Add(new GradientStop(initialGradientColor1, 1));

           
            LoginBtn.Background = normalBrush;
            SignUpBtn.Background = normalBrush;
            SendCodeBtn.Background = normalBrush;
        }
        private void LoginBtn_MouseEnter(object sender, MouseEventArgs e)
        {
           
            LoginBtn.Background = hoverBrush;
        }

        private void LoginBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            
            LoginBtn.Background = normalBrush;
        }

        private void StartHueRotation(string imagePath)
        {
            bitmap = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += (s, e) => ApplyHueRotation();
            timer.Start();
        }
        
        private void ApplyHueRotation()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);

            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixels = new int[width * height];
            writeableBitmap.CopyPixels(pixels, width * 4, 0);

            hueAngle = (hueAngle + 1) % 360;
            double radians = hueAngle * Math.PI / 180;

            Parallel.For(0, pixels.Length, i =>
            {
                Color originalColor = Color.FromArgb(
                    (byte)(pixels[i] >> 24),
                    (byte)(pixels[i] >> 16),
                    (byte)(pixels[i] >> 8),
                    (byte)pixels[i]);

                if (originalColor.A > 0)
                {
                    Color newColor = RotateHue(originalColor, radians);
                    pixels[i] = (newColor.A << 24) | (newColor.R << 16) | (newColor.G << 8) | newColor.B;
                }
            });

            writeableBitmap.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels, width * 4, 0);

            BackgroundImage.ImageSource = writeableBitmap;

            Color newBackgroundColor1 = RotateHue(initialGradientColor1, radians);
            Color newBackgroundColor2 = RotateHue(initialGradientColor2, radians);

            normalBrush.GradientStops[0].Color = newBackgroundColor1;
            normalBrush.GradientStops[1].Color = newBackgroundColor2;

            hoverBrush.GradientStops[0].Color = newBackgroundColor2;
            hoverBrush.GradientStops[1].Color = newBackgroundColor1;
        }

        private Color RotateHue(Color originalColor, double radians)
        {
            double hue, saturation, value;
            RgbToHsv(originalColor.R, originalColor.G, originalColor.B, out hue, out saturation, out value);

            hue = (hue + radians * 180 / Math.PI) % 360;

            byte r, g, b;
            HsvToRgb(hue, saturation, value, out r, out g, out b);

            return Color.FromArgb(originalColor.A, r, g, b);
        }

        private void RgbToHsv(byte r, byte g, byte b, out double hue, out double saturation, out double value)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));

            double h = 0;
            double s;
            double v = max;

            double delta = max - min;

            if (max != 0)
            {
                s = delta / max;
            }
            else
            {
                s = 0;
                h = -1;
                hue = h;
                saturation = s;
                value = v;
                return;
            }

            if (rd == max)
            {
                h = (gd - bd) / delta;
            }
            else if (gd == max)
            {
                h = 2 + (bd - rd) / delta;
            }
            else
            {
                h = 4 + (rd - gd) / delta;
            }

            h *= 60;
            if (h < 0)
            {
                h += 360;
            }

            hue = h;
            saturation = s;
            value = v;
        }

        private void HsvToRgb(double hue, double saturation, double value, out byte r, out byte g, out byte b)
        {
            int i;
            double f, p, q, t;

            if (saturation == 0)
            {
                r = g = b = (byte)(value * 255.0);
                return;
            }

            hue /= 60;
            i = (int)Math.Floor(hue);
            f = hue - i;
            p = value * (1 - saturation);
            q = value * (1 - saturation * f);
            t = value * (1 - saturation * (1 - f));

            switch (i)
            {
                case 0:
                    r = (byte)(value * 255.0);
                    g = (byte)(t * 255.0);
                    b = (byte)(p * 255.0);
                    break;
                case 1:
                    r = (byte)(q * 255.0);
                    g = (byte)(value * 255.0);
                    b = (byte)(p * 255.0);
                    break;
                case 2:
                    r = (byte)(p * 255.0);
                    g = (byte)(value * 255.0);
                    b = (byte)(t * 255.0);
                    break;
                case 3:
                    r = (byte)(p * 255.0);
                    g = (byte)(q * 255.0);
                    b = (byte)(value * 255.0);
                    break;
                case 4:
                    r = (byte)(t * 255.0);
                    g = (byte)(p * 255.0);
                    b = (byte)(value * 255.0);
                    break;
                default:
                    r = (byte)(value * 255.0);
                    g = (byte)(p * 255.0);
                    b = (byte)(q * 255.0);
                    break;
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {

            string username = Email.Text;
            string password = Passwords.Password;


            if (AuthenticateUser(username, password))
            {
                MessageBox.Show("Login successful!");

            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void ForgotPass_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoginBorder.Visibility = Visibility.Collapsed;
            ForgotPassword.Visibility = Visibility.Visible;
        }


        private bool AuthenticateUser(string username, string Password)
        {
            bool isAuthenticated = false;


            using (MySqlConnection DataBaseConnetion = new MySqlConnection(Connection))
            {

                try
                {
                    string Query = "Select Count(*) From Users Where UID = @username and Password = @Password";
                    DataBaseConnetion.Open();
                    using (MySqlCommand command = new MySqlCommand(Query, DataBaseConnetion))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", Password);

                        int userCount = Convert.ToInt32(command.ExecuteScalar());
                        if (userCount > 0)
                        {
                            isAuthenticated = true;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            return isAuthenticated;

        }

        private void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = NewEmail.Text;
            string password = NewPassword.Password;

            Action<string, string> signUpAction = (user, pass) =>
            {
                if (SignUp(user, pass))
                {
                    MessageBox.Show("Sign Up successful!");
                }
                else
                {
                    MessageBox.Show("Try Again");
                }
            };

            signUpAction(username, password);
        }

        private bool SignUp(string username, string password)
        {
            bool isSignUp = false;

            using (MySqlConnection databaseConnection = new MySqlConnection(Connection))
            {
                try
                {
                    string query = "INSERT INTO Users (UID, Password) VALUES (@username, @password)";
                    databaseConnection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, databaseConnection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            isSignUp = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return isSignUp;
        }



        private void SignUpInLoginPage_Click(object sender, RoutedEventArgs e)
        {
            LoginBorder.Visibility = Visibility.Collapsed;
            SignUpBorder.Visibility = Visibility.Visible;
        }

        private void SignUpBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            SignUpBtn.Background = hoverBrush;
        }

        private void SignUpBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            SignUpBtn.Background = normalBrush;
        }

        private void LoginInSignUpPage_Click(object sender, RoutedEventArgs e)
        {
            LoginBorder.Visibility = Visibility.Visible;
            SignUpBorder.Visibility = Visibility.Collapsed;
        }

        private void NumberMethod_Click(object sender, RoutedEventArgs e)
        {
            NumberPanel.Visibility = Visibility.Visible;
            EmailPanel.Visibility = Visibility.Collapsed;
            EmailMethod.IsChecked = false;
            NumberMethod.IsChecked = true;

        }

        private void EmailMethod_Click(object sender, RoutedEventArgs e)
        {
            EmailMethod.IsChecked = true;
            NumberMethod.IsChecked = false;
            NumberPanel.Visibility = Visibility.Collapsed;
            EmailPanel.Visibility = Visibility.Visible;

        }



        private void LoginInForgotPage_Click(object sender, RoutedEventArgs e)
        {
            LoginBorder.Visibility = Visibility.Visible;
            ForgotPassword.Visibility = Visibility.Collapsed;
        }

        private void SendCodeBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            SendCodeBtn.Background = hoverBrush;
        }

        private void SendCodeBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            SendCodeBtn.Background = normalBrush;
        }
        private void SendCodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
