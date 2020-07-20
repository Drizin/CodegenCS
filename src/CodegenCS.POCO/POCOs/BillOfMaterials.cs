using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class BillOfMaterials
    {
        #region Members
        [Key]
        public int BillOfMaterialsId { get; set; }
        public short BomLevel { get; set; }
        public int ComponentId { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal PerAssemblyQty { get; set; }
        public int? ProductAssemblyId { get; set; }
        public DateTime StartDate { get; set; }
        public string UnitMeasureCode { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BillOfMaterialsId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [BillOfMaterials]
                (
                    [BOMLevel],
                    [ComponentID],
                    [EndDate],
                    [ModifiedDate],
                    [PerAssemblyQty],
                    [ProductAssemblyID],
                    [StartDate],
                    [UnitMeasureCode]
                )
                VALUES
                (
                    @BomLevel,
                    @ComponentId,
                    @EndDate,
                    @ModifiedDate,
                    @PerAssemblyQty,
                    @ProductAssemblyId,
                    @StartDate,
                    @UnitMeasureCode
                )";

                this.BillOfMaterialsId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [BillOfMaterials]
                    SET [BOMLevel] = @BomLevel,
                    SET [ComponentID] = @ComponentId,
                    SET [EndDate] = @EndDate,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [PerAssemblyQty] = @PerAssemblyQty,
                    SET [ProductAssemblyID] = @ProductAssemblyId,
                    SET [StartDate] = @StartDate,
                    SET [UnitMeasureCode] = @UnitMeasureCode
                WHERE
                    [BillOfMaterialsID] = @BillOfMaterialsId";
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
            BillOfMaterials other = obj as BillOfMaterials;
            if (other == null) return false;

            if (BomLevel != other.BomLevel)
                return false;
            if (ComponentId != other.ComponentId)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PerAssemblyQty != other.PerAssemblyQty)
                return false;
            if (ProductAssemblyId != other.ProductAssemblyId)
                return false;
            if (StartDate != other.StartDate)
                return false;
            if (UnitMeasureCode != other.UnitMeasureCode)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BomLevel == default(short) ? 0 : BomLevel.GetHashCode());
                hash = hash * 23 + (ComponentId == default(int) ? 0 : ComponentId.GetHashCode());
                hash = hash * 23 + (EndDate == null ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PerAssemblyQty == default(decimal) ? 0 : PerAssemblyQty.GetHashCode());
                hash = hash * 23 + (ProductAssemblyId == null ? 0 : ProductAssemblyId.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                hash = hash * 23 + (UnitMeasureCode == null ? 0 : UnitMeasureCode.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(BillOfMaterials left, BillOfMaterials right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BillOfMaterials left, BillOfMaterials right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
