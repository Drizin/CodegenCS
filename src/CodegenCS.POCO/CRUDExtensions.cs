using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public static class CRUDExtensions
    {

        #region Address
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Address e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.AddressId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Address e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[Address]
            (
                [AddressLine1],
                [AddressLine2],
                [City],
                [ModifiedDate],
                [PostalCode],
                [StateProvinceID]
            )
            VALUES
            (
                @AddressLine1,
                @AddressLine2,
                @City,
                @ModifiedDate,
                @PostalCode,
                @StateProvinceId
            )";

            e.AddressId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Address e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[Address] SET
                [AddressLine1] = @AddressLine1,
                [AddressLine2] = @AddressLine2,
                [City] = @City,
                [ModifiedDate] = @ModifiedDate,
                [PostalCode] = @PostalCode,
                [StateProvinceID] = @StateProvinceId
            WHERE
                [AddressID] = @AddressId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Address

        #region AddressType
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, AddressType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.AddressTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, AddressType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[AddressType]
            (
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @ModifiedDate,
                @Name
            )";

            e.AddressTypeId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, AddressType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[AddressType] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [AddressTypeID] = @AddressTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion AddressType

        #region AWBuildVersion
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, AWBuildVersion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SystemInformationId == default(byte))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, AWBuildVersion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [AWBuildVersion]
            (
                [Database Version],
                [ModifiedDate],
                [VersionDate]
            )
            VALUES
            (
                @DatabaseVersion,
                @ModifiedDate,
                @VersionDate
            )";

            e.SystemInformationId = conn.Query<byte>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, AWBuildVersion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [AWBuildVersion] SET
                [Database Version] = @DatabaseVersion,
                [ModifiedDate] = @ModifiedDate,
                [VersionDate] = @VersionDate
            WHERE
                [SystemInformationID] = @SystemInformationId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion AWBuildVersion

        #region BillOfMaterials
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, BillOfMaterials e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BillOfMaterialsId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, BillOfMaterials e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[BillOfMaterials]
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

            e.BillOfMaterialsId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, BillOfMaterials e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[BillOfMaterials] SET
                [BOMLevel] = @BomLevel,
                [ComponentID] = @ComponentId,
                [EndDate] = @EndDate,
                [ModifiedDate] = @ModifiedDate,
                [PerAssemblyQty] = @PerAssemblyQty,
                [ProductAssemblyID] = @ProductAssemblyId,
                [StartDate] = @StartDate,
                [UnitMeasureCode] = @UnitMeasureCode
            WHERE
                [BillOfMaterialsID] = @BillOfMaterialsId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion BillOfMaterials

        #region BusinessEntity
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, BusinessEntity e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, BusinessEntity e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[BusinessEntity]
            (
                [ModifiedDate]
            )
            VALUES
            (
                @ModifiedDate
            )";

            e.BusinessEntityId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, BusinessEntity e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[BusinessEntity] SET
                [ModifiedDate] = @ModifiedDate
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion BusinessEntity

        #region BusinessEntityAddress
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, BusinessEntityAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.AddressId == default(int) && e.AddressTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, BusinessEntityAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[BusinessEntityAddress]
            (
                [AddressID],
                [AddressTypeID],
                [BusinessEntityID],
                [ModifiedDate]
            )
            VALUES
            (
                @AddressId,
                @AddressTypeId,
                @BusinessEntityId,
                @ModifiedDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, BusinessEntityAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[BusinessEntityAddress] SET
                [AddressID] = @AddressId,
                [AddressTypeID] = @AddressTypeId,
                [BusinessEntityID] = @BusinessEntityId,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [AddressID] = @AddressId AND 
                [AddressTypeID] = @AddressTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion BusinessEntityAddress

        #region BusinessEntityContact
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, BusinessEntityContact e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.PersonId == default(int) && e.ContactTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, BusinessEntityContact e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[BusinessEntityContact]
            (
                [BusinessEntityID],
                [ContactTypeID],
                [ModifiedDate],
                [PersonID]
            )
            VALUES
            (
                @BusinessEntityId,
                @ContactTypeId,
                @ModifiedDate,
                @PersonId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, BusinessEntityContact e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[BusinessEntityContact] SET
                [BusinessEntityID] = @BusinessEntityId,
                [ContactTypeID] = @ContactTypeId,
                [ModifiedDate] = @ModifiedDate,
                [PersonID] = @PersonId
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [PersonID] = @PersonId AND 
                [ContactTypeID] = @ContactTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion BusinessEntityContact

        #region ContactType
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ContactType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ContactTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ContactType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[ContactType]
            (
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @ModifiedDate,
                @Name
            )";

            e.ContactTypeId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ContactType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[ContactType] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [ContactTypeID] = @ContactTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ContactType

        #region CountryRegion
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, CountryRegion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CountryRegionCode == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, CountryRegion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[CountryRegion]
            (
                [CountryRegionCode],
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @CountryRegionCode,
                @ModifiedDate,
                @Name
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, CountryRegion e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[CountryRegion] SET
                [CountryRegionCode] = @CountryRegionCode,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [CountryRegionCode] = @CountryRegionCode";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion CountryRegion

        #region CountryRegionCurrency
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, CountryRegionCurrency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CountryRegionCode == null && e.CurrencyCode == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, CountryRegionCurrency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[CountryRegionCurrency]
            (
                [CountryRegionCode],
                [CurrencyCode],
                [ModifiedDate]
            )
            VALUES
            (
                @CountryRegionCode,
                @CurrencyCode,
                @ModifiedDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, CountryRegionCurrency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[CountryRegionCurrency] SET
                [CountryRegionCode] = @CountryRegionCode,
                [CurrencyCode] = @CurrencyCode,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [CountryRegionCode] = @CountryRegionCode AND 
                [CurrencyCode] = @CurrencyCode";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion CountryRegionCurrency

        #region CreditCard
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, CreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CreditCardId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, CreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[CreditCard]
            (
                [CardNumber],
                [CardType],
                [ExpMonth],
                [ExpYear],
                [ModifiedDate]
            )
            VALUES
            (
                @CardNumber,
                @CardType,
                @ExpMonth,
                @ExpYear,
                @ModifiedDate
            )";

            e.CreditCardId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, CreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[CreditCard] SET
                [CardNumber] = @CardNumber,
                [CardType] = @CardType,
                [ExpMonth] = @ExpMonth,
                [ExpYear] = @ExpYear,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [CreditCardID] = @CreditCardId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion CreditCard

        #region Culture
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Culture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CultureId == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Culture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[Culture]
            (
                [CultureID],
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @CultureId,
                @ModifiedDate,
                @Name
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Culture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[Culture] SET
                [CultureID] = @CultureId,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [CultureID] = @CultureId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Culture

        #region Currency
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Currency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CurrencyCode == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Currency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[Currency]
            (
                [CurrencyCode],
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @CurrencyCode,
                @ModifiedDate,
                @Name
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Currency e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[Currency] SET
                [CurrencyCode] = @CurrencyCode,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [CurrencyCode] = @CurrencyCode";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Currency

        #region CurrencyRate
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, CurrencyRate e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CurrencyRateId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, CurrencyRate e, IDbTransaction transaction = null, int? commandTimeout = null)
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

            e.CurrencyRateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, CurrencyRate e, IDbTransaction transaction = null, int? commandTimeout = null)
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
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion CurrencyRate

        #region Customer
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Customer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.CustomerId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Customer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[Customer]
            (
                [ModifiedDate],
                [PersonID],
                [StoreID],
                [TerritoryID]
            )
            VALUES
            (
                @ModifiedDate,
                @PersonId,
                @StoreId,
                @TerritoryId
            )";

            e.CustomerId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Customer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[Customer] SET
                [ModifiedDate] = @ModifiedDate,
                [PersonID] = @PersonId,
                [StoreID] = @StoreId,
                [TerritoryID] = @TerritoryId
            WHERE
                [CustomerID] = @CustomerId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Customer

        #region DatabaseLog
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, DatabaseLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.DatabaseLogId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, DatabaseLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [DatabaseLog]
            (
                [DatabaseUser],
                [Event],
                [Object],
                [PostTime],
                [Schema],
                [TSQL],
                [XmlEvent]
            )
            VALUES
            (
                @DatabaseUser,
                @Event,
                @Object,
                @PostTime,
                @Schema,
                @Tsql,
                @XmlEvent
            )";

            e.DatabaseLogId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, DatabaseLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [DatabaseLog] SET
                [DatabaseUser] = @DatabaseUser,
                [Event] = @Event,
                [Object] = @Object,
                [PostTime] = @PostTime,
                [Schema] = @Schema,
                [TSQL] = @Tsql,
                [XmlEvent] = @XmlEvent
            WHERE
                [DatabaseLogID] = @DatabaseLogId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion DatabaseLog

        #region Department
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Department e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.DepartmentId == default(short))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Department e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [HumanResources].[Department]
            (
                [GroupName],
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @GroupName,
                @ModifiedDate,
                @Name
            )";

            e.DepartmentId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Department e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [HumanResources].[Department] SET
                [GroupName] = @GroupName,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [DepartmentID] = @DepartmentId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Department

        #region EmailAddress
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, EmailAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.EmailAddressId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, EmailAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[EmailAddress]
            (
                [BusinessEntityID],
                [EmailAddress],
                [ModifiedDate]
            )
            VALUES
            (
                @BusinessEntityId,
                @EmailAddress1,
                @ModifiedDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, EmailAddress e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[EmailAddress] SET
                [BusinessEntityID] = @BusinessEntityId,
                [EmailAddress] = @EmailAddress1,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [EmailAddressID] = @EmailAddressId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion EmailAddress

        #region Employee
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Employee e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Employee e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [HumanResources].[Employee]
            (
                [BirthDate],
                [BusinessEntityID],
                [CurrentFlag],
                [Gender],
                [HireDate],
                [JobTitle],
                [LoginID],
                [MaritalStatus],
                [ModifiedDate],
                [NationalIDNumber],
                [SalariedFlag],
                [SickLeaveHours],
                [VacationHours]
            )
            VALUES
            (
                @BirthDate,
                @BusinessEntityId,
                @CurrentFlag,
                @Gender,
                @HireDate,
                @JobTitle,
                @LoginId,
                @MaritalStatus,
                @ModifiedDate,
                @NationalIdNumber,
                @SalariedFlag,
                @SickLeaveHours,
                @VacationHours
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Employee e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [HumanResources].[Employee] SET
                [BirthDate] = @BirthDate,
                [BusinessEntityID] = @BusinessEntityId,
                [CurrentFlag] = @CurrentFlag,
                [Gender] = @Gender,
                [HireDate] = @HireDate,
                [JobTitle] = @JobTitle,
                [LoginID] = @LoginId,
                [MaritalStatus] = @MaritalStatus,
                [ModifiedDate] = @ModifiedDate,
                [NationalIDNumber] = @NationalIdNumber,
                [SalariedFlag] = @SalariedFlag,
                [SickLeaveHours] = @SickLeaveHours,
                [VacationHours] = @VacationHours
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Employee

        #region EmployeeDepartmentHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, EmployeeDepartmentHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.DepartmentId == default(short) && e.ShiftId == default(byte) && e.StartDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, EmployeeDepartmentHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [HumanResources].[EmployeeDepartmentHistory]
            (
                [BusinessEntityID],
                [DepartmentID],
                [EndDate],
                [ModifiedDate],
                [ShiftID],
                [StartDate]
            )
            VALUES
            (
                @BusinessEntityId,
                @DepartmentId,
                @EndDate,
                @ModifiedDate,
                @ShiftId,
                @StartDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, EmployeeDepartmentHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [HumanResources].[EmployeeDepartmentHistory] SET
                [BusinessEntityID] = @BusinessEntityId,
                [DepartmentID] = @DepartmentId,
                [EndDate] = @EndDate,
                [ModifiedDate] = @ModifiedDate,
                [ShiftID] = @ShiftId,
                [StartDate] = @StartDate
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [DepartmentID] = @DepartmentId AND 
                [ShiftID] = @ShiftId AND 
                [StartDate] = @StartDate";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion EmployeeDepartmentHistory

        #region EmployeePayHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, EmployeePayHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.RateChangeDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, EmployeePayHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
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

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, EmployeePayHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
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
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion EmployeePayHistory

        #region ErrorLog
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ErrorLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ErrorLogId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ErrorLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [ErrorLog]
            (
                [ErrorLine],
                [ErrorMessage],
                [ErrorNumber],
                [ErrorProcedure],
                [ErrorSeverity],
                [ErrorState],
                [ErrorTime],
                [UserName]
            )
            VALUES
            (
                @ErrorLine,
                @ErrorMessage,
                @ErrorNumber,
                @ErrorProcedure,
                @ErrorSeverity,
                @ErrorState,
                @ErrorTime,
                @UserName
            )";

            e.ErrorLogId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ErrorLog e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [ErrorLog] SET
                [ErrorLine] = @ErrorLine,
                [ErrorMessage] = @ErrorMessage,
                [ErrorNumber] = @ErrorNumber,
                [ErrorProcedure] = @ErrorProcedure,
                [ErrorSeverity] = @ErrorSeverity,
                [ErrorState] = @ErrorState,
                [ErrorTime] = @ErrorTime,
                [UserName] = @UserName
            WHERE
                [ErrorLogID] = @ErrorLogId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ErrorLog

        #region Illustration
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Illustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.IllustrationId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Illustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[Illustration]
            (
                [Diagram],
                [ModifiedDate]
            )
            VALUES
            (
                @Diagram,
                @ModifiedDate
            )";

            e.IllustrationId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Illustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[Illustration] SET
                [Diagram] = @Diagram,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [IllustrationID] = @IllustrationId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Illustration

        #region JobCandidate
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, JobCandidate e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.JobCandidateId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, JobCandidate e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [HumanResources].[JobCandidate]
            (
                [BusinessEntityID],
                [ModifiedDate],
                [Resume]
            )
            VALUES
            (
                @BusinessEntityId,
                @ModifiedDate,
                @Resume
            )";

            e.JobCandidateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, JobCandidate e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [HumanResources].[JobCandidate] SET
                [BusinessEntityID] = @BusinessEntityId,
                [ModifiedDate] = @ModifiedDate,
                [Resume] = @Resume
            WHERE
                [JobCandidateID] = @JobCandidateId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion JobCandidate

        #region Location
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Location e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.LocationId == default(short))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Location e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[Location]
            (
                [Availability],
                [CostRate],
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @Availability,
                @CostRate,
                @ModifiedDate,
                @Name
            )";

            e.LocationId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Location e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[Location] SET
                [Availability] = @Availability,
                [CostRate] = @CostRate,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [LocationID] = @LocationId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Location

        #region Password
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Password e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Password e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[Password]
            (
                [BusinessEntityID],
                [ModifiedDate],
                [PasswordHash],
                [PasswordSalt]
            )
            VALUES
            (
                @BusinessEntityId,
                @ModifiedDate,
                @PasswordHash,
                @PasswordSalt
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Password e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[Password] SET
                [BusinessEntityID] = @BusinessEntityId,
                [ModifiedDate] = @ModifiedDate,
                [PasswordHash] = @PasswordHash,
                [PasswordSalt] = @PasswordSalt
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Password

        #region Person
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Person e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Person e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[Person]
            (
                [AdditionalContactInfo],
                [BusinessEntityID],
                [Demographics],
                [EmailPromotion],
                [FirstName],
                [LastName],
                [MiddleName],
                [ModifiedDate],
                [NameStyle],
                [PersonType],
                [Suffix],
                [Title]
            )
            VALUES
            (
                @AdditionalContactInfo,
                @BusinessEntityId,
                @Demographics,
                @EmailPromotion,
                @FirstName,
                @LastName,
                @MiddleName,
                @ModifiedDate,
                @NameStyle,
                @PersonType,
                @Suffix,
                @Title
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Person e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[Person] SET
                [AdditionalContactInfo] = @AdditionalContactInfo,
                [BusinessEntityID] = @BusinessEntityId,
                [Demographics] = @Demographics,
                [EmailPromotion] = @EmailPromotion,
                [FirstName] = @FirstName,
                [LastName] = @LastName,
                [MiddleName] = @MiddleName,
                [ModifiedDate] = @ModifiedDate,
                [NameStyle] = @NameStyle,
                [PersonType] = @PersonType,
                [Suffix] = @Suffix,
                [Title] = @Title
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Person

        #region PersonCreditCard
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, PersonCreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.CreditCardId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, PersonCreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[PersonCreditCard]
            (
                [BusinessEntityID],
                [CreditCardID],
                [ModifiedDate]
            )
            VALUES
            (
                @BusinessEntityId,
                @CreditCardId,
                @ModifiedDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, PersonCreditCard e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[PersonCreditCard] SET
                [BusinessEntityID] = @BusinessEntityId,
                [CreditCardID] = @CreditCardId,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [CreditCardID] = @CreditCardId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion PersonCreditCard

        #region PersonPhone
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, PersonPhone e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.PhoneNumber == null && e.PhoneNumberTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, PersonPhone e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[PersonPhone]
            (
                [BusinessEntityID],
                [ModifiedDate],
                [PhoneNumber],
                [PhoneNumberTypeID]
            )
            VALUES
            (
                @BusinessEntityId,
                @ModifiedDate,
                @PhoneNumber,
                @PhoneNumberTypeId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, PersonPhone e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[PersonPhone] SET
                [BusinessEntityID] = @BusinessEntityId,
                [ModifiedDate] = @ModifiedDate,
                [PhoneNumber] = @PhoneNumber,
                [PhoneNumberTypeID] = @PhoneNumberTypeId
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [PhoneNumber] = @PhoneNumber AND 
                [PhoneNumberTypeID] = @PhoneNumberTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion PersonPhone

        #region PhoneNumberType
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, PhoneNumberType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.PhoneNumberTypeId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, PhoneNumberType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Person].[PhoneNumberType]
            (
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @ModifiedDate,
                @Name
            )";

            e.PhoneNumberTypeId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, PhoneNumberType e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Person].[PhoneNumberType] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [PhoneNumberTypeID] = @PhoneNumberTypeId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion PhoneNumberType

        #region Product
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Product e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Product e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[Product]
            (
                [Class],
                [Color],
                [DaysToManufacture],
                [DiscontinuedDate],
                [FinishedGoodsFlag],
                [ListPrice],
                [MakeFlag],
                [ModifiedDate],
                [Name],
                [ProductLine],
                [ProductModelID],
                [ProductNumber],
                [ProductSubcategoryID],
                [ReorderPoint],
                [SafetyStockLevel],
                [SellEndDate],
                [SellStartDate],
                [Size],
                [SizeUnitMeasureCode],
                [StandardCost],
                [Style],
                [Weight],
                [WeightUnitMeasureCode]
            )
            VALUES
            (
                @Class,
                @Color,
                @DaysToManufacture,
                @DiscontinuedDate,
                @FinishedGoodsFlag,
                @ListPrice,
                @MakeFlag,
                @ModifiedDate,
                @Name,
                @ProductLine,
                @ProductModelId,
                @ProductNumber,
                @ProductSubcategoryId,
                @ReorderPoint,
                @SafetyStockLevel,
                @SellEndDate,
                @SellStartDate,
                @Size,
                @SizeUnitMeasureCode,
                @StandardCost,
                @Style,
                @Weight,
                @WeightUnitMeasureCode
            )";

            e.ProductId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Product e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[Product] SET
                [Class] = @Class,
                [Color] = @Color,
                [DaysToManufacture] = @DaysToManufacture,
                [DiscontinuedDate] = @DiscontinuedDate,
                [FinishedGoodsFlag] = @FinishedGoodsFlag,
                [ListPrice] = @ListPrice,
                [MakeFlag] = @MakeFlag,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [ProductLine] = @ProductLine,
                [ProductModelID] = @ProductModelId,
                [ProductNumber] = @ProductNumber,
                [ProductSubcategoryID] = @ProductSubcategoryId,
                [ReorderPoint] = @ReorderPoint,
                [SafetyStockLevel] = @SafetyStockLevel,
                [SellEndDate] = @SellEndDate,
                [SellStartDate] = @SellStartDate,
                [Size] = @Size,
                [SizeUnitMeasureCode] = @SizeUnitMeasureCode,
                [StandardCost] = @StandardCost,
                [Style] = @Style,
                [Weight] = @Weight,
                [WeightUnitMeasureCode] = @WeightUnitMeasureCode
            WHERE
                [ProductID] = @ProductId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Product

        #region ProductCategory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductCategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductCategoryId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductCategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductCategory]
            (
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @ModifiedDate,
                @Name
            )";

            e.ProductCategoryId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductCategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductCategory] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [ProductCategoryID] = @ProductCategoryId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductCategory

        #region ProductCostHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductCostHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int) && e.StartDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductCostHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductCostHistory]
            (
                [EndDate],
                [ModifiedDate],
                [ProductID],
                [StandardCost],
                [StartDate]
            )
            VALUES
            (
                @EndDate,
                @ModifiedDate,
                @ProductId,
                @StandardCost,
                @StartDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductCostHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductCostHistory] SET
                [EndDate] = @EndDate,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [StandardCost] = @StandardCost,
                [StartDate] = @StartDate
            WHERE
                [ProductID] = @ProductId AND 
                [StartDate] = @StartDate";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductCostHistory

        #region ProductDescription
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductDescription e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductDescriptionId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductDescription e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductDescription]
            (
                [Description],
                [ModifiedDate]
            )
            VALUES
            (
                @Description,
                @ModifiedDate
            )";

            e.ProductDescriptionId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductDescription e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductDescription] SET
                [Description] = @Description,
                [ModifiedDate] = @ModifiedDate
            WHERE
                [ProductDescriptionID] = @ProductDescriptionId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductDescription

        #region ProductDocument
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductDocument e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductDocument e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductDocument]
            (
                [ModifiedDate],
                [ProductID]
            )
            VALUES
            (
                @ModifiedDate,
                @ProductId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductDocument e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductDocument] SET
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId
            WHERE
                [ProductID] = @ProductId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductDocument

        #region ProductInventory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductInventory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int) && e.LocationId == default(short))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductInventory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductInventory]
            (
                [Bin],
                [LocationID],
                [ModifiedDate],
                [ProductID],
                [Quantity],
                [Shelf]
            )
            VALUES
            (
                @Bin,
                @LocationId,
                @ModifiedDate,
                @ProductId,
                @Quantity,
                @Shelf
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductInventory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductInventory] SET
                [Bin] = @Bin,
                [LocationID] = @LocationId,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [Quantity] = @Quantity,
                [Shelf] = @Shelf
            WHERE
                [ProductID] = @ProductId AND 
                [LocationID] = @LocationId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductInventory

        #region ProductListPriceHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductListPriceHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int) && e.StartDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductListPriceHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductListPriceHistory]
            (
                [EndDate],
                [ListPrice],
                [ModifiedDate],
                [ProductID],
                [StartDate]
            )
            VALUES
            (
                @EndDate,
                @ListPrice,
                @ModifiedDate,
                @ProductId,
                @StartDate
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductListPriceHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductListPriceHistory] SET
                [EndDate] = @EndDate,
                [ListPrice] = @ListPrice,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [StartDate] = @StartDate
            WHERE
                [ProductID] = @ProductId AND 
                [StartDate] = @StartDate";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductListPriceHistory

        #region ProductModel
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductModel e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductModelId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductModel e, IDbTransaction transaction = null, int? commandTimeout = null)
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

            e.ProductModelId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductModel e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductModel] SET
                [CatalogDescription] = @CatalogDescription,
                [Instructions] = @Instructions,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [ProductModelID] = @ProductModelId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductModel

        #region ProductModelIllustration
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductModelIllustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductModelId == default(int) && e.IllustrationId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductModelIllustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductModelIllustration]
            (
                [IllustrationID],
                [ModifiedDate],
                [ProductModelID]
            )
            VALUES
            (
                @IllustrationId,
                @ModifiedDate,
                @ProductModelId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductModelIllustration e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductModelIllustration] SET
                [IllustrationID] = @IllustrationId,
                [ModifiedDate] = @ModifiedDate,
                [ProductModelID] = @ProductModelId
            WHERE
                [ProductModelID] = @ProductModelId AND 
                [IllustrationID] = @IllustrationId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductModelIllustration

        #region ProductModelProductDescriptionCulture
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductModelProductDescriptionCulture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductModelId == default(int) && e.ProductDescriptionId == default(int) && e.CultureId == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductModelProductDescriptionCulture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductModelProductDescriptionCulture]
            (
                [CultureID],
                [ModifiedDate],
                [ProductDescriptionID],
                [ProductModelID]
            )
            VALUES
            (
                @CultureId,
                @ModifiedDate,
                @ProductDescriptionId,
                @ProductModelId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductModelProductDescriptionCulture e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductModelProductDescriptionCulture] SET
                [CultureID] = @CultureId,
                [ModifiedDate] = @ModifiedDate,
                [ProductDescriptionID] = @ProductDescriptionId,
                [ProductModelID] = @ProductModelId
            WHERE
                [ProductModelID] = @ProductModelId AND 
                [ProductDescriptionID] = @ProductDescriptionId AND 
                [CultureID] = @CultureId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductModelProductDescriptionCulture

        #region ProductPhoto
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductPhotoId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductPhoto]
            (
                [LargePhoto],
                [LargePhotoFileName],
                [ModifiedDate],
                [ThumbNailPhoto],
                [ThumbnailPhotoFileName]
            )
            VALUES
            (
                @LargePhoto,
                @LargePhotoFileName,
                @ModifiedDate,
                @ThumbNailPhoto,
                @ThumbnailPhotoFileName
            )";

            e.ProductPhotoId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductPhoto] SET
                [LargePhoto] = @LargePhoto,
                [LargePhotoFileName] = @LargePhotoFileName,
                [ModifiedDate] = @ModifiedDate,
                [ThumbNailPhoto] = @ThumbNailPhoto,
                [ThumbnailPhotoFileName] = @ThumbnailPhotoFileName
            WHERE
                [ProductPhotoID] = @ProductPhotoId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductPhoto

        #region ProductProductPhoto
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int) && e.ProductPhotoId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductProductPhoto]
            (
                [ModifiedDate],
                [Primary],
                [ProductID],
                [ProductPhotoID]
            )
            VALUES
            (
                @ModifiedDate,
                @Primary,
                @ProductId,
                @ProductPhotoId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductProductPhoto e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductProductPhoto] SET
                [ModifiedDate] = @ModifiedDate,
                [Primary] = @Primary,
                [ProductID] = @ProductId,
                [ProductPhotoID] = @ProductPhotoId
            WHERE
                [ProductID] = @ProductId AND 
                [ProductPhotoID] = @ProductPhotoId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductProductPhoto

        #region ProductReview
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductReview e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductReviewId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductReview e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductReview]
            (
                [Comments],
                [EmailAddress],
                [ModifiedDate],
                [ProductID],
                [Rating],
                [ReviewDate],
                [ReviewerName]
            )
            VALUES
            (
                @Comments,
                @EmailAddress,
                @ModifiedDate,
                @ProductId,
                @Rating,
                @ReviewDate,
                @ReviewerName
            )";

            e.ProductReviewId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductReview e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductReview] SET
                [Comments] = @Comments,
                [EmailAddress] = @EmailAddress,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [Rating] = @Rating,
                [ReviewDate] = @ReviewDate,
                [ReviewerName] = @ReviewerName
            WHERE
                [ProductReviewID] = @ProductReviewId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductReview

        #region ProductSubcategory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductSubcategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductSubcategoryId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductSubcategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ProductSubcategory]
            (
                [ModifiedDate],
                [Name],
                [ProductCategoryID]
            )
            VALUES
            (
                @ModifiedDate,
                @Name,
                @ProductCategoryId
            )";

            e.ProductSubcategoryId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductSubcategory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ProductSubcategory] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [ProductCategoryID] = @ProductCategoryId
            WHERE
                [ProductSubcategoryID] = @ProductSubcategoryId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductSubcategory

        #region ProductVendor
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ProductVendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ProductId == default(int) && e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ProductVendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Purchasing].[ProductVendor]
            (
                [AverageLeadTime],
                [BusinessEntityID],
                [LastReceiptCost],
                [LastReceiptDate],
                [MaxOrderQty],
                [MinOrderQty],
                [ModifiedDate],
                [OnOrderQty],
                [ProductID],
                [StandardPrice],
                [UnitMeasureCode]
            )
            VALUES
            (
                @AverageLeadTime,
                @BusinessEntityId,
                @LastReceiptCost,
                @LastReceiptDate,
                @MaxOrderQty,
                @MinOrderQty,
                @ModifiedDate,
                @OnOrderQty,
                @ProductId,
                @StandardPrice,
                @UnitMeasureCode
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ProductVendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Purchasing].[ProductVendor] SET
                [AverageLeadTime] = @AverageLeadTime,
                [BusinessEntityID] = @BusinessEntityId,
                [LastReceiptCost] = @LastReceiptCost,
                [LastReceiptDate] = @LastReceiptDate,
                [MaxOrderQty] = @MaxOrderQty,
                [MinOrderQty] = @MinOrderQty,
                [ModifiedDate] = @ModifiedDate,
                [OnOrderQty] = @OnOrderQty,
                [ProductID] = @ProductId,
                [StandardPrice] = @StandardPrice,
                [UnitMeasureCode] = @UnitMeasureCode
            WHERE
                [ProductID] = @ProductId AND 
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ProductVendor

        #region PurchaseOrderDetail
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, PurchaseOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.PurchaseOrderId == default(int) && e.PurchaseOrderDetailId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, PurchaseOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Purchasing].[PurchaseOrderDetail]
            (
                [DueDate],
                [ModifiedDate],
                [OrderQty],
                [ProductID],
                [PurchaseOrderID],
                [ReceivedQty],
                [RejectedQty],
                [UnitPrice]
            )
            VALUES
            (
                @DueDate,
                @ModifiedDate,
                @OrderQty,
                @ProductId,
                @PurchaseOrderId,
                @ReceivedQty,
                @RejectedQty,
                @UnitPrice
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, PurchaseOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Purchasing].[PurchaseOrderDetail] SET
                [DueDate] = @DueDate,
                [ModifiedDate] = @ModifiedDate,
                [OrderQty] = @OrderQty,
                [ProductID] = @ProductId,
                [PurchaseOrderID] = @PurchaseOrderId,
                [ReceivedQty] = @ReceivedQty,
                [RejectedQty] = @RejectedQty,
                [UnitPrice] = @UnitPrice
            WHERE
                [PurchaseOrderID] = @PurchaseOrderId AND 
                [PurchaseOrderDetailID] = @PurchaseOrderDetailId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion PurchaseOrderDetail

        #region PurchaseOrderHeader
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, PurchaseOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.PurchaseOrderId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, PurchaseOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Purchasing].[PurchaseOrderHeader]
            (
                [EmployeeID],
                [Freight],
                [ModifiedDate],
                [OrderDate],
                [RevisionNumber],
                [ShipDate],
                [ShipMethodID],
                [Status],
                [SubTotal],
                [TaxAmt],
                [VendorID]
            )
            VALUES
            (
                @EmployeeId,
                @Freight,
                @ModifiedDate,
                @OrderDate,
                @RevisionNumber,
                @ShipDate,
                @ShipMethodId,
                @Status,
                @SubTotal,
                @TaxAmt,
                @VendorId
            )";

            e.PurchaseOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, PurchaseOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Purchasing].[PurchaseOrderHeader] SET
                [EmployeeID] = @EmployeeId,
                [Freight] = @Freight,
                [ModifiedDate] = @ModifiedDate,
                [OrderDate] = @OrderDate,
                [RevisionNumber] = @RevisionNumber,
                [ShipDate] = @ShipDate,
                [ShipMethodID] = @ShipMethodId,
                [Status] = @Status,
                [SubTotal] = @SubTotal,
                [TaxAmt] = @TaxAmt,
                [VendorID] = @VendorId
            WHERE
                [PurchaseOrderID] = @PurchaseOrderId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion PurchaseOrderHeader

        #region SalesOrderDetail
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SalesOrderId == default(int) && e.SalesOrderDetailId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesOrderDetail]
            (
                [CarrierTrackingNumber],
                [ModifiedDate],
                [OrderQty],
                [ProductID],
                [SalesOrderID],
                [SpecialOfferID],
                [UnitPrice],
                [UnitPriceDiscount]
            )
            VALUES
            (
                @CarrierTrackingNumber,
                @ModifiedDate,
                @OrderQty,
                @ProductId,
                @SalesOrderId,
                @SpecialOfferId,
                @UnitPrice,
                @UnitPriceDiscount
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesOrderDetail e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesOrderDetail] SET
                [CarrierTrackingNumber] = @CarrierTrackingNumber,
                [ModifiedDate] = @ModifiedDate,
                [OrderQty] = @OrderQty,
                [ProductID] = @ProductId,
                [SalesOrderID] = @SalesOrderId,
                [SpecialOfferID] = @SpecialOfferId,
                [UnitPrice] = @UnitPrice,
                [UnitPriceDiscount] = @UnitPriceDiscount
            WHERE
                [SalesOrderID] = @SalesOrderId AND 
                [SalesOrderDetailID] = @SalesOrderDetailId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesOrderDetail

        #region SalesOrderHeader
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SalesOrderId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesOrderHeader]
            (
                [AccountNumber],
                [BillToAddressID],
                [Comment],
                [CreditCardApprovalCode],
                [CreditCardID],
                [CurrencyRateID],
                [CustomerID],
                [DueDate],
                [Freight],
                [ModifiedDate],
                [OnlineOrderFlag],
                [OrderDate],
                [PurchaseOrderNumber],
                [RevisionNumber],
                [SalesPersonID],
                [ShipDate],
                [ShipMethodID],
                [ShipToAddressID],
                [Status],
                [SubTotal],
                [TaxAmt],
                [TerritoryID]
            )
            VALUES
            (
                @AccountNumber,
                @BillToAddressId,
                @Comment,
                @CreditCardApprovalCode,
                @CreditCardId,
                @CurrencyRateId,
                @CustomerId,
                @DueDate,
                @Freight,
                @ModifiedDate,
                @OnlineOrderFlag,
                @OrderDate,
                @PurchaseOrderNumber,
                @RevisionNumber,
                @SalesPersonId,
                @ShipDate,
                @ShipMethodId,
                @ShipToAddressId,
                @Status,
                @SubTotal,
                @TaxAmt,
                @TerritoryId
            )";

            e.SalesOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesOrderHeader e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesOrderHeader] SET
                [AccountNumber] = @AccountNumber,
                [BillToAddressID] = @BillToAddressId,
                [Comment] = @Comment,
                [CreditCardApprovalCode] = @CreditCardApprovalCode,
                [CreditCardID] = @CreditCardId,
                [CurrencyRateID] = @CurrencyRateId,
                [CustomerID] = @CustomerId,
                [DueDate] = @DueDate,
                [Freight] = @Freight,
                [ModifiedDate] = @ModifiedDate,
                [OnlineOrderFlag] = @OnlineOrderFlag,
                [OrderDate] = @OrderDate,
                [PurchaseOrderNumber] = @PurchaseOrderNumber,
                [RevisionNumber] = @RevisionNumber,
                [SalesPersonID] = @SalesPersonId,
                [ShipDate] = @ShipDate,
                [ShipMethodID] = @ShipMethodId,
                [ShipToAddressID] = @ShipToAddressId,
                [Status] = @Status,
                [SubTotal] = @SubTotal,
                [TaxAmt] = @TaxAmt,
                [TerritoryID] = @TerritoryId
            WHERE
                [SalesOrderID] = @SalesOrderId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesOrderHeader

        #region SalesOrderHeaderSalesReason
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesOrderHeaderSalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SalesOrderId == default(int) && e.SalesReasonId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesOrderHeaderSalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesOrderHeaderSalesReason]
            (
                [ModifiedDate],
                [SalesOrderID],
                [SalesReasonID]
            )
            VALUES
            (
                @ModifiedDate,
                @SalesOrderId,
                @SalesReasonId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesOrderHeaderSalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesOrderHeaderSalesReason] SET
                [ModifiedDate] = @ModifiedDate,
                [SalesOrderID] = @SalesOrderId,
                [SalesReasonID] = @SalesReasonId
            WHERE
                [SalesOrderID] = @SalesOrderId AND 
                [SalesReasonID] = @SalesReasonId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesOrderHeaderSalesReason

        #region SalesPerson
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesPerson e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesPerson e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesPerson]
            (
                [Bonus],
                [BusinessEntityID],
                [CommissionPct],
                [ModifiedDate],
                [SalesLastYear],
                [SalesQuota],
                [SalesYTD],
                [TerritoryID]
            )
            VALUES
            (
                @Bonus,
                @BusinessEntityId,
                @CommissionPct,
                @ModifiedDate,
                @SalesLastYear,
                @SalesQuota,
                @SalesYtd,
                @TerritoryId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesPerson e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesPerson] SET
                [Bonus] = @Bonus,
                [BusinessEntityID] = @BusinessEntityId,
                [CommissionPct] = @CommissionPct,
                [ModifiedDate] = @ModifiedDate,
                [SalesLastYear] = @SalesLastYear,
                [SalesQuota] = @SalesQuota,
                [SalesYTD] = @SalesYtd,
                [TerritoryID] = @TerritoryId
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesPerson

        #region SalesPersonQuotaHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesPersonQuotaHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.QuotaDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesPersonQuotaHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesPersonQuotaHistory]
            (
                [BusinessEntityID],
                [ModifiedDate],
                [QuotaDate],
                [SalesQuota]
            )
            VALUES
            (
                @BusinessEntityId,
                @ModifiedDate,
                @QuotaDate,
                @SalesQuota
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesPersonQuotaHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesPersonQuotaHistory] SET
                [BusinessEntityID] = @BusinessEntityId,
                [ModifiedDate] = @ModifiedDate,
                [QuotaDate] = @QuotaDate,
                [SalesQuota] = @SalesQuota
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [QuotaDate] = @QuotaDate";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesPersonQuotaHistory

        #region SalesReason
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SalesReasonId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesReason]
            (
                [ModifiedDate],
                [Name],
                [ReasonType]
            )
            VALUES
            (
                @ModifiedDate,
                @Name,
                @ReasonType
            )";

            e.SalesReasonId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesReason] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [ReasonType] = @ReasonType
            WHERE
                [SalesReasonID] = @SalesReasonId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesReason

        #region SalesTaxRate
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesTaxRate e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SalesTaxRateId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesTaxRate e, IDbTransaction transaction = null, int? commandTimeout = null)
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

            e.SalesTaxRateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesTaxRate e, IDbTransaction transaction = null, int? commandTimeout = null)
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
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesTaxRate

        #region SalesTerritory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesTerritory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.TerritoryId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesTerritory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesTerritory]
            (
                [CostLastYear],
                [CostYTD],
                [CountryRegionCode],
                [Group],
                [ModifiedDate],
                [Name],
                [SalesLastYear],
                [SalesYTD]
            )
            VALUES
            (
                @CostLastYear,
                @CostYtd,
                @CountryRegionCode,
                @Group,
                @ModifiedDate,
                @Name,
                @SalesLastYear,
                @SalesYtd
            )";

            e.TerritoryId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesTerritory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesTerritory] SET
                [CostLastYear] = @CostLastYear,
                [CostYTD] = @CostYtd,
                [CountryRegionCode] = @CountryRegionCode,
                [Group] = @Group,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [SalesLastYear] = @SalesLastYear,
                [SalesYTD] = @SalesYtd
            WHERE
                [TerritoryID] = @TerritoryId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesTerritory

        #region SalesTerritoryHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SalesTerritoryHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int) && e.TerritoryId == default(int) && e.StartDate == default(DateTime))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SalesTerritoryHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SalesTerritoryHistory]
            (
                [BusinessEntityID],
                [EndDate],
                [ModifiedDate],
                [StartDate],
                [TerritoryID]
            )
            VALUES
            (
                @BusinessEntityId,
                @EndDate,
                @ModifiedDate,
                @StartDate,
                @TerritoryId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SalesTerritoryHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SalesTerritoryHistory] SET
                [BusinessEntityID] = @BusinessEntityId,
                [EndDate] = @EndDate,
                [ModifiedDate] = @ModifiedDate,
                [StartDate] = @StartDate,
                [TerritoryID] = @TerritoryId
            WHERE
                [BusinessEntityID] = @BusinessEntityId AND 
                [TerritoryID] = @TerritoryId AND 
                [StartDate] = @StartDate";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SalesTerritoryHistory

        #region ScrapReason
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ScrapReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ScrapReasonId == default(short))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ScrapReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[ScrapReason]
            (
                [ModifiedDate],
                [Name]
            )
            VALUES
            (
                @ModifiedDate,
                @Name
            )";

            e.ScrapReasonId = conn.Query<short>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ScrapReason e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[ScrapReason] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name
            WHERE
                [ScrapReasonID] = @ScrapReasonId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ScrapReason

        #region Shift
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Shift e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ShiftId == default(byte))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Shift e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [HumanResources].[Shift]
            (
                [EndTime],
                [ModifiedDate],
                [Name],
                [StartTime]
            )
            VALUES
            (
                @EndTime,
                @ModifiedDate,
                @Name,
                @StartTime
            )";

            e.ShiftId = conn.Query<byte>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Shift e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [HumanResources].[Shift] SET
                [EndTime] = @EndTime,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [StartTime] = @StartTime
            WHERE
                [ShiftID] = @ShiftId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Shift

        #region ShipMethod
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ShipMethod e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ShipMethodId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ShipMethod e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Purchasing].[ShipMethod]
            (
                [ModifiedDate],
                [Name],
                [ShipBase],
                [ShipRate]
            )
            VALUES
            (
                @ModifiedDate,
                @Name,
                @ShipBase,
                @ShipRate
            )";

            e.ShipMethodId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ShipMethod e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Purchasing].[ShipMethod] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [ShipBase] = @ShipBase,
                [ShipRate] = @ShipRate
            WHERE
                [ShipMethodID] = @ShipMethodId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ShipMethod

        #region ShoppingCartItem
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, ShoppingCartItem e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.ShoppingCartItemId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, ShoppingCartItem e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[ShoppingCartItem]
            (
                [DateCreated],
                [ModifiedDate],
                [ProductID],
                [Quantity],
                [ShoppingCartID]
            )
            VALUES
            (
                @DateCreated,
                @ModifiedDate,
                @ProductId,
                @Quantity,
                @ShoppingCartId
            )";

            e.ShoppingCartItemId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, ShoppingCartItem e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[ShoppingCartItem] SET
                [DateCreated] = @DateCreated,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [Quantity] = @Quantity,
                [ShoppingCartID] = @ShoppingCartId
            WHERE
                [ShoppingCartItemID] = @ShoppingCartItemId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion ShoppingCartItem

        #region SpecialOffer
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SpecialOffer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SpecialOfferId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SpecialOffer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SpecialOffer]
            (
                [Category],
                [Description],
                [DiscountPct],
                [EndDate],
                [MaxQty],
                [MinQty],
                [ModifiedDate],
                [StartDate],
                [Type]
            )
            VALUES
            (
                @Category,
                @Description,
                @DiscountPct,
                @EndDate,
                @MaxQty,
                @MinQty,
                @ModifiedDate,
                @StartDate,
                @Type
            )";

            e.SpecialOfferId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SpecialOffer e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SpecialOffer] SET
                [Category] = @Category,
                [Description] = @Description,
                [DiscountPct] = @DiscountPct,
                [EndDate] = @EndDate,
                [MaxQty] = @MaxQty,
                [MinQty] = @MinQty,
                [ModifiedDate] = @ModifiedDate,
                [StartDate] = @StartDate,
                [Type] = @Type
            WHERE
                [SpecialOfferID] = @SpecialOfferId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SpecialOffer

        #region SpecialOfferProduct
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, SpecialOfferProduct e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.SpecialOfferId == default(int) && e.ProductId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, SpecialOfferProduct e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[SpecialOfferProduct]
            (
                [ModifiedDate],
                [ProductID],
                [SpecialOfferID]
            )
            VALUES
            (
                @ModifiedDate,
                @ProductId,
                @SpecialOfferId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, SpecialOfferProduct e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[SpecialOfferProduct] SET
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [SpecialOfferID] = @SpecialOfferId
            WHERE
                [SpecialOfferID] = @SpecialOfferId AND 
                [ProductID] = @ProductId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion SpecialOfferProduct

        #region StateProvince
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, StateProvince e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.StateProvinceId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, StateProvince e, IDbTransaction transaction = null, int? commandTimeout = null)
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

            e.StateProvinceId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, StateProvince e, IDbTransaction transaction = null, int? commandTimeout = null)
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
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion StateProvince

        #region Store
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Store e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Store e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Sales].[Store]
            (
                [BusinessEntityID],
                [Demographics],
                [ModifiedDate],
                [Name],
                [SalesPersonID]
            )
            VALUES
            (
                @BusinessEntityId,
                @Demographics,
                @ModifiedDate,
                @Name,
                @SalesPersonId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Store e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Sales].[Store] SET
                [BusinessEntityID] = @BusinessEntityId,
                [Demographics] = @Demographics,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [SalesPersonID] = @SalesPersonId
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Store

        #region TransactionHistory
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, TransactionHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.TransactionId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, TransactionHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[TransactionHistory]
            (
                [ActualCost],
                [ModifiedDate],
                [ProductID],
                [Quantity],
                [ReferenceOrderID],
                [ReferenceOrderLineID],
                [TransactionDate],
                [TransactionType]
            )
            VALUES
            (
                @ActualCost,
                @ModifiedDate,
                @ProductId,
                @Quantity,
                @ReferenceOrderId,
                @ReferenceOrderLineId,
                @TransactionDate,
                @TransactionType
            )";

            e.TransactionId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, TransactionHistory e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[TransactionHistory] SET
                [ActualCost] = @ActualCost,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [Quantity] = @Quantity,
                [ReferenceOrderID] = @ReferenceOrderId,
                [ReferenceOrderLineID] = @ReferenceOrderLineId,
                [TransactionDate] = @TransactionDate,
                [TransactionType] = @TransactionType
            WHERE
                [TransactionID] = @TransactionId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion TransactionHistory

        #region TransactionHistoryArchive
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, TransactionHistoryArchive e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.TransactionId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, TransactionHistoryArchive e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[TransactionHistoryArchive]
            (
                [ActualCost],
                [ModifiedDate],
                [ProductID],
                [Quantity],
                [ReferenceOrderID],
                [ReferenceOrderLineID],
                [TransactionDate],
                [TransactionID],
                [TransactionType]
            )
            VALUES
            (
                @ActualCost,
                @ModifiedDate,
                @ProductId,
                @Quantity,
                @ReferenceOrderId,
                @ReferenceOrderLineId,
                @TransactionDate,
                @TransactionId,
                @TransactionType
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, TransactionHistoryArchive e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[TransactionHistoryArchive] SET
                [ActualCost] = @ActualCost,
                [ModifiedDate] = @ModifiedDate,
                [ProductID] = @ProductId,
                [Quantity] = @Quantity,
                [ReferenceOrderID] = @ReferenceOrderId,
                [ReferenceOrderLineID] = @ReferenceOrderLineId,
                [TransactionDate] = @TransactionDate,
                [TransactionID] = @TransactionId,
                [TransactionType] = @TransactionType
            WHERE
                [TransactionID] = @TransactionId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion TransactionHistoryArchive

        #region UnitMeasure
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, UnitMeasure e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.UnitMeasureCode == null)
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, UnitMeasure e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[UnitMeasure]
            (
                [ModifiedDate],
                [Name],
                [UnitMeasureCode]
            )
            VALUES
            (
                @ModifiedDate,
                @Name,
                @UnitMeasureCode
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, UnitMeasure e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[UnitMeasure] SET
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [UnitMeasureCode] = @UnitMeasureCode
            WHERE
                [UnitMeasureCode] = @UnitMeasureCode";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion UnitMeasure

        #region Vendor
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Vendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.BusinessEntityId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, Vendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Purchasing].[Vendor]
            (
                [AccountNumber],
                [ActiveFlag],
                [BusinessEntityID],
                [CreditRating],
                [ModifiedDate],
                [Name],
                [PreferredVendorStatus],
                [PurchasingWebServiceURL]
            )
            VALUES
            (
                @AccountNumber,
                @ActiveFlag,
                @BusinessEntityId,
                @CreditRating,
                @ModifiedDate,
                @Name,
                @PreferredVendorStatus,
                @PurchasingWebServiceUrl
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, Vendor e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Purchasing].[Vendor] SET
                [AccountNumber] = @AccountNumber,
                [ActiveFlag] = @ActiveFlag,
                [BusinessEntityID] = @BusinessEntityId,
                [CreditRating] = @CreditRating,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [PreferredVendorStatus] = @PreferredVendorStatus,
                [PurchasingWebServiceURL] = @PurchasingWebServiceUrl
            WHERE
                [BusinessEntityID] = @BusinessEntityId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion Vendor

        #region WorkOrder
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, WorkOrder e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.WorkOrderId == default(int))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, WorkOrder e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[WorkOrder]
            (
                [DueDate],
                [EndDate],
                [ModifiedDate],
                [OrderQty],
                [ProductID],
                [ScrappedQty],
                [ScrapReasonID],
                [StartDate]
            )
            VALUES
            (
                @DueDate,
                @EndDate,
                @ModifiedDate,
                @OrderQty,
                @ProductId,
                @ScrappedQty,
                @ScrapReasonId,
                @StartDate
            )";

            e.WorkOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, WorkOrder e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[WorkOrder] SET
                [DueDate] = @DueDate,
                [EndDate] = @EndDate,
                [ModifiedDate] = @ModifiedDate,
                [OrderQty] = @OrderQty,
                [ProductID] = @ProductId,
                [ScrappedQty] = @ScrappedQty,
                [ScrapReasonID] = @ScrapReasonId,
                [StartDate] = @StartDate
            WHERE
                [WorkOrderID] = @WorkOrderId";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion WorkOrder

        #region WorkOrderRouting
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, WorkOrderRouting e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (e.WorkOrderId == default(int) && e.ProductId == default(int) && e.OperationSequence == default(short))
                conn.Insert(e, transaction, commandTimeout);
            else
                conn.Update(e, transaction, commandTimeout);
        }
        /// <summary>
        /// Saves new record
        /// </summary>
        public static void Insert(this IDbConnection conn, WorkOrderRouting e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            INSERT INTO [Production].[WorkOrderRouting]
            (
                [ActualCost],
                [ActualEndDate],
                [ActualResourceHrs],
                [ActualStartDate],
                [LocationID],
                [ModifiedDate],
                [OperationSequence],
                [PlannedCost],
                [ProductID],
                [ScheduledEndDate],
                [ScheduledStartDate],
                [WorkOrderID]
            )
            VALUES
            (
                @ActualCost,
                @ActualEndDate,
                @ActualResourceHrs,
                @ActualStartDate,
                @LocationId,
                @ModifiedDate,
                @OperationSequence,
                @PlannedCost,
                @ProductId,
                @ScheduledEndDate,
                @ScheduledStartDate,
                @WorkOrderId
            )";

            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        /// <summary>
        /// Updates existing record
        /// </summary>
        public static void Update(this IDbConnection conn, WorkOrderRouting e, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string cmd = @"
            UPDATE [Production].[WorkOrderRouting] SET
                [ActualCost] = @ActualCost,
                [ActualEndDate] = @ActualEndDate,
                [ActualResourceHrs] = @ActualResourceHrs,
                [ActualStartDate] = @ActualStartDate,
                [LocationID] = @LocationId,
                [ModifiedDate] = @ModifiedDate,
                [OperationSequence] = @OperationSequence,
                [PlannedCost] = @PlannedCost,
                [ProductID] = @ProductId,
                [ScheduledEndDate] = @ScheduledEndDate,
                [ScheduledStartDate] = @ScheduledStartDate,
                [WorkOrderID] = @WorkOrderId
            WHERE
                [WorkOrderID] = @WorkOrderId AND 
                [ProductID] = @ProductId AND 
                [OperationSequence] = @OperationSequence";
            conn.Execute(cmd, e, transaction, commandTimeout);

            e.MarkAsClean();
        }
        #endregion WorkOrderRouting
    }
}
