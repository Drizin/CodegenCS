using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Department", Schema = "HumanResources")]
    public partial class Department : INotifyPropertyChanged
    {
        #region Members
        private short _departmentId;
        [Key]
        public short DepartmentId 
        { 
            get { return _departmentId; } 
            set { SetField(ref _departmentId, value, nameof(DepartmentId)); } 
        }
        private string _groupName;
        public string GroupName 
        { 
            get { return _groupName; } 
            set { SetField(ref _groupName, value, nameof(GroupName)); } 
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
            if (DepartmentId == default(short))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [HumanResources].[Department]
                (
                    [GroupName],
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @GroupName,
                    @ModifiedDate,
                    @Name
                )";

                this.DepartmentId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[Department] SET
                    [GroupName] = @GroupName,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name
                WHERE
                    [DepartmentID] = @DepartmentId";
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
            Department other = obj as Department;
            if (other == null) return false;

            if (GroupName != other.GroupName)
                return false;
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
                hash = hash * 23 + (GroupName == null ? 0 : GroupName.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Department left, Department right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Department left, Department right)
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
