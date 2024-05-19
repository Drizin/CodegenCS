namespace MyNamespace
{
    /// <summary>
    /// POCO for AWBuildVersion
    /// </summary>
    public class AWBuildVersion
    {
        // class members...
        public System.Byte SystemInformationID { get; set; }
        public System.String Database Version { get; set; }
        public System.DateTime VersionDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for DatabaseLog
    /// </summary>
    public class DatabaseLog
    {
        // class members...
        public System.Int32 DatabaseLogID { get; set; }
        public System.DateTime PostTime { get; set; }
        public System.String DatabaseUser { get; set; }
        public System.String Event { get; set; }
        public System.String Schema { get; set; }
        public System.String Object { get; set; }
        public System.String TSQL { get; set; }
        public System.String XmlEvent { get; set; }
    }

    /// <summary>
    /// POCO for ErrorLog
    /// </summary>
    public class ErrorLog
    {
        // class members...
        public System.Int32 ErrorLogID { get; set; }
        public System.DateTime ErrorTime { get; set; }
        public System.String UserName { get; set; }
        public System.Int32 ErrorNumber { get; set; }
        public System.Int32 ErrorSeverity { get; set; }
        public System.Int32 ErrorState { get; set; }
        public System.String ErrorProcedure { get; set; }
        public System.Int32 ErrorLine { get; set; }
        public System.String ErrorMessage { get; set; }
    }

    /// <summary>
    /// POCO for Department
    /// </summary>
    public class Department
    {
        // class members...
        public System.Int16 DepartmentID { get; set; }
        public System.String Name { get; set; }
        public System.String GroupName { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Employee
    /// </summary>
    public class Employee
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String NationalIDNumber { get; set; }
        public System.String LoginID { get; set; }
        public Microsoft.SqlServer.Types.SqlHierarchyId OrganizationNode { get; set; }
        public System.Int16 OrganizationLevel { get; set; }
        public System.String JobTitle { get; set; }
        public System.DateTime BirthDate { get; set; }
        public System.String MaritalStatus { get; set; }
        public System.String Gender { get; set; }
        public System.DateTime HireDate { get; set; }
        public System.Boolean SalariedFlag { get; set; }
        public System.Int16 VacationHours { get; set; }
        public System.Int16 SickLeaveHours { get; set; }
        public System.Boolean CurrentFlag { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for EmployeeDepartmentHistory
    /// </summary>
    public class EmployeeDepartmentHistory
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int16 DepartmentID { get; set; }
        public System.Byte ShiftID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for EmployeePayHistory
    /// </summary>
    public class EmployeePayHistory
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.DateTime RateChangeDate { get; set; }
        public System.Decimal Rate { get; set; }
        public System.Byte PayFrequency { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for JobCandidate
    /// </summary>
    public class JobCandidate
    {
        // class members...
        public System.Int32 JobCandidateID { get; set; }
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Resume { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Shift
    /// </summary>
    public class Shift
    {
        // class members...
        public System.Byte ShiftID { get; set; }
        public System.String Name { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vEmployee
    /// </summary>
    public class vEmployee
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String JobTitle { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.String PhoneNumberType { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 EmailPromotion { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegionName { get; set; }
        public System.String AdditionalContactInfo { get; set; }
    }

    /// <summary>
    /// POCO for vEmployeeDepartment
    /// </summary>
    public class vEmployeeDepartment
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String JobTitle { get; set; }
        public System.String Department { get; set; }
        public System.String GroupName { get; set; }
        public System.DateTime StartDate { get; set; }
    }

    /// <summary>
    /// POCO for vEmployeeDepartmentHistory
    /// </summary>
    public class vEmployeeDepartmentHistory
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String Shift { get; set; }
        public System.String Department { get; set; }
        public System.String GroupName { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    }

    /// <summary>
    /// POCO for vJobCandidate
    /// </summary>
    public class vJobCandidate
    {
        // class members...
        public System.Int32 JobCandidateID { get; set; }
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name.Prefix { get; set; }
        public System.String Name.First { get; set; }
        public System.String Name.Middle { get; set; }
        public System.String Name.Last { get; set; }
        public System.String Name.Suffix { get; set; }
        public System.String Skills { get; set; }
        public System.String Addr.Type { get; set; }
        public System.String Addr.Loc.CountryRegion { get; set; }
        public System.String Addr.Loc.State { get; set; }
        public System.String Addr.Loc.City { get; set; }
        public System.String Addr.PostalCode { get; set; }
        public System.String EMail { get; set; }
        public System.String WebSite { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vJobCandidateEducation
    /// </summary>
    public class vJobCandidateEducation
    {
        // class members...
        public System.Int32 JobCandidateID { get; set; }
        public System.String Edu.Level { get; set; }
        public System.DateTime Edu.StartDate { get; set; }
        public System.DateTime Edu.EndDate { get; set; }
        public System.String Edu.Degree { get; set; }
        public System.String Edu.Major { get; set; }
        public System.String Edu.Minor { get; set; }
        public System.String Edu.GPA { get; set; }
        public System.String Edu.GPAScale { get; set; }
        public System.String Edu.School { get; set; }
        public System.String Edu.Loc.CountryRegion { get; set; }
        public System.String Edu.Loc.State { get; set; }
        public System.String Edu.Loc.City { get; set; }
    }

    /// <summary>
    /// POCO for vJobCandidateEmployment
    /// </summary>
    public class vJobCandidateEmployment
    {
        // class members...
        public System.Int32 JobCandidateID { get; set; }
        public System.DateTime Emp.StartDate { get; set; }
        public System.DateTime Emp.EndDate { get; set; }
        public System.String Emp.OrgName { get; set; }
        public System.String Emp.JobTitle { get; set; }
        public System.String Emp.Responsibility { get; set; }
        public System.String Emp.FunctionCategory { get; set; }
        public System.String Emp.IndustryCategory { get; set; }
        public System.String Emp.Loc.CountryRegion { get; set; }
        public System.String Emp.Loc.State { get; set; }
        public System.String Emp.Loc.City { get; set; }
    }

    /// <summary>
    /// POCO for Address
    /// </summary>
    public class Address
    {
        // class members...
        public System.Int32 AddressID { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.Int32 StateProvinceID { get; set; }
        public System.String PostalCode { get; set; }
        public Microsoft.SqlServer.Types.SqlGeography SpatialLocation { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for AddressType
    /// </summary>
    public class AddressType
    {
        // class members...
        public System.Int32 AddressTypeID { get; set; }
        public System.String Name { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for BusinessEntity
    /// </summary>
    public class BusinessEntity
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for BusinessEntityAddress
    /// </summary>
    public class BusinessEntityAddress
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 AddressID { get; set; }
        public System.Int32 AddressTypeID { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for BusinessEntityContact
    /// </summary>
    public class BusinessEntityContact
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 PersonID { get; set; }
        public System.Int32 ContactTypeID { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ContactType
    /// </summary>
    public class ContactType
    {
        // class members...
        public System.Int32 ContactTypeID { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for CountryRegion
    /// </summary>
    public class CountryRegion
    {
        // class members...
        public System.String CountryRegionCode { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for EmailAddress
    /// </summary>
    public class EmailAddress
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 EmailAddressID { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Password
    /// </summary>
    public class Password
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String PasswordHash { get; set; }
        public System.String PasswordSalt { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Person
    /// </summary>
    public class Person
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String PersonType { get; set; }
        public System.Boolean NameStyle { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.Int32 EmailPromotion { get; set; }
        public System.String AdditionalContactInfo { get; set; }
        public System.String Demographics { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for PersonPhone
    /// </summary>
    public class PersonPhone
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.Int32 PhoneNumberTypeID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for PhoneNumberType
    /// </summary>
    public class PhoneNumberType
    {
        // class members...
        public System.Int32 PhoneNumberTypeID { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for StateProvince
    /// </summary>
    public class StateProvince
    {
        // class members...
        public System.Int32 StateProvinceID { get; set; }
        public System.String StateProvinceCode { get; set; }
        public System.String CountryRegionCode { get; set; }
        public System.Boolean IsOnlyStateProvinceFlag { get; set; }
        public System.String Name { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vAdditionalContactInfo
    /// </summary>
    public class vAdditionalContactInfo
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String TelephoneNumber { get; set; }
        public System.String TelephoneSpecialInstructions { get; set; }
        public System.String Street { get; set; }
        public System.String City { get; set; }
        public System.String StateProvince { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegion { get; set; }
        public System.String HomeAddressSpecialInstructions { get; set; }
        public System.String EMailAddress { get; set; }
        public System.String EMailSpecialInstructions { get; set; }
        public System.String EMailTelephoneNumber { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vStateProvinceCountryRegion
    /// </summary>
    public class vStateProvinceCountryRegion
    {
        // class members...
        public System.Int32 StateProvinceID { get; set; }
        public System.String StateProvinceCode { get; set; }
        public System.Boolean IsOnlyStateProvinceFlag { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.String CountryRegionCode { get; set; }
        public System.String CountryRegionName { get; set; }
    }

    /// <summary>
    /// POCO for BillOfMaterials
    /// </summary>
    public class BillOfMaterials
    {
        // class members...
        public System.Int32 BillOfMaterialsID { get; set; }
        public System.Int32 ProductAssemblyID { get; set; }
        public System.Int32 ComponentID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.String UnitMeasureCode { get; set; }
        public System.Int16 BOMLevel { get; set; }
        public System.Decimal PerAssemblyQty { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Culture
    /// </summary>
    public class Culture
    {
        // class members...
        public System.String CultureID { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Document
    /// </summary>
    public class Document
    {
        // class members...
        public Microsoft.SqlServer.Types.SqlHierarchyId DocumentNode { get; set; }
        public System.Int16 DocumentLevel { get; set; }
        public System.String Title { get; set; }
        public System.Int32 Owner { get; set; }
        public System.Boolean FolderFlag { get; set; }
        public System.String FileName { get; set; }
        public System.String FileExtension { get; set; }
        public System.String Revision { get; set; }
        public System.Int32 ChangeNumber { get; set; }
        public System.Byte Status { get; set; }
        public System.String DocumentSummary { get; set; }
        public System.Byte[] Document { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Illustration
    /// </summary>
    public class Illustration
    {
        // class members...
        public System.Int32 IllustrationID { get; set; }
        public System.String Diagram { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Location
    /// </summary>
    public class Location
    {
        // class members...
        public System.Int16 LocationID { get; set; }
        public System.String Name { get; set; }
        public System.Decimal CostRate { get; set; }
        public System.Decimal Availability { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Product
    /// </summary>
    public class Product
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.String Name { get; set; }
        public System.String ProductNumber { get; set; }
        public System.Boolean MakeFlag { get; set; }
        public System.Boolean FinishedGoodsFlag { get; set; }
        public System.String Color { get; set; }
        public System.Int16 SafetyStockLevel { get; set; }
        public System.Int16 ReorderPoint { get; set; }
        public System.Decimal StandardCost { get; set; }
        public System.Decimal ListPrice { get; set; }
        public System.String Size { get; set; }
        public System.String SizeUnitMeasureCode { get; set; }
        public System.String WeightUnitMeasureCode { get; set; }
        public System.Decimal Weight { get; set; }
        public System.Int32 DaysToManufacture { get; set; }
        public System.String ProductLine { get; set; }
        public System.String Class { get; set; }
        public System.String Style { get; set; }
        public System.Int32 ProductSubcategoryID { get; set; }
        public System.Int32 ProductModelID { get; set; }
        public System.DateTime SellStartDate { get; set; }
        public System.DateTime SellEndDate { get; set; }
        public System.DateTime DiscontinuedDate { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductCategory
    /// </summary>
    public class ProductCategory
    {
        // class members...
        public System.Int32 ProductCategoryID { get; set; }
        public System.String Name { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductCostHistory
    /// </summary>
    public class ProductCostHistory
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.Decimal StandardCost { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductDescription
    /// </summary>
    public class ProductDescription
    {
        // class members...
        public System.Int32 ProductDescriptionID { get; set; }
        public System.String Description { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductDocument
    /// </summary>
    public class ProductDocument
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public Microsoft.SqlServer.Types.SqlHierarchyId DocumentNode { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductInventory
    /// </summary>
    public class ProductInventory
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.Int16 LocationID { get; set; }
        public System.String Shelf { get; set; }
        public System.Byte Bin { get; set; }
        public System.Int16 Quantity { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductListPriceHistory
    /// </summary>
    public class ProductListPriceHistory
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.Decimal ListPrice { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductModel
    /// </summary>
    public class ProductModel
    {
        // class members...
        public System.Int32 ProductModelID { get; set; }
        public System.String Name { get; set; }
        public System.String CatalogDescription { get; set; }
        public System.String Instructions { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductModelIllustration
    /// </summary>
    public class ProductModelIllustration
    {
        // class members...
        public System.Int32 ProductModelID { get; set; }
        public System.Int32 IllustrationID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductModelProductDescriptionCulture
    /// </summary>
    public class ProductModelProductDescriptionCulture
    {
        // class members...
        public System.Int32 ProductModelID { get; set; }
        public System.Int32 ProductDescriptionID { get; set; }
        public System.String CultureID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductPhoto
    /// </summary>
    public class ProductPhoto
    {
        // class members...
        public System.Int32 ProductPhotoID { get; set; }
        public System.Byte[] ThumbNailPhoto { get; set; }
        public System.String ThumbnailPhotoFileName { get; set; }
        public System.Byte[] LargePhoto { get; set; }
        public System.String LargePhotoFileName { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductProductPhoto
    /// </summary>
    public class ProductProductPhoto
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.Int32 ProductPhotoID { get; set; }
        public System.Boolean Primary { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductReview
    /// </summary>
    public class ProductReview
    {
        // class members...
        public System.Int32 ProductReviewID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.String ReviewerName { get; set; }
        public System.DateTime ReviewDate { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 Rating { get; set; }
        public System.String Comments { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductSubcategory
    /// </summary>
    public class ProductSubcategory
    {
        // class members...
        public System.Int32 ProductSubcategoryID { get; set; }
        public System.Int32 ProductCategoryID { get; set; }
        public System.String Name { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ScrapReason
    /// </summary>
    public class ScrapReason
    {
        // class members...
        public System.Int16 ScrapReasonID { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for TransactionHistory
    /// </summary>
    public class TransactionHistory
    {
        // class members...
        public System.Int32 TransactionID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Int32 ReferenceOrderID { get; set; }
        public System.Int32 ReferenceOrderLineID { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public System.String TransactionType { get; set; }
        public System.Int32 Quantity { get; set; }
        public System.Decimal ActualCost { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for TransactionHistoryArchive
    /// </summary>
    public class TransactionHistoryArchive
    {
        // class members...
        public System.Int32 TransactionID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Int32 ReferenceOrderID { get; set; }
        public System.Int32 ReferenceOrderLineID { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public System.String TransactionType { get; set; }
        public System.Int32 Quantity { get; set; }
        public System.Decimal ActualCost { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for UnitMeasure
    /// </summary>
    public class UnitMeasure
    {
        // class members...
        public System.String UnitMeasureCode { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for WorkOrder
    /// </summary>
    public class WorkOrder
    {
        // class members...
        public System.Int32 WorkOrderID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Int32 OrderQty { get; set; }
        public System.Int32 StockedQty { get; set; }
        public System.Int16 ScrappedQty { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public System.Int16 ScrapReasonID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for WorkOrderRouting
    /// </summary>
    public class WorkOrderRouting
    {
        // class members...
        public System.Int32 WorkOrderID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Int16 OperationSequence { get; set; }
        public System.Int16 LocationID { get; set; }
        public System.DateTime ScheduledStartDate { get; set; }
        public System.DateTime ScheduledEndDate { get; set; }
        public System.DateTime ActualStartDate { get; set; }
        public System.DateTime ActualEndDate { get; set; }
        public System.Decimal ActualResourceHrs { get; set; }
        public System.Decimal PlannedCost { get; set; }
        public System.Decimal ActualCost { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vProductAndDescription
    /// </summary>
    public class vProductAndDescription
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.String Name { get; set; }
        public System.String ProductModel { get; set; }
        public System.String CultureID { get; set; }
        public System.String Description { get; set; }
    }

    /// <summary>
    /// POCO for vProductModelCatalogDescription
    /// </summary>
    public class vProductModelCatalogDescription
    {
        // class members...
        public System.Int32 ProductModelID { get; set; }
        public System.String Name { get; set; }
        public System.String Summary { get; set; }
        public System.String Manufacturer { get; set; }
        public System.String Copyright { get; set; }
        public System.String ProductURL { get; set; }
        public System.String WarrantyPeriod { get; set; }
        public System.String WarrantyDescription { get; set; }
        public System.String NoOfYears { get; set; }
        public System.String MaintenanceDescription { get; set; }
        public System.String Wheel { get; set; }
        public System.String Saddle { get; set; }
        public System.String Pedal { get; set; }
        public System.String BikeFrame { get; set; }
        public System.String Crankset { get; set; }
        public System.String PictureAngle { get; set; }
        public System.String PictureSize { get; set; }
        public System.String ProductPhotoID { get; set; }
        public System.String Material { get; set; }
        public System.String Color { get; set; }
        public System.String ProductLine { get; set; }
        public System.String Style { get; set; }
        public System.String RiderExperience { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vProductModelInstructions
    /// </summary>
    public class vProductModelInstructions
    {
        // class members...
        public System.Int32 ProductModelID { get; set; }
        public System.String Name { get; set; }
        public System.String Instructions { get; set; }
        public System.Int32 LocationID { get; set; }
        public System.Decimal SetupHours { get; set; }
        public System.Decimal MachineHours { get; set; }
        public System.Decimal LaborHours { get; set; }
        public System.Int32 LotSize { get; set; }
        public System.String Step { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ProductVendor
    /// </summary>
    public class ProductVendor
    {
        // class members...
        public System.Int32 ProductID { get; set; }
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 AverageLeadTime { get; set; }
        public System.Decimal StandardPrice { get; set; }
        public System.Decimal LastReceiptCost { get; set; }
        public System.DateTime LastReceiptDate { get; set; }
        public System.Int32 MinOrderQty { get; set; }
        public System.Int32 MaxOrderQty { get; set; }
        public System.Int32 OnOrderQty { get; set; }
        public System.String UnitMeasureCode { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for PurchaseOrderDetail
    /// </summary>
    public class PurchaseOrderDetail
    {
        // class members...
        public System.Int32 PurchaseOrderID { get; set; }
        public System.Int32 PurchaseOrderDetailID { get; set; }
        public System.DateTime DueDate { get; set; }
        public System.Int16 OrderQty { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Decimal UnitPrice { get; set; }
        public System.Decimal LineTotal { get; set; }
        public System.Decimal ReceivedQty { get; set; }
        public System.Decimal RejectedQty { get; set; }
        public System.Decimal StockedQty { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for PurchaseOrderHeader
    /// </summary>
    public class PurchaseOrderHeader
    {
        // class members...
        public System.Int32 PurchaseOrderID { get; set; }
        public System.Byte RevisionNumber { get; set; }
        public System.Byte Status { get; set; }
        public System.Int32 EmployeeID { get; set; }
        public System.Int32 VendorID { get; set; }
        public System.Int32 ShipMethodID { get; set; }
        public System.DateTime OrderDate { get; set; }
        public System.DateTime ShipDate { get; set; }
        public System.Decimal SubTotal { get; set; }
        public System.Decimal TaxAmt { get; set; }
        public System.Decimal Freight { get; set; }
        public System.Decimal TotalDue { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ShipMethod
    /// </summary>
    public class ShipMethod
    {
        // class members...
        public System.Int32 ShipMethodID { get; set; }
        public System.String Name { get; set; }
        public System.Decimal ShipBase { get; set; }
        public System.Decimal ShipRate { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Vendor
    /// </summary>
    public class Vendor
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String AccountNumber { get; set; }
        public System.String Name { get; set; }
        public System.Byte CreditRating { get; set; }
        public System.Boolean PreferredVendorStatus { get; set; }
        public System.Boolean ActiveFlag { get; set; }
        public System.String PurchasingWebServiceURL { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vVendorWithAddresses
    /// </summary>
    public class vVendorWithAddresses
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.String AddressType { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegionName { get; set; }
    }

    /// <summary>
    /// POCO for vVendorWithContacts
    /// </summary>
    public class vVendorWithContacts
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.String ContactType { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.String PhoneNumberType { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 EmailPromotion { get; set; }
    }

    /// <summary>
    /// POCO for CountryRegionCurrency
    /// </summary>
    public class CountryRegionCurrency
    {
        // class members...
        public System.String CountryRegionCode { get; set; }
        public System.String CurrencyCode { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for CreditCard
    /// </summary>
    public class CreditCard
    {
        // class members...
        public System.Int32 CreditCardID { get; set; }
        public System.String CardType { get; set; }
        public System.String CardNumber { get; set; }
        public System.Byte ExpMonth { get; set; }
        public System.Int16 ExpYear { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Currency
    /// </summary>
    public class Currency
    {
        // class members...
        public System.String CurrencyCode { get; set; }
        public System.String Name { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for CurrencyRate
    /// </summary>
    public class CurrencyRate
    {
        // class members...
        public System.Int32 CurrencyRateID { get; set; }
        public System.DateTime CurrencyRateDate { get; set; }
        public System.String FromCurrencyCode { get; set; }
        public System.String ToCurrencyCode { get; set; }
        public System.Decimal AverageRate { get; set; }
        public System.Decimal EndOfDayRate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Customer
    /// </summary>
    public class Customer
    {
        // class members...
        public System.Int32 CustomerID { get; set; }
        public System.Int32 PersonID { get; set; }
        public System.Int32 StoreID { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.String AccountNumber { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for PersonCreditCard
    /// </summary>
    public class PersonCreditCard
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 CreditCardID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesOrderDetail
    /// </summary>
    public class SalesOrderDetail
    {
        // class members...
        public System.Int32 SalesOrderID { get; set; }
        public System.Int32 SalesOrderDetailID { get; set; }
        public System.String CarrierTrackingNumber { get; set; }
        public System.Int16 OrderQty { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Int32 SpecialOfferID { get; set; }
        public System.Decimal UnitPrice { get; set; }
        public System.Decimal UnitPriceDiscount { get; set; }
        public System.Decimal LineTotal { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesOrderHeader
    /// </summary>
    public class SalesOrderHeader
    {
        // class members...
        public System.Int32 SalesOrderID { get; set; }
        public System.Byte RevisionNumber { get; set; }
        public System.DateTime OrderDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public System.DateTime ShipDate { get; set; }
        public System.Byte Status { get; set; }
        public System.Boolean OnlineOrderFlag { get; set; }
        public System.String SalesOrderNumber { get; set; }
        public System.String PurchaseOrderNumber { get; set; }
        public System.String AccountNumber { get; set; }
        public System.Int32 CustomerID { get; set; }
        public System.Int32 SalesPersonID { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.Int32 BillToAddressID { get; set; }
        public System.Int32 ShipToAddressID { get; set; }
        public System.Int32 ShipMethodID { get; set; }
        public System.Int32 CreditCardID { get; set; }
        public System.String CreditCardApprovalCode { get; set; }
        public System.Int32 CurrencyRateID { get; set; }
        public System.Decimal SubTotal { get; set; }
        public System.Decimal TaxAmt { get; set; }
        public System.Decimal Freight { get; set; }
        public System.Decimal TotalDue { get; set; }
        public System.String Comment { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesOrderHeaderSalesReason
    /// </summary>
    public class SalesOrderHeaderSalesReason
    {
        // class members...
        public System.Int32 SalesOrderID { get; set; }
        public System.Int32 SalesReasonID { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesPerson
    /// </summary>
    public class SalesPerson
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.Decimal SalesQuota { get; set; }
        public System.Decimal Bonus { get; set; }
        public System.Decimal CommissionPct { get; set; }
        public System.Decimal SalesYTD { get; set; }
        public System.Decimal SalesLastYear { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesPersonQuotaHistory
    /// </summary>
    public class SalesPersonQuotaHistory
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.DateTime QuotaDate { get; set; }
        public System.Decimal SalesQuota { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesReason
    /// </summary>
    public class SalesReason
    {
        // class members...
        public System.Int32 SalesReasonID { get; set; }
        public System.String Name { get; set; }
        public System.String ReasonType { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesTaxRate
    /// </summary>
    public class SalesTaxRate
    {
        // class members...
        public System.Int32 SalesTaxRateID { get; set; }
        public System.Int32 StateProvinceID { get; set; }
        public System.Byte TaxType { get; set; }
        public System.Decimal TaxRate { get; set; }
        public System.String Name { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesTerritory
    /// </summary>
    public class SalesTerritory
    {
        // class members...
        public System.Int32 TerritoryID { get; set; }
        public System.String Name { get; set; }
        public System.String CountryRegionCode { get; set; }
        public System.String Group { get; set; }
        public System.Decimal SalesYTD { get; set; }
        public System.Decimal SalesLastYear { get; set; }
        public System.Decimal CostYTD { get; set; }
        public System.Decimal CostLastYear { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SalesTerritoryHistory
    /// </summary>
    public class SalesTerritoryHistory
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Int32 TerritoryID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for ShoppingCartItem
    /// </summary>
    public class ShoppingCartItem
    {
        // class members...
        public System.Int32 ShoppingCartItemID { get; set; }
        public System.String ShoppingCartID { get; set; }
        public System.Int32 Quantity { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SpecialOffer
    /// </summary>
    public class SpecialOffer
    {
        // class members...
        public System.Int32 SpecialOfferID { get; set; }
        public System.String Description { get; set; }
        public System.Decimal DiscountPct { get; set; }
        public System.String Type { get; set; }
        public System.String Category { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public System.Int32 MinQty { get; set; }
        public System.Int32 MaxQty { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for SpecialOfferProduct
    /// </summary>
    public class SpecialOfferProduct
    {
        // class members...
        public System.Int32 SpecialOfferID { get; set; }
        public System.Int32 ProductID { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for Store
    /// </summary>
    public class Store
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.Int32 SalesPersonID { get; set; }
        public System.String Demographics { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// POCO for vIndividualCustomer
    /// </summary>
    public class vIndividualCustomer
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.String PhoneNumberType { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 EmailPromotion { get; set; }
        public System.String AddressType { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegionName { get; set; }
        public System.String Demographics { get; set; }
    }

    /// <summary>
    /// POCO for vPersonDemographics
    /// </summary>
    public class vPersonDemographics
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.Decimal TotalPurchaseYTD { get; set; }
        public System.DateTime DateFirstPurchase { get; set; }
        public System.DateTime BirthDate { get; set; }
        public System.String MaritalStatus { get; set; }
        public System.String YearlyIncome { get; set; }
        public System.String Gender { get; set; }
        public System.Int32 TotalChildren { get; set; }
        public System.Int32 NumberChildrenAtHome { get; set; }
        public System.String Education { get; set; }
        public System.String Occupation { get; set; }
        public System.Boolean HomeOwnerFlag { get; set; }
        public System.Int32 NumberCarsOwned { get; set; }
    }

    /// <summary>
    /// POCO for vSalesPerson
    /// </summary>
    public class vSalesPerson
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String JobTitle { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.String PhoneNumberType { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 EmailPromotion { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegionName { get; set; }
        public System.String TerritoryName { get; set; }
        public System.String TerritoryGroup { get; set; }
        public System.Decimal SalesQuota { get; set; }
        public System.Decimal SalesYTD { get; set; }
        public System.Decimal SalesLastYear { get; set; }
    }

    /// <summary>
    /// POCO for vSalesPersonSalesByFiscalYears
    /// </summary>
    public class vSalesPersonSalesByFiscalYears
    {
        // class members...
        public System.Int32 SalesPersonID { get; set; }
        public System.String FullName { get; set; }
        public System.String JobTitle { get; set; }
        public System.String SalesTerritory { get; set; }
        public System.Decimal 2002 { get; set; }
        public System.Decimal 2003 { get; set; }
        public System.Decimal 2004 { get; set; }
    }

    /// <summary>
    /// POCO for vStoreWithAddresses
    /// </summary>
    public class vStoreWithAddresses
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.String AddressType { get; set; }
        public System.String AddressLine1 { get; set; }
        public System.String AddressLine2 { get; set; }
        public System.String City { get; set; }
        public System.String StateProvinceName { get; set; }
        public System.String PostalCode { get; set; }
        public System.String CountryRegionName { get; set; }
    }

    /// <summary>
    /// POCO for vStoreWithContacts
    /// </summary>
    public class vStoreWithContacts
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.String ContactType { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String MiddleName { get; set; }
        public System.String LastName { get; set; }
        public System.String Suffix { get; set; }
        public System.String PhoneNumber { get; set; }
        public System.String PhoneNumberType { get; set; }
        public System.String EmailAddress { get; set; }
        public System.Int32 EmailPromotion { get; set; }
    }

    /// <summary>
    /// POCO for vStoreWithDemographics
    /// </summary>
    public class vStoreWithDemographics
    {
        // class members...
        public System.Int32 BusinessEntityID { get; set; }
        public System.String Name { get; set; }
        public System.Decimal AnnualSales { get; set; }
        public System.Decimal AnnualRevenue { get; set; }
        public System.String BankName { get; set; }
        public System.String BusinessType { get; set; }
        public System.Int32 YearOpened { get; set; }
        public System.String Specialty { get; set; }
        public System.Int32 SquareFeet { get; set; }
        public System.String Brands { get; set; }
        public System.String Internet { get; set; }
        public System.Int32 NumberEmployees { get; set; }
    }
}
