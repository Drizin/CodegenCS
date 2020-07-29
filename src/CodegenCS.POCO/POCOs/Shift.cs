using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Shift", Schema = "HumanResources")]
    public partial class Shift : INotifyPropertyChanged
    {
        #region Members
        private byte _shiftId;
        [Key]
        public byte ShiftId 
        { 
            get { return _shiftId; } 
            set { SetField(ref _shiftId, value, nameof(ShiftId)); } 
        }
        private DateTime _endTime;
        public DateTime EndTime 
        { 
            get { return _endTime; } 
            set { SetField(ref _endTime, value, nameof(EndTime)); } 
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
        private DateTime _startTime;
        public DateTime StartTime 
        { 
            get { return _startTime; } 
            set { SetField(ref _startTime, value, nameof(StartTime)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ShiftId == default(byte))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [HumanResources].[Shift]
                (
                    [EndTime],
                    [ModifiedDate],
                    [Name],
                    [StartTime]
                )
                VALUES
                (
                    @EndTime,
                    @ModifiedDate,
                    @Name,
                    @StartTime
                )";

                this.ShiftId = conn.Query<byte>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[Shift] SET
                    [EndTime] = @EndTime,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [StartTime] = @StartTime
                WHERE
                    [ShiftID] = @ShiftId";
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
            Shift other = obj as Shift;
            if (other == null) return false;

            if (EndTime != other.EndTime)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (StartTime != other.StartTime)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (EndTime == default(DateTime) ? 0 : EndTime.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (StartTime == default(DateTime) ? 0 : StartTime.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Shift left, Shift right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Shift left, Shift right)
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
