using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesTaxRate", Schema = "Sales")]
    public partial class SalesTaxRate : INotifyPropertyChanged
    {
        #region Members
        private int _salesTaxRateId;
        [Key]
        public int SalesTaxRateId 
        { 
            get { return _salesTaxRateId; } 
            set { SetField(ref _salesTaxRateId, value, nameof(SalesTaxRateId)); } 
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
        private int _stateProvinceId;
        public int StateProvinceId 
        { 
            get { return _stateProvinceId; } 
            set { SetField(ref _stateProvinceId, value, nameof(StateProvinceId)); } 
        }
        private decimal _taxRate;
        public decimal TaxRate 
        { 
            get { return _taxRate; } 
            set { SetField(ref _taxRate, value, nameof(TaxRate)); } 
        }
        private byte _taxType;
        public byte TaxType 
        { 
            get { return _taxType; } 
            set { SetField(ref _taxType, value, nameof(TaxType)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SalesTaxRateId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesTaxRate]
                (
                    [ModifiedDate],
                    [Name],
                    [StateProvinceID],
                    [TaxRate],
                    [TaxType]
                )
                VALUES
                (
                    @ModifiedDate,
                    @Name,
                    @StateProvinceId,
                    @TaxRate,
                    @TaxType
                )";

                this.SalesTaxRateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesTaxRate] SET
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [StateProvinceID] = @StateProvinceId,
                    [TaxRate] = @TaxRate,
                    [TaxType] = @TaxType
                WHERE
                    [SalesTaxRateID] = @SalesTaxRateId";
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
            SalesTaxRate other = obj as SalesTaxRate;
            if (other == null) return false;

            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StateProvinceId != other.StateProvinceId)
                return false;
            if (TaxRate != other.TaxRate)
                return false;
            if (TaxType != other.TaxType)
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
                hash = hash * 23 + (StateProvinceId == default(int) ? 0 : StateProvinceId.GetHashCode());
                hash = hash * 23 + (TaxRate == default(decimal) ? 0 : TaxRate.GetHashCode());
                hash = hash * 23 + (TaxType == default(byte) ? 0 : TaxType.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesTaxRate left, SalesTaxRate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesTaxRate left, SalesTaxRate right)
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
