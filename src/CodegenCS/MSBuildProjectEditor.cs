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

        public Dictionary<string, List<string>> _addedDependentItems = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);



        /// <summary>
        /// Adds a single item, optionally dependent of a parent item (DependentUpon). 
        /// </summary>
        public void AddItem(string itemPath, string parentItemPath = null, OutputFileType itemType = OutputFileType.Compile)
        {
            if (itemType == OutputFileType.NonProjectItem)
                return;

            itemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(itemPath).FullName)).ToString().Replace("/", "\\");

            if (IsCore && parentItemPath == null) // no need to add files, should be automatically under csproj folder
            {
                //TODO: double check that file is under csproj folder?
                return;
            }

            XmlNode itemGroup = null;
            if (parentItemPath != null)
            {
                parentItemPath = new Uri(this._projectFileFullPath).MakeRelativeUri(new Uri(new FileInfo(parentItemPath).FullName)).ToString().Replace("/", "\\");
                if (!_addedDependentItems.ContainsKey(parentItemPath))
                    _addedDependentItems[parentItemPath] = new List<string>();
                _addedDependentItems[parentItemPath].Add(itemPath);

                if (!IsCore)
                {
                    XmlNode parentNode = _doc.SelectSingleNode("//*[@Include='" + parentItemPath + "'][local-name()='None' or local-name()='Compile']");
                    if (!IsCore && parentNode == null)
                        throw new Exception($"Parent item not found in project file {this._projectFileFullPath}");
                    itemGroup = parentNode.ParentNode;
                }
            }
            if (itemGroup == null)
                itemGroup = _doc.SelectSingleNode("//msbuild:ItemGroup", _nsmgr);


            // If there's the exact node we want, skip it
            if (parentItemPath != null && _doc.SelectSingleNode("//msbuild:" + itemType.ToString() + "[msbuild:DependentUpon/text()='" + parentItemPath + "'][@" + (IsCore?"Update":"Include") + "='" + itemPath + "']", _nsmgr) != null)
                return;

            if (parentItemPath == null && _doc.SelectSingleNode("//msbuild:" + itemType.ToString() + "[@Include='" + itemPath + "']", _nsmgr) != null) // IsCore==false
                return;

            // If there's same file, maybe other type or maybe other dependency, remove it
            XmlNode existingNode = _doc.SelectSingleNode("//*[local-name()='None' or local-name()='Compile'][@" + (IsCore ? "Update" : "Include") + "='" + itemPath + "']", _nsmgr);
            if (existingNode != null)
                existingNode.ParentNode.RemoveChild(existingNode);

            XmlElement newElement = _doc.CreateElement(itemType.ToString(), _doc.DocumentElement.NamespaceURI);
            if (IsCore)
            {
                //TODO: double check that file is under csproj folder?
                newElement.SetAttribute("Update", itemPath);
            }
            else
            {
                newElement.SetAttribute((IsCore ? "Update" : "Include"), itemPath);
            }

            if (parentItemPath != null)
            {
                XmlElement dependentUpon = _doc.CreateElement("DependentUpon", _doc.DocumentElement.NamespaceURI);
                dependentUpon.InnerText = parentItemPath;
                newElement.AppendChild(dependentUpon);
            }
            itemGroup.AppendChild(newElement);
        }

        /// <summary>
        /// Given an item in the project, will remove all items which depende on this parent item, except the items which were added using AddItem
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

        public void Save()
        {
            _doc.Save(this._projectFileFullPath);
        }

    }
}
