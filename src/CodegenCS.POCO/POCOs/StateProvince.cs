﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("StateProvince", Schema = "Person")]
    public partial class StateProvince : INotifyPropertyChanged
    {
        #region Members
        private int _stateProvinceId;
        [Key]
        public int StateProvinceId 
        { 
            get { return _stateProvinceId; } 
            set { SetField(ref _stateProvinceId, value, nameof(StateProvinceId)); } 
        }
        private string _countryRegionCode;
        public string CountryRegionCode 
        { 
            get { return _countryRegionCode; } 
            set { SetField(ref _countryRegionCode, value, nameof(CountryRegionCode)); } 
        }
        private bool _isOnlyStateProvinceFlag;
        public bool IsOnlyStateProvinceFlag 
        { 
            get { return _isOnlyStateProvinceFlag; } 
            set { SetField(ref _isOnlyStateProvinceFlag, value, nameof(IsOnlyStateProvinceFlag)); } 
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
        private string _stateProvinceCode;
        public string StateProvinceCode 
        { 
            get { return _stateProvinceCode; } 
            set { SetField(ref _stateProvinceCode, value, nameof(StateProvinceCode)); } 
        }
        private int _territoryId;
        public int TerritoryId 
        { 
            get { return _territoryId; } 
            set { SetField(ref _territoryId, value, nameof(TerritoryId)); } 
        }
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
                INSERT INTO [Person].[StateProvince]
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
                UPDATE [Person].[StateProvince] SET
                    [CountryRegionCode] = @CountryRegionCode,
                    [IsOnlyStateProvinceFlag] = @IsOnlyStateProvinceFlag,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [StateProvinceCode] = @StateProvinceCode,
                    [TerritoryID] = @TerritoryId
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
