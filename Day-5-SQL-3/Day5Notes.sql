-- JOINS DAY 5

-- cross join

select * from products cross join orders;-- cartesian product
										 -- total records is product of records of both tables
										 -- total attributes is sum of both attributes

-- natural join - becomes inner join if common column exists
				-- becomes cross join if common coloumn doesnt exist

select productid,productname,orderid,orderdate 
from products natural join orders;

-- inner join  - if you want only participating records

-- print productname,orderdate and quantity 
select productname,orderdate,quantity
from products p join order_details d on p.productid = d.productid
join orders o on d.orderid = o.orderid;

-- print same but also the products that were not ordered
select productname,orderdate,quantity
from products p left outer join order_details d on p.productid = d.productid
left outer join orders o on d.orderid = o.orderid;


-- print employees with managers

select emp.employeeid,concat(emp.firstname,' ',emp.lastname) as fullname,
mgr.employeeid,concat(mgr.firstname,' ',mgr.lastname) as fullname
from employees emp left outer join employees mgr on emp.reportsto = mgr.employeeid;

-- stored procedure
create or replace procedure proc_ex(name varchar(50))
language plpgsql
as $$
begin
	raise notice 'hello %', name;
end
$$;

call proc_ex('laxman');

-- in postgres doesnt allow storeprocedures to return data
-- so create a temp table and populate based on logic

-- create or replace procedure get_emp_details(empid int)
-- language plpgsql
-- as $$ 

-- begin
-- 	create temp table 
-- end

-- $$;

-- print the manager and number of employees reporting to him or her

select mgr.employeeid,concat(mgr.firstname,' ',mgr.lastname) as fullname,
count(emp.employeeid) as emp_cnt_under
from employees mgr join employees emp
on mgr.employeeid  = emp.reportsto
group by mgr.employeeid;

-- -- the above as function in postgres
create or replace function getManagerEmployee()
returns table(employeeid int,fullname text,emp_cnt_under bigint)
language plpgsql
as $$
begin
	return query
		select mgr.employeeid,concat(mgr.firstname,' ',mgr.lastname) as fullname,
		count(emp.employeeid) as emp_cnt_under
		from employees mgr join employees emp
		on mgr.employeeid  = emp.reportsto
		group by mgr.employeeid;
end
$$;
-- executing function
select * from getManagerEmployee();
-- dropping function
drop function getManagerEmployee();
-- print products and total quantity 
select p.productid,p.productname,sum(d.quantity) as totalquantity
from products p join order_details d on p.productid = d.productid
group by p.productid;

-- transactions
-- set of queries u do it all or do anything at all
-- ie commit or rollback

create table if not exists accounts(
    accno serial,
    balance numeric(8,2),
    constraint pk_accounts primary key(accno),
    constraint chk_balance_non_negative check(balance >= 0)
);

create table if not exists transac(
    id serial,
    fromaccno int not null,
    toaccno int not null,
    amount numeric(8,2) not null,

    constraint pk_transac primary key (id),

    constraint fk_transac_fromacc
        foreign key (fromaccno)
        references accounts(accno),

    constraint fk_transac_toacc
        foreign key (toaccno)
        references accounts(accno),

    constraint chk_amount_positive
        check (amount > 0),

    constraint chk_no_self_transfer
        check (fromaccno <> toaccno)
);

insert into accounts(balance)
values (1000),(500),(2000);

