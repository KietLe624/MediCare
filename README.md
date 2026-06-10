# 🏥 MediCare - Hospital Management System

A comprehensive Hospital Management System built with **ASP.NET Core Web API** and **Angular**, designed to streamline hospital operations and improve patient care management.

## 🎯 Overview

MediCare is a full-stack hospital management solution that enables healthcare facilities to efficiently manage:
- Patient information and medical records
- Appointment scheduling
- Doctor and staff management
- Hospital resources and inventory
- Billing and insurance management

Built with modern technologies to ensure scalability, security, and user-friendly interfaces for both administrators and medical professionals.

## ✨ Features

### Core Functionality
- 👥 **Patient Management** - Complete patient profiles and medical history
- 📅 **Appointment Scheduling** - Efficient booking and management system
- 👨‍⚕️ **Doctor Management** - Staff profiles and availability tracking
- 📊 **Dashboard & Analytics** - Real-time insights and reporting
- 🏥 **Department Management** - Organize and manage hospital departments
- 💊 **Inventory Management** - Track medications and medical supplies
- 💰 **Billing System** - Invoice generation and payment tracking
- 🔐 **Role-Based Access Control** - Secure authentication and authorization
- 📱 **Responsive UI** - Works seamlessly on desktop, tablet, and mobile devices

## 🛠️ Technology Stack

### Backend
- **ASP.NET Core** - RESTful Web API framework
- **C#** - Primary backend language (51.6%)
- **SQL Server/Database** - Data persistence

### Frontend
- **Angular** - Modern web application framework
- **TypeScript** - Primary frontend language (23.4%)
- **HTML** - Markup language (23.5%)
- **SCSS/CSS** - Styling (1.4%)
- **Bootstrap/Material UI** - UI components

### Architecture
- **MVC/MVVM Pattern** - Clean separation of concerns
- **RESTful API** - Standard HTTP methods for API communication
- **JWT Authentication** - Secure token-based authentication

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK** 6.0 or higher
- **Node.js** 16.0 or higher
- **npm** or **yarn** package manager
- **SQL Server** 2019 or higher (or SQL Server Express)
- **Git** version control

## 🚀 Installation

### Clone the Repository

```bash
git clone https://github.com/KietLe624/MediCare.git
cd MediCare
```

### Backend Setup (ASP.NET Core)

1. Navigate to the backend directory:
```bash
cd Backend
```

2. Install dependencies:
```bash
dotnet restore
```

3. Configure database connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MediCareDB;Trusted_Connection=true;"
  }
}
```

4. Run database migrations:
```bash
dotnet ef database update
```

5. Start the API server:
```bash
dotnet run
```

The API will run on `https://localhost:5001`

### Frontend Setup (Angular)

1. Navigate to the frontend directory:
```bash
cd Frontend
```

2. Install dependencies:
```bash
npm install
```

3. Configure API endpoint in `environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'
};
```

4. Start the development server:
```bash
ng serve
```

Navigate to `http://localhost:4200/` in your browser.

## 📁 Project Structure

```
MediCare/
├── Backend/
│   ├── Controllers/        # API endpoints
│   ├── Models/            # Data models
│   ├── Services/          # Business logic
│   ├── Data/              # Database context & migrations
│   ├── DTOs/              # Data Transfer Objects
│   └── appsettings.json   # Configuration
├── Frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/    # Reusable components
│   │   │   ├── services/      # API services
│   │   │   ├── models/        # TypeScript interfaces
│   │   │   └── pages/         # Page components
│   │   ├── assets/
│   │   └── environments/      # Environment config
│   └── package.json
├── Database/              # Database scripts (optional)
└── README.md
```

## ⚙️ Configuration

### Environment Variables

Create a `.env` file in the backend directory:

```
DATABASE_URL=your_database_connection_string
JWT_SECRET=your_jwt_secret_key
JWT_EXPIRATION=24
CORS_ORIGINS=http://localhost:4200
```

### API Configuration

Update `appsettings.json` for different environments:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "ExpirationInHours": 24
  }
}
```

## 💻 Usage

### Running the Application

1. **Start the backend API:**
```bash
cd Backend
dotnet run
```

2. **In a new terminal, start the frontend:**
```bash
cd Frontend
ng serve
```

3. **Open your browser** and navigate to `http://localhost:4200`

### Default Credentials

Use the following credentials to log in (if seeded):
- **Username:** admin
- **Password:** admin123

## 📚 API Documentation

API endpoints are documented at:
- Swagger UI: `https://localhost:5001/swagger/index.html`

### Common Endpoints

```
GET    /api/patients              - Get all patients
POST   /api/patients              - Create new patient
GET    /api/patients/{id}         - Get patient details
PUT    /api/patients/{id}         - Update patient
DELETE /api/patients/{id}         - Delete patient

GET    /api/appointments          - Get all appointments
POST   /api/appointments          - Book appointment
GET    /api/doctors               - Get all doctors
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a new branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 📞 Support & Contact

For questions, issues, or suggestions, please:
- Open an [Issue](https://github.com/KietLe624/MediCare/issues)
- Contact the development team

---

**Made with ❤️ by the MediCare Team**
