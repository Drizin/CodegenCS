using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ProductModel
    {
        #region Members
        [Key]
        public int ProductModelId { get; set; }
        public string CatalogDescription { get; set; }
        public string Instructions { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductModelId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ProductModel]
                (
                    [CatalogDescription],
                    [Instructions],
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @CatalogDescription,
                    @Instructions,
                    @ModifiedDate,
                    @Name
                )";

                this.ProductModelId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ProductModel] SET
                    [CatalogDescription] = @CatalogDescription,
                    [Instructions] = @Instructions,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name
                WHERE
                    [ProductModelID] = @ProductModelId";
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
            ProductModel other = obj as ProductModel;
            if (other == null) return false;

            if (CatalogDescription != other.CatalogDescription)
                return false;
            if (Instructions != other.Instructions)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CatalogDescription == null ? 0 : CatalogDescription.GetHashCode());
                hash = hash * 23 + (Instructions == null ? 0 : Instructions.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductModel left, ProductModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductModel left, ProductModel right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
