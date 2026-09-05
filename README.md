# 🔐 Secure JWT-Authenticated ASP.NET Core Web API

A production-oriented **RESTful Web API** built with **ASP.NET Core 8**, implementing secure authentication and role-based authorization using **ASP.NET Core Identity** and **JWT Bearer tokens**.

The project demonstrates how to build a secure API with user registration, authentication, protected resources, role-based access control, and automatic administrator seeding.

---

## 🚀 Overview

This project demonstrates a practical implementation of **JWT-based authentication and role-based authorization** in ASP.NET Core.

Authenticated users can access their own profile information, while users assigned the **Admin** role can access administrative resources.

The API follows modern ASP.NET Core security practices and provides an interactive **Swagger/OpenAPI** interface for testing authenticated endpoints.

### Key Security Concepts

* 🔑 User registration and authentication
* 🎫 JWT Bearer token authentication
* 🛡️ Role-based authorization
* 👤 ASP.NET Core Identity user management
* 🔒 Protected API endpoints
* 👑 Admin-only resources
* 📖 Swagger/OpenAPI authentication support
* 🌱 Automatic database and admin-user seeding

---

## 🧰 Technology Stack

| Technology                    | Purpose                       |
| ----------------------------- | ----------------------------- |
| **.NET 8**                    | Application framework         |
| **ASP.NET Core Web API**      | REST API development          |
| **C#**                        | Primary programming language  |
| **Entity Framework Core**     | ORM and data access           |
| **SQL Server / LocalDB**      | Relational database           |
| **ASP.NET Core Identity**     | User and role management      |
| **JWT Bearer Authentication** | Stateless API authentication  |
| **Swagger / OpenAPI**         | API documentation and testing |

---

## ✨ Features

### 🔓 Public Endpoints

Users can create an account and authenticate without providing a JWT.

| Method | Endpoint             | Description                    |
| ------ | -------------------- | ------------------------------ |
| `POST` | `/api/auth/register` | Register a new user            |
| `POST` | `/api/auth/login`    | Authenticate and receive a JWT |

---

### 🔒 Protected Endpoints

These endpoints require a valid JWT Bearer token.

| Method | Endpoint             | Authorization      | Description                         |
| ------ | -------------------- | ------------------ | ----------------------------------- |
| `GET`  | `/api/users/profile` | Authenticated User | Retrieve the current user's profile |

---

### 👑 Admin Endpoint

The following endpoint requires both a valid JWT and the **Admin** role.

| Method | Endpoint               | Authorization | Description                         |
| ------ | ---------------------- | ------------- | ----------------------------------- |
| `GET`  | `/api/admin/dashboard` | `Admin`       | Access the administrative dashboard |

---

## 🔐 Authentication Flow

The authentication process follows a standard JWT-based workflow:

```text
┌──────────────┐
│    Client    │
└──────┬───────┘
       │
       │ Register
       ▼
┌─────────────────────┐
│  ASP.NET Core API   │
│   + Identity        │
└──────────┬──────────┘
           │
           │ Login
           ▼
┌─────────────────────┐
│   JWT Access Token  │
└──────────┬──────────┘
           │
           │ Authorization: Bearer <token>
           ▼
┌─────────────────────┐
│ Protected Endpoint  │
└──────────┬──────────┘
           │
           ▼
     Authorized Request
```

### Example Request

After logging in, include the JWT in the `Authorization` header:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

ASP.NET Core validates the token before allowing access to protected resources.

---

## 🌱 Automatic Admin Seeding

The application automatically creates an administrator account and assigns the `Admin` role during application startup.

### Default Admin

```text
Email:    admin@example.com
Password: Admin123!
Role:     Admin
```

> ⚠️ **Security Notice:** These credentials are intended for local development and demonstration purposes only. Never use default credentials in a production environment. Production applications should store credentials and secrets securely using environment variables, Azure Key Vault, user secrets, or another secure secrets-management solution.

---

## 🛠️ Getting Started

### Prerequisites

Before running the application, make sure you have:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server LocalDB or another SQL Server instance
* Git
* An API client such as Swagger UI, Postman, or Insomnia

---

### 1. Clone the Repository

```bash
git clone <YOUR_REPOSITORY_URL>
```

Navigate into the project:

```bash
cd SecureJwtApi
```

---

### 2. Configure the Database

Update the connection string in:

```text
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SecureJwtApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

### 3. Configure JWT Settings

Configure your JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YOUR_DEVELOPMENT_SECRET_KEY",
    "Issuer": "SecureJwtApi",
    "Audience": "SecureJwtApiUsers",
    "DurationInMinutes": 60
  }
}
```

> ⚠️ Do not commit production secrets or JWT signing keys to source control.

For production environments, use secure configuration such as environment variables, Azure Key Vault, or another secrets-management solution.

---

### 4. Apply Database Migrations

Run:

```bash
dotnet ef database update
```

If Entity Framework tools are not installed:

```bash
dotnet tool install --global dotnet-ef
```

---

### 5. Run the Application

```bash
dotnet run
```

The API will start on the configured HTTP/HTTPS ports.

---

## 📖 Swagger / OpenAPI

Once the application is running, open the Swagger UI using the URL configured by your application.

Swagger allows you to:

* View available API endpoints
* Register users
* Authenticate users
* Obtain JWT access tokens
* Authorize Swagger using a Bearer token
* Test protected endpoints
* Test Admin-only endpoints

### Using JWT Authentication in Swagger

1. Call `/api/auth/login`
2. Copy the returned JWT token
3. Click **Authorize** in Swagger
4. Enter:

```text
Bearer YOUR_JWT_TOKEN
```

5. Execute the protected endpoint.

---

## 🗂️ Project Structure

A typical structure for the project is:

```text
SecureJwtApi/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   └── AdminController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   └── DTOs/
│
├── Migrations/
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── SecureJwtApi.csproj
```

---

## 🧪 API Testing

### Register

```http
POST /api/auth/register
Content-Type: application/json
```

Example:

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json
```

Example:

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

The API returns a JWT access token that can be used to access protected resources.

---

## 🎯 Learning Objectives

This project was built to demonstrate practical understanding of:

* ASP.NET Core Web API development
* RESTful API design
* Authentication vs. authorization
* ASP.NET Core Identity
* JWT token generation and validation
* Role-based authorization
* Entity Framework Core
* SQL Server database integration
* Dependency Injection
* Middleware configuration
* Swagger/OpenAPI
* Database migrations
* Secure application configuration

---

## 🔮 Future Improvements

Potential enhancements include:

* [ ] Refresh token implementation
* [ ] Email verification
* [ ] Password reset functionality
* [ ] Account lockout policies
* [ ] Two-factor authentication
* [ ] Rate limiting
* [ ] Global exception handling
* [ ] Structured logging
* [ ] Docker containerization
* [ ] Automated unit and integration tests
* [ ] CI/CD pipeline with GitHub Actions
* [ ] Azure deployment
* [ ] Azure Key Vault integration

---

## 👨‍💻 Author

**Thabang Rakgoropo**

Software Developer focused on building secure, scalable applications with the **Microsoft .NET ecosystem**.

### Core Areas

* C# / .NET
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* REST APIs
* Authentication & Authorization
* Docker
* Azure
* Infrastructure as Code

---

## 📄 License

This project is intended for educational and portfolio purposes.
