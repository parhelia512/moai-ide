﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MOAI.Designers;

namespace MOAI.Menus.Definitions.Actions
{
    class Open : Action
    {
        public Open() : base() { }
        public Open(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_open;
            this.Text = "Open File";
            this.Enabled = true;
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Management.File)
                Program.Manager.DesignersManager.OpenDesigner(this.Context as Management.File);
            else
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Filter = "Lua Scripts;*.lua|All Files;*.*";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Program.Manager.DesignersManager.OpenDesigner(new Management.File(null, null, ofd.FileName));
            }
        }
    }

    class OpenInWindowsExplorer : Action
    {
        public OpenInWindowsExplorer() : base() { }
        public OpenInWindowsExplorer(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_open;
            this.Text = "Open in Windows Explorer";
            this.Enabled = true;
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Management.Project)
                System.Diagnostics.Process.Start((this.Context as Management.Project).ProjectInfo.DirectoryName);
            else if (this.Context is Management.Folder)
                System.Diagnostics.Process.Start((this.Context as Management.Folder).FolderInfo.FullName);
            else if (this.Context is Management.File)
                System.Diagnostics.Process.Start((this.Context as Management.File).FileInfo.FullName);
        }
    }

    class Close : Action
    {
        public Close() : base() { }
        public Close(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Close";
            this.Enabled = false;
        }
    }

    class Save : Action
    {
        private Operatables.ISavable m_CurrentSavable = null;

        public Save() : base() { }
        public Save(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_save;
            this.Text = "Save";
            this.Enabled = false;
            this.Shortcut = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;

            // Listen for events.
            Program.Manager.DesignersManager.DesignerChanged += new MOAI.Designers.Manager.DesignerEventHandler(DesignersManager_DesignerChanged);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.m_CurrentSavable != null)
                this.m_CurrentSavable.SaveFile();
        }

        /// <summary>
        /// This event is raised when the active tab (designer) changes.
        /// </summary>
        private void DesignersManager_DesignerChanged(object sender, MOAI.Designers.Manager.DesignerEventArgs e)
        {
            if (e.Designer == null || e.Designer.File == null || !(e.Designer is Operatables.ISavable))
            {
                this.Enabled = false;
                this.Text = "Save";
                return;
            }

            this.m_CurrentSavable = (e.Designer as Operatables.ISavable);
            this.Enabled = this.m_CurrentSavable.CanSave;
            this.Text = "Save " + this.m_CurrentSavable.SaveName;
        }
    }

    class SaveAs : Action
    {
        private Operatables.ISavable m_CurrentSavable = null;

        public SaveAs() : base() { }
        public SaveAs(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = null;
            this.Text = "Save as...";
            this.Enabled = false;

            // Listen for events.
            Program.Manager.DesignersManager.DesignerChanged += new MOAI.Designers.Manager.DesignerEventHandler(DesignersManager_DesignerChanged);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.m_CurrentSavable != null)
            {
                this.m_CurrentSavable.SaveFileAs();
                Program.Manager.DesignersManager.EmulateDesignerChange();
            }
        }

        /// <summary>
        /// This event is raised when the active tab (designer) changes.
        /// </summary>
        private void DesignersManager_DesignerChanged(object sender, MOAI.Designers.Manager.DesignerEventArgs e)
        {
            if (e.Designer == null || e.Designer.File == null || !(e.Designer is Operatables.ISavable))
            {
                this.Enabled = false;
                this.Text = "Save as...";
                return;
            }

            this.m_CurrentSavable = (e.Designer as Operatables.ISavable);
            this.Enabled = this.m_CurrentSavable.CanSave;
            this.Text = "Save " + this.m_CurrentSavable.SaveName + " as...";
        }
    }

    class SaveAll : Action
    {
        public SaveAll() : base() { }
        public SaveAll(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = MOAI.Properties.Resources.actions_save_all;
            this.Text = "Save All";
            this.Enabled = false;
        }
    }

    class Exit : Action
    {
        public Exit() : base() { }
        public Exit(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = null;
            this.Text = "Exit";
            this.Enabled = true;

            // Listen for events.
            Program.Manager.SolutionLoaded += new EventHandler(Manager_OnSolutionLoaded);
            Program.Manager.SolutionUnloaded += new EventHandler(Manager_OnSolutionUnloaded);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            // TODO: Add proper unsaved changes checking etc.. here
            Program.Manager.Stop();
        }

        /// <summary>
        /// This event is raised when a solution is loaded (opened).
        /// </summary>
        private void Manager_OnSolutionLoaded(object sender, EventArgs e)
        {
            this.Enabled = true;
        }

        /// <summary>
        /// This event is raised when a solution is unloaded (closed).
        /// </summary>
        private void Manager_OnSolutionUnloaded(object sender, EventArgs e)
        {
            this.Enabled = false;
        }
    }

    class Undo : Action
    {
        public Undo() : base() { }
        public Undo(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = MOAI.Properties.Resources.actions_undo;
            this.Text = "Undo";
            this.Enabled = false;
        }
    }

    class Redo : Action
    {
        public Redo() : base() { }
        public Redo(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = MOAI.Properties.Resources.actions_redo;
            this.Text = "Redo";
            this.Enabled = false;
        }
    }

    class Cut : Action
    {
        public Cut() : base() { }
        public Cut(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_cut;
            this.Text = "Cut";
            this.Enabled = (this.Context != null);
            this.Shortcut = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;

            // Check Scintilla context.
            if (this.Context is ScintillaNet.Scintilla &&
                (this.Context as ScintillaNet.Scintilla).Selection.Length == 0)
            {
                this.Context = null;
                this.Enabled = false;
            }

            // Listen for global context changes if we are not
            // provided with a specific context.
            if (this.Context == null)
            {
                Program.Manager.CacheManager.Context.ContextChanged += new EventHandler<MOAI.Cache.ContextEventArgs>(Context_ContextChanged);

                // Simulate a context change initially.
                this.Context_ContextChanged(this, new MOAI.Cache.ContextEventArgs(Program.Manager.CacheManager.Context.Object));
            }
        }

        /// <summary>
        /// This event is raised when the global context changes.
        /// </summary>
        void Context_ContextChanged(object sender, MOAI.Cache.ContextEventArgs e)
        {
            // Set our context to the global context object.
            if (e.Object is Operatables.ICuttable &&
                (e.Object as Operatables.ICuttable).CanCut)
                this.Context = e.Object;
            else
                this.Context = null;

            this.Enabled = (this.Context != null);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Operatables.ICuttable)
            {
                // Ask the object to cut itself.
                (this.Context as Operatables.ICuttable).Cut();
            }
        }
    }

    class Copy : Action
    {
        public Copy() : base() { }
        public Copy(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_copy;
            this.Text = "Copy";
            this.Enabled = (this.Context != null);
            this.Shortcut = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;

            // Check Scintilla context.
            if (this.Context is ScintillaNet.Scintilla &&
                (this.Context as ScintillaNet.Scintilla).Selection.Length == 0)
            {
                this.Context = null;
                this.Enabled = false;
            }

            // Listen for global context changes if we are not
            // provided with a specific context.
            if (this.Context == null)
            {
                Program.Manager.CacheManager.Context.ContextChanged += new EventHandler<MOAI.Cache.ContextEventArgs>(Context_ContextChanged);

                // Simulate a context change initially.
                this.Context_ContextChanged(this, new MOAI.Cache.ContextEventArgs(Program.Manager.CacheManager.Context.Object));
            }
        }

        /// <summary>
        /// This event is raised when the global context changes.
        /// </summary>
        void Context_ContextChanged(object sender, MOAI.Cache.ContextEventArgs e)
        {
            // Set our context to the global context object.
            if (e.Object is Operatables.ICopyable &&
                (e.Object as Operatables.ICopyable).CanCopy)
                this.Context = e.Object;
            else
                this.Context = null;

            this.Enabled = (this.Context != null);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Operatables.ICopyable)
                (this.Context as Operatables.ICopyable).Copy();
        }
    }

    class Paste : Action
    {
        public Paste() : base() { }
        public Paste(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = MOAI.Properties.Resources.actions_paste;
            this.Text = "Paste";
            this.Enabled = (this.Context != null);
            this.Shortcut = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;

            // Listen for global context changes if we are not
            // provided with a specific context.
            if (this.Context == null)
            {
                Program.Manager.CacheManager.Context.ContextChanged += new EventHandler<MOAI.Cache.ContextEventArgs>(Context_ContextChanged);

                // Simulate a context change initially.
                this.Context_ContextChanged(this, new MOAI.Cache.ContextEventArgs(Program.Manager.CacheManager.Context.Object));
            }
        }

        /// <summary>
        /// This event is raised when the global context changes.
        /// </summary>
        void Context_ContextChanged(object sender, MOAI.Cache.ContextEventArgs e)
        {
            // Set our context to the global context object.
            if (e.Object is Operatables.IPastable &&
                (e.Object as Operatables.IPastable).CanPaste)
                this.Context = e.Object;
            else
                this.Context = null;

            this.Enabled = (this.Context != null);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Operatables.IPastable)
                (this.Context as Operatables.IPastable).Paste();
        }
    }

    class Delete : Action
    {
        public Delete() : base() { }
        public Delete(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = null;
            this.Text = "Delete";
            this.Enabled = (this.Context != null);

            // Check Scintilla context.
            if (this.Context is ScintillaNet.Scintilla &&
                (this.Context as ScintillaNet.Scintilla).Selection.Length == 0)
            {
                this.Context = null;
                this.Enabled = false;
            }

            // Listen for global context changes if we are not
            // provided with a specific context.
            if (this.Context == null)
            {
                Program.Manager.CacheManager.Context.ContextChanged += new EventHandler<MOAI.Cache.ContextEventArgs>(Context_ContextChanged);

                // Simulate a context change initially.
                this.Context_ContextChanged(this, new MOAI.Cache.ContextEventArgs(Program.Manager.CacheManager.Context.Object));
            }
        }

        /// <summary>
        /// This event is raised when the global context changes.
        /// </summary>
        void Context_ContextChanged(object sender, MOAI.Cache.ContextEventArgs e)
        {
            // Set our context to the global context object.
            if (e.Object is ScintillaNet.Scintilla &&
                (e.Object as ScintillaNet.Scintilla).Selection.Length > 0)
                this.Context = e.Object;
            else
                this.Context = null;

            this.Enabled = (this.Context != null);
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is ScintillaNet.Scintilla)
            {
                // Delete the selected text.
                (this.Context as ScintillaNet.Scintilla).NativeInterface.DeleteBack();
            }
        }
    }

    class Remove : Action
    {
        public Remove() : base() { }
        public Remove(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Remove";
            this.Enabled = false;
        }
    }

    class Rename : Action
    {
        public Rename() : base() { }
        public Rename(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = null;
            this.Text = "Rename";
            this.Enabled = true;
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Management.File)
            {
                System.Windows.Forms.NodeLabelEditEventHandler handler = null;
                handler = new System.Windows.Forms.NodeLabelEditEventHandler((sender, e) =>
                {
                    // Check this is the node we are interested in.
                    if (e.Node == this.Context)
                    {
                        // Store the original name.
                        string original = null;
                        if (this.Context is Management.Folder)
                            original = (this.Context as Management.Folder).FolderInfo.Name;
                        else if (this.Context is Management.File)
                            original = (this.Context as Management.File).FileInfo.Name;

                        // Check to see if the new label is null (indiciating don't change).
                        if (e.Label == null)
                        {
                            (this.Context as Management.File).Text = original;
                            (this.Context as Management.File).TreeView.AfterLabelEdit -= handler;
                            (this.Context as Management.File).TreeView.LabelEdit = false;
                            return;
                        }
                        
                        // Check to see if the label is valid.
                        if (e.Label.IndexOfAny(new char[] { '/', '\\', ':', '*', '"', '<', '>', '|' }) != -1)
                        {
                            (this.Context as Management.File).Text = original;
                            System.Windows.Forms.MessageBox.Show(@"Please enter a filename; it must not contain any
    " + "of the following characters: / \\ : * \" < > |", "Rename Failed", System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                            (this.Context as Management.File).BeginEdit();
                        }
                        else
                        {
                            // Attempt to rename the file.
                            try
                            {
                                if (this.Context is Management.Folder)
                                    (this.Context as Management.Folder).FolderInfo.MoveTo(
                                        Path.Combine(
                                            (this.Context as Management.Folder).FolderInfo.Parent.FullName,
                                            e.Label
                                        )
                                    );
                                else if (this.Context is Management.File)
                                    (this.Context as Management.File).FileInfo.MoveTo(
                                        Path.Combine(
                                            (this.Context as Management.File).FileInfo.DirectoryName,
                                            e.Label
                                        )
                                    );
                                (this.Context as Management.File).TreeView.AfterLabelEdit -= handler;
                                (this.Context as Management.File).TreeView.LabelEdit = false;
                                (this.Context as Management.File).Text = e.Label;
                                (this.Context as Management.File).PerformRenamed();

                                // Force the solution explorer to refresh.
                                MOAI.Tools.SolutionExplorerTool s = Program.Manager.ToolsManager.Get(typeof(MOAI.Tools.SolutionExplorerTool)) as MOAI.Tools.SolutionExplorerTool;
                                if (s != null)
                                    s.ReloadTree();
                            }
                            catch (IOException)
                            {
                                (this.Context as Management.File).Text = original;
                                System.Windows.Forms.MessageBox.Show("Unable to rename file.", "Rename Failed", System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Error);
                                (this.Context as Management.File).BeginEdit();
                            }
                        }
                    }
                });
                (this.Context as Management.File).TreeView.AfterLabelEdit += handler;
                (this.Context as Management.File).TreeView.LabelEdit = true;
                (this.Context as Management.File).BeginEdit();
            }
        }
    }

    class Exclude : Action
    {
        public Exclude() : base() { }
        public Exclude(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.ItemIcon = null;
            this.Text = "Exclude From Project";
            this.Enabled = true;
        }

        /// <summary>
        /// This event is raised when the menu item is clicked or otherwise activated.
        /// </summary>
        public override void OnActivate()
        {
            if (this.Context is Management.File || this.Context is Management.Folder)
            {
                // Remove the file from the tree view.
                (this.Context as Management.File).Project.RemoveFile((this.Context as Management.File));
            }
        }
    }

    class SelectAll : Action
    {
        public SelectAll() : base() { }
        public SelectAll(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Select All";
            this.Enabled = false;
        }
    }

    class QuickFind : Action
    {
        public QuickFind() : base() { }
        public QuickFind(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Quick Find";
            this.Enabled = false;
        }
    }

    class QuickReplace : Action
    {
        public QuickReplace() : base() { }
        public QuickReplace(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Quick Replace";
            this.Enabled = false;
        }
    }

    class FindInFiles : Action
    {
        public FindInFiles() : base() { }
        public FindInFiles(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = MOAI.Properties.Resources.actions_find_in_files;
            this.Text = "Find in Files";
            this.Enabled = false;
        }
    }

    class ReplaceInFiles : Action
    {
        public ReplaceInFiles() : base() { }
        public ReplaceInFiles(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Replace in Files";
            this.Enabled = false;
        }
    }

    class GoTo : Action
    {
        public GoTo() : base() { }
        public GoTo(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Go To...";
            this.Enabled = false;
        }
    }

    class Preferences : Action
    {
        public Preferences() : base() { }
        public Preferences(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Preferences";
            this.Enabled = false;
        }
    }

    class Properties : Action
    {
        public Properties() : base() { }
        public Properties(object context) : base(context) { }

        /// <summary>
        /// This event is raied when the menu item is to be initalized.
        /// </summary>
        public override void OnInitialize()
        {
            this.Implemented = false;
            this.ItemIcon = null;
            this.Text = "Properties";
            this.Enabled = false;
        }
    }
}
