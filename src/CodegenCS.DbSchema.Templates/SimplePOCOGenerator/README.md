This page is only about this specific template - if you're looking for main library, utilities (e.g. database extractors) or other templates (e.g. Entity Framework), please check the [Main Page](https://github.com/Drizin/CodegenCS/).

# Simple POCO Generator

**Simple POCO Generator** is a CodegenCS template which reads a JSON database schema and generates [POCOS](https://stackoverflow.com/a/250006/3606250) for your tables. (See [example POCO](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/SampleOutput/Product.generated.cs)).

These POCOs can be used by Dapper, PetaPoco or any other micro-ORMs or full ORMs.

The template has multiple optional features including: 

* `override bool Equals()`, `override int GetHashCode()`, override `==` and `!=`
* Can create CRUD statements as extension methods extending IDbConnection (like Dapper) and invoking Dapper
* Can create CRUD statements as class methods invoking Dapper (or you can easily change to other micro ORM)
* Can create CRUD statements with **ActiveRecord** pattern (Insert/Update directly from inside the POCO).

# Usage

## 1. Ensure you have [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) installed, and install **dotnet-codegencs tool**:

```dotnet tool install -g dotnet-codegencs```

## 2. Extract the Database Schema

To use this template you first need to run [codegencs extract-dbschema](https://github.com/Drizin/CodegenCS#dotnet-codegencs-extract-dbschema) to extracts the schema of a MSSQL or PostgreSQL database into a JSON file.

**Sample usage**:

```codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

## 3. Generate the POCOs

**Sample usage**:

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=OutputFolder --Namespace=MyProject.POCOs```

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=. --Namespace=MyProject.POCOs --SingleFile --CrudExtensionMethods```

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=. --Namespace=MyProject.POCOs --CrudClassMethods```

**To see all available options use** ```codegencs simplepocogenerator -?```

# <a name="customizing"></a>Customizing the Templates

- After running the template for the first time (instructions above) you'll find (in the same output folder where your POCOs were generated) a CSX file named `SimplePOCOGenerator.csx`.
  This file is a copy of the standard template ([`SimplePOCOGenerator.cs`](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/SimplePOCOGenerator.cs)) enriched with the invocation parameters that you used in the command line.
  This file is plain C#, easy to understand and customize, and Visual Studio will show full intellisense (autocomplete) for the CSX file.  
  (CSX extension is just a trick because if the template was saved with CS extension then your main project with the POCOs would try to include and compile this template as part of the project)
- You can edit `SimplePOCOGenerator.csx` if you want to make adjustments. 
- Some common customizations are modifying `bool ShouldProcessColumn(Table table, Column column)` and `bool ShouldProcessTable(Table table)` where you'll be able to define which tables and columns you want (or don't want) POCOs.')
- After making customizations or after a schema refresh (e.g. new tables added to the json) you'll want to run the templates again (regenerate the output) with `codegencs run SimplePOCOGenerator.csx`.
  This command will create a csproj for the csx, will invoke `dotnet build` and `dotnet run`, and then after execution will clean outputs (the temporary csproj, bin and obj folders).

# Sample Template Output

See [example POCO here](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/SampleOutput/Product.generated.cs).  
See [example POCO usage here](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.Tests/POCOTests/POCOTests.cs).

The generated POCOs are based on [Dapper](https://github.com/DapperLib/Dapper) but you can easily modify the templates for other ORMs.

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
