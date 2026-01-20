# Trucks Weighing Web App

A production-ready ASP.NET Core MVC web application for truck-based cargo weight control used by cargo surveyors and tally teams during loading and discharging operations at ports, terminals, warehouses, and silos.

The application supports real-time multi-user workflows, tracking each truck from initial weighing through cargo operations to final weighing, with automated totals and PDF reporting.

## Project highlights

- Production-ready ASP.NET Core MVC application, deployed on Linux (Nginx + systemd)
- Real-world inspection workflow used during cargo operations (ports / terminals)
- Authentication & authorization:
  - ASP.NET Core Identity
  - Email confirmation, password recovery
  - Roles (User / Admin) 
- Admin panel for inspections, statistics, and feedback management
- Multi-user concurrent input (weighbridge operator + cargo operations operator)
- State-driven truck lifecycle tracking:
  - Initial weighing
  - Cargo operations started / finished
  - Final weighing
- Excel-like operational UI, optimized for fast data entry
- Responsive and mobile-friendly design
- Server-side data validation & consistency
- Pagination and server-side filtering for large truck datasets
- PDF export (daily and period-based tally sheets with totals and balances)
- Persistent Data Protection keys (cookies remain valid after restarts)
- Deployed and running in production

## Features

### Inspections

- Create inspections with vessel, cargo, location, timezone
- Optional inspection logo and notes

**Truck workflow**

- Initial and final weight capture
- Automatic net weight calculation
- Plate number autocomplete from historical data

**Operational safety logic**

- Trucks must pass initial weighing before cargo operations
- Visual separation of truck states

**Truck status dashboard**

- Waiting for cargo operations
- Under cargo operations
- Waiting for final weighing
- Completed

**Team collaboration**

- Multiple users working on the same inspection simultaneously
- Shared real-time status visibility

**Reporting**

- PDF export using QuestPDF
- Export entire inspection or selected date/time range
- Daily summaries and balance figures for selected period or from the beginning


## How it works (real workflow)

- Create an inspection (vessel, cargo, location, timezone, optional logo & notes)
- At the weighbridge, record initial truck weight
- At the cargo operations point, mark start and finish of loading/discharging
- Perform final weighing at the weighbridge
- Review totals and balance figures
- Export a PDF tally sheet for reporting or handover

## Screenshots

![Screenshot 2026-01-20 170525](https://github.com/user-attachments/assets/afea8ff1-2bc7-45ea-bf04-052000517d68)

![Screenshot 2026-01-20 170540](https://github.com/user-attachments/assets/86d41867-8b82-416e-838d-6d297f44b087)

![Screenshot 2026-01-20 170602](https://github.com/user-attachments/assets/12516f24-c987-4cf0-98b6-c765cce5f7c8)

![Screenshot 2026-01-20 170626](https://github.com/user-attachments/assets/625d66fa-84e0-4a08-acf8-e4444e4e664f)

![Screenshot 2026-01-20 170645](https://github.com/user-attachments/assets/99c4a452-1d02-4789-a285-c7f4c965a3aa)

![Screenshot 2026-01-20 170720](https://github.com/user-attachments/assets/9f0fc127-909f-4248-857b-3e29f2cd3155)

![Screenshot 2026-01-20 170733](https://github.com/user-attachments/assets/6f773ff8-2e44-4f3e-9370-a58113e03445)

![Screenshot 2026-01-20 170802](https://github.com/user-attachments/assets/82f3e9d9-5f77-4d66-82f9-bfb61c9b128f)

![Screenshot 2026-01-20 170825](https://github.com/user-attachments/assets/77aedf7e-ea74-4b1c-b736-9363405527d9)

![Screenshot 2026-01-20 170834](https://github.com/user-attachments/assets/4ba19257-e4de-49c8-b21e-9dde6c0a04e3)

## Technology stack

**Backend / Web**

- ASP.NET Core MVC + Razor Views
- ASP.NET Core Identity + Roles
- Entity Framework Core (Code First, migrations)
- PostgreSQL
- AutoMapper

**Frontend / UI**

- ASP.NET Core MVC with Razor Views
- Server-side validation with domain rules
- Bootstrap-based responsive layout

**Infrastructure / Deployment**

- Linux server deployment
- Nginx reverse proxy
- systemd services
- Persistent Data Protection keys
- Email delivery: SendGrid
- PDF generation: QuestPDF

## Project structure

```text
TrucksWeighingWebApp/
├─ Areas/
│  └─ Identity/                         # ASP.NET Core Identity UI
│
├─ Controllers/
│  ├─ Admin/                            # Admin-only endpoints
│  ├─ HomeController.cs
│  ├─ InspectionsController.cs          # Core inspections workflow
│  ├─ TruckRecordsController.cs         # Truck weighing records
│  ├─ GalleryController.cs
│  └─ ContactController.cs
│
├─ Data/
│  └─ ApplicationDbContext.cs            # EF Core DbContext
│
├─ DTOs/
│  └─ Export/                           # Excel / PDF export DTOs
│
├─ Infrastructure/
│  ├─ Identity/                         # Role & admin seeding
│  │  ├─ IdentitySeed.cs
│  │  ├─ RoleNames.cs
│  │  └─ SeedOptions.cs
│  ├─ Telemetry/
│  │  └─ UserSessionMiddleware.cs       # Session & activity tracking
│  └─ TimeZone/
│     └─ Tz.cs
│
├─ Interfaces/
│
├─ Mappings/
│  └─ MappingProfile.cs                 # AutoMapper profiles
│
├─ Models/
│  ├─ ApplicationUser.cs
│  ├─ Inspection.cs
│  ├─ TruckRecord.cs
│  ├─ UserSession.cs
│  ├─ UserLogo.cs
│  ├─ FeedbackTicket.cs
│  ├─ ErrorViewModel.cs
│  └─ SendGridOptions.cs
│
├─ Services/
│  ├─ Auth/
│  │  └─ AppCookieEvents.cs             # Login counters
│  ├─ Export/
│  │  ├─ TruckExcelExporter.cs
│  │  └─ TruckPdfExporter.cs
│  ├─ Logos/
│  │  └─ UserLogoService.cs
│  └─ SendGridEmailService.cs
│
├─ ViewModels/
│  ├─ InspectionCreateViewModel.cs
│  ├─ InspectionEditViewModel.cs
│  ├─ TruckRecordCreateViewModel.cs
│  ├─ TruckRecordEditViewModel.cs
│  ├─ PeriodStatsViewModel.cs
│  └─ FeedbackTicketViewModel.cs
│
├─ Views/
│  ├─ Inspections/
│  ├─ TruckRecords/
│  ├─ Gallery/
│  ├─ Stats/
│  ├─ Home/
│  └─ Shared/
│     ├─ _Layout.cshtml
│     ├─ _LoginPartial.cshtml
│     └─ Error.cshtml
│
├─ wwwroot/
│  └─ uploads/
│     └─ logos/                         # User-uploaded logos
│
├─ keys/                                # Data Protection keys
│
├─ Migrations/
├─ appsettings.json
├─ appsettings.Development.json
├─ appsettings.Production.json
├─ Program.cs                           # DI + middleware pipeline
└─ TrucksWeighingWebApp.csproj
```

## Project status

This is a live, deployed, and actively used application, built as practical tool for the cargo surveyors and tally teams that helps control cargo weight during truck operations at terminals - in ports, warehouses, and silos (loading or discharging).
