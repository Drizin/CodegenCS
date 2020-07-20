using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesTaxRate", Schema = "Sales")]
    public partial class SalesTaxRate
    {
        #region Members
        [Key]
        public int SalesTaxRateId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public int StateProvinceId { get; set; }
        public decimal TaxRate { get; set; }
        public byte TaxType { get; set; }
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
    }
}
