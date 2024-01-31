using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class Product
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public string ProductNumber { get; set; }
    public double StandardCost { get; set; }
    public double ListPrice { get; set; }
    public int DaysToManufacture { get; set; }
    public DateTime SellStartDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string? Color { get; set; }
    public string SizeUnitMeasureCode { get; set; }

    public List<ProductHistory> ProductHistories { get; set; }
    public string? Size { get; set; }
    public double? Weight { get; set; }
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
    public int ProductID { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double ListPrice { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Product Product { get; set; }

    public override void PrintExpirationDate()
    {
        Console.WriteLine($"Product ID: {ProductID} expires on {EndDate}.");
    }
}

public class ProductPriceHistory
{
    public int ProductID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ListPrice { get; set; }
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
            PropertyNameCaseInsensitive = true
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
            var priceHistories = JsonSerializer.Deserialize<List<ProductPriceHistory>>(priceHistoryJson, options) ?? new List<ProductPriceHistory>();

            foreach (var product in products)
            {
                product.ProductHistories = priceHistories
                    .Where(ph => ph.ProductID == product.ProductID)
                    .Select(ph => new ProductHistory
                    {
                        ProductID = ph.ProductID,
                        StartDate = ph.StartDate,
                        EndDate = ph.EndDate,
                        ListPrice = ph.ListPrice,
                        ModifiedDate = ph.ModifiedDate,
                        Product = product
                    })
                    .ToList();
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
        var existingProduct = products.FirstOrDefault(p => p.ProductID == record.ProductID);
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
        products.RemoveAll(p => p.ProductID == record.ProductID);
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

        // LINQ užklausos LAB3

        var productPriceHistories = _dbManager.Products
                                       .SelectMany(p => p.ProductHistories)
                                       .ToList();


        // A) Užklausa grąžinanti visus objekto duomenis:
        /*
         
        var taskAQuerySyntax = (from c in productPriceHistories
                                select c).ToList();

        var taskAMethodSyntax = productPriceHistories.Select(c => c).ToList();

        Console.WriteLine("Scenario A: ProductID / Start Date / EndDate / ListPrice / ModifiedDate");
        foreach (var item in taskAQuerySyntax)
        {
            Console.WriteLine($"ProductID: {item.ProductId}, Start Date: {item.StartDate}, End Date: {item.EndDate}, ListPrice: {item.ListPrice}, Modified Date: {item.ModifiedDate}");
        }
        */

        // B) Užklausa grąžinanti specifinius stulpelius:
        /*
        var taskBQuerySyntax = (from c in productPriceHistories
                                select new
                                {
                                    c.ProductId,
                                    c.ListPrice,

                                }).ToList();

        var taskBMethodSyntax = productPriceHistories.Select(c => new
        {
            c.ProductId,
            c.ListPrice,
        }).ToList();

        Console.WriteLine("\nScenario B: ProductID / ListPrice");
        foreach (var item in taskBMethodSyntax)
        {
            Console.WriteLine($"ProductID: {item.ProductId}, ListPrice: {item.ListPrice}");
        }
        */

        // 2 užduotis


        // C)	Užklausa, kuri išrenka visus duomenis (ProductPriceHistory), produkto su ID 739
        /*
        var querySyntaxC = from pph in productPriceHistories
                           where pph.ProductID == 739
                           select pph;

        var methodSyntaxC = productPriceHistories
            .Where(pph => pph.ProductID == 739)
            .ToList();

        Console.WriteLine("Scenario C: ProductID / StartDate / EndDate / ListPrice / ModifiedDate");
        foreach (var item in querySyntaxC)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Start Date: {item.StartDate}, End Date: {item.EndDate}, ListPrice: {item.ListPrice}, Modified Date: {item.ModifiedDate}");
        }
        */
        // D)	Užklausa, kuri išrenka aktyvias kainas (pateiktų stulpelių), kai EndDate nėra NULL
        /*

        var querySyntaxD = from pph in productPriceHistories
                           where pph.EndDate != null
                           select new
                           {
                               pph.ProductID,
                               pph.StartDate,
                               pph.EndDate,
                               pph.ListPrice
                           };

        var methodSyntaxD = productPriceHistories
            .Where(pph => pph.EndDate != null)
            .Select(pph => new
            {
                pph.ProductID,
                pph.StartDate,
                pph.EndDate,
                pph.ListPrice
            })
            .ToList();

        Console.WriteLine("\nScenario D: ProductID / StartDate / EndDate / ListPrice");
        foreach (var item in querySyntaxD)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Start Date: {item.StartDate}, End Date: {item.EndDate}, ListPrice: {item.ListPrice}");
        }
        */

        // E)	Užklausa, su požymiais ir rūšiavimu nuo A – Z pagal Name
        /*
        var querySyntaxE = from product in _dbManager.Products
                           orderby product.Name ascending
                           select new
                           {
                               product.ProductID,
                               product.Name,
                               product.ProductNumber,
                               product.Color,
                               product.StandardCost,
                               product.ListPrice
                           };

        var methodSyntaxE = _dbManager.Products
            .OrderBy(product => product.Name)
            .Select(product => new
            {
                product.ProductID,
                product.Name,
                product.ProductNumber,
                product.Color,
                product.StandardCost,
                product.ListPrice
            })
            .ToList();

        Console.WriteLine("Scenario E: ProductID / Name / ProductNumber / Color / StandardCost / ListPrice");
        foreach (var item in querySyntaxE)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Name: {item.Name}, ProductNumber: {item.ProductNumber}, Color: {item.Color}, StandardCost: {item.StandardCost}, ListPrice: {item.ListPrice}");
        }
        */

        //F)	Užklausa su požymiais ir rūšiavimu nuo Z iki A pagal StandardCost, o Name stulpelio pavadinimą pakeisti į ProductName
        /*
        var querySyntaxF = from product in _dbManager.Products
                           orderby product.StandardCost descending
                           select new
                           {
                               ProductName = product.Name,
                               product.ProductNumber,
                               product.Color,
                               product.StandardCost
                           };

        foreach (var item in querySyntaxF)
        {
            Console.WriteLine($"ProductName: {item.ProductName}, ProductNumber: {item.ProductNumber}, Color: {item.Color}, StandardCost: {item.StandardCost}");
        }

         var methodSyntaxF = _dbManager.Products
        .OrderByDescending(product => product.StandardCost)
        .Select(product => new
        {
            ProductName = product.Name,
            product.ProductNumber,
            product.Color,
            product.StandardCost
        });

        */

        // G)	Užklausa, kuri grąžina (ProductPriceHistory) kiekvieną produktą ir didžiausia to produkto istorijos kainą bei kiek kainų pokyčių atlikta
        /*
         
        var querySyntaxG = (from product in _dbManager.Products
                            let maxListPrice = product.ProductHistories.Max(pph => pph.ListPrice)
                            let modificationCount = product.ProductHistories.Count
                            select new
                            {
                                product.ProductID,
                                MaxListPrice = maxListPrice,
                                ModificationCount = modificationCount
                            }).ToList();


        var methodSyntaxG = _dbManager.Products
    .Select(product => new
    {
        product.ProductID,
        MaxListPrice = product.ProductHistories.Max(pph => pph.ListPrice),
        ModificationCount = product.ProductHistories.Count
    })
    .ToList();

        foreach (var item in methodSyntaxG)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, MaxListPrice: {item.MaxListPrice}, ModificationCount: {item.ModificationCount}");
        }

        */

        // H)	Užklausa, kuri grąžina(Product) sugrupuotus įrašus pagal Color ir SizeUnitMeasureCode bei paskaičiuoja ListPrice sumą, vidurkį, mažiausią, didžiausią reikšmę, apjungia Color ir SizeUnitMeasureCode į vieną bendrą stulpelį, filtracija pagal ProductName, įrašai, kurie prasideda „BK -“ simboliais

        /*
         
        var querySyntaxH = (from product in _dbManager.Products
                            where product.Name.StartsWith("BK-")
                            group product by new
                            {
                                Color = product.Color,
                                SizeUnitMeasureCode = product.SizeUnitMeasureCode
                            } into grouped
                            select new
                            {
                                ColorWithSizeUnitMeasureCode = $"{grouped.Key.Color} / {grouped.Key.SizeUnitMeasureCode}",
                                SummaryListPrice = grouped.Sum(p => p.ListPrice),
                                AverageListPrice = grouped.Average(p => p.ListPrice),
                                MaxListPrice = grouped.Max(p => p.ListPrice),
                                MinListPrice = grouped.Min(p => p.ListPrice)
                            }).ToList();

        foreach (var item in querySyntaxH)
        {
            Console.WriteLine($"ColorWithSizeUnitMeasureCode: {item.ColorWithSizeUnitMeasureCode}, SummaryListPrice: {item.SummaryListPrice}, AverageListPrice: {item.AverageListPrice}, MaxListPrice: {item.MaxListPrice}, MinListPrice: {item.MinListPrice}");
        }

             var methodSyntaxH = _dbManager.Products
            .Where(product => product.Name.StartsWith("BK-"))
            .GroupBy(product => new
            {
                Color = product.Color,
                SizeUnitMeasureCode = product.SizeUnitMeasureCode
            })
            .Select(grouped => new
            {
                ColorWithSizeUnitMeasureCode = $"{grouped.Key.Color} / {grouped.Key.SizeUnitMeasureCode}",
                SummaryListPrice = grouped.Sum(p => p.ListPrice),
                AverageListPrice = grouped.Average(p => p.ListPrice),
                MaxListPrice = grouped.Max(p => p.ListPrice),
                MinListPrice = grouped.Min(p => p.ListPrice)
            })
            .ToList();

        foreach (var item in methodSyntaxH)
        {
            Console.WriteLine($"ColorWithSizeUnitMeasureCode: {item.ColorWithSizeUnitMeasureCode}, SummaryListPrice: {item.SummaryListPrice}, AverageListPrice: {item.AverageListPrice}, MaxListPrice: {item.MaxListPrice}, MinListPrice: {item.MinListPrice}");
        }

        */

        // I)	Užklausa, kuri apjungia ProductListPriceHistory ir Product (per ProductID) lenteles/sąrašus bei iš jų abiejų išrenka reikiamus duomenų atributus
        /*
        var querySyntaxI = (from product in _dbManager.Products
                            join priceHistory in productPriceHistories on product.ProductID equals priceHistory.ProductID
                            select new
                            {
                                product.ProductID,
                                product.Name,
                                product.Size,
                                product.Weight,
                                priceHistory.StartDate,
                                priceHistory.EndDate,
                                priceHistory.ListPrice,
                                priceHistory.ModifiedDate
                            }).ToList();

        foreach (var item in querySyntaxI)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Name: {item.Name}, Size: {item.Size}, Weight: {item.Weight}, StartDate: {item.StartDate}, EndDate: {item.EndDate}, ListPrice: {item.ListPrice}, ModifiedDate: {item.ModifiedDate}");
        }
        */

        // J)	Užklausa, kuri sudarytų sąrašą kiekvieno Product įrašo (ProductID, Name) ir to produkto pirmąją ir paskutiniąją publikuotą istorijos kainą (StartDate – min ir max, gali reikėti kelių join‘ų). Sąrašą sudaryti tik iš įrašų kai FirstPrice ir LastPrice skiriasi, pavyzdžiui – viso 77 įrašai:

        /*
        var querySyntaxJ = (from product in _dbManager.Products
                            join priceHistory in productPriceHistories on product.ProductID equals priceHistory.ProductID
                            group priceHistory by new { product.ProductID, product.Name } into grouped
                            let firstPrice = grouped.OrderBy(ph => ph.StartDate).FirstOrDefault()
                            let lastPrice = grouped.OrderByDescending(ph => ph.StartDate).FirstOrDefault()
                            where firstPrice != null && lastPrice != null && firstPrice.ListPrice != lastPrice.ListPrice
                            select new
                            {
                                grouped.Key.ProductID,
                                grouped.Key.Name,
                                FirstPrice = firstPrice.ListPrice,
                                LastPrice = lastPrice.ListPrice
                            }).ToList();

        foreach (var item in querySyntaxJ)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Name: {item.Name}, FirstPrice: {item.FirstPrice}, LastPrice: {item.LastPrice}");
        }
        */

        // K)	Užklausa, kuri išrenka 2012 ir 2013 metais galiojusias pradžios (StartDate) kainas ir prekes. Duomenys surūšiuojami pagal Name didėjimo tvarka. Prie istorinių duomenų prijungiamas Product sąrašas ir paskaičiuojamas kiekvienos prekės kainų istorijos vidurkis. Papildomai reikia praleisti 3 pirmus įrašus ir paimti tik sekančius 5 įrašus.

        var querySyntaxK = (from productHistory in productPriceHistories
                            join product in _dbManager.Products on productHistory.ProductID equals product.ProductID
                            where productHistory.StartDate.Year == 2012 || productHistory.StartDate.Year == 2013
                            orderby product.Name
                            group productHistory by new { product.ProductID, product.Name } into grouped
                            select new
                            {
                                ProductID = grouped.Key.ProductID,
                                Name = grouped.Key.Name,
                                AverageListPrice = grouped.Average(ph => ph.ListPrice)
                            })
                    .Skip(3)
                    .Take(5)
                    .ToList();

        string jsonResult = JsonSerializer.Serialize(querySyntaxK, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("result.json", jsonResult);

        foreach (var item in querySyntaxK)
        {
            Console.WriteLine($"ProductID: {item.ProductID}, Name: {item.Name}, AverageListPrice: {item.AverageListPrice}");
        }

    }
}

