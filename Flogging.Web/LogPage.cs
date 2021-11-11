﻿using Flogging.Core;
using System;
using System.Web;

namespace Flogging.Web
{
    public abstract class LogPage : System.Web.UI.Page
    {
        protected PerfTracker Tracker;

        protected override void OnLoad(EventArgs e)
        {
            var name = Page.Request.Path + (IsPostBack ? "_POSTBACK" : "");

            string userId, userName, location;
            var data = Helpers.GetWebFloggingData(out userId, out userName, out location);

            var sessionId = HttpContext.Current.Session.SessionID;

            Tracker = new PerfTracker(name, userId, userName, location, "ToDos", "WebForms", sessionId, data);
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            if (Tracker != null)
                Tracker.Stop();
        }
    }
}
