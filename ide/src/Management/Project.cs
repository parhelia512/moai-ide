﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MOAI.Collections;
using System.Xml;
using System.Windows.Forms;

namespace MOAI.Management
{
    public class Project
    {
        private bool p_Initalized = false;
        private FileInfo p_ProjectInfo = null;
        private List<File> p_Files = new List<File>();

        /// <summary>
        /// Creates a new instance of the Project class that is not associated
        /// with any on-disk solution.
        /// </summary>
        public Project()
        {
            this.p_ProjectInfo = null;
        }

        /// <summary>
        /// Loads a new instance of the Project class from a file on disk.
        /// </summary>
        /// <param name="file">The solution file to be loaded.</param>
        public Project(FileInfo file)
        {
            this.p_ProjectInfo = file;

            // Read the project data from the file.
            this.LoadFromXml(this.p_ProjectInfo);
        }

        /// <summary>
        /// Gets the File object based on the relative path provided.  The specified path must
        /// be a file that is included in the project.
        /// </summary>
        /// <param name="path">The relative path to the file from the project file.</param>
        /// <returns>The File object that represents this path, or null if not found.</returns>
        public File GetByPath(string path)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            File f = this.p_Files.Find(a => a.FileInfo.Name == parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                if (f is Folder)
                {
                    Folder ff = f as Folder;
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

            // Create the new XmlWriter.
            XmlWriter writer = XmlWriter.Create(this.p_ProjectInfo.FullName, settings);

            // Generate the XML from the project data.
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("ToolsVersion", "1.0");
            writer.WriteString(""); // Force the root element to not be self-closing.
            this.WriteFiles(writer, this.p_Files.AsReadOnly(), "");
            writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// Recursively writes all the files owned by this project to the XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to write to.</param>
        /// <param name="files">The file collection.</param>
        /// <param name="path">The current path that this call is made under ("" for root).</param>
        private void WriteFiles(XmlWriter writer, System.Collections.ObjectModel.ReadOnlyCollection<File> files, string path)
        {
            foreach (File f in files)
            {
                if (f is Folder)
                    this.WriteFiles(writer, (f as Folder).Files, (f as Folder).FolderInfo.Name);
                else
                {
                    writer.WriteStartElement("File");
                    writer.WriteAttributeString("Include", path + f.FileInfo.Name);
                    writer.WriteString("");
                    writer.WriteEndElement();
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a new Project object that represents a Roket3D project.  The
        /// constructor attempts to load the from disk using the specified path.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        public Project(string path)
        {
            this.p_ProjectInfo = new FileInfo(path);
            this.p_Files = new List<File>();

            if (this.p_ProjectInfo.Exists)
            {
                this.LoadFromXml(this.p_ProjectInfo);
                this.p_Initalized = true;
            }
            else
                this.p_Initalized = false;
        }

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
                    Folder ff = null;

                    // Loop through until we get the parent directory
                    // if needed.
                    string path = "";
                    foreach (string s in sf)
                    {
                        path += s + "\\";
                        bool handled = false;
                        if (ff == null)
                        {
                            foreach (File f2 in this.p_Files)
                                if (f2 is Folder && (f2 as Folder).FolderInfo.Name == s)
                                {
                                    ff = f2 as Folder;
                                    handled = true;
                                    break;
                                }

                            if (!handled)
                            {
                                Folder newf = new Folder(this, file.Directory.FullName, path.Substring(0, path.Length - 1));
                                this.p_Files.Add(newf);
                                ff = newf;
                            }
                        }
                        else
                        {
                            foreach (File f2 in ff.Files)
                                if (f2 is Folder && (f2 as Folder).FolderInfo.Name == s)
                                {
                                    ff = f2 as Folder;
                                    handled = true;
                                    break;
                                }

                            if (!handled)
                            {
                                Folder newf = new Folder(this, file.Directory.FullName, path.Substring(0, path.Length - 1));
                                ff.Add(newf);
                                ff = newf;
                            }
                        }
                    }

                    // Now associate the file with the directory or project,
                    // depending on whether or not we have a parent directory.
                    if (ff == null)
                        this.p_Files.Add(new File(this, file.Directory.FullName, f.Attributes["Include"]));
                    else
                        ff.Add(new File(this, file.Directory.FullName, f.Attributes["Include"]));
                }
            }
        }

        /// <summary>
        /// Returns a TreeNode for TreeView that represents this project.
        /// </summary>
        /// <returns></returns>
        public System.Windows.Forms.TreeNode ToTreeNode()
        {
            // Construct the tree node.
            System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode(
                this.p_ProjectInfo.Name.Substring(0, this.p_ProjectInfo.Name.Length - this.p_ProjectInfo.Extension.Length)
                );
            foreach (File f in this.p_Files)
                f.Associate(node);

            // Set the context menu for the node.
            node.ContextMenuStrip = new ContextMenuStrip();
            node.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.Build(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.Rebuild(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.Clean(this)),
                    new ToolStripSeparator(),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.ProjDependencies(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.ProjBuildOrder(this)),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Add", null, new ToolStripItem[] {
                        Menus.Manager.WrapAction(new Menus.Definitions.Project.AddScript(this)),
                        Menus.Manager.WrapAction(new Menus.Definitions.Project.AddClass(this))
                    }),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.AddReference(this)),
                    new ToolStripSeparator(),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.SetAsStartupProject(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.StartWithDebug(this)),
                    new ToolStripSeparator(),
                    Menus.Manager.WrapAction(new Menus.Definitions.Actions.Cut(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Actions.Paste(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Actions.Remove(this)),
                    Menus.Manager.WrapAction(new Menus.Definitions.Actions.Rename(this)),
                    new ToolStripSeparator(),
                    Menus.Manager.WrapAction(new Menus.Definitions.Actions.OpenInWindowsExplorer(this)),
                    new ToolStripSeparator(),
                    Menus.Manager.WrapAction(new Menus.Definitions.Project.ProjProperties(this))
                });

            return node;
        }

        /// <summary>
        /// Adds a file to the project.
        /// </summary>
        /// <param name="f">The file to add.</param>
        public void AddFile(File f)
        {
            this.p_Files.Add(f);
        }

        /// <summary>
        /// Removes a file from the project.
        /// </summary>
        /// <param name="f">The file to remove.</param>
        public void RemoveFile(File f)
        {
            this.p_Files.Remove(f);
        }

        /// <summary>
        /// A read-only list of the files within the root directory of the project.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<File> Files
        {
            get
            {
                return this.p_Files.AsReadOnly();
            }
        }
    }
}
