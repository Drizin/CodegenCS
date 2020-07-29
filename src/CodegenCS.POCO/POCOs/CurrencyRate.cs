using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("CurrencyRate", Schema = "Sales")]
    public partial class CurrencyRate : INotifyPropertyChanged
    {
        #region Members
        private int _currencyRateId;
        [Key]
        public int CurrencyRateId 
        { 
            get { return _currencyRateId; } 
            set { SetField(ref _currencyRateId, value, nameof(CurrencyRateId)); } 
        }
        private decimal _averageRate;
        public decimal AverageRate 
        { 
            get { return _averageRate; } 
            set { SetField(ref _averageRate, value, nameof(AverageRate)); } 
        }
        private DateTime _currencyRateDate;
        public DateTime CurrencyRateDate 
        { 
            get { return _currencyRateDate; } 
            set { SetField(ref _currencyRateDate, value, nameof(CurrencyRateDate)); } 
        }
        private decimal _endOfDayRate;
        public decimal EndOfDayRate 
        { 
            get { return _endOfDayRate; } 
            set { SetField(ref _endOfDayRate, value, nameof(EndOfDayRate)); } 
        }
        private string _fromCurrencyCode;
        public string FromCurrencyCode 
        { 
            get { return _fromCurrencyCode; } 
            set { SetField(ref _fromCurrencyCode, value, nameof(FromCurrencyCode)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private string _toCurrencyCode;
        public string ToCurrencyCode 
        { 
            get { return _toCurrencyCode; } 
            set { SetField(ref _toCurrencyCode, value, nameof(ToCurrencyCode)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CurrencyRateId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[CurrencyRate]
                (
                    [AverageRate],
                    [CurrencyRateDate],
                    [EndOfDayRate],
                    [FromCurrencyCode],
                    [ModifiedDate],
                    [ToCurrencyCode]
                )
                VALUES
                (
                    @AverageRate,
                    @CurrencyRateDate,
                    @EndOfDayRate,
                    @FromCurrencyCode,
                    @ModifiedDate,
                    @ToCurrencyCode
                )";

                this.CurrencyRateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[CurrencyRate] SET
                    [AverageRate] = @AverageRate,
                    [CurrencyRateDate] = @CurrencyRateDate,
                    [EndOfDayRate] = @EndOfDayRate,
                    [FromCurrencyCode] = @FromCurrencyCode,
                    [ModifiedDate] = @ModifiedDate,
                    [ToCurrencyCode] = @ToCurrencyCode
                WHERE
                    [CurrencyRateID] = @CurrencyRateId";
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
            CurrencyRate other = obj as CurrencyRate;
            if (other == null) return false;

            if (AverageRate != other.AverageRate)
                return false;
            if (CurrencyRateDate != other.CurrencyRateDate)
                return false;
            if (EndOfDayRate != other.EndOfDayRate)
                return false;
            if (FromCurrencyCode != other.FromCurrencyCode)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (ToCurrencyCode != other.ToCurrencyCode)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AverageRate == default(decimal) ? 0 : AverageRate.GetHashCode());
                hash = hash * 23 + (CurrencyRateDate == default(DateTime) ? 0 : CurrencyRateDate.GetHashCode());
                hash = hash * 23 + (EndOfDayRate == default(decimal) ? 0 : EndOfDayRate.GetHashCode());
                hash = hash * 23 + (FromCurrencyCode == null ? 0 : FromCurrencyCode.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (ToCurrencyCode == null ? 0 : ToCurrencyCode.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(CurrencyRate left, CurrencyRate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CurrencyRate left, CurrencyRate right)
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
