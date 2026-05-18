-- Master tables

-- membership type
create table if not exists membership_type(
membership_type_id smallint primary key generated always as identity,
type_name text not null unique,
max_active_borrows smallint not null constraint chk_active_borrows check(max_active_borrows > 0),
max_borrow_days smallint not null constraint chk_max_day check(max_borrow_days>0),
fine_block_limit numeric(6,2) not null default 500.00 constraint chk_fine_limit check(fine_block_limit > 0),
createdat timestamp not null default now()
);

-- book_category
create table if not exists book_category(
category_id smallint primary key generated always as identity,
category_name text not null unique constraint chk_category check(length(trim(category_name)) between 1 and 100),
description text ,--nullable
createdat timestamp not null default now()
);

-- book 
create table if not exists book(
book_id int primary key generated always as identity,
category_id smallint not null constraint fk_book_to_category references book_category(category_id),
title text not null constraint chk_title check(length(trim(title)) between 1 and 100),
author text not null constraint chk_author check(length(trim(author)) between 1 and 70),
isbn_base char(13),
createdat timestamp not null default now(),
constraint uniq_authorandtitle unique(author,title)
);

create index idx_book_title on book(lower(title));
create index idx_book_author on book(lower(author));
create index idx_book_category on book(category_id);

-- book editon
create table if not exists book_edition(
edition_id int primary key generated always as identity,
book_id int  not null constraint fk_bookeditiontobook references book(book_id),
edition_number int not null constraint chk_edition check(edition_number > 0),
edition_label text, --nullable/optional
isbn char(13) constraint unique_isbn unique,
publisher text ,
published_year smallint check(published_year between 1500 and 2100),
createdat timestamp not null default now(),
constraint uq_bookediton unique(book_id,edition_number)
);


-- TransactionalTables

-- member
create table if not exists members(
member_id int primary key generated always as identity,
membership_type_id smallint not null constraint fk_membertotype references membership_type(membership_type_id),
full_name text not null constraint chk_membername check(length(trim(full_name)) between 1 and 100),
email text not null unique,
phone char(10) not null unique,
address text,
date_of_birth date constraint chk_dob check(date_of_birth < current_date),
joined_on date not null default current_date ,
is_active boolean not null default true,
deletedat timestamp,
createdat timestamp not null default now(),
updatedat timestamp not null default now()
);

create index idx_member_email on members(lower(email)) where deletedat is null;
create index idx_member_phone on members(phone) where deletedat is null;
create index idx_member_active on members(is_active) where deletedat is null;

-- book copy
create table if not exists book_copy(
copy_id int primary key generated always as identity,
edition_id int not null constraint fk_copytoedition references book_edition(edition_id),
copy_status text not null default 'Available' constraint chk_copy_status check(copy_status in (
'Available','Borrowed', 'Damaged_Usable',-- Damaged but can still be lent
'Damaged_Unusable',  -- Damaged beyond lending
'Lost')),
acquired_on date,
createdat timestamp not null default now(),
updatedat timestamp not null default now()
);
create index idx_copy_edition_status on book_copy (edition_id, copy_status);

--book_damage_log
create table if not exists book_damage_log(
damage_log_id int primary key generated always as identity,
copy_id int not null constraint fk_damagetocopy references book_copy(copy_id),
member_id int references members(member_id),
reported_on date not null default current_date,
damage_description text not null constraint chk_damage_desc check (length(trim(damage_description)) > 0),
damage_severity text not null constraint chk_damage_type check (damage_severity in ('Minor', 'Moderate', 'Severe')),
resulting_status text not null constraint chk_resulting_status check (resulting_status in ('Damaged_Usable', 'Damaged_Unusable', 'Lost')),
fine_applied numeric(7,2) constraint chk_fineapplied check(fine_applied >=0),
createdat timestamp not null default now()
);

create index idx_damage_copy on book_damage_log (copy_id);
create index idx_damage_member on book_damage_log (member_id) where member_id is not null;

-- borrowing
create table if not exists borrowing(
borrowing_id int primary key generated always as identity,
member_id int not null constraint fk_borrowingtomembers references members(member_id),
copy_id int not null constraint fk_borrowingtocopy references book_copy(copy_id),
borrowed_on date not null default current_date ,
due_date date not null constraint chk_due check(due_date > borrowed_on),
returned_on date constraint chk_return check(returned_on >=borrowed_on),
borrow_status text not null default 'Active' constraint chk_borrowstatus check(borrow_status in ('Active', 'Returned', 'Lost')),
createdat timestamp not null default now(),
updatedat timestamp not null default now(),
constraint uq_copy_active_borrow unique(copy_id, borrow_status),
constraint chk_returned_when_status check ((borrow_status ='Returned' and returned_on is not null)or(borrow_status != 'Returned'))
);

--fine
create table if not exists fine (
fine_id int primary key generated always as identity,
borrowing_id int not null unique constraint fk_finetoborrow references borrowing(borrowing_id),
member_id int not null constraint fk_finetomember references members(member_id),
days_overdue smallint not null check (days_overdue > 0),
fine_per_day numeric(6,2) not null default 10.00 check (fine_per_day > 0),
fine_status text not null default 'pending'
check (fine_status in ('pending', 'paid', 'waived')),
waiver_reason text,
created_at timestamp not null default now(),
updated_at timestamp not null default now(),

constraint chk_waiver_requires_reason check (fine_status != 'waived' or waiver_reason is not null)
);

create index idx_fine_member_status on fine (member_id, fine_status);

-- fine payment
create table if not exists fine_payment (
 payment_id int primary key generated always as identity,
 fine_id int not null unique constraint fk_finepaytofine references fine(fine_id),
 amount_paid numeric(8,2) not null constraint chk_fine_payment check (amount_paid > 0),
 paid_on timestamptz not null default now(),
 payment_mode text not null default 'cash'
 constraint chk_fine_payment_mode check (
  payment_mode in ('cash', 'upi', 'card', 'online')
 ),
 remarks text,
 created_at timestamp not null default now()
);

create index idx_fine_payment_fine on fine_payment (fine_id);

--------------------------------------------------------------------------------------------------------------------
-- Seed data

-- seed data for membershiptypes
insert into membership_type(type_name,max_active_borrows,max_borrow_days) values('Basic',2,7),('Student',3,10),
('Premium',5,15);

-- seed data for categories

insert into book_category (category_name, description)
values
('Fiction', 'Stories created from imagination including novels and short stories'),
('Non-Fiction', 'Books based on real events, facts, and information'),
('Science Fiction', 'Futuristic stories involving science and technology'),
('Fantasy', 'Books featuring magic and imaginary worlds'),
('Mystery', 'Stories focused on solving crimes or uncovering secrets'),
('Romance', 'Books centered around love and relationships');

-------------------------------------------------------------------------------------------------------------------
-- procedure to pay fine
create or replace procedure pay_fine
(
    p_fine_id int
)
language plpgsql
as
$$
begin

    update fine
    set
        fine_status = 'paid',
        updated_at = now()
    where fine_id = p_fine_id;

end;
$$;

-- function to calculate total fine and return
create or replace function calculate_member_fine
(
    p_member_id int
)
returns numeric
language plpgsql
as
$$
declare
    total_fine numeric := 0;
begin

    select coalesce(sum(days_overdue * fine_per_day),0)
    into total_fine
    from fine
    where member_id = p_member_id
    and fine_status = 'pending';

    return total_fine;

end;
$$;

-- select calculate_member_fine(3);
-- select * from fine;
