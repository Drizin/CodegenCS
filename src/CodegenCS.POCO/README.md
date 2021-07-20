This page is only about this specific template - if you're looking for main library, utilities (e.g. database extractors) or other templates (e.g. Entity Framework), please check the [Main Page](https://github.com/Drizin/CodegenCS/).

# CodegenCS.POCO

**CodegenCS.POCO** is a CodegenCS template which reads a JSON database schema and generates [POCOS](https://stackoverflow.com/a/250006/3606250) for your tables. (See [example POCO](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/POCOs/Product.cs)).

These POCOs can be used by Dapper, PetaPoco or any other micro-ORMs or full ORMs.

The template has multiple optional features including: 

* `override bool Equals()`, `override int GetHashCode()`, override `==` and `!=`
* Can create CRUD statements as extension methods extending IDbConnection (like Dapper) and invoking Dapper
* Can create CRUD statements as class methods invoking Dapper (or you can easily change to other micro ORM)
* Can create CRUD statements with **ActiveRecord** pattern (Insert/Update directly from inside the POCO).

# Usage (easy method)

## 1. Ensure you have dotnet-codegencs tool installed

```dotnet tool install -g dotnet-codegencs```

## 2. Extract the Database Schema

To use this template you first need to run [codegencs dbschema-extractor](https://github.com/Drizin/CodegenCS#dotnet-codegencs-dbschema-extractor) to extracts the schema of a MSSQL or PostgreSQL database into a JSON file.

**Sample usage**:

```codegencs dbschema-extractor /mssql /cn="Server=MYSERVER; Database=AdventureWorks; Integrated Security=True" /output=AdventureWorks.json```

## 3. Generate the POCOs

**Sample usage**:

```codegencs poco /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs```

```codegencs poco /input=AdventureWorks.json /targetFolder=. /namespace=MyProject.POCOs /SingleFile=POCOs.generated.cs /CrudExtensions```

```codegencs poco /input=AdventureWorks.json /targetFolder=. /namespace=MyProject.POCOs /CrudClassMethods```

For more options use ```codegencs poco /?``` or check out [Simple POCO documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO)

# Usage (alternative method using Powershell)

- Check how to invoke [DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) using PowerShell
- Copy the SimplePOCOGenerator files (CSX/PS1/CS) into any folder in your project (set the CS files to NOT be part of your build)
- Edit the paths and the POCOs Namespace in [GenerateSimplePOCOs.csx](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/GenerateSimplePOCOs.csx)
- Execute the PowerShell script [GenerateSimplePOCOs.ps1](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/GenerateSimplePOCOs.ps1)
- The script will automatically install required NuGet packages (CodegenCS and Newtonsoft), will read the JSON file and generates POCOs.
- Optionally you can specify a csproj file and all POCOs will be added to your csproj file.

The [generator script](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/SimplePOCOGenerator.cs) is very simple to understand and customize.

# Sample code (what this Template generates)

See [example POCO here](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/POCOs/Product.cs).  
See [example POCO usage here](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.Tests/POCOTests/POCOTests.cs).

The generated POCOs are based on [Dapper](https://github.com/StackExchange/Dapper/) but you can easily modify the templates for other ORMs.

**Sample POCOs usage**:

```cs
var cn = new SqlConnection(connectionString);

var product = new Product() { 
  Name = "ProductName", 
  ProductNumber = "1234", 
  SellStartDate = DateTime.Now, 
  ModifiedDate = DateTime.Now, 
  SafetyStockLevel = 5, 
  ReorderPoint = 700 
};

cn.Save(product);

product.Name = "Name2";
product.ProductNumber = "12345";

cn.Update(product);
```

Or you can use **Dapper transactions**:

```cs
var product = new Product()
{
  Name = "ProductName",
  ProductNumber = "1234",
  SellStartDate = DateTime.Now,
  ModifiedDate = DateTime.Now,
  SafetyStockLevel = 5,
  ReorderPoint = 700
};

var review = new ProductReview()
{
  ReviewerName = "Rick Drizin",
  ReviewDate = DateTime.Now,
  EmailAddress = "Drizin@users.noreply.github.com",
  Rating = 5,
  Comments = "Amazing code generator",
  ModifiedDate = DateTime.Now
};


cn.Open();

using (var tran = cn.BeginTransaction())
{
  cn.Insert(product, tran);
  review.ProductId = product.ProductId;
  cn.Insert(review, tran);
  tran.Commit(); // or tran.Rollback();
}
```


# License
MIT License
