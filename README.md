# CoffeeNChill — Part 1

Cloud foundation for the CoffeeNChill canteen system: digital menu items stored in **Azure Table Storage**, staff documents stored in **Azure Blob Storage**, exposed through **HTTP-triggered Azure Functions** (.NET isolated worker), all running locally against **Azurite** and packaged as standalone **Docker** containers.

## Requirements

- .NET Azure Functions, ASP.NET Core integration
- Azure.Data.Tables / Azure.Storage.Blobs SDKs
- Docker Hub
- Postman for endpoint testing

## Project Structure

```
CoffeeNChillFunctions/
├── Models/
│   └── MenuItemEntity.cs
├── MenuFunctions.cs
├── DocumentFunctions.cs
├── Program.cs
├── Dockerfile
├── host.json
└── local.settings.json
docs/
└── CoffeeNChill.postman_collection.json
README.md
```

## Prerequisites

- Visual Studio 2022 (or later) with the **Azure development** workload
- Docker Desktop (running)
- Postman

## Local Setup & Running

### 1. Start Azurite (storage emulator) in Docker

```bash
docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

### 2. Run the Function App from Visual Studio

1. Open `CoffeeNChillFunctions.sln`.
2. Press `F5` (or click the green **Start** button).
3. Note the local URL printed in the console (default `http://localhost:7071`).

The app will automatically create the `MenuItems` table and the `staff-docs` blob container in Azurite the first time it runs.

## Testing with Postman

1. Import `docs/CoffeeNChill.postman_collection.json` into Postman.
2. Confirm the collection variable `baseUrl` matches the port shown in your console (`http://localhost:7071` by default).
3. Run each request in the **Menu Items** and **Staff Documents** folders (or use the collection **Run** button to execute them all in sequence).

## API Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/menu` | Create a new menu item |
| GET | `/api/menu` | Get all menu items |
| GET | `/api/menu/category/{category}` | Get menu items by category |
| PUT | `/api/menu/{category}/{id}` | Update a menu item |
| DELETE | `/api/menu/{category}/{id}` | Delete a menu item |
| POST | `/api/documents/upload` | Upload a staff document (multipart/form-data, key `file`) |
| GET | `/api/documents` | List all staff documents |
| GET | `/api/documents/download/{fileName}` | Download a staff document |

## Docker — Standalone Containers

No Docker Compose is used in Part 1 — each container is built and run individually.

### 1. Push the Azurite image to Docker Hub

```bash
docker pull mcr.microsoft.com/azure-storage/azurite:latest
docker tag mcr.microsoft.com/azure-storage/azurite:latest <dockerhub_username>/coffeenchill-azurite:v1.0
docker push <dockerhub_username>/coffeenchill-azurite:v1.0
```

### 2. Build and push the Function App image

Run this from the **solution folder** (the folder that contains `CoffeeNChillFunctions/`):

```bash
docker build -t <dockerhub_username>/coffeenchill-functions:v1.0 -f CoffeeNChillFunctions/Dockerfile .
docker push <dockerhub_username>/coffeenchill-functions:v1.0
```

### 3. Run both containers standalone

```bash
docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 <dockerhub_username>/coffeenchill-azurite:v1.0
docker run -d --name coffeenchill-functions -p 7071:80 <dockerhub_username>/coffeenchill-functions:v1.0
```

Docker Hub images:
- Functions: `https://hub.docker.com/r/<dockerhub_username>/coffeenchill-functions`
- Azurite: `https://hub.docker.com/r/<dockerhub_username>/coffeenchill-azurite`

## Team Contributions

| Member | Student Number | Contribution |
|---|---|---|
| [Name] | [Number] | [What they worked on] |
| [Name] | [Number] | [What they worked on] |

## Video Demonstration

[Unlisted YouTube link here]

## References

Microsoft (2026) _Azure Functions documentation_. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/ (Accessed: 13 September 2026).

Microsoft (2026) _Guide for running C# Azure Functions in the isolated worker model_. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide (Accessed: 13 September 2026).

Microsoft (2026) _Azure Functions HTTP trigger_. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger (Accessed: 13 September 2026).

Microsoft (2026) _What is Azure Table storage?_. Available at: https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-overview (Accessed: 13 September 2026).

Microsoft (2026) _Azure Blob storage documentation_. Available at: https://learn.microsoft.com/en-us/azure/storage/blobs/ (Accessed: 13 September 2026).

Microsoft (2026) _Use Azurite emulator for local Azure Storage development_. Available at: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite (Accessed: 13 September 2026).

Microsoft (2026) _Azure Tables client library for .NET - version 12.12.0_. Available at: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme (Accessed: 13 September 2026).

Microsoft (2026) _Azure Storage Blobs client library for .NET - version 12.29.2_. Available at: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme (Accessed: 13 September 2026).

Microsoft (2026) _Upload files in ASP.NET Core_. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads (Accessed: 13 September 2026).

W3Schools (2026) _C# Files_. Available at: https://www.w3schools.com/cs/cs_files.php (Accessed: 13 September 2026).

W3Schools (2026) _JavaScript JSON_. Available at: https://www.w3schools.com/js/js_json.asp (Accessed: 13 September 2026).

Docker Inc. (2026) _Docker Manuals_. Available at: https://docs.docker.com/manuals/ (Accessed: 13 September 2026).

Postman Inc. (2026) _Get started in Postman_. Available at: https://learning.postman.com/docs/getting-started/overview/ (Accessed: 13 September 2026).
