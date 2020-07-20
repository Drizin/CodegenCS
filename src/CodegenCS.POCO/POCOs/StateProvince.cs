using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class StateProvince
    {
        #region Members
        [Key]
        public int StateProvinceId { get; set; }
        public string CountryRegionCode { get; set; }
        public bool IsOnlyStateProvinceFlag { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public string StateProvinceCode { get; set; }
        public int TerritoryId { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (StateProvinceId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [StateProvince]
                (
                    [CountryRegionCode],
                    [IsOnlyStateProvinceFlag],
                    [ModifiedDate],
                    [Name],
                    [StateProvinceCode],
                    [TerritoryID]
                )
                VALUES
                (
                    @CountryRegionCode,
                    @IsOnlyStateProvinceFlag,
                    @ModifiedDate,
                    @Name,
                    @StateProvinceCode,
                    @TerritoryId
                )";

                this.StateProvinceId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [StateProvince]
                    SET [CountryRegionCode] = @CountryRegionCode,
                    SET [IsOnlyStateProvinceFlag] = @IsOnlyStateProvinceFlag,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Name] = @Name,
                    SET [StateProvinceCode] = @StateProvinceCode,
                    SET [TerritoryID] = @TerritoryId
                WHERE
                    [StateProvinceID] = @StateProvinceId";
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
            StateProvince other = obj as StateProvince;
            if (other == null) return false;

            if (CountryRegionCode != other.CountryRegionCode)
                return false;
            if (IsOnlyStateProvinceFlag != other.IsOnlyStateProvinceFlag)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StateProvinceCode != other.StateProvinceCode)
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
                hash = hash * 23 + (CountryRegionCode == null ? 0 : CountryRegionCode.GetHashCode());
                hash = hash * 23 + (IsOnlyStateProvinceFlag == default(bool) ? 0 : IsOnlyStateProvinceFlag.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StateProvinceCode == null ? 0 : StateProvinceCode.GetHashCode());
                hash = hash * 23 + (TerritoryId == default(int) ? 0 : TerritoryId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(StateProvince left, StateProvince right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StateProvince left, StateProvince right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
