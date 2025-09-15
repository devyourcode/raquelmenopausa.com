using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;

namespace Yourcode.CMS.Controllers
{
    public class RobotController : CustomController
    {
    }
}

//        public partial class _Default : System.Web.UI.Page
//        {
//            private object txtRobots;

//            protected void Page_Load(object sender, EventArgs e)
//            {
//                if (!Page.IsPostBack)
//                {
//                    //Path do arquivo robots.txt
//                    //C:\inetpub\wwwroot\nomedosite\robots.txt
//                    string strFile = System.Web.HttpContext.Current.Server.MapPath("~/robots.txt");

//                    //Se arquivo não existir
//                    if (!File.Exists(strFile))
//                    {
//                        //Criar o arquivo,
//                        //Estou usando o using para fazer o Dispose automático do arquivo após criá-lo.
//                        using (FileStream fs = File.Create(strFile)) { }
//                    }
//                    //Abrindo o arquivo
//                    using (StreamReader sr = File.OpenText(strFile))
//                    {
//                        //Ler o conteúdo do txt e preencher o TextBox
//                        txtRobots.Text = sr.ReadToEnd();
//                    }
//                }
//            }

//            protected void btnSave_Click(object sender, EventArgs e)
//            {
//                //Path do arquivo robots.txt
//                //C:\inetpub\wwwroot\nomedosite\robots.txt
//                string strFile = System.Web.HttpContext.Current.Server.MapPath("~/robots.txt");

//                //Escreve no robots.txt
//                File.WriteAllText(strFile, txtRobots.Text);
//            }
//        }
//    }
//}
