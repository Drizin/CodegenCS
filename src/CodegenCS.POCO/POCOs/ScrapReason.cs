using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ScrapReason", Schema = "Production")]
    public partial class ScrapReason : INotifyPropertyChanged
    {
        #region Members
        private short _scrapReasonId;
        [Key]
        public short ScrapReasonId 
        { 
            get { return _scrapReasonId; } 
            set { SetField(ref _scrapReasonId, value, nameof(ScrapReasonId)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private string _name;
        public string Name 
        { 
            get { return _name; } 
            set { SetField(ref _name, value, nameof(Name)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ScrapReasonId == default(short))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[ScrapReason]
                (
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name
                )";

                this.ScrapReasonId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[ScrapReason] SET
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name
                WHERE
                    [ScrapReasonID] = @ScrapReasonId";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

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
            ScrapReason other = obj as ScrapReason;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ScrapReason left, ScrapReason right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScrapReason left, ScrapReason right)
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
