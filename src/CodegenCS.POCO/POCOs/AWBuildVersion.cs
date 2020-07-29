using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class AWBuildVersion : INotifyPropertyChanged
    {
        #region Members
        private byte _systemInformationId;
        [Key]
        public byte SystemInformationId 
        { 
            get { return _systemInformationId; } 
            set { SetField(ref _systemInformationId, value, nameof(SystemInformationId)); } 
        }
        private string _databaseVersion;
        [Column("Database Version")]
        public string DatabaseVersion 
        { 
            get { return _databaseVersion; } 
            set { SetField(ref _databaseVersion, value, nameof(DatabaseVersion)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private DateTime _versionDate;
        public DateTime VersionDate 
        { 
            get { return _versionDate; } 
            set { SetField(ref _versionDate, value, nameof(VersionDate)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SystemInformationId == default(byte))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [AWBuildVersion]
                (
                    [Database Version],
                    [ModifiedDate],
                    [VersionDate]
                )
                VALUES
                (
                    @DatabaseVersion,
                    @ModifiedDate,
                    @VersionDate
                )";

                this.SystemInformationId = conn.Query<byte>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [AWBuildVersion] SET
                    [Database Version] = @DatabaseVersion,
                    [ModifiedDate] = @ModifiedDate,
                    [VersionDate] = @VersionDate
                WHERE
                    [SystemInformationID] = @SystemInformationId";
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
            AWBuildVersion other = obj as AWBuildVersion;
            if (other == null) return false;

            if (DatabaseVersion != other.DatabaseVersion)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (VersionDate != other.VersionDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (DatabaseVersion == null ? 0 : DatabaseVersion.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (VersionDate == default(DateTime) ? 0 : VersionDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(AWBuildVersion left, AWBuildVersion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AWBuildVersion left, AWBuildVersion right)
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
