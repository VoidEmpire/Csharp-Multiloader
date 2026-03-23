using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace redskyservice_multiloader
{
    public partial class Form2 : Form
    {
        private const string API_URL = "http://YOUR_API_IP:PORT";
        private const string REBRAND_NAME = "oniware";
        private string NEW_EXE_NAME => $"{REBRAND_NAME}_multiloader_updated.exe";
        private string OLD_EXE_NAME => $"{REBRAND_NAME}_multiloader.exe";
        private const string CURRENT_VERSION = "1.3";

        private Label lblCurrentVersion;
        private HttpClient httpClient;

        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private System.Windows.Forms.Timer timerParticles = new System.Windows.Forms.Timer();
        private Point previousFormLocation;
        private bool isFirstUpdate = true;

        private void InitializeParticles()
        {
            int numParticles = 50;
            for (int i = 0; i < numParticles; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double speed = random.Next(1, 3);
                particles.Add(new Particle()
                {
                    Position = new PointF(random.Next(0, ClientSize.Width), random.Next(0, ClientSize.Height)),
                    Velocity = new PointF((float)(Math.Cos(angle) * speed), (float)(Math.Sin(angle) * speed)),
                    Radius = random.Next(2, 5),
                    Color = Color.Red
                });
            }
        }

        private void UpdateParticles()
        {
            foreach (var particle in particles)
            {
                particle.Position = new PointF(particle.Position.X + particle.Velocity.X * 0.5f, particle.Position.Y + particle.Velocity.Y);

                if (particle.Position.X < 0 || particle.Position.X > ClientSize.Width)
                {
                    particle.Velocity = new PointF(-particle.Velocity.X, particle.Velocity.Y);
                    particle.Position = new PointF(particle.Position.X + particle.Velocity.X * 0.5f, particle.Position.Y);
                }
                if (particle.Position.Y < 0 || particle.Position.Y > ClientSize.Height)
                {
                    particle.Velocity = new PointF(particle.Velocity.X, -particle.Velocity.Y);
                    particle.Position = new PointF(particle.Position.X, particle.Position.Y + particle.Velocity.Y * 0.5f);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var particle in particles)
            {
                int transparency = (int)((1.0f - (particle.Position.Y / (float)ClientSize.Height)) * 255);
                if (transparency > 225) transparency = 255;
                if (transparency < 0) transparency = 0;

                Color particleColor = Color.FromArgb(transparency, ColorTranslator.FromHtml("#F7F9F2"));

                int reduceRadius = particle.Radius / 2;
                e.Graphics.FillEllipse(new SolidBrush(particleColor),
                    particle.Position.X - reduceRadius,
                    particle.Position.Y - reduceRadius,
                    reduceRadius * 2, reduceRadius * 2
                );
            }

            foreach (var particle in particles)
            {
                foreach (var otherParticle in particles)
                {
                    if (particle != otherParticle)
                    {
                        float dx = particle.Position.X - otherParticle.Position.X;
                        float dy = particle.Position.Y - otherParticle.Position.Y;
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (distance < 50)
                        {
                            int alpha = (int)((1.0f - (distance / 50.0f)) * 255.0f);
                            Color lineColor = Color.FromArgb(alpha, 255, 0, 255);
                            e.Graphics.DrawLine(new Pen(lineColor, 1),
                                particle.Position, otherParticle.Position);
                        }
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateParticles();
            Invalidate();
        }

        public class Particle
        {
            public PointF Position { get; set; }
            public PointF Velocity { get; set; }
            public int Radius { get; set; }
            public Color Color { get; set; }
        }

        public Form2()
        {
            InitializeComponent();
            DoubleBuffered = true;
            InitializeParticles();
            timerParticles.Interval = 1;
            timerParticles.Tick += timer1_Tick;
            timerParticles.Start();
            DoubleBuffered = true;
            this.ShowIcon = false;
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            await CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            try
            {
                var response = await httpClient.GetAsync($"{API_URL}/check_oni_update");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                var result = serializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                if (result.ContainsKey("version"))
                {
                    string latestVersion = result["version"].ToString();
                    bool updateAvailable = Version.Parse(latestVersion) > Version.Parse(CURRENT_VERSION);

                    if (updateAvailable)
                    {
                        DialogResult dialogResult = MessageBox.Show($"Update available: Version {latestVersion}\nDo you want to download and install the update now?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dialogResult == DialogResult.Yes)
                        {
                            if (result.ContainsKey("download_url"))
                            {
                                await DownloadAndInstallUpdate(result["download_url"].ToString());
                            }
                            else
                            {
                                MessageBox.Show("Download URL not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"No update required. Current version: {CURRENT_VERSION}", "No Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await Task.Delay(2000);
                        CloseWithAnimation();
                    }
                }
                else
                {
                    throw new Exception("Invalid response format from server");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Connection error: Please check your internet connection\n\nDetails: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while checking for updates: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadAndInstallUpdate(string downloadUrl)
        {
            string tempPath = Path.GetTempPath();
            string tempFilePath = Path.Combine(tempPath, NEW_EXE_NAME);

            try
            {
                using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var buffer = new byte[8192];
                    var bytesRead = 0L;
                    var stream = await response.Content.ReadAsStreamAsync();

                    while (true)
                    {
                        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                            break;

                        await fileStream.WriteAsync(buffer, 0, read);
                        bytesRead += read;

                        if (totalBytes != -1)
                        {
                            var percentage = (int)((float)bytesRead / totalBytes * 100);
                            guna2ProgressBar1.Value = percentage;
                        }
                    }
                }

                await RunUpdateBatchFile(tempFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while downloading the update: {ex.Message}", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RunUpdateBatchFile(string newExePath)
        {
            string currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string oldExePath = Path.Combine(currentDir, OLD_EXE_NAME);
            string batFilePath = Path.Combine(Path.GetTempPath(), "update_helper.bat");

            File.WriteAllText(batFilePath, $@"
@echo off
taskkill /IM ""{OLD_EXE_NAME}"" /F
ping 127.0.0.1 -n 2 > nul
if exist ""{oldExePath}"" (
    del ""{oldExePath}"" >nul
) else (
    echo ERROR: Old file not found.
    echo Current directory: %CD%
    echo Files in directory:
    dir *.exe
    pause
    exit /b 1
)
move /Y ""{newExePath}"" ""{oldExePath}""
if errorlevel 1 (
    powershell.exe -Command ""Add-Type -AssemblyName PresentationFramework; [System.Windows.MessageBox]::Show('Update failed: Could not move new file.', 'Update Error', [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Error)""
    exit /b 1
)
powershell.exe -WindowStyle Hidden -Command ""Clear-RecycleBin -Confirm:$false *>$null""
powershell.exe -WindowStyle Hidden -Command ""Add-Type -AssemblyName PresentationFramework; [System.Windows.MessageBox]::Show('Update completed successfully!', 'Update Status', [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Information)""
start """" ""{oldExePath}""
del ""%~f0""
");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{batFilePath}\"",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while starting the batch file: {ex.Message}", "Batch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseWithAnimation()
        {
            Timer fadeTimer = new Timer();
            fadeTimer.Interval = 50;
            fadeTimer.Tick += (s, e) =>
            {
                if (this.Opacity > 0)
                {
                    this.Opacity -= 0.05;
                }
                else
                {
                    fadeTimer.Stop();
                    this.Close();
                }
            };
            fadeTimer.Start();
        }

        private void guna2ProgressBar1_ValueChanged(object sender, EventArgs e)
        {
            // This method can be used to update UI based on progress if needed
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            httpClient?.Dispose();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // This method can be used to handle label2 click events if needed
        }
    }
}

