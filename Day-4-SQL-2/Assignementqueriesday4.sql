-- Basic

-- 1. List all customers from USA.
--    Hint: Customers, WHERE

      -- filter based on country
      select * from customers where country = 'USA';

-- 2. List all products where UnitPrice is greater than 20.
   -- Hint: Products, WHERE

   -- filtering using relational operator on numeric value
   select * from products where unitprice >20;

-- 3. List all orders placed after 1997-01-01.
--    Hint: Orders, WHERE

   --after mean greater than 1997-01-01
   select * from orders where orderdate > '1997-01-01';
  

-- 4. Display customers ordered by Country and then CompanyName.
--    Hint: Customers, ORDER BY

      -- first sort based on country then for redundant values based on companyname
	  select * from customers order by country asc, companyname asc; 

-- 5. List products ordered by highest UnitPrice first.
--    Hint: Products, ORDER BY

-- get highest by sorting descending order and choose 1st one
	  select * from products
	  order by unitprice desc limit 1;

--  Group By

-- 6. Count how many customers are there in each country.
--    Hint: Customers, GROUP BY

-- count the rows/customerid and group countries
select country,count(customerid) as num_of_Customers from customers
group by country;

-- 7. Find the number of products in each category.
--    Hint: Products, GROUP BY

-- count prod id in each category 
select categoryid,count(productid) as totalproducts
from products group by categoryid;

-- 8. Find the total number of orders handled by each employee.
--    Hint: Orders, GROUP BY

-- group based on employee id and count orderid
select employeeid ,count(orderid) as num_of_orders_handled
from orders group by employeeid;

-- 9. Find the average freight amount for each customer.
--    Hint: Orders, GROUP BY

-- first group by customer and extract average of freight
select customerid,round(avg(freight),2) as avg_freight from orders
group by customerid ;
   

-- 10. Find the maximum unit price in each category.
--     Hint: Products, GROUP BY

-- group based on the category and find max of unit price
    select categoryid,max(unitprice) as max_unit_price 
	from products group by categoryid;

--Having

-- 11. Show countries having more than 5 customers.
--     Hint: Customers, GROUP BY, HAVING

-- group based on country and using having filter the grouped rows
	select country,count(customerid) as num_of_customers
	from customers group by country having count(customerid) > 5;

-- 12. Show employees who handled more than 50 orders.
--     Hint: Orders, GROUP BY, HAVING

-- group by employee id
	select employeeid ,count(orderid) as orders_handled
	from orders group by employeeid having count(orderid)>50;
	

-- 13. Show customers whose average freight is greater than 50.
--     Hint: Orders, GROUP BY, HAVING

	select customerid,round(avg(freight),2) as avg_freight from orders
	group by customerid having avg(freight) > 50;
	

-- 14. Show categories where the average product price is greater than 30.
--     Hint: Products, GROUP BY, HAVING

	select categoryid,round(avg(unitprice),2) as avg_unit_price
	from products group by categoryid having avg(unitprice)>30;

-- 15. Show ship countries having more than 20 orders.
--     Hint: Orders, GROUP BY, HAVING

	select shipcountry ,count(orderid) as num_of_orders
	from orders group by shipcountry having count(orderid) > 20;

Joins

-- 16. List each order with customer company name.
--     Hint: Orders, Customers, JOIN
	
	select o.orderid , c.contactname,c.companyname 
	from orders o join customers c
	on o.customerid = c.customerid;

-- 17. List each order with employee first name and last name.
--     Hint: Orders, Employees, JOIN
    
	-- using order id as the common attribute firstname and lastname is queried
	select o.orderid,e.firstname,e.lastname
	from orders o join employees e on o.employeeid = e.employeeid limit 5;
	

-- 18. List products with their category name.
--     Hint: Products, Categories, JOIN
select p.productid,p.productname,c.categoryid,c.categoryname
from products p join categories c
on p.categoryid = c.categoryid;


	

-- 19. List products with supplier company name.
--     Hint: Products, Suppliers, JOIN
    select p.productid,p.productname,s.supplierid,s.companyname
	from products p join suppliers s on p.supplierid = s.supplierid;


-- 20. List orders with shipper company name.
--     Hint: Orders, Shippers, JOIN

select o.orderid,o.shipvia , s.companyname
from orders o join shippers s
on o.shipvia = s.shipperid

-- Medium

-- 21. Find total orders per customer and display customer company name.
--     Hint: Customers, Orders, JOIN, GROUP BY

-- join based on the customer id betwween orders and customers
-- and group by based on customerid
	select c.customerid,c.companyname, count(o.orderid) as total_orders 
	from customers c join orders o on o.customerid = c.customerid group by c.customerid;

-- 22. Find total products supplied by each supplier.
--     Hint: Suppliers, Products, JOIN, GROUP BY

-- join suppliers and products based on the supplierid and then group by the same
-- count the number of product id in each group
select s.supplierid , s.companyname , count(p.productid) as total_products
from suppliers s join products p on s.supplierid = p.supplierid 
group by s.supplierid;


-- 23. Find average product price per category with category name.
--     Hint: Categories, Products, JOIN, GROUP BY

-- group based on category id and aggregate avg on unitprice
select c.categoryid,c.categoryname,round(avg(p.unitprice),2) as avg_product_price
from categories c join products p on c.categoryid=p.categoryid group by c.categoryid;

-- 24. Find total freight per customer and order by highest total freight.
--     Hint: Customers, Orders, JOIN, GROUP BY, ORDER BY

-- group customer id aggregate sum of freight display in desending order
select c.customerid,c.companyname,sum(o.freight) as total_freight
from customers c join orders o on o.customerid = c.customerid 
group by c.customerid order by sum(o.freight) desc;


-- 25. Find employees who handled more than 25 orders.
--     Hint: Employees, Orders, JOIN, GROUP BY, HAVING

-- group by employee ids and using having filter employees
select e.employeeid,e.firstname,count(o.orderid) as total_orders
from employees e join orders o on e.employeeid = o.employeeid 
group by e.employeeid having count(o.orderid)>25;

 Advanced

-- 26. Find total sales amount per order.
--     Hint: Orders, Order Details, JOIN, GROUP BY

select o.orderid , sum(d.unitprice*d.quantity*(1-d.discount)) as totalamount
from orders o join order_details d on o.orderid=d.orderid group by o.orderid;

-- 27. Find total sales amount per customer.
--     Hint: Customers, Orders, Order Details, JOIN, GROUP BY

select c.customerid,sum(d.unitprice*d.quantity*(1-d.discount)) as totalamount
from customers c join orders o on c.customerid = o.customerid
join order_details d on o.orderid = d.orderid group by c.customerid;

-- 28. Find top 10 products by total quantity sold.
--     Hint: Products, Order Details, JOIN, GROUP BY, ORDER BY

select p.productid,p.productname , sum(d.quantity) as total_quantity
from products p join order_details d on p.productid = d.productid
group by p.productid order by sum(d.quantity) desc limit 10;


-- 29. Find categories whose total sales are greater than 50000.
--     Hint: Categories, Products, Order Details, JOIN, GROUP BY, HAVING

select c.categoryid,c.categoryname,sum(d.unitprice*d.quantity*(1-d.discount)) as totalsales
from categories c join products p on c.categoryid = p.categoryid
join order_details d on p.productid = d.productid group by c.categoryid having sum(d.unitprice * d.quantity * 1-d.discount) > 50000;

-- 30. Find employees whose total sales are greater than 100000.
--     Hint: Employees, Orders, Order Details, JOIN, GROUP BY, HAVING

-- first join based on empid,orderid and then group by empid
-- using having filter aggregate
select e.employeeid ,e.firstname,sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from employees e join orders o on e.employeeid = o.employeeid
join order_details d on o.orderid = d.orderid 
group by e.employeeid having sum(d.unitprice*d.quantity*(1-d.discount)) > 100000;

-- 31. Find total sales per country based on customer country.
--     Hint: Customers, Orders, Order Details, JOIN, GROUP BY

-- join customers->orders->orderdetails on basis of common columns
-- grouping based on country and calculate sales
select c.country,sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from customers c join orders o on c.customerid = o.customerid
join order_details d on o.orderid = d.orderid
group by c.country;
	
-- 32. Find suppliers whose products generated sales above 30000.
--     Hint: Suppliers, Products, Order Details, JOIN, GROUP BY, HAVING

-- join suppliers>prods>orderdetails on common columns
-- group by supplier and having filter aggregation
select s.supplierid,s.companyname , sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from suppliers s join products p on s.supplierid = p.supplierid
join order_details d on p.productid = d.productid
group by s.supplierid having sum(d.unitprice*d.quantity*(1-d.discount)) > 30000;


-- 33. Find customers who placed more than 10 orders and sort by order count descending.
--     Hint: Customers, Orders, JOIN, GROUP BY, HAVING, ORDER BY

-- join customers>orders on customerid 
-- group by customerid , filter more than 10 orders orderby desc
select c.customerid,c.contactname,count(o.orderid) as total_orders_made
from customers c join orders o on c.customerid = o.customerid
group by c.customerid having count(o.orderid)>10
order by total_orders_made desc;

-- 34. Find monthly order count for each year and month.
--     Hint: Orders, GROUP BY, ORDER BY

-- extract year and month from orderdate multigroup and then order by cnt
select extract(year from orderdate),extract(month from orderdate),count(orderid) as order_cnt
from orders group by extract(year from orderdate),extract(month from orderdate)
order by order_cnt desc;


-- 35. Find monthly sales amount ordered by year and month.
--     Hint: Orders, Order Details, JOIN, GROUP BY, ORDER BY

-- extract year and month from orderdate join order-> orderdetails
-- multigroup and then order by totalsales
select extract(year from o.orderdate),extract(month from o.orderdate),sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from orders o join order_details d on o.orderid = d.orderid
group by extract(year from o.orderdate),extract(month from o.orderdate)
order by total_sales desc;
