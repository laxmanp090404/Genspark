# Notification System - 3 Tier Architecture - EFCore CodeFirst Database Integration

## Overview
This project is a simple console-based notification system built using a 3-tier architecture.

- The front-end layer is the console application in NOTIFICATIONFE.
- The business logic layer is in NOTIFICATIONBL, where user input is validated and notification flow is managed.
- The data access layer is in NOTIFICATIONDALL, which manages database operations using Entity Framework Core with CodeFirst approach.
- The model layer is in NOTIFICATIONMODELS, which contains the entities, notification types, and custom exceptions.

The application allows you to create users, view users, update and delete users, send email or SMS notifications, and view stored notifications.

## Database Integration - EFCore CodeFirst Approach
- Implemented database integration using **Entity Framework Core with CodeFirst approach**.
- Database models are defined as entity classes in the project, and the database schema is automatically generated from these models.
- The DbContext class manages entity configurations and database operations seamlessly.
- Sample output could be found on [Output file](output.txt)

## Project Structure 

```text
NOTIFICATION-3TIER/
├── NOTIFICATIONFE/
│   ├── Program.cs
│   └── NotificationApp.csproj
├── NOTIFICATIONBL/
│   ├── Interfaces/
│   │   ├── INotificationService.cs
│   │   └── IUserService.cs
│   ├── Services/
│   │   ├── NotificationService.cs
│   │   └── UserService.cs
│   └── Validators/
│       ├── MessageValidator.cs
│       └── UserDetailValidator.cs
├── NOTIFICATIONDALL/
│   ├── Context/
│   │   └── NotificationContext.cs
│   ├── Interfaces/
│   │   └── IRepository.cs
│   └── Repositories/
│       ├── AbstractRepository.cs
│       ├── NotificationRepository.cs
│       └── UserRepository.cs
|       
├── NOTIFICATIONMODELS/
│   ├── Exceptions/
│   │   ├── InvalidMessageException.cs
│   │   ├── InvalidUserDetailsException.cs
│   │   └── NotFoundException.cs
│   └── Models/
│       ├── EmailNotification.cs
│       ├── Notification.cs
│       ├── SMSNotification.cs
│       ├── User.cs
│       └── User.Ops.cs
└──  output.txt
```

## Each File Function

### NOTIFICATIONFE
- [NOTIFICATIONFE/Program.cs](NOTIFICATIONFE/Program.cs): Starts the console application, shows the menu, accepts user choices, and calls the service layer.

### NOTIFICATIONBL
- [NOTIFICATIONBL/Interfaces/INotificationService.cs](NOTIFICATIONBL/Interfaces/INotificationService.cs): Defines the notification-related business operations.
- [NOTIFICATIONBL/Interfaces/IUserService.cs](NOTIFICATIONBL/Interfaces/IUserService.cs): Defines the user-related business operations.
- [NOTIFICATIONBL/Services/NotificationService.cs](NOTIFICATIONBL/Services/NotificationService.cs): Handles notification creation, sending, and retrieval logic.
- [NOTIFICATIONBL/Services/UserService.cs](NOTIFICATIONBL/Services/UserService.cs): Handles create, read, update, and delete operations for users.
- [NOTIFICATIONBL/Validators/MessageValidator.cs](NOTIFICATIONBL/Validators/MessageValidator.cs): Extension methods that Validates notification messages and SMS length and throw exceptions.
- [NOTIFICATIONBL/Validators/UserDetailValidator.cs](NOTIFICATIONBL/Validators/UserDetailValidator.cs): Extension methods Validates user email and phone details and throw exceptions.
- [NOTIFICATIONBL/NOTIFICATIONBL.csproj](NOTIFICATIONBL/NOTIFICATIONBL.csproj): Project file for the business logic layer.

### NOTIFICATIONDALL
- [NOTIFICATIONDALL/Context/NotificationContext.cs](NOTIFICATIONDALL/Context/NotificationContext.cs): Entity Framework Core DbContext that defines entity sets and database configurations. Manages the connection to the database and handles entity mappings.
- [NOTIFICATIONDALL/Interfaces/IRepository.cs](NOTIFICATIONDALL/Interfaces/IRepository.cs): Generic repository contract used by the data layer for abstraction.
- [NOTIFICATIONDALL/Repositories/AbstractRepository.cs](NOTIFICATIONDALL/Repositories/AbstractRepository.cs): Abstract base repository implementing the generic repository interface using EFCore. Provides common CRUD operations for all concrete repositories.
- [NOTIFICATIONDALL/Repositories/UserRepository.cs](NOTIFICATIONDALL/Repositories/UserRepository.cs): Handles database operations related to users using EFCore:
    - Create user
    - Fetch all active users
    - Fetch user by ID
    - Update user details
    - Soft delete users using isdeleted flag
    - Uses EFCore LINQ queries and SaveChanges() for persistence
- [NOTIFICATIONDALL/Repositories/NotificationRepository.cs](NOTIFICATIONDALL/Repositories/NotificationRepository.cs): Handles database operations related to notifications using EFCore CodeFirst approach.
- [NOTIFICATIONDALL/NOTIFICATIONDALL.csproj](NOTIFICATIONDALL/NOTIFICATIONDALL.csproj): Project file for the data access layer.

### NOTIFICATIONMODELS
- [NOTIFICATIONMODELS/Models/Notification.cs](NOTIFICATIONMODELS/Models/Notification.cs): Abstract base class for notifications and notification type enum.
- [NOTIFICATIONMODELS/Models/EmailNotification.cs](NOTIFICATIONMODELS/Models/EmailNotification.cs): Email notification implementation and Polymorphism happens.
- [NOTIFICATIONMODELS/Models/SMSNotification.cs](NOTIFICATIONMODELS/Models/SMSNotification.cs): SMS notification implementation and Polymorphism happens.
- [NOTIFICATIONMODELS/Models/User.cs](NOTIFICATIONMODELS/Models/User.cs): Partial class with User entity definition.
- [NOTIFICATIONMODELS/Models/User.Ops.cs](NOTIFICATIONMODELS/Models/User.Ops.cs):Partial class User helper methods such as comparison and string formatting.
- [NOTIFICATIONMODELS/Exceptions/InvalidMessageException.cs](NOTIFICATIONMODELS/Exceptions/InvalidMessageException.cs): Exception raised for invalid notification messages.
- [NOTIFICATIONMODELS/Exceptions/InvalidUserDetailsException.cs](NOTIFICATIONMODELS/Exceptions/InvalidUserDetailsException.cs): Exception raised for invalid user details.
- [NOTIFICATIONMODELS/Exceptions/NotFoundException.cs](NOTIFICATIONMODELS/Exceptions/NotFoundException.cs): Exception raised when a requested entity is not found.
- [NOTIFICATIONMODELS/NOTIFICATIONMODELS.csproj](NOTIFICATIONMODELS/NOTIFICATIONMODELS.csproj): Project file for the model layer.

## Sample Output
Sample output from a run of the application is available in [output.txt](output.txt).
