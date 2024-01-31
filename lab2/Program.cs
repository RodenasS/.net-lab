using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string ProductNumber { get; set; }
    public decimal StandardCost { get; set; }
    public decimal ListPrice { get; set; }
    public int DaysToManufacture { get; set; }
    public DateTime SellStartDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public decimal ListPriceCost => StandardCost * ListPrice;

    public List<ProductHistory> ProductHistories { get; set; }

    public Product()
    {
        ProductHistories = new List<ProductHistory>();
    }
}

public class ItemArticle : Product
{
    public int ArticleNumber { get; set; }
    private string Description { get; set; }

}

public class ItemElement : Product
{
    public long ElementNumber { get; set; }
    private string Description { get; set; }
}


public abstract class History
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public abstract void PrintExpirationDate();

    public void PrintHistoryDetails()
    {
        Console.WriteLine($"Product ID: {ProductId}, Start: {StartDate}, End: {EndDate}");
    }
}

public class ProductHistory : History
{
    public new int ProductId { get; set; }
    public new DateTime StartDate { get; set; }
    public new DateTime? EndDate { get; set; }
    public decimal ListPrice { get; set; }
    public DateTime ModifiedDate { get; set; }

    public Product Product { get; set; }

    public override void PrintExpirationDate()
    {
        Console.WriteLine($"Product ID: {ProductId} expires on {EndDate}.");
    }
}

public class ProductPriceHistory
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal ListPrice { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public interface ICrudRepository<T>
{
    void LoadData();
    void InsertRecord(T record);
    void UpdateRecord(T record);
    void DeleteRecord(T record);
}

public class DatabaseManager : ICrudRepository<Product>
{
    private readonly string directoryPath;
    private readonly string productFileName;
    private List<Product> products = new List<Product>();

    public List<Product> Products => products;

    public DatabaseManager(string dirPath, string prodFileName)
    {
        directoryPath = @"C:\Users\roden\Desktop\lab2\database";
        productFileName = prodFileName;
    }

    public void LoadData()
    {
            string productsPath = Path.Combine(directoryPath, "Products.json");
            string priceHistoryPath = Path.Combine(directoryPath, "ProductsPriceHistory.json");


            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                PropertyNameCaseInsensitive = true // If your property names in C# are case-sensitive
            };

            if (File.Exists(productsPath))
            {
                string productsJson = File.ReadAllText(productsPath);
                products = JsonSerializer.Deserialize<List<Product>>(productsJson, options) ?? new List<Product>();
            }
            else
            {
                Console.WriteLine($"File not found at {productsPath}");
            }

        if (File.Exists(priceHistoryPath))
            {
                string priceHistoryJson = File.ReadAllText(priceHistoryPath);
                var priceHistories = JsonSerializer.Deserialize<List<ProductPriceHistory>>(priceHistoryJson) ?? new List<ProductPriceHistory>();

                foreach (var product in products)
                {
                    var histories = priceHistories
                        .Where(ph => ph.ProductId == product.ProductId)
                        .Select(ph => new ProductHistory
                        {
                            ProductId = ph.ProductId,
                            StartDate = ph.StartDate,
                            EndDate = ph.EndDate,
                            ListPrice = ph.ListPrice,
                            ModifiedDate = ph.ModifiedDate,
                            Product = product
                        })
                        .ToList();
                    product.ProductHistories = histories;
                }
            }
            else
            {
                Console.WriteLine($"File not found at {priceHistoryPath}");
            }
    }


    public void InsertRecord(Product record)
    {
        products.Add(record);
        SaveToFile();
    }

    public void UpdateRecord(Product record)
    {
        var existingProduct = products.FirstOrDefault(p => p.ProductId == record.ProductId);
        if (existingProduct != null)
        {
            existingProduct.Name = record.Name;
            existingProduct.ProductNumber = record.ProductNumber;
            existingProduct.StandardCost = record.StandardCost;
            existingProduct.ListPrice = record.ListPrice;
            existingProduct.DaysToManufacture = record.DaysToManufacture;
            existingProduct.SellStartDate = record.SellStartDate;
            existingProduct.ModifiedDate = DateTime.Now;

            SaveToFile();
        }
    }

    public void DeleteRecord(Product record)
    {
        products.RemoveAll(p => p.ProductId == record.ProductId);
        SaveToFile();
    }

    private void SaveToFile()
    {
        string fullPath = Path.Combine(directoryPath, "Products.json");
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };
        string productsJson = JsonSerializer.Serialize(products, options);
        File.WriteAllText(fullPath, productsJson);
    }

}

class Program
{
    private static DatabaseManager _dbManager;

    static Program()
    {
        _dbManager = new DatabaseManager("lab2/database", "Products.json");
    }

    static void Main(string[] args)
    {
        _dbManager.LoadData();

        // 1 užduotis
        /*
        List<History> histories = new List<History>
        {
            new ProductHistory { ProductId = 1, EndDate = DateTime.Now.AddDays(30) },
            new ProductHistory { ProductId = 2, EndDate = DateTime.Now.AddDays(60) }
        };

        foreach (var history in histories)
        {
            history.PrintExpirationDate(); 
        }

        */

        // 4 užduotis
        /*
        History productHistory = new ProductHistory
        {
            ProductId = 1,
            StartDate = DateTime.Now.AddDays(-10),
            EndDate = DateTime.Now.AddDays(10)
        };

        // Iškviečiame abstrakčioje klasėje apibrėžtą metodą
        productHistory.PrintExpirationDate();

        // Iškviečiame abstrakčioje klasėje įgyvendintą metodą
        productHistory.PrintHistoryDetails();
        */

        // 5 užduotis
        /*
         
        _dbManager.LoadData();

        */

        // CRUD
        /*
         
        Product newProduct = new Product
        {
            ProductId = 4,
            Name = "Engine Oil",
            ProductNumber = "P-1774",
            StandardCost = 10.00m,
            ListPrice = 15.00m,
            DaysToManufacture = 2,
            SellStartDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        };

        _dbManager.InsertRecord(newProduct);

        foreach (var product in _dbManager.Products)
        {
            Console.WriteLine($"Product ID: {product.ProductId}, Name: {product.Name}");
        }

        var existingProduct = _dbManager.Products.FirstOrDefault(p => p.ProductId == 4);
        if (existingProduct != null)
        {
            existingProduct.Name = "Updated Product Name";
            existingProduct.ListPrice = 20.00m;

            _dbManager.UpdateRecord(existingProduct);
        }
        else
        {
            Console.WriteLine("Product not found.");
        }

        var productToDelete = _dbManager.Products.FirstOrDefault(p => p.ProductId == 4);
        if (productToDelete != null)
        {
            _dbManager.DeleteRecord(productToDelete);
        }
        else
        {
            Console.WriteLine("Product not found.");
        }

        */

    }
}

