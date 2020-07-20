using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Document", Schema = "Production")]
    public partial class Document
    {
        #region Members
        public int ChangeNumber { get; set; }
        [Column("Document")]
        public Byte[] Document1 { get; set; }
        public short? DocumentLevel { get; set; }
        public string DocumentSummary { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public bool FolderFlag { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int Owner { get; set; }
        public string Revision { get; set; }
        public Guid Rowguid { get; set; }
        public byte Status { get; set; }
        public string Title { get; set; }
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
    }
}
