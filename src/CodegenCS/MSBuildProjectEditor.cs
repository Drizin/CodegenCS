using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CodegenCS
{
    public class MSBuildProjectEditor
    {
        string _projectFileFullPath;
        XmlDocument _doc;
        XmlNamespaceManager _nsmgr;

        bool IsCore = false;

        public MSBuildProjectEditor(string projectFilePath)
        {
            this._projectFileFullPath = new FileInfo(projectFilePath).FullName;
            _doc = new XmlDocument();
            _doc.Load(this._projectFileFullPath);
            _nsmgr = new XmlNamespaceManager(_doc.NameTable);
            _nsmgr.AddNamespace("msbuild", _doc.DocumentElement.NamespaceURI);
            XmlNode element = _doc.SelectSingleNode("//msbuild:Project", _nsmgr);
            if (element.Attributes["Sdk"] != null) // Microsoft.NET.Sdk
                IsCore = true;
        }

        public Dictionary<string, List<string>> _addedDependentItems = new Dictionary<string, List<string>>();
        public HashSet<string> _addedIndependentItems = new HashSet<string>();

        /// <summary>
        /// Adds a single item, optionally dependent of a parent item (DependentUpon). 
        /// </summary>
        public void AddItem(string itemPath, MSBuildActionType itemType = MSBuildActionType.Compile, string parentItemPath = null)
        {
            XmlNode itemGroup = null;
            itemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(itemPath).FullName)).ToString().Replace("/", "\\");
            if (parentItemPath != null)
                parentItemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(parentItemPath).FullName)).ToString().Replace("/", "\\");

            if (parentItemPath != null && !IsCore)
            {
                // ensure parent file is in msbuild project
                XmlNode parentNode = _doc.SelectSingleNode("//*[@Include='" + parentItemPath + "'][local-name()='None' or local-name()='Compile']");
                if (!IsCore && parentNode == null)
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


            if (IsCore)
            {
                //TODO: double check that file is under csproj folder?
            }

            // Checking the netcore "Compile Remove" nodes.
            if (IsCore)
            {
                XmlNode compileRemoveNode = _doc.SelectSingleNode("//msbuild:Compile[@Remove='" + itemPath + "']", _nsmgr);

                // Compiled files shouldn't have a "Compile Remove" node. "None" files should.
                if (itemType == MSBuildActionType.Compile)
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
                if (itemType == MSBuildActionType.NonProjectItem)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but wrong type
                if (!IsCore && itemType.ToString() != existingElement.Name)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but wrong type
                if (!IsCore && itemType.ToString() != existingElement.Name)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                // node exists but is not necessary
                if (IsCore && itemType == MSBuildActionType.Compile && parentItemPath == null)
                { existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }

                //// node exists shouldn't be there
                //if (IsCore && itemType == OutputFileType.Compile && parentItemPath != null 
                //    && existingElement.SelectSingleNode("[msbuild:DependentUpon/text()='" + parentItemPath + "'][@" + ((IsCore && itemType == OutputFileType.Compile) ? "Update" : "Include") + "='" + itemPath + "']", _nsmgr) != null)
                //{ existingElement.ParentNode.RemoveChild(existingElement); existingElement = null; }
                //_doc.SelectSingleNode("//msbuild:" + itemType.ToString() + 
            }

            if (itemType == MSBuildActionType.NonProjectItem)
                return;
            // core projects don't need to declare regular compiled files
            if (existingElement == null && (IsCore && itemType == MSBuildActionType.Compile) && parentItemPath == null)
                return;

            string expectedType = "Include";
            if (IsCore && itemType == MSBuildActionType.Compile)
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
                existingElement.SetAttribute(((IsCore && itemType == MSBuildActionType.Compile) ? "Update" : "Include"), itemPath);
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
            if (!IsCore && parentNode == null)
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

            if (!IsCore)
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

        public void Save()
        {
            _doc.Save(this._projectFileFullPath);
        }

    }
}
