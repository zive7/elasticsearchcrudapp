# Elasticsearch CRUD Demo

A .NET 8 console application that demonstrates basic CRUD (Create, Read, Update, Delete) operations with Elasticsearch using the NEST client library.

## Prerequisites

1. **Docker**: You need to have Docker installed and running
   - Download Docker Desktop from: https://www.docker.com/products/docker-desktop/
   - For Windows: Install Docker Desktop for Windows
   - For macOS: Install Docker Desktop for Mac
   - For Linux: Install Docker Engine

2. **.NET 8 SDK**: Make sure you have .NET 8 SDK installed
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

## Elasticsearch Installation with Docker

### Step 1: Verify Docker Installation

First, ensure Docker is installed and running:

```bash
docker --version
docker ps
```

You should see Docker version information and an empty list of containers.

### Step 2: Pull Elasticsearch Image

Pull the official Elasticsearch Docker image:

```bash
docker pull elasticsearch:8.11.0
```

### Step 3: Run Elasticsearch Container

Run Elasticsearch in a Docker container with the following command:

```bash
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  elasticsearch:8.11.0
```

**Command Explanation:**
- `-d`: Run container in detached mode (background)
- `--name elasticsearch`: Name the container "elasticsearch"
- `-p 9200:9200`: Map host port 9200 to container port 9200 (HTTP API)
- `-p 9300:9300`: Map host port 9300 to container port 9300 (Transport)
- `-e "discovery.type=single-node"`: Configure as single-node cluster
- `-e "xpack.security.enabled=false"`: Disable security for demo purposes
- `-e "ES_JAVA_OPTS=-Xms512m -Xmx512m"`: Set Java heap size to 512MB

### Step 4: Verify Elasticsearch is Running

Check if the container is running:

```bash
docker ps
```

You should see the elasticsearch container in the list.

### Step 5: Test Elasticsearch Connection

Test if Elasticsearch is accessible:

```bash
# Using curl (if available)
curl http://localhost:9200

# Or visit in your browser
# http://localhost:9200
```

You should see a JSON response like:
```json
{
  "name" : "elasticsearch",
  "cluster_name" : "docker-cluster",
  "cluster_uuid" : "...",
  "version" : {
    "number" : "8.11.0",
    ...
  },
  "tagline" : "You Know, for Search"
}
```

### Step 6: Check Cluster Health

Verify the cluster health:

```bash
curl http://localhost:9200/_cluster/health?pretty
```

You should see:
```json
{
  "cluster_name" : "docker-cluster",
  "status" : "green",
  "timed_out" : false,
  "number_of_nodes" : 1,
  "number_of_data_nodes" : 1,
  ...
}
```

## Docker Management Commands

### Start/Stop Container

```bash
# Stop the container
docker stop elasticsearch

# Start the container
docker start elasticsearch

# Restart the container
docker restart elasticsearch
```

### View Logs

```bash
# View container logs
docker logs elasticsearch

# Follow logs in real-time
docker logs -f elasticsearch
```

### Remove Container

```bash
# Stop and remove the container
docker stop elasticsearch
docker rm elasticsearch

# Remove with force (if running)
docker rm -f elasticsearch
```

### Update Elasticsearch

```bash
# Stop and remove old container
docker stop elasticsearch
docker rm elasticsearch

# Pull new image
docker pull elasticsearch:8.12.0

# Run new version
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  elasticsearch:8.12.0
```

## Setup

1. **Clone or navigate to the project directory**:
   ```bash
   cd ElasticsearchCrudDemo
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

## Features

The application provides the following CRUD operations:

### 1. Create Product
- Add new products with name, description, price, and category
- Automatically generates unique IDs and timestamps

### 2. Read Product by ID
- Retrieve a specific product using its ID
- Displays all product details

### 3. Read All Products
- List all products in the index
- Shows basic information for each product

### 4. Search Products
- **Search by name**: Full-text search in product names
- **Search by category**: Exact match on category field
- **Search by price range**: Find products within a price range

### 5. Update Product
- Modify existing product details
- Keep current values by pressing Enter for unchanged fields

### 6. Delete Product
- Remove products by ID with confirmation prompt

## Data Model

The application uses a `Product` model with the following fields:

```csharp
public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Elasticsearch Index Mapping

The application automatically creates an index called `products` with the following mapping:

- **Name**: Text field with standard analyzer for full-text search
- **Description**: Text field with standard analyzer for full-text search
- **Price**: Double number field for numeric operations
- **Category**: Keyword field for exact matching
- **CreatedAt**: Date field for timestamp operations

## Configuration

The application is configured to connect to Elasticsearch at `http://localhost:9200`. To change the connection settings, modify the `appsettings.json` file:

```json
{
  "Elasticsearch": {
    "Url": "http://localhost:9200",
    "DefaultIndex": "products",
    "EnableDebugMode": true,
    "PrettyJson": true
  }
}
```

## Error Handling

The application includes comprehensive error handling:
- Connection validation with helpful error messages
- Index creation error handling
- CRUD operation error reporting
- Input validation for user data
- Docker-specific troubleshooting instructions

## Dependencies

- **NEST**: Official .NET client for Elasticsearch
- **Microsoft.Extensions.Configuration**: Configuration management
- **.NET 8**: Runtime and framework

## Troubleshooting

### Common Issues

1. **Connection refused**: 
   - Make sure Elasticsearch container is running: `docker ps`
   - Check if port 9200 is available: `netstat -an | findstr 9200`
   - Restart the container: `docker restart elasticsearch`

2. **Container won't start**:
   - Check Docker logs: `docker logs elasticsearch`
   - Ensure port 9200 is not in use by another application
   - Try different ports: `-p 9201:9200`

3. **Out of memory errors**:
   - Increase Docker memory allocation in Docker Desktop settings
   - Reduce Elasticsearch heap size: `-e "ES_JAVA_OPTS=-Xms256m -Xmx256m"`

4. **Permission denied**:
   - Run Docker commands with appropriate permissions
   - On Linux, add your user to the docker group

### Elasticsearch Health Check

You can check if Elasticsearch is running by visiting:
```
http://localhost:9200/_cluster/health
```

### Docker Alternative Commands

For Windows PowerShell:
```powershell
docker run -d --name elasticsearch -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" elasticsearch:8.11.0
```

For Windows Command Prompt:
```cmd
docker run -d --name elasticsearch -p 9200:9200 -e discovery.type=single-node -e xpack.security.enabled=false elasticsearch:8.11.0
```

## Example Usage

1. **Start Elasticsearch**: Follow the Docker installation steps above
2. **Start the application**: `dotnet run`
3. **Choose option 1** to create a product
4. **Enter product details** (name, description, price, category)
5. **Use option 3** to view all products
6. **Use option 4** to search for products
7. **Use option 5** to update existing products
8. **Use option 6** to delete products

## Next Steps

This demo can be extended with:
- Bulk operations
- Advanced search queries
- Aggregations
- Real-time indexing
- Authentication and security
- Connection pooling
- Logging and monitoring
- Docker Compose for multi-service setup 
