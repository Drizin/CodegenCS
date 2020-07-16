using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CodegenCS.DotNet
{
    /// <summary>
    /// Allows to load a MSBuild project (csproj, vbproj, etc), to automatically include files. <br />
    /// Accepts projects both in new SDK-Style format and in the old non-SDK-Style format, <br />
    /// although the new SDK-Style automatically builds all CS files even if they are not referenced <br />
    /// (actually it's the opposite, in new SDK-Style the EXCLUDED files should be described).
    /// </summary>
    public class MSBuildProjectEditor
    {
        private string _projectFileFullPath;
        private XmlDocument _doc;
        private XmlNamespaceManager _nsmgr;
        private ProjectType _projectType;
        private Dictionary<string, List<string>> _addedDependentItems = new Dictionary<string, List<string>>();
        private HashSet<string> _addedIndependentItems = new HashSet<string>();

        /// <summary>
        /// New in-memory editor.
        /// </summary>
        /// <param name="projectFilePath"></param>
        public MSBuildProjectEditor(string projectFilePath)
        {
            this._projectFileFullPath = new FileInfo(projectFilePath).FullName;
            _doc = new XmlDocument();
            _doc.Load(this._projectFileFullPath);
            _nsmgr = new XmlNamespaceManager(_doc.NameTable);
            _nsmgr.AddNamespace("msbuild", _doc.DocumentElement.NamespaceURI);
            XmlNode element = _doc.SelectSingleNode("//msbuild:Project", _nsmgr);
            if (element.Attributes["Sdk"] != null) // Microsoft.NET.Sdk
                _projectType = ProjectType.SDKStyle;
            else
                _projectType = ProjectType.NonSDKStyle;

        }

        /// <summary>
        /// Adds a single item, optionally dependent of a parent item (DependentUpon). 
        /// </summary>
        public void AddItem(string itemPath, BuildActionType itemType = BuildActionType.Compile, string parentItemPath = null)
        {
            XmlNode itemGroup = null;
            itemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(itemPath).FullName)).ToString().Replace("/", "\\");
            if (parentItemPath != null)
                parentItemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(parentItemPath).FullName)).ToString().Replace("/", "\\");

            if (parentItemPath != null && _projectType == ProjectType.NonSDKStyle)
            {
                // ensure parent file is in msbuild project
                XmlNode parentNode = _doc.SelectSingleNode("//*[@Include='" + parentItemPath + "'][local-name()='None' or local-name()='Compile']");
                if (_projectType == ProjectType.NonSDKStyle && parentNode == null)
                    throw new Exception($"Parent item not found in project file {this._projectFileFullPath}");
                itemGroup = parentNode.ParentNode;
            }

            if (itemGroup == null)
                itemGroup = _doc.SelectSingleNode("//msbuild:ItemGroup", _nsmgr);
            if (itemGroup == null)
            {
                XmlNode project = _doc.SelectSingleNode("//msbuild:Project", _nsmgr);
                itemGroup = _doc.CreateElement("ItemGroup", _doc.DocumentElement.NamespaceURI);
                project.AppendChild(itemGroup);
            }


            if (parentItemPath != null)
            {
                if (!_addedDependentItems.ContainsKey(parentItemPath))
                    _addedDependentItems[parentItemPath] = new List<string>();
                _addedDependentItems[parentItemPath].Add(itemPath);
            }
            else
            {
                if (!_addedDependentItems.ContainsKey(itemPath))
                    _addedIndependentItems.Add(itemPath);
            }


            if (_projectType == ProjectType.SDKStyle)
            {
                //TODO: double check that file is under csproj folder? if it's not under we have to include!
            }

            // Checking the netcore "Compile Remove" nodes.
            if (_projectType == ProjectType.SDKStyle)
            {
                XmlNode compileRemoveNode = _doc.SelectSingleNode("//msbuild:Compile[@Remove='" + itemPath + "']", _nsmgr);

                // Compiled files shouldn't have a "Compile Remove" node. "None" files should.
                if (itemType == BuildActionType.Compile)
                {
                    if (compileRemoveNode != null)
                        compileRemoveNode.ParentNode.RemoveChild(compileRemoveNode);
                }
                else
                {
                    if (compileRemoveNode == null)
                    {
                        XmlElement newCompileRemoveNode = _doc.CreateElement("Compile", _doc.DocumentElement.NamespaceURI);
                        newCompileRemoveNode.SetAttribute("Remove", itemPath);
                        itemGroup.AppendChild(newCompileRemoveNode); //TODO: does this need to be before the "None Update" node?
                    }
                }
            }

            // Checking the "None/Compile" nodes
            //XmlNode existingElement = _doc.SelectSingleNode("//*[local-name()='None' or local-name()='Compile'][@" + ((IsCore && itemType == OutputFileType.Compile) ? "Update" : "Include") + "='" + itemPath + "']", _nsmgr);
            XmlElement existingElement = (XmlElement)_doc.SelectSingleNode("//*[local-name()='None' or local-name()='Compile'][@Include='" + itemPath + "' or @Update='" + itemPath + "']", _nsmgr);
            if (existingElement != null)
            {
                // node exists in csproj but shouldn't be there
                if (itemType == BuildActionType.NonProjectItem)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but wrong type
                if (_projectType == ProjectType.NonSDKStyle && itemType.ToString() != existingElement.Name)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but wrong type
                if (_projectType == ProjectType.NonSDKStyle && itemType.ToString() != existingElement.Name)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but is not necessary
                if (_projectType == ProjectType.SDKStyle && itemType == BuildActionType.Compile && parentItemPath == null)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                //// node exists shouldn't be there
                //if (IsCore && itemType == OutputFileType.Compile && parentItemPath != null 
                //    && existingElement.SelectSingleNode("[msbuild:DependentUpon/text()='" + parentItemPath + "'][@" + ((IsCore && itemType == OutputFileType.Compile) ? "Update" : "Include") + "='" + itemPath + "']", _nsmgr) != null)
                //{ existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }
                //_doc.SelectSingleNode("//msbuild:" + itemType.ToString() + 
            }

            if (itemType == BuildActionType.NonProjectItem)
                return;
            // core projects don't need to declare regular compiled files
            if (existingElement == null && (_projectType == ProjectType.SDKStyle && itemType == BuildActionType.Compile) && parentItemPath == null)
                return;

            string expectedType = "Include";
            if (_projectType == ProjectType.SDKStyle && itemType == BuildActionType.Compile)
                expectedType = "Update";

            if (existingElement != null)
            {
                if (existingElement.Attributes["Include"] != null && (expectedType != "Include" || existingElement.Attributes["Include"].Value != itemPath))
                    existingElement.Attributes.RemoveNamedItem("Include");
                if (existingElement.Attributes["Update"] != null && (expectedType != "Update" || existingElement.Attributes["Update"].Value != itemPath))
                    existingElement.Attributes.RemoveNamedItem("Update");
            }
            if (existingElement == null)
            {
                existingElement = _doc.CreateElement(itemType.ToString(), _doc.DocumentElement.NamespaceURI);
                existingElement.SetAttribute(((_projectType == ProjectType.SDKStyle && itemType == BuildActionType.Compile) ? "Update" : "Include"), itemPath);
            }
            if (existingElement.Attributes["Include"] == null && expectedType == "Include")
                existingElement.SetAttribute("Include", itemPath);
            if (existingElement.Attributes["Update"] == null && expectedType == "Update")
                existingElement.SetAttribute("Update", itemPath);


            if (parentItemPath != null)
            {
                if (existingElement.SelectSingleNode("//msbuild:DependentUpon[text()='" + parentItemPath + "']", _nsmgr) == null)
                {
                    var nodes = existingElement.SelectNodes("//msbuild:DependentUpon", _nsmgr);
                    for (int i = 0; i < nodes.Count; i++)
                        nodes.Item(i).ParentNode.RemoveChild(nodes.Item(i));

                    XmlElement dependentUpon = _doc.CreateElement("DependentUpon", _doc.DocumentElement.NamespaceURI);
                    dependentUpon.InnerText = parentItemPath;
                    existingElement.AppendChild(dependentUpon);
                }
            }

            if (existingElement.ParentNode == null) // new elements
                itemGroup.AppendChild(existingElement);
        }

        /// <summary>
        /// Given an item in the project, will remove all items which depend on this parent item, except the items which were added using AddItem
        /// </summary>
        public void RemoveUnusedDependentItems(string parentItemPath)
        {
            parentItemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(parentItemPath).FullName)).ToString().Replace("/", "\\");
            XmlNode parentNode = _doc.SelectSingleNode("//*[@Include='" + parentItemPath + "'][local-name()='None' or local-name()='Compile']");
            if (_projectType == ProjectType.NonSDKStyle && parentNode == null)
                throw new Exception($"Parent item not found in project file {this._projectFileFullPath}");

            XmlNodeList children = _doc.SelectNodes("//msbuild:*[msbuild:DependentUpon/text()='" + parentItemPath + "'][local-name()='None' or local-name()='Compile']", _nsmgr);
            for (int i = 0; i < children.Count; i++)
            {
                string itemName = children.Item(i).Attributes["Include"].Value;
                if (!_addedDependentItems[parentItemPath].Contains(itemName))
                    children.Item(i).ParentNode.RemoveChild(children.Item(i));
            }
        }

        /// <summary>
        /// Given a folder in the project, will remove all items which are under this folder, except the items which were added using AddItem
        /// </summary>
        public void RemoveUnusedItems(string parentFolderPath)
        {
            parentFolderPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new DirectoryInfo(parentFolderPath).FullName)).ToString().Replace("/", "\\");

            if (_projectType == ProjectType.NonSDKStyle)
            {
                XmlNodeList children = _doc.SelectNodes("//*[contains(@Include,'" + parentFolderPath + "')][local-name()='None' or local-name()='Compile']");
                for (int i = 0; i < children.Count; i++)
                {
                    string itemName = children.Item(i).Attributes["Include"].Value;
                    if (!_addedIndependentItems.Contains(itemName))
                        children.Item(i).ParentNode.RemoveChild(children.Item(i));
                }
            }
            //TODO: for core, do we need to remove anything?
        }

        /// <summary>
        /// Saves all changes into the csproj/vbproj
        /// </summary>
        public void Save()
        {
            _doc.Save(this._projectFileFullPath);
        }

    }
}
