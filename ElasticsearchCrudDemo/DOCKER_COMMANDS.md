# Elasticsearch Docker Commands Quick Reference

## Installation & Setup

### Install Elasticsearch Container
```bash
# Pull the image
docker pull elasticsearch:8.11.0

# Run the container
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  elasticsearch:8.11.0
```

### Windows PowerShell Version
```powershell
docker run -d --name elasticsearch -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" elasticsearch:8.11.0
```

### Windows Command Prompt Version
```cmd
docker run -d --name elasticsearch -p 9200:9200 -e discovery.type=single-node -e xpack.security.enabled=false elasticsearch:8.11.0
```

## Container Management

### Check Status
```bash
# List running containers
docker ps

# List all containers (including stopped)
docker ps -a

# Check container logs
docker logs elasticsearch

# Follow logs in real-time
docker logs -f elasticsearch
```

### Start/Stop/Restart
```bash
# Start container
docker start elasticsearch

# Stop container
docker stop elasticsearch

# Restart container
docker restart elasticsearch
```

### Remove Container
```bash
# Stop and remove
docker stop elasticsearch
docker rm elasticsearch

# Force remove (if running)
docker rm -f elasticsearch
```

## Testing Connection

### Test Elasticsearch API
```bash
# Basic health check
curl http://localhost:9200

# Cluster health
curl http://localhost:9200/_cluster/health?pretty

# Node info
curl http://localhost:9200/_nodes?pretty
```

### Browser Testing
- Visit: `http://localhost:9200`
- Visit: `http://localhost:9200/_cluster/health`

## Troubleshooting

### Common Issues

#### Container Won't Start
```bash
# Check logs
docker logs elasticsearch

# Check if port is in use
netstat -an | findstr 9200

# Try different port
docker run -d --name elasticsearch -p 9201:9200 -e "discovery.type=single-node" elasticsearch:8.11.0
```

#### Out of Memory
```bash
# Reduce heap size
docker run -d --name elasticsearch -p 9200:9200 -e "discovery.type=single-node" -e "ES_JAVA_OPTS=-Xms256m -Xmx256m" elasticsearch:8.11.0
```

#### Permission Issues
```bash
# On Linux, add user to docker group
sudo usermod -aG docker $USER

# Restart Docker service
sudo systemctl restart docker
```

## Advanced Configuration

### Persistent Data
```bash
# Create volume for data persistence
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -v elasticsearch-data:/usr/share/elasticsearch/data \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  elasticsearch:8.11.0
```

### Custom Configuration
```bash
# Mount custom config file
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -v ./elasticsearch.yml:/usr/share/elasticsearch/config/elasticsearch.yml \
  -e "discovery.type=single-node" \
  elasticsearch:8.11.0
```

### Network Configuration
```bash
# Create custom network
docker network create elastic-net

# Run with custom network
docker run -d \
  --name elasticsearch \
  --network elastic-net \
  -p 9200:9200 \
  -e "discovery.type=single-node" \
  elasticsearch:8.11.0
```

## Update Elasticsearch

### Update to New Version
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
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  elasticsearch:8.12.0
```

## Docker Compose (Alternative)

### docker-compose.yml
```yaml
version: '3.8'
services:
  elasticsearch:
    image: elasticsearch:8.11.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - ES_JAVA_OPTS=-Xms512m -Xmx512m
    ports:
      - "9200:9200"
      - "9300:9300"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data

volumes:
  elasticsearch-data:
```

### Run with Docker Compose
```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs elasticsearch
```

## Performance Tuning

### Memory Settings
```bash
# For development (512MB)
-e "ES_JAVA_OPTS=-Xms512m -Xmx512m"

# For production (2GB)
-e "ES_JAVA_OPTS=-Xms2g -Xmx2g"

# For production (4GB)
-e "ES_JAVA_OPTS=-Xms4g -Xmx4g"
```

### System Settings
```bash
# Increase virtual memory
sysctl -w vm.max_map_count=262144

# Make permanent (Linux)
echo 'vm.max_map_count=262144' >> /etc/sysctl.conf
```

## Security (Production)

### Enable Security
```bash
# Generate passwords
docker run --rm -it elasticsearch:8.11.0 /usr/share/elasticsearch/bin/elasticsearch-setup-passwords auto

# Run with security enabled
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -e "discovery.type=single-node" \
  -e "ELASTIC_PASSWORD=your_password" \
  elasticsearch:8.11.0
```

### SSL/TLS Configuration
```bash
# Run with SSL enabled
docker run -d \
  --name elasticsearch \
  -p 9200:9200 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=true" \
  -e "xpack.security.http.ssl.enabled=true" \
  elasticsearch:8.11.0
``` 