# HomeCare Platform

HomeCare connects customers with verified professionals for home services. The platform features seamless scheduling, secure checkout with discount support, and comprehensive partner onboarding. Admins manage service catalogs, user profiles, and track real-time analytics via a central dashboard.

## 🚀 Key Features

### For Customers
* **Passwordless Authentication:** Secure OTP-based login via email.
* **Service Discovery:** Search and filter through categories, sub-categories, and specific services.
* **Smart Booking Engine:** Select interactive map-based addresses, dates, and available time slots.
* **Secure Checkout:** Support for cash and online card payments with dynamic coupon code application.
* **Booking Management:** Track upcoming and completed bookings, view assigned expert details, and download invoices.

### For Service Partners
* **Comprehensive Onboarding:** Multi-step registration capturing personal details, education, professional experience, skills, and document uploads.
* **Job Management:** Track assigned service requests, customer locations, and completed jobs.
* **Profile Management:** Manage service offerings and availability.

### For Administrators
* **Real-Time Dashboard:** SignalR-powered dashboard showing live revenue trends, top performing services, and booking metrics with time filters.
* **Master Data Management:** Full CRUD control over Service Types, Categories, and individual services.
* **User & Partner Management:** Approve/reject service partner applications, block/delete users, and manage admin roles.
* **Order Lifecycle Control:** Reassign experts, cancel, or mark bookings as completed.
* **Offer Management:** Create and track fixed-percentage discount coupons.

## 💻 Tech Stack

* **Backend:** .NET Web API
* **Database:** SQL Server
* **Authentication:** JWT (JSON Web Tokens)
* **Real-time Communication:** SignalR
* **Frontend:** Modern Web UI (HTML/CSS/JS, Chart.js for analytics)

## 🛠️ Architecture & Database Guidelines

* **Auditable Data:** All major database tables include tracking columns (`CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`).
* **Relational Integrity:** Strict foreign key constraints prevent orphaned records (e.g., cannot delete a category if services are attached).
* **ID Formatting:** Primary keys utilize standard auto-incrementing integers, with formatted display IDs (e.g., `001`, `012`) rendered via backend DTOs.
* **UI Consistency:** Standardized right-side filter panels, grid pagination/sorting globally, and confirmation modals for all state changes.

### Installation Steps

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/HomeCare.git](https://github.com/your-username/HomeCare.git)
   cd HomeCare
