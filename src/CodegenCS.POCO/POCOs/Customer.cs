using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Customer", Schema = "Sales")]
    public partial class Customer : INotifyPropertyChanged
    {
        #region Members
        private int _customerId;
        [Key]
        public int CustomerId 
        { 
            get { return _customerId; } 
            set { SetField(ref _customerId, value, nameof(CustomerId)); } 
        }
        private string _accountNumber;
        public string AccountNumber 
        { 
            get { return _accountNumber; } 
            set { SetField(ref _accountNumber, value, nameof(AccountNumber)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private int? _personId;
        public int? PersonId 
        { 
            get { return _personId; } 
            set { SetField(ref _personId, value, nameof(PersonId)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private int? _storeId;
        public int? StoreId 
        { 
            get { return _storeId; } 
            set { SetField(ref _storeId, value, nameof(StoreId)); } 
        }
        private int? _territoryId;
        public int? TerritoryId 
        { 
            get { return _territoryId; } 
            set { SetField(ref _territoryId, value, nameof(TerritoryId)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CustomerId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[Customer]
                (
                    [ModifiedDate],
                    [PersonID],
                    [StoreID],
                    [TerritoryID]
                )
                VALUES
                (
                    @ModifiedDate,
                    @PersonId,
                    @StoreId,
                    @TerritoryId
                )";

                this.CustomerId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[Customer] SET
                    [ModifiedDate] = @ModifiedDate,
                    [PersonID] = @PersonId,
                    [StoreID] = @StoreId,
                    [TerritoryID] = @TerritoryId
                WHERE
                    [CustomerID] = @CustomerId";
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
            Customer other = obj as Customer;
            if (other == null) return false;

            if (AccountNumber != other.AccountNumber)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PersonId != other.PersonId)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StoreId != other.StoreId)
                return false;
            if (TerritoryId != other.TerritoryId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AccountNumber == null ? 0 : AccountNumber.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PersonId == null ? 0 : PersonId.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StoreId == null ? 0 : StoreId.GetHashCode());
                hash = hash * 23 + (TerritoryId == null ? 0 : TerritoryId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Customer left, Customer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Customer left, Customer right)
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
