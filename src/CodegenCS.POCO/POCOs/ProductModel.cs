using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("ProductModel", Schema = "Production")]
    public partial class ProductModel : INotifyPropertyChanged
    {
        #region Members
        private int _productModelId;
        [Key]
        public int ProductModelId 
        { 
            get { return _productModelId; } 
            set { SetField(ref _productModelId, value, nameof(ProductModelId)); } 
        }
        private string _catalogDescription;
        public string CatalogDescription 
        { 
            get { return _catalogDescription; } 
            set { SetField(ref _catalogDescription, value, nameof(CatalogDescription)); } 
        }
        private string _instructions;
        public string Instructions 
        { 
            get { return _instructions; } 
            set { SetField(ref _instructions, value, nameof(Instructions)); } 
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
                INSERT INTO [Production].[ProductModel]
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
                UPDATE [Production].[ProductModel] SET
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
