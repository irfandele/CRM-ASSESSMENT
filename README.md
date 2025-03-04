# CRM Case Management System

## Project Overview
This is a simple CRM Case Management System designed for customer support teams. It allows managing cases via a web interface and supports role-based authentication.

## Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- Microsoft SQL Server (Database should already be set up)

## Setup Instructions
1. **Clone the Repository**
   - Download or clone the project to your local machine.

2. **Ensure Database is Set Up**
   - Make sure you have the `CaseManagementDB` database configured in Microsoft SQL Server.

3. **Configure the Connection String**
   - Open `appsettings.json`.
   - Update the `ConnectionStrings` section with your SQL Server credentials:
     ```json
     "ConnectionStrings": {
      "DefaultConnection": "Server=DESKTOP-1O5P4AA\\SQLEXPRESS;Database=CaseManagementDB;Integrated Security=True;TrustServerCertificate=True;"
     }
     ```

4. **Run the Application**
   - Open the project in Visual Studio 2022.
   - Set `CRM` as the startup project.
   - Press `F5` to run the project.

## Login Credentials
- **Whatsapp:**
  - Username: `Irfan`
  - Password: `admin`
- **Customer Support:**
  - Username: `yasmin`
  - Password: `123`

## Troubleshooting
- If the application fails to connect to the database, ensure that SQL Server is running and the connection string is correct.
- If you encounter a `400 Bad Request` error, verify that the API endpoints match the expected request format.
- Check Visual Studio’s output window for additional debugging information.

## Features
- User Authentication 
- Case Management (CRUD Operations)
- Session-based Login System

## Notes
- This project runs locally using Visual Studio and does not require a live server.
- For further debugging, check the logs in `logs/` if available.

Enjoy developing! 🚀

