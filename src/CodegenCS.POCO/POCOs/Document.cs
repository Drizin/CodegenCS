﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Document", Schema = "Production")]
    public partial class Document : INotifyPropertyChanged
    {
        #region Members
        private int _changeNumber;
        public int ChangeNumber 
        { 
            get { return _changeNumber; } 
            set { SetField(ref _changeNumber, value, nameof(ChangeNumber)); } 
        }
        private Byte[] _document1;
        [Column("Document")]
        public Byte[] Document1 
        { 
            get { return _document1; } 
            set { SetField(ref _document1, value, nameof(Document1)); } 
        }
        private short? _documentLevel;
        public short? DocumentLevel 
        { 
            get { return _documentLevel; } 
            set { SetField(ref _documentLevel, value, nameof(DocumentLevel)); } 
        }
        private string _documentSummary;
        public string DocumentSummary 
        { 
            get { return _documentSummary; } 
            set { SetField(ref _documentSummary, value, nameof(DocumentSummary)); } 
        }
        private string _fileExtension;
        public string FileExtension 
        { 
            get { return _fileExtension; } 
            set { SetField(ref _fileExtension, value, nameof(FileExtension)); } 
        }
        private string _fileName;
        public string FileName 
        { 
            get { return _fileName; } 
            set { SetField(ref _fileName, value, nameof(FileName)); } 
        }
        private bool _folderFlag;
        public bool FolderFlag 
        { 
            get { return _folderFlag; } 
            set { SetField(ref _folderFlag, value, nameof(FolderFlag)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private int _owner;
        public int Owner 
        { 
            get { return _owner; } 
            set { SetField(ref _owner, value, nameof(Owner)); } 
        }
        private string _revision;
        public string Revision 
        { 
            get { return _revision; } 
            set { SetField(ref _revision, value, nameof(Revision)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private byte _status;
        public byte Status 
        { 
            get { return _status; } 
            set { SetField(ref _status, value, nameof(Status)); } 
        }
        private string _title;
        public string Title 
        { 
            get { return _title; } 
            set { SetField(ref _title, value, nameof(Title)); } 
        }
        #endregion Members

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            Document other = obj as Document;
            if (other == null) return false;

            if (ChangeNumber != other.ChangeNumber)
                return false;
            if (Document1 != other.Document1)
                return false;
            if (DocumentLevel != other.DocumentLevel)
                return false;
            if (DocumentSummary != other.DocumentSummary)
                return false;
            if (FileExtension != other.FileExtension)
                return false;
            if (FileName != other.FileName)
                return false;
            if (FolderFlag != other.FolderFlag)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Owner != other.Owner)
                return false;
            if (Revision != other.Revision)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (Status != other.Status)
                return false;
            if (Title != other.Title)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ChangeNumber == default(int) ? 0 : ChangeNumber.GetHashCode());
                hash = hash * 23 + (Document1 == null ? 0 : Document1.GetHashCode());
                hash = hash * 23 + (DocumentLevel == null ? 0 : DocumentLevel.GetHashCode());
                hash = hash * 23 + (DocumentSummary == null ? 0 : DocumentSummary.GetHashCode());
                hash = hash * 23 + (FileExtension == null ? 0 : FileExtension.GetHashCode());
                hash = hash * 23 + (FileName == null ? 0 : FileName.GetHashCode());
                hash = hash * 23 + (FolderFlag == default(bool) ? 0 : FolderFlag.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Owner == default(int) ? 0 : Owner.GetHashCode());
                hash = hash * 23 + (Revision == null ? 0 : Revision.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (Status == default(byte) ? 0 : Status.GetHashCode());
                hash = hash * 23 + (Title == null ? 0 : Title.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Document left, Document right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Document left, Document right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
