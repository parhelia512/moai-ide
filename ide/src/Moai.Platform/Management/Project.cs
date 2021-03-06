﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using Moai.Platform.Collections;
using Moai.Platform.UI;
using Moai.Platform.Menus;
using Moai.Platform.Templates.Files;

namespace Moai.Platform.Management
{
    public class Project : IPastable, ISyncable
    {
        private bool p_Initalized = false;
        private FileInfo p_ProjectInfo = null;
        private List<Management.File> p_Files = new List<Management.File>();

        public event EventHandler FileAdded;
        public event EventHandler FileRemoved;
        public event EventHandler<FileEventArgs> FileRenamed;

        /// <summary>
        /// Creates a new instance of the Project class that is not associated
        /// with any on-disk solution.
        /// </summary>
        public Project()
        {
            this.p_ProjectInfo = null;

            // Add our own events for handling when files are added
            // or removed.
            this.FileAdded += new EventHandler((sender, e) =>
            {
                this.Save();
            });
            this.FileRemoved += new EventHandler((sender, e) =>
            {
                this.Save();
            });
            this.FileRenamed += new EventHandler<FileEventArgs>((sender, e) =>
            {
                this.Save();
            });
        }

        /// <summary>
        /// Loads a new instance of the Project class from a file on disk.
        /// </summary>
        /// <param name="file">The solution file to be loaded.</param>
        public Project(FileInfo file)
            : this()
        {
            this.p_ProjectInfo = file;

            // Read the project data from the file.
            this.LoadFromXml(this.p_ProjectInfo);
        }

        /// <summary>
        /// Creates a new Project object that represents a Moai project.  The
        /// constructor attempts to load the from disk using the specified path.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        public Project(string path)
            : this()
        {
            this.p_ProjectInfo = new FileInfo(path);
            this.p_Files = new List<Management.File>();

            if (this.p_ProjectInfo.Exists)
            {
                this.LoadFromXml(this.p_ProjectInfo);
                this.p_Initalized = true;
            }
            else
                this.p_Initalized = false;
        }

        /// <summary>
        /// Gets the File object based on the relative path provided.  The specified path must
        /// be a file that is included in the project.
        /// </summary>
        /// <param name="path">The relative path to the file from the project file.</param>
        /// <returns>The File object that represents this path, or null if not found.</returns>
        public Management.File GetByPath(string path)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            Management.File f = this.p_Files.Find(a => a.FileInfo.Name == parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                if (f is Management.Folder)
                {
                    Management.Folder ff = f as Management.Folder;
                    f = ff.Files.ToList().Find(a => a.FileInfo.Name == parts[i]);
                }
                else
                    return null;
            }
            return f;
        }

        #region Disk Operations

        /// <summary>
        /// Creates a new project on disk with the specified name in the
        /// specified directory.  The resulting location of the file will
        /// be path\name.mproj
        /// </summary>
        /// <param name="name">The name of the solution.</param>
        /// <param name="path">The path to the solution.</param>
        public static Project Create(string name, string path)
        {
            // Create the directory if it does not exist.
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Create a new empty instance.
            Project p = new Project();

            // Associate a FileInfo instance with it.
            p.p_ProjectInfo = new FileInfo(Path.Combine(path, name + ".mproj"));

            // Request that the project be saved.
            p.Save();

            // Return the new project.
            return p;
        }

        /// <summary>
        /// Saves the project file to disk; this project must have a
        /// project file associated with it in order to save it to disk.
        /// </summary>
        public void Save()
        {
            if (this.p_ProjectInfo == null)
                throw new InvalidOperationException();

            // Configure the settings for the XmlWriter.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            for (int i = 0; i < 5; i += 1)
            {
                try
                {
                    // Create the new XmlWriter.
                    XmlWriter writer = XmlWriter.Create(this.p_ProjectInfo.FullName, settings);

                    // Generate the XML from the project data.
                    writer.WriteStartElement("Project");
                    writer.WriteAttributeString("ToolsVersion", "1.0");
                    writer.WriteString(""); // Force the root element to not be self-closing.
                    this.WriteFiles(writer, this.p_Files.AsReadOnly(), "");
                    writer.WriteEndElement();
                    writer.Close();
                    return;
                }
                catch (IOException e)
                {
                    Thread.Sleep(0);
                    continue;
                }
            }
        }

        /// <summary>
        /// Recursively writes all the files owned by this project to the XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to write to.</param>
        /// <param name="files">The file collection.</param>
        /// <param name="path">The current path that this call is made under ("" for root).</param>
        private void WriteFiles(XmlWriter writer, System.Collections.ObjectModel.ReadOnlyCollection<Management.File> files, string path)
        {
            foreach (Management.File f in files)
            {
                if (f is Management.Folder)
                    this.WriteFiles(writer, (f as Management.Folder).Files, (path + "/" + (f as Management.Folder).FolderInfo.Name).TrimStart(new char[] { '/' }));
                else
                {
                    writer.WriteStartElement("File");
                    writer.WriteAttributeString("Include", (path + "/" + f.FileInfo.Name).TrimStart(new char[] { '/' }));
                    writer.WriteString("");
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Loads the project data from the specified XML document.
        /// </summary>
        /// <param name="file">The XML document represented as a FileInfo object.</param>
        private void LoadFromXml(FileInfo file)
        {
            // Import the XML document using the Tree collection.
            Tree t = null;
            using (XmlReader reader = XmlReader.Create(file.FullName))
            {
                t = Tree.FromXml(reader);
            }

            Node p = t.GetChildElement("project");
            if (p == null)
                return;

            List<Node> childs = p.GetChildElements("file");
            if (childs != null)
            {
                foreach (Node f in childs)
                {
                    List<string> sf = f.Attributes["Include"].Split(new char[] { '\\', '/' }).ToList();
                    string fn = sf[sf.Count - 1];
                    sf.RemoveAt(sf.Count - 1);
                    Management.Folder ff = null;

                    // Loop through until we get the parent directory
                    // if needed.
                    string path = "";
                    foreach (string s in sf)
                    {
                        path += s + Path.DirectorySeparatorChar;
                        bool handled = false;
                        if (ff == null)
                        {
                            foreach (Management.File f2 in this.p_Files)
                                if (f2 is Management.Folder && (f2 as Management.Folder).FolderInfo.Name == s)
                                {
                                    ff = f2 as Management.Folder;
                                    handled = true;
                                    break;
                                }

                            if (!handled)
                            {
                                Management.Folder newf = new Folder(this, file.Directory.FullName, path.Substring(0, path.Length - 1));
                                newf.FileAdded += new EventHandler(ff_FileAdded);
                                newf.FileRemoved += new EventHandler(ff_FileRemoved);
                                this.p_Files.Add(newf);
                                ff = newf;
                            }
                        }
                        else
                        {
                            foreach (Management.File f2 in ff.Files)
                                if (f2 is Management.Folder && (f2 as Management.Folder).FolderInfo.Name == s)
                                {
                                    ff = f2 as Management.Folder;
                                    handled = true;
                                    break;
                                }

                            if (!handled)
                            {
                                Management.Folder newf = new Folder(this, file.Directory.FullName, path.Substring(0, path.Length - 1));
                                newf.FileAdded += new EventHandler(ff_FileAdded);
                                newf.FileRemoved += new EventHandler(ff_FileRemoved);
                                ff.AddWithoutEvent(newf);
                                ff = newf;
                            }
                        }
                    }

                    // Skip the file if it doesn't exist on disk.
                    if (!System.IO.File.Exists(Path.Combine(file.Directory.FullName, f.Attributes["Include"])))
                        continue;

                    // Now associate the file with the directory or project,
                    // depending on whether or not we have a parent directory.
                    if (ff == null)
                        this.p_Files.Add(new File(this, file.Directory.FullName, f.Attributes["Include"]));
                    else
                        ff.AddWithoutEvent(new File(this, file.Directory.FullName, f.Attributes["Include"]));
                }
            }
        }

        #endregion

        /// <summary>
        /// Whether the project has been initialized.
        /// </summary>
        public bool Initalized
        {
            get
            {
                return this.p_Initalized;
            }
        }

        /// <summary>
        /// The FileInfo object that represents the on-disk project file for
        /// this project.
        /// </summary>
        public FileInfo ProjectInfo
        {
            get
            {
                return this.p_ProjectInfo;
            }
        }

        /// <summary>
        /// Returns the context menu for this project.
        /// </summary>
        public virtual Moai.Platform.Menus.Action[] ContextActions
        {
            get
            {
                // Create the context action list.
                return new Moai.Platform.Menus.Action[]
                {
                    new Menus.Definitions.Project.Build(this),
                    new Menus.Definitions.Project.Rebuild(this),
                    new Menus.Definitions.Project.Clean(this),
                    new SeperatorAction(),
                    new Menus.Definitions.Project.ProjDependencies(this),
                    new Menus.Definitions.Project.ProjBuildOrder(this),
                    new SeperatorAction(),
                    new GroupAction("Add", null, new Moai.Platform.Menus.Action[] {
                        new Menus.Definitions.Project.AddNewItem(this),
                        new Menus.Definitions.Project.AddExistingItem(this),
                        new Menus.Definitions.Project.AddFolder(this),
                        new SeperatorAction(),
                        new Menus.Definitions.Project.AddScript(this),
                        new Menus.Definitions.Project.AddClass(this)
                    }),
                    new Menus.Definitions.Project.AddReference(this),
                    new SeperatorAction(),
                    new Menus.Definitions.Project.SetAsStartupProject(this),
                    new Menus.Definitions.Project.StartWithDebug(this),
                    new SeperatorAction(),
                    new Menus.Definitions.Actions.Cut(this),
                    new Menus.Definitions.Actions.Paste(this),
                    new Menus.Definitions.Actions.Remove(this),
                    new Menus.Definitions.Actions.Rename(this),
                    new SeperatorAction(),
                    new Menus.Definitions.Actions.OpenInWindowsExplorer(this),
                    new SeperatorAction(),
                    new Menus.Definitions.Actions.Properties(this)
                };
            }
        }

        /// <summary>
        /// Adds a file interactively to the project (i.e. prompts the user).
        /// </summary>
        public void AddFileInteractive()
        {
            this.AddFileInteractive(null, null);
        }

        /// <summary>
        /// Adds a file interactively to the project (i.e. prompts the user).
        /// </summary>
        public void AddFileInteractive(string preselected)
        {
            this.AddFileInteractive(preselected, null);
        }

        /// <summary>
        /// Adds a file interactively to the project (i.e. prompts the user).
        /// </summary>
        public void AddFileInteractive(Management.Folder f)
        {
            this.AddFileInteractive(null, f);
        }

        /// <summary>
        /// Adds a file interactively to the project (i.e. prompts the user).
        /// </summary>
        /// <param name="f">The folder to place the file in, or null for the project root.</param>
        public void AddFileInteractive(string preselected, Management.Folder f)
        {
            FileCreationData fcd = Central.Platform.UI.PickNewFile();
            fcd.Template.Create(fcd.Name, this, f);
        }

        /// <summary>
        /// Adds a file to the project.
        /// </summary>
        /// <param name="f">The file to add.</param>
        public void AddFile(Management.File f)
        {
            this.p_Files.Add(f);
            if (f is Management.Folder)
            {
                Management.Folder ff = f as Management.Folder;
                ff.FileAdded += new EventHandler(ff_FileAdded);
                ff.FileRemoved += new EventHandler(ff_FileRemoved);
            }
            if (this.FileAdded != null)
                this.FileAdded(this, new EventArgs());
        }

        /// <summary>
        /// Removes a file from the project, either by removing it directory
        /// or asking subfolders to remove it.
        /// </summary>
        /// <param name="file">The file to remove.</param>
        public void PerformRemove(Management.File file)
        {
            if (this.p_Files.Contains(file))
            {
                this.p_Files.Remove(file);
                if (file is Management.Folder)
                {
                    Management.Folder ff = file as Management.Folder;
                    ff.FileAdded -= new EventHandler(ff_FileAdded);
                    ff.FileRemoved -= new EventHandler(ff_FileRemoved);
                }
                if (this.FileRemoved != null)
                    this.FileRemoved(this, new EventArgs());
            }
            else
            {
                foreach (Management.File f in this.p_Files.ToList())
                {
                    if (f is Management.Folder)
                    {
                        // Request the subfolder to remove the file if they
                        // have it.
                        (f as Management.Folder).Remove(file);
                    }
                }
            }
        }

        /// <summary>
        /// This function propagates FileAdded events from folders as
        /// FileAdded events on the project itself.
        /// </summary>
        private void ff_FileAdded(object sender, EventArgs e)
        {
            if (this.FileAdded != null)
                this.FileAdded(this, new EventArgs());
        }

        /// <summary>
        /// This function propagates FileRemoved events from folders as
        /// FileRemoved events on the project itself.
        /// </summary>
        private void ff_FileRemoved(object sender, EventArgs e)
        {
            if (this.FileRemoved != null)
                this.FileRemoved(this, new EventArgs());
        }

        /// <summary>
        /// Performs the FileRenamed event.  Used by external code that renames
        /// files on disk so that any relevant aspects of the IDE get updated.
        /// </summary>
        public void PerformRename(Management.File file)
        {
            if (this.FileRenamed != null)
                this.FileRenamed(this, new FileEventArgs(file));
        }

        /// <summary>
        /// A read-only list of the files within the root directory of the project.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Management.File> Files
        {
            get
            {
                return this.p_Files.AsReadOnly();
            }
        }

        #region Operation Implementations

        /// <summary>
        /// Boolean value indicating whether this project can be pasted into.
        /// </summary>
        bool IPastable.CanPaste
        {
            get { return true; }
        }

        /// <summary>
        /// Called when the user wants to paste into the root of this project.
        /// </summary>
        void IPastable.Paste()
        {
            // We are copying a set of files or folders into a project using the solution
            // explorer.
            throw new NotImplementedException();
            /* FIXME: System.Windows.Forms.IDataObject data = Moai.Cache.Clipboard.Contents;
            if (!data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
                return;

            // Check to see whether we are doing a cut or copy.
            bool iscut = false;
            if (data.GetDataPresent("Preferred DropEffect"))
                iscut = ((data.GetData("Preferred DropEffect") as MemoryStream).ReadByte() == 2);

            // Get the target folder.
            string folder = this.ProjectInfo.DirectoryName;

            // Move or copy the selected files.
            string[] files = data.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
            foreach (FileInfo f in files.Select(input => new FileInfo(input)))
            {
                // Check to make sure the file doesn't already exist in the destination.
                if (System.IO.File.Exists(Path.Combine(folder, f.Name)))
                {
                    System.Windows.Forms.MessageBox.Show(
                        f.Name + " already exists in the destination folder.  It will not be copied or moved.",
                        "File Already Exists",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                    continue;
                }

                if (iscut)
                    f.MoveTo(Path.Combine(folder, f.Name));
                else
                    f.CopyTo(Path.Combine(folder, f.Name));

                this.AddFile(new Management.File(
                    this,
                    this.ProjectInfo.DirectoryName,
                    PathHelpers.GetRelativePath(
                        this.ProjectInfo.DirectoryName,
                        Path.Combine(folder, f.Name)
                        )
                    ));
            }

            // Force the project to be saved now.
            this.Save();*/
        }

        #endregion

        #region ISyncable Members

        public event EventHandler SyncDataChanged;

        public ISyncData GetSyncData()
        {
            return new FileSyncData
            {
                Text = this.ProjectInfo.Name.Substring(0, this.ProjectInfo.Name.Length - this.ProjectInfo.Extension.Length),
                ImageKey = "project"
            };
        }

        #endregion
    }
}
