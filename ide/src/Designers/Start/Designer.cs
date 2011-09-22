﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MOAI.Management;
using MOAI.Tools;

namespace MOAI.Designers.Start
{
    public partial class Designer : MOAI.Designers.Designer
    {
        /// <summary>
        /// Creates a new start page.
        /// </summary>
        /// <param name="manager">The main MOAI manager.</param>
        /// <param name="file">The associated file.</param>
        public Designer(MOAI.Manager manager, File file)
            : base(manager, file)
        {
            InitializeComponent();

            // Listen for events.
            this.Manager.IDEWindow.ActiveTabChanged += (sender, e) =>
            {
                this.OnTabChanged();
            };
            this.Manager.IDEWindow.ResizeEnd += (sender, e) =>
            {
                this.OnResize();
            };

            this.TabText = "Cloud Dashboard";
            this.c_WebBrowser.Url = new System.Uri("http://dashboard.moaicloud.com/login.php", System.UriKind.Absolute);
        }

        /// <summary>
        /// This function is called after the IDE has finished resizing itself, or the
        /// total size of this dock content has otherwise changed.
        /// </summary>
        private void OnResize()
        {
        }

        /// <summary>
        /// This function is called after the active tab has changed.
        /// </summary>
        private void OnTabChanged()
        {
        }

        /// <summary>
        /// This event is raised when the user has navigated within the web browser.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The navigation event information.</param>
        private void c_WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Uri uri = e.Url;

            if (uri.ToString().StartsWith("about:"))
            {
                e.Cancel = true;
                Dictionary<String, String> query = this.GetDictionaryFromQuery(uri.Query);

                switch (uri.LocalPath)
                {
                    case "solution":
                        switch (query["mode"])
                        {
                            case "new":
                                // Call the "New Solution" menu option.
                                new Menus.Definitions.Solution.New().OnActivate();
                                break;
                            case "open":
                                // Call the "Open Solution" menu option.
                                new Menus.Definitions.Solution.Open().OnActivate();
                                break;
                        }
                        break;
                    case "tutorial":
                        MessageBox.Show(
                            "The selected tutorial is currently unavailable.",
                            "Tutorial Unavailable",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        break;
                }
            }
        }

        private Dictionary<String, String> GetDictionaryFromQuery(String query)
        {
            if (query.StartsWith("?")) query = query.Substring(1);
            String[] keypairs = query.Split('&');
            Dictionary<String, String> result = new Dictionary<String, String>();
            foreach (String keypair in keypairs)
            {
                String[] keyvalue = keypair.Split('=');
                if (keyvalue.Length == 2)
                {
                    result.Add(keyvalue[0], keyvalue[1]);
                }
            }
            return result;
        }
    }
}
