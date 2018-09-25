using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ninject;
using Proxy.Ninject;

namespace Proxy
{
    static class Program
    {



        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            IKernel formDIKernel = new StandardKernel(new FormLoadDIConfig("parameters.dat", "client_secret.json"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(formDIKernel.Get<Form1>());
        }
    }
}
