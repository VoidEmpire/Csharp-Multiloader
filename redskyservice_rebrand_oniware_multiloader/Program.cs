using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using redskyservice_multiloader;

static class Program
{
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (var form2 = new Form2())
        {
            form2.ShowDialog();
        }

        Application.Run(new Form1());
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        string dllName = new AssemblyName(args.Name).Name + ".dll";
        string resourceName = Array.Find(Assembly.GetExecutingAssembly().GetManifestResourceNames(), element => element.EndsWith(dllName));

        if (resourceName == null) return null;

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null) return null;

            byte[] assemblyData = new byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            return Assembly.Load(assemblyData);
        }
    }
}
