using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebSockets;
using Flogging.Core;
using Flogging.Web;

namespace SandBox
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var myUser = new UserInfo();

            var detail = new FlogDetail() { Message = "Test NULL"};

            Flogger.WritePerf(detail);

            Session["User"] = myUser;


        }
    }
}