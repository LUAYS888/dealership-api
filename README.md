# Dealership Management API

This project is a simple but secure **car dealership management system** built with **.NET 9 (ASP.NET Core Web API)**.  
It demonstrates how to build an API with **CRUD operations**, **role-based access control**, and **OTP protection** for sensitive actions.

The goal is to show professional API design and security best practices in a small project that can run locally with minimal setup.

---

## How to Run

1. Clone this repository:
   git clone https://github.com/LUAYS888/dealership-api.git
   cd dealership-api

2. Restore packages:
   dotnet restore

3. Apply migrations (creates the SQLite database):
   dotnet ef database update

4. Start the API:
   dotnet run

5. Open Swagger UI in your browser:
   https://localhost:7272/swagger

---

## Default Data

On first run the system seeds:

- 10 sample vehicles (for browsing and purchasing)
- One Admin user:

  Email:    luay@local.test  
  Password: Admin@12345

When you log in, an OTP code will be printed in the console output (simulating SMS/email). You need that OTP to complete login or registration.

---

## Authentication Flow

The API uses JWT tokens for authentication.

- Register  
  • POST /api/auth/register/start → generates OTP (console)  
  • POST /api/auth/register/verify?password=YourPassword → creates account  

- Login  
  • POST /api/auth/login/start → generates OTP (console)  
  • POST /api/auth/login/verify → returns JWT token  

Once you have the token:  
- In Swagger, click **Authorize** at the top.  
- Paste the token (Swagger automatically adds Bearer).  
- Now you can access Admin or Customer endpoints depending on your role.

---

## Available Endpoints

### Auth
- POST /api/auth/register/start
- POST /api/auth/register/verify
- POST /api/auth/login/start
- POST /api/auth/login/verify

### Vehicles
- GET /api/vehicles – browse with filters
- GET /api/vehicles/{id} – view details

### Admin (requires Admin role)
- POST /api/admin/vehicles – add vehicle
- POST /api/admin/vehicles/{id}/update/start – request OTP for update
- PUT /api/admin/vehicles/{id}/update/verify?otp=CODE – confirm update with OTP
- GET /api/admin/users – list all customers
- POST /api/sales – process a sale

### Customer
- POST /api/purchase-requests/start – start a purchase (OTP)
- POST /api/purchase-requests/confirm – confirm purchase with OTP
- GET /api/purchase-requests/mine – view your requests
- GET /api/sales/mine – view your sales history

---

## Key Features

- Role-based authentication (Admin / Customer) with JWT  
- OTP protection on registration, login, vehicle updates, and purchase requests  
- Passwords stored securely using PBKDF2 hashing  
- Vehicles and sales managed with EF Core + SQLite  
- Input validation handled with FluentValidation  
- Global error handling returning Problem Details (RFC 7807)  
- Swagger/OpenAPI documentation with built-in Authorize button  

---

## Assumptions & Design Decisions

- **SQLite chosen for simplicity**  
  SQLite keeps the project self-contained and easy to run without extra setup. In production, a more robust database like SQL Server or PostgreSQL would be preferable.

- **OTP delivery simulated**  
  Instead of integrating with SMS or email services, OTPs are written to the console. This keeps the project simple but still demonstrates the full OTP flow.

- **Password hashing with PBKDF2**  
  A secure algorithm (PBKDF2 via Rfc2898DeriveBytes) is used to hash passwords. This avoids storing plain text passwords.

- **Pre-seeded admin account**  
  An admin user is seeded on startup to make evaluation easier. In real applications, admins should be provisioned more securely (environment config or identity provider).

- **JWT claims kept minimal**  
  Tokens include only `sub`, `email`, and `role`. This balances simplicity with role-based access control.

- **Validation first**  
  All inputs are validated using FluentValidation before hitting business logic. This avoids dirty data and ensures predictable behavior.

- **Global error handling**  
  Centralized middleware converts exceptions into standard Problem Details JSON (RFC 7807). This provides consistency for clients.

- **Role-based authorization attributes**  
  Controllers use `[Authorize(Roles="Admin")]` or `[Authorize(Roles="Customer")]` to clearly enforce boundaries.

---

## What Could Be Added Next

- A Dockerfile for containerized deployment  
- CI/CD pipeline with GitHub Actions  
- Advanced logging with Serilog  
- More detailed vehicle filtering (price ranges, availability)  
- Unit tests and integration tests  




