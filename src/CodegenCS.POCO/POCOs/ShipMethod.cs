using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ShipMethod", Schema = "Purchasing")]
    public partial class ShipMethod : INotifyPropertyChanged
    {
        #region Members
        private int _shipMethodId;
        [Key]
        public int ShipMethodId 
        { 
            get { return _shipMethodId; } 
            set { SetField(ref _shipMethodId, value, nameof(ShipMethodId)); } 
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
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private decimal _shipBase;
        public decimal ShipBase 
        { 
            get { return _shipBase; } 
            set { SetField(ref _shipBase, value, nameof(ShipBase)); } 
        }
        private decimal _shipRate;
        public decimal ShipRate 
        { 
            get { return _shipRate; } 
            set { SetField(ref _shipRate, value, nameof(ShipRate)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ShipMethodId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Purchasing].[ShipMethod]
                (
                    [ModifiedDate],
                    [Name],
                    [ShipBase],
                    [ShipRate]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name,
                    @ShipBase,
                    @ShipRate
                )";

                this.ShipMethodId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Purchasing].[ShipMethod] SET
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [ShipBase] = @ShipBase,
                    [ShipRate] = @ShipRate
                WHERE
                    [ShipMethodID] = @ShipMethodId";
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
            ShipMethod other = obj as ShipMethod;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (ShipBase != other.ShipBase)
                return false;
            if (ShipRate != other.ShipRate)
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
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (ShipBase == default(decimal) ? 0 : ShipBase.GetHashCode());
                hash = hash * 23 + (ShipRate == default(decimal) ? 0 : ShipRate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ShipMethod left, ShipMethod right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ShipMethod left, ShipMethod right)
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
