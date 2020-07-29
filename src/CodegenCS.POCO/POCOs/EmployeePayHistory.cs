using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("EmployeePayHistory", Schema = "HumanResources")]
    public partial class EmployeePayHistory : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private DateTime _rateChangeDate;
        [Key]
        public DateTime RateChangeDate 
        { 
            get { return _rateChangeDate; } 
            set { SetField(ref _rateChangeDate, value, nameof(RateChangeDate)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private byte _payFrequency;
        public byte PayFrequency 
        { 
            get { return _payFrequency; } 
            set { SetField(ref _payFrequency, value, nameof(PayFrequency)); } 
        }
        private decimal _rate;
        public decimal Rate 
        { 
            get { return _rate; } 
            set { SetField(ref _rate, value, nameof(Rate)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && RateChangeDate == default(DateTime))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [HumanResources].[EmployeePayHistory]
                (
                    [BusinessEntityID],
                    [ModifiedDate],
                    [PayFrequency],
                    [Rate],
                    [RateChangeDate]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ModifiedDate,
                    @PayFrequency,
                    @Rate,
                    @RateChangeDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[EmployeePayHistory] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate,
                    [PayFrequency] = @PayFrequency,
                    [Rate] = @Rate,
                    [RateChangeDate] = @RateChangeDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [RateChangeDate] = @RateChangeDate";
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
            EmployeePayHistory other = obj as EmployeePayHistory;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PayFrequency != other.PayFrequency)
                return false;
            if (Rate != other.Rate)
                return false;
            if (RateChangeDate != other.RateChangeDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PayFrequency == default(byte) ? 0 : PayFrequency.GetHashCode());
                hash = hash * 23 + (Rate == default(decimal) ? 0 : Rate.GetHashCode());
                hash = hash * 23 + (RateChangeDate == default(DateTime) ? 0 : RateChangeDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(EmployeePayHistory left, EmployeePayHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmployeePayHistory left, EmployeePayHistory right)
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
