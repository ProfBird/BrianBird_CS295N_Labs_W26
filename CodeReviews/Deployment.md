# User Secrets and Web Deployment Guide

## Secrets Management

This application uses different approaches for managing sensitive database credentials depending on the environment:

### Local Development Environment

For local development, database credentials are stored using **Visual Studio User Secrets**. This keeps sensitive information out of source control while making it easily accessible during development.

#### Setting Up User Secrets

1. Right-click on the `CodeReviews` project in Solution Explorer
2. Select **Manage User Secrets**
3. Add your database credentials in the `secrets.json` file:

```json
{
  "DbUser": "your_local_database_username",
  "DbPassword": "your_local_database_password"
}
```

User Secrets are stored outside the project directory at:
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

### Production Environment (Azure)

For production deployments to Azure, credentials are managed through **Azure App Service Application Settings**. These settings are:
- Encrypted at rest
- Not stored in source control
- Injected as environment variables at runtime
- Easily updated without redeploying the application

The `appsettings.Production.json` file contains only non-sensitive connection information:

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "server=cs295mysqlserver.mysql.database.azure.com;database=code_reviews;"
  }
}
```

### How Configuration Works

The `Program.cs` file builds the connection string by combining values from multiple sources:

```csharp
var baseConnectionString = builder.Configuration.GetConnectionString("MySqlConnection");
var user = builder.Configuration["DbUser"];        // From User Secrets (dev) or App Settings (prod)
var password = builder.Configuration["DbPassword"];  // From User Secrets (dev) or App Settings (prod)
var connectionString = $"{baseConnectionString}userid={user};password={password};";
```

ASP.NET Core's configuration system automatically merges settings from multiple sources in priority order:
1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Production.json`)
3. User Secrets (Development environment only)
4. Environment variables (which include Azure App Service settings)

---

## Deploying to Azure

### Prerequisites

- An Azure subscription
- An Azure App Service created
- An Azure Database for MySQL created and configured
- Visual Studio 2022 or later

### Step 1: Configure Azure App Service Environment Variables

Before deploying, you must configure the database credentials in Azure:

1. Navigate to the [Azure Portal](https://portal.azure.com)
2. Open your **App Service** (make sure you're in the App Service, not the database)
3. In the left navigation menu under **Settings**, click **Environment variables**
4. Click on the **App settings** tab (at the top of the Environment variables page)
5. Click **+ Add** button
6. In the slide-out panel, enter:
   - **Name:** `DbUser`
   - **Value:** Your MySQL username (e.g., `adminuser`)
7. Click **Apply**
8. Click **+ Add** again to add the second setting:
   - **Name:** `DbPassword`
   - **Value:** Your MySQL password
9. Click **Apply**
10. Click **Apply** at the bottom of the Environment variables page
11. Click **Confirm** when prompted (this will restart your App Service)

**Note:** If you don't see "Environment variables" in the Settings menu, look for **Configuration** instead, and then look for an "Application settings" section on that page (older portal UI).

### Step 2: Configure Azure MySQL Database Firewall

Ensure your Azure Database for MySQL allows connections from your App Service:

1. In the Azure Portal, navigate to your **Azure Database for MySQL** (this is a separate resource from your App Service)
2. Go to **Settings > Networking** (or **Connection security** in older UI)
3. Either:
   - Check the box for **Allow public access from any Azure service within Azure to this server** (simplest option)
   - Or under **Firewall rules**, add your App Service's outbound IP addresses

### Step 3: Publish from Visual Studio

1. In Visual Studio, right-click on the `CodeReviews` project in **Solution Explorer**
2. Select **Publish...**
3. If this is your first publish:
   - Click **Add a publish profile**
   - Choose **Azure** as the target
   - Select **Azure App Service (Windows)** or **Azure App Service (Linux)**
   - Sign in with your Azure account
   - Select your subscription and App Service
   - Click **Finish**
4. Click the **Publish** button

Visual Studio will:
- Build the application in **Release** configuration
- Automatically use `appsettings.Production.json` settings
- Deploy all files to Azure App Service
- Start the application

### Step 4: Verify Deployment

1. Once publishing completes, Visual Studio will open your browser to the App Service URL
2. Verify the application loads correctly
3. Navigate through the application to ensure database connectivity works
4. Check that reviews are displayed (confirming database connection and seed data)

### Step 5: Monitor and Troubleshoot

If you encounter issues:

1. **View Application Logs:**
   - In Azure Portal, go to your App Service
   - Navigate to **Monitoring > Log stream**
   - Watch for errors or connection issues

2. **Check Environment Variables:**
   - Go back to **Settings > Environment variables > App settings**
   - Verify `DbUser` and `DbPassword` are listed
   - Ensure there are no extra spaces or special characters in the values

3. **Test Database Connection:**
   - Verify the MySQL server firewall/networking allows Azure connections
   - Check that the connection string in `appsettings.Production.json` is correct
   - Ensure the database name matches what's in your connection string

---

## Database Migrations

Entity Framework Core migrations are **NOT automatically applied** when you publish your application to Azure. You need to explicitly handle this.

### When to Apply Migrations

You need to apply migrations in these scenarios:
- **Initial deployment:** Creating the database schema for the first time
- **Schema changes:** After adding new migrations locally (adding/modifying entities, relationships, etc.)
- **After pulling changes:** When team members add new migrations to the repository

### Setting Environment Variables Locally

To run migrations against your Azure database from your local machine, you need to set the `DbUser` and `DbPassword` environment variables. Here's how:

#### Windows PowerShell:

```powershell
$env:DbUser="your_azure_username"
$env:DbPassword="your_azure_password"
```

**Note:** These variables only persist for the current PowerShell session. Once you close the window, they're gone.

#### Windows Command Prompt:

```cmd
set DbUser=your_azure_username
set DbPassword=your_azure_password
```

**Note:** These variables only persist for the current Command Prompt session.

#### To Verify Environment Variables are Set:

**PowerShell:**
```powershell
echo $env:DbUser
echo $env:DbPassword
```

**Command Prompt:**
```cmd
echo %DbUser%
echo %DbPassword%
```

### Applying Migrations Manually (Recommended Approach)

For production deployments, it's best practice to apply migrations manually to your Azure database **before** publishing your application.

#### Using .NET CLI with Production Environment:

1. Open a terminal in your project directory
2. Set the environment variables (see above)
3. Run the migration command:
   ```bash
   dotnet ef database update -- --environment production
   ```

This command:
- Uses your `appsettings.Production.json` for the base connection string
- Reads `DbUser` and `DbPassword` from the environment variables you just set
- Applies all pending migrations to your Azure database

#### Alternative: Using Connection String Directly

If you prefer not to use environment variables, you can specify the complete connection string:

**PowerShell:**
```powershell
Update-Database -ConnectionString "server=cs295mysqlserver.mysql.database.azure.com;database=code_reviews;userid=YOUR_USERNAME;password=YOUR_PASSWORD;SslMode=None;AllowPublicKeyRetrieval=True;"
```

**.NET CLI:**
```bash
dotnet ef database update --connection "server=cs295mysqlserver.mysql.database.azure.com;database=code_reviews;userid=YOUR_USERNAME;password=YOUR_PASSWORD;SslMode=None;AllowPublicKeyRetrieval=True;"
```

**Important Notes:**
- Replace `YOUR_USERNAME` and `YOUR_PASSWORD` with your actual Azure MySQL credentials
- Include `SslMode=None;AllowPublicKeyRetrieval=True;` if you disabled SSL on Azure MySQL
- This connects directly to your Azure database and applies all pending migrations
- You only need to do this when you have new migrations to apply
- Always test migrations on a development/staging database first

#### Recommended Deployment Workflow

**Initial Deployment:**
1. Create your Azure Database for MySQL
2. Set local environment variables with Azure database credentials
3. Apply migrations to the Azure database: `dotnet ef database update -- --environment production`
4. Configure App Service Environment Variables (DbUser, DbPassword) in Azure Portal
5. Configure MySQL firewall to allow Azure connections
6. Publish your application from Visual Studio

**Deploying Schema Changes:**
1. Create new migration locally: `Add-Migration MigrationName`
2. Test migration on your local database
3. Set environment variables for Azure database
4. Apply migration to Azure database: `dotnet ef database update -- --environment production`
5. Publish your updated application

**Advantages:**
- Full control over when migrations run
- Can verify migration success before deploying the app
- No performance impact on application startup
- Easier to troubleshoot if migration fails
- Follows production best practices

**Alternative: Automatic Migrations on Startup**

For development or simple projects, you can have migrations apply automatically when the app starts. Add this code to `Program.cs` before the seeding code:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Apply any pending migrations automatically
    dbContext.Database.Migrate();
    
    // Then seed data
    SeedData.Seed(dbContext);
}
```

?? **Warning:** This approach can cause startup delays and is not recommended for production databases with significant data.

---

## Database Seeding

The application automatically runs seed data on startup via `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Seed(dbContext);
}
```

The `SeedData.Seed()` method checks if data already exists before inserting:

```csharp
if (!context.Reviews.Any())
{
    // Add seed data...
}
```

**Behavior:**
- **First deployment:** Seed data will be added to the database
- **Subsequent deployments/restarts:** If reviews exist, seeding is skipped

**Optional:** To disable seeding in production, you can modify `Program.cs` to only seed during development:

```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData.Seed(dbContext);
    }
}
```

---

## Redeployment

To redeploy after making changes:

1. Make your code changes
2. Commit changes to source control (optional but recommended)
3. Right-click the `CodeReviews` project and select **Publish...**
4. Click **Publish** on your existing publish profile

The application will be redeployed without needing to reconfigure Application Settings (unless you're changing the database credentials).

---

## Alternative: Azure Key Vault (Advanced)

For enhanced security in enterprise scenarios, you can use **Azure Key Vault** to store secrets:

1. Create an Azure Key Vault
2. Store `DbUser` and `DbPassword` as secrets in the Key Vault
3. Enable **Managed Identity** on your App Service
4. Grant the Managed Identity access to the Key Vault
5. Add Key Vault configuration to `Program.cs`:

```csharp
using Azure.Identity;

if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var kvUri = $"https://{keyVaultName}.vault.azure.net";
    
    builder.Configuration.AddAzureKeyVault(
        new Uri(kvUri),
        new DefaultAzureCredential());
}
```

This approach provides:
- Centralized secret management
- Audit logging of secret access
- Automatic secret rotation capabilities
- Role-based access control (RBAC)
