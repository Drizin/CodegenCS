using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("CurrencyRate", Schema = "Sales")]
    public partial class CurrencyRate
    {
        #region Members
        [Key]
        public int CurrencyRateId { get; set; }
        public decimal AverageRate { get; set; }
        public DateTime CurrencyRateDate { get; set; }
        public decimal EndOfDayRate { get; set; }
        public string FromCurrencyCode { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ToCurrencyCode { get; set; }
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
    }
}
