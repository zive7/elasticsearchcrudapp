using Nest;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ElasticsearchCrudDemo;

public class Program
{
    private static ElasticClient? _client;
    private static IConfiguration? _configuration;
    private static string IndexName = "products";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Elasticsearch CRUD Demo ===");
        
        // Load configuration
        LoadConfiguration();
        
        Console.WriteLine("Initializing Elasticsearch client...");

        // Initialize Elasticsearch client
        InitializeElasticsearchClient();

        // Test connection
        await TestConnection();

        // Create index if it doesn't exist
        await CreateIndexIfNotExists();

        // Main menu loop
        await RunMainMenu();
    }

    private static void LoadConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        IndexName = _configuration["Elasticsearch:DefaultIndex"] ?? "products";
    }

    private static void InitializeElasticsearchClient()
    {
        var elasticsearchUrl = _configuration?["Elasticsearch:Url"] ?? "http://localhost:9200";
        var enableDebugMode = bool.Parse(_configuration?["Elasticsearch:EnableDebugMode"] ?? "true");
        var prettyJson = bool.Parse(_configuration?["Elasticsearch:PrettyJson"] ?? "true");

        var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
            .DefaultIndex(IndexName);

        if (enableDebugMode)
            settings.EnableDebugMode();
        
        if (prettyJson)
            settings.PrettyJson();

        _client = new ElasticClient(settings);
        Console.WriteLine($"Elasticsearch client initialized successfully! Connected to: {elasticsearchUrl}");
    }

    private static async Task TestConnection()
    {
        if (_client == null) return;

        try
        {
            Console.WriteLine("Testing connection to Elasticsearch...");
            var response = await _client.PingAsync();
            
            if (response.IsValid)
            {
                Console.WriteLine("✅ Successfully connected to Elasticsearch!");
            }
            else
            {
                throw new Exception("Ping failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ ERROR: Cannot connect to Elasticsearch!");
            Console.WriteLine("==========================================");
            Console.WriteLine("Elasticsearch is not running or not accessible.");
            Console.WriteLine("\nTo fix this, you need to start Elasticsearch:");
            Console.WriteLine("\nOption 1 - Using Docker:");
            Console.WriteLine("  docker run -d --name elasticsearch -p 9200:9200 -e \"discovery.type=single-node\" -e \"xpack.security.enabled=false\" elasticsearch:8.11.0");
            Console.WriteLine("\nOption 2 - Manual Installation:");
            Console.WriteLine("  1. Download from: https://www.elastic.co/downloads/elasticsearch");
            Console.WriteLine("  2. Extract and run: bin\\elasticsearch.bat");
            Console.WriteLine("\nOption 3 - Check if Elasticsearch is running:");
            Console.WriteLine("  Visit: http://localhost:9200 in your browser");
            Console.WriteLine("\nAfter starting Elasticsearch, restart this application.");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static async Task CreateIndexIfNotExists()
    {
        if (_client == null) return;

        try
        {
            var indexExists = await _client.Indices.ExistsAsync(IndexName);
            
            if (!indexExists.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(IndexName, c => c
                    .Map<Product>(m => m
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Name).Analyzer("standard"))
                            .Text(t => t.Name(n => n.Description).Analyzer("standard"))
                            .Number(n => n.Name(n => n.Price).Type(NumberType.Double))
                            .Keyword(k => k.Name(n => n.Category))
                            .Date(d => d.Name(n => n.CreatedAt))
                        )
                    )
                );

                if (createIndexResponse.IsValid)
                {
                    Console.WriteLine($"Index '{IndexName}' created successfully!");
                }
                else
                {
                    Console.WriteLine($"Error creating index: {createIndexResponse.DebugInformation}");
                }
            }
            else
            {
                Console.WriteLine($"Index '{IndexName}' already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ ERROR: Cannot connect to Elasticsearch!");
            Console.WriteLine("==========================================");
            Console.WriteLine("Elasticsearch is not running or not accessible.");
            Console.WriteLine("\nTo fix this, you need to start Elasticsearch:");
            Console.WriteLine("\nOption 1 - Using Docker:");
            Console.WriteLine("  docker run -d --name elasticsearch -p 9200:9200 -e \"discovery.type=single-node\" -e \"xpack.security.enabled=false\" elasticsearch:8.11.0");
            Console.WriteLine("\nOption 2 - Manual Installation:");
            Console.WriteLine("  1. Download from: https://www.elastic.co/downloads/elasticsearch");
            Console.WriteLine("  2. Extract and run: bin\\elasticsearch.bat");
            Console.WriteLine("\nOption 3 - Check if Elasticsearch is running:");
            Console.WriteLine("  Visit: http://localhost:9200 in your browser");
            Console.WriteLine("\nAfter starting Elasticsearch, restart this application.");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static async Task RunMainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== Main Menu ===");
            Console.WriteLine("1. Create Product");
            Console.WriteLine("2. Read Product by ID");
            Console.WriteLine("3. Read All Products");
            Console.WriteLine("4. Search Products");
            Console.WriteLine("5. Update Product");
            Console.WriteLine("6. Delete Product");
            Console.WriteLine("7. Exit");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateProduct();
                    break;
                case "2":
                    await ReadProduct();
                    break;
                case "3":
                    await ReadAllProducts();
                    break;
                case "4":
                    await SearchProducts();
                    break;
                case "5":
                    await UpdateProduct();
                    break;
                case "6":
                    await DeleteProduct();
                    break;
                case "7":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task CreateProduct()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== Create Product ===");
        
        Console.Write("Enter product name: ");
        var name = Console.ReadLine() ?? "";

        Console.Write("Enter product description: ");
        var description = Console.ReadLine() ?? "";

        Console.Write("Enter product price: ");
        if (!decimal.TryParse(Console.ReadLine(), out var price))
        {
            Console.WriteLine("Invalid price format.");
            return;
        }

        Console.Write("Enter product category: ");
        var category = Console.ReadLine() ?? "";

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Price = price,
            Category = category,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _client.IndexDocumentAsync(product);

        if (response.IsValid)
        {
            Console.WriteLine($"Product created successfully with ID: {response.Id}");
        }
        else
        {
            Console.WriteLine($"Error creating product: {response.DebugInformation}");
        }
    }

    private static async Task ReadProduct()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== Read Product ===");
        Console.Write("Enter product ID: ");
        var id = Console.ReadLine();

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var response = await _client.GetAsync<Product>(id);

        if (response.IsValid)
        {
            var product = response.Source;
            Console.WriteLine($"\nProduct Details:");
            Console.WriteLine($"ID: {response.Id}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Description: {product.Description}");
            Console.WriteLine($"Price: ${product.Price}");
            Console.WriteLine($"Category: {product.Category}");
            Console.WriteLine($"Created At: {product.CreatedAt}");
        }
        else
        {
            Console.WriteLine($"Product not found or error: {response.DebugInformation}");
        }
    }

    private static async Task ReadAllProducts()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== All Products ===");

        var response = await _client.SearchAsync<Product>(s => s
            .Query(q => q.MatchAll())
            .Size(100)
        );

        if (response.IsValid)
        {
            var products = response.Documents;
            Console.WriteLine($"Found {products.Count} products:");

            foreach (var product in products)
            {
                Console.WriteLine($"\nID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Price: ${product.Price}");
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine("---");
            }
        }
        else
        {
            Console.WriteLine($"Error retrieving products: {response.DebugInformation}");
        }
    }

    private static async Task SearchProducts()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== Search Products ===");
        Console.WriteLine("1. Search by name");
        Console.WriteLine("2. Search by category");
        Console.WriteLine("3. Search by price range");
        Console.Write("Select search type: ");

        var searchType = Console.ReadLine();

        switch (searchType)
        {
            case "1":
                await SearchByName();
                break;
            case "2":
                await SearchByCategory();
                break;
            case "3":
                await SearchByPriceRange();
                break;
            default:
                Console.WriteLine("Invalid search type.");
                break;
        }
    }

    private static async Task SearchByName()
    {
        if (_client == null) return;

        Console.Write("Enter product name to search: ");
        var searchTerm = Console.ReadLine();

        if (string.IsNullOrEmpty(searchTerm)) return;

        var response = await _client.SearchAsync<Product>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(searchTerm)
                )
            )
        );

        DisplaySearchResults(response);
    }

    private static async Task SearchByCategory()
    {
        if (_client == null) return;

        Console.Write("Enter category to search: ");
        var category = Console.ReadLine();

        if (string.IsNullOrEmpty(category)) return;

        var response = await _client.SearchAsync<Product>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Category)
                    .Value(category)
                )
            )
        );

        DisplaySearchResults(response);
    }

    private static async Task SearchByPriceRange()
    {
        if (_client == null) return;

        Console.Write("Enter minimum price: ");
        if (!decimal.TryParse(Console.ReadLine(), out var minPrice))
        {
            Console.WriteLine("Invalid minimum price.");
            return;
        }

        Console.Write("Enter maximum price: ");
        if (!decimal.TryParse(Console.ReadLine(), out var maxPrice))
        {
            Console.WriteLine("Invalid maximum price.");
            return;
        }

        var response = await _client.SearchAsync<Product>(s => s
            .Query(q => q
                .Range(r => r
                    .Field(f => f.Price)
                    .GreaterThanOrEquals((double)minPrice)
                    .LessThanOrEquals((double)maxPrice)
                )
            )
        );

        DisplaySearchResults(response);
    }

    private static void DisplaySearchResults(ISearchResponse<Product> response)
    {
        if (response.IsValid)
        {
            var products = response.Documents;
            Console.WriteLine($"\nFound {products.Count} products:");

            foreach (var product in products)
            {
                Console.WriteLine($"\nID: {product.Id}");
                Console.WriteLine($"Name: {product.Name}");
                Console.WriteLine($"Description: {product.Description}");
                Console.WriteLine($"Price: ${product.Price}");
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine("---");
            }
        }
        else
        {
            Console.WriteLine($"Error searching products: {response.DebugInformation}");
        }
    }

    private static async Task UpdateProduct()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== Update Product ===");
        Console.Write("Enter product ID to update: ");
        var id = Console.ReadLine();

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        // First, get the existing product
        var getResponse = await _client.GetAsync<Product>(id);
        if (!getResponse.IsValid)
        {
            Console.WriteLine("Product not found.");
            return;
        }

        var existingProduct = getResponse.Source;
        Console.WriteLine($"Current product: {existingProduct.Name} - ${existingProduct.Price}");

        Console.Write("Enter new product name (or press Enter to keep current): ");
        var name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name))
            existingProduct.Name = name;

        Console.Write("Enter new product description (or press Enter to keep current): ");
        var description = Console.ReadLine();
        if (!string.IsNullOrEmpty(description))
            existingProduct.Description = description;

        Console.Write("Enter new product price (or press Enter to keep current): ");
        var priceInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(priceInput) && decimal.TryParse(priceInput, out var price))
            existingProduct.Price = price;

        Console.Write("Enter new product category (or press Enter to keep current): ");
        var category = Console.ReadLine();
        if (!string.IsNullOrEmpty(category))
            existingProduct.Category = category;

        var updateResponse = await _client.UpdateAsync<Product>(id, u => u
            .Doc(existingProduct)
        );

        if (updateResponse.IsValid)
        {
            Console.WriteLine("Product updated successfully!");
        }
        else
        {
            Console.WriteLine($"Error updating product: {updateResponse.DebugInformation}");
        }
    }

    private static async Task DeleteProduct()
    {
        if (_client == null) return;

        Console.WriteLine("\n=== Delete Product ===");
        Console.Write("Enter product ID to delete: ");
        var id = Console.ReadLine();

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        Console.Write("Are you sure you want to delete this product? (y/N): ");
        var confirmation = Console.ReadLine()?.ToLower();

        if (confirmation != "y")
        {
            Console.WriteLine("Deletion cancelled.");
            return;
        }

        var response = await _client.DeleteAsync<Product>(id);

        if (response.IsValid)
        {
            Console.WriteLine("Product deleted successfully!");
        }
        else
        {
            Console.WriteLine($"Error deleting product: {response.DebugInformation}");
        }
    }
}

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
