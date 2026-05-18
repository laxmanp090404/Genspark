# Community Library Membership & Book Lending System

# 1. Project Overview

The Library Management System is a console-based backend application developed using:

* .NET EF Core (Database First Approach)
* PostgreSQL
* 3-Tier Architecture
* Repository Pattern
* Service Layer Architecture
* Transaction Handling
* Stored Procedures & PostgreSQL Functions

The system manages:

* Members
* Books and editions
* Physical book copies
* Borrowing and returning
* Fine management
* Damage tracking
* Reporting and analytics

---

# 2. Database Design Overview

The schema is divided into:

## Master Tables

### membership_type

* membership_type_id smallint PK
* type_name text UNIQUE CHECK ('Basic', 'Student', 'Premium')
* max_active_borrows smallint NOT NULL
* max_borrow_days smallint NOT NULL
* fine_block_limit numeric(8,2) NOT NULL

### book_category

* category_id smallint PK
* category_name text UNIQUE NOT NULL
* description text
* createdat timestampz NOT NULL DEFAUL NOW


### book

* book_id int PK
* title text NOT NULL
* author text NOT NULL
* isbn_base char(13)
* category_id smallint FK references `book_category`

### book_edition

* edition_id int PK
* book_id int FK references `book`
* edition_number smallint NOT NULL
* edition_label text
* isbn char(13) UNIQUE
* publisher text
* published_year smallint

---

## Transactional Tables

### member

* member_id int PK
* full_name text NOT NULL
* email text UNIQUE NOT NULL
* phone varchar(15) UNIQUE NOT NULL
* address text
* date_of_birth date
* joined_on date NOT NULL
* is_active boolean DEFAULT true
* deleted_at timestamptz
* membership_type_id smallint FK references `membership_type`

### book_copy

* copy_id int PK
* edition_id int FK references `book_edition`
* barcode text UNIQUE
* copy_status text CHECK ('Available', 'Borrowed', 'Damaged_Usable', 'Damaged_Unusable', 'Lost', 'Retired')
* acquired_on date

### borrowing

* borrowing_id int PK
* member_id int FK references `member`
* copy_id int FK references `book_copy`
* borrowed_on date NOT NULL
* due_date date NOT NULL
* returned_on date
* borrow_status text CHECK ('Active', 'Returned', 'Lost')

### fine

* fine_id int PK
* borrowing_id int UNIQUE FK references `borrowing`
* member_id int FK references `member`
* days_overdue smallint NOT NULL
* fine_per_day numeric(6,2) NOT NULL
* fine_status text CHECK ('Pending', 'Paid', 'Waived')
* waiver_reason text

### fine_payment

* payment_id int PK
* fine_id int UNIQUE FK references `fine`
* amount_paid numeric(8,2) NOT NULL
* paid_on timestamptz NOT NULL
* payment_mode text CHECK ('Cash', 'UPI', 'Card', 'Online')
* remarks text

### book_damage_log

* damage_log_id int PK
* copy_id int FK references `book_copy`
* member_id int FK references `member` (nullable)
* reported_on date NOT NULL
* damage_description text NOT NULL
* damage_severity text CHECK ('Minor', 'Moderate', 'Severe')
* resulting_status text CHECK ('Damaged_Usable', 'Damaged_Unusable', 'Lost')
* fine_applied numeric(8,2)



# 6. Business Functionalities Implemented

---

# Member Management

Features:

* Add member
* Update member
* Search member
* Deactivate member
* Membership type handling

---

# Book Management

Features:

* Add books
* Add editions
* Add physical copies
* Browse catalog
* Inventory management
* Book searching

Search options:

* by title
* by author
* by category
* smart search

---

# Borrowing System

Features:

* Borrow books
* Return books
* Lost book handling
* Damaged return handling
* Due date management

---

# Fine Management

Features:

* Fine calculation
* Fine history
* Fine payment
* Student fine waiver
* Payment mode tracking

---

# Damage Management

Features:

* Damage logging
* Damage severity tracking
* Lost book handling
* Copy status updates

---

# Reports Module

Features:

* Currently borrowed books
* Overdue books
* Members with pending fines
* Most borrowed books
* Borrowing history
* Available books by category

---

# 7. PostgreSQL Procedure & Function

---

# Stored Procedure — pay_fine

Purpose:
Marks a fine as paid. 

Used from:

* EF Core
* Fine payment workflow

---

# PostgreSQL Function — calculate_member_fine

Purpose:
Calculates total pending fine for a member. 

Logic:

* sums overdue fine amounts
* returns total unpaid fine

Used from:

* EF Core backend service
* fine management module

---


