﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Moai.Platform.UI;
using Moai.Platform.Windows.UI;
using Moai.Platform.Windows.Management;
using Moai.Platform.Designers;
using Moai.Platform.Management;

namespace Moai.Platform.Windows
{
    public class WindowsUI : IUIProvider
    {
        public IImageList CreateImageList()
        {
            return new WindowsImageList();
        }

        public void ShowMessage(string message, string title, Moai.Platform.UI.MessageBoxButtons buttons, Moai.Platform.UI.MessageBoxIcon icon)
        {
            System.Windows.Forms.MessageBoxButtons nativeButtons;
            System.Windows.Forms.MessageBoxIcon nativeIcon;
            switch (buttons)
            {
                case Moai.Platform.UI.MessageBoxButtons.OK:
                    nativeButtons = System.Windows.Forms.MessageBoxButtons.OK;
                    break;
                case Moai.Platform.UI.MessageBoxButtons.OKCancel:
                    nativeButtons = System.Windows.Forms.MessageBoxButtons.OKCancel;
                    break;
                case Moai.Platform.UI.MessageBoxButtons.YesNo:
                    nativeButtons = System.Windows.Forms.MessageBoxButtons.YesNo;
                    break;
                default:
                    throw new NotSupportedException();
            }
            switch (icon)
            {
                case Moai.Platform.UI.MessageBoxIcon.None:
                    nativeIcon = System.Windows.Forms.MessageBoxIcon.None;
                    break;
                case Moai.Platform.UI.MessageBoxIcon.Information:
                    nativeIcon = System.Windows.Forms.MessageBoxIcon.Information;
                    break;
                case Moai.Platform.UI.MessageBoxIcon.Warning:
                    nativeIcon = System.Windows.Forms.MessageBoxIcon.Warning;
                    break;
                case Moai.Platform.UI.MessageBoxIcon.Error:
                    nativeIcon = System.Windows.Forms.MessageBoxIcon.Error;
                    break;
                default:
                    throw new NotSupportedException();
            }

            MessageBox.Show(message, title, nativeButtons, nativeIcon);
        }

        public void ShowMessage(string message, string title, Moai.Platform.UI.MessageBoxButtons buttons)
        {
            this.ShowMessage(message, title, buttons, Moai.Platform.UI.MessageBoxIcon.None);
        }

        public void ShowMessage(string message, string title)
        {
            this.ShowMessage(message, title, Moai.Platform.UI.MessageBoxButtons.OK, Moai.Platform.UI.MessageBoxIcon.None);
        }

        public void ShowMessage(string message)
        {
            this.ShowMessage(message, "", Moai.Platform.UI.MessageBoxButtons.OK, Moai.Platform.UI.MessageBoxIcon.None);
        }

        public string PickExistingFile(PickingData data)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = data.Filter;
            ofd.CheckFileExists = data.CheckFileExists;
            ofd.CheckPathExists = data.CheckPathExists;
            ofd.RestoreDirectory = data.RestoreDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
                return ofd.FileName;
            else
                return null;
        }

        public Moai.Platform.Templates.Solutions.SolutionCreationData PickNewSolution()
        {
            NewSolutionForm nsf = new NewSolutionForm();
            if (nsf.ShowDialog() == DialogResult.OK)
                return nsf.Result;
            else
                return null;
        }

        public Moai.Platform.Templates.Files.FileCreationData PickNewFile()
        {
            return this.PickNewFile(null);
        }

        public Moai.Platform.Templates.Files.FileCreationData PickNewFile(string preselected)
        {
            NewFileForm nff = new NewFileForm(preselected);
            if (nff.ShowDialog() == DialogResult.OK)
                return nff.Result;
            else
                return null;
        }
    }
}
