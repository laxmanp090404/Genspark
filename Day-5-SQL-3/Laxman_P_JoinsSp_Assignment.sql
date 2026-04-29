-- BASIC TO INTERMEDIATE

-- 1. List all customers and the total number of orders they have placed.
-- Show only customers with more than 5 orders. Sort by total orders
-- descending.

-- retreive customer specific group by customer data 
-- filter order cnt using having
select c.customerid,c.companyname,c.contactname, count(o.orderid) as total_orders
from customers c join orders o on c.customerid = o.customerid
group by c.customerid,c.companyname,c.contactname
having count(o.orderid) > 5
order by total_orders desc;


-- 2. Retrieve the total sales amount per customer by joining customers, orders,
-- and order_details.
-- Show only customers whose total sales exceed 10,000. Sort by total sales
-- descending.

-- select customer data attributes join customers->orders->orderdetails
-- calculate total sales and the filter sales by having 
select c.customerid,c.companyname,c.contactname,sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from customers c join orders o on c.customerid = o.customerid
join order_details d on o.orderid = d.orderid
group by c.customerid,c.companyname,c.contactname
having sum(d.unitprice*d.quantity*(1-d.discount)) > 10000
order by total_sales desc; 


-- 3. Get the number of products per category.
-- Show only categories having more than 10 products. Sort by product count
-- descending.

-- select category attributes join categories->products
-- group by and filter using having
select c.categoryid,c.categoryname,count(p.productid) as total_products
from categories c join products p on c.categoryid = p.categoryid
group by c.categoryid,c.categoryname
having count(productid)>10;

-- 4. Display the total quantity sold per product.
-- Include only products where total quantity sold is greater than 100. Sort by
-- quantity descending.

-- select appropriate product attributes and then calculate sum of quantity
-- group by products  and filter using having 
select p.productid,p.productname,sum(d.quantity) as total_quantity
from products p join order_details d on p.productid = d.productid
group by p.productid,p.productname
having sum(d.quantity) > 100
order by total_quantity desc;

-- 5. Find the total number of orders handled by each employee.
-- Show only employees who handled more than 20 orders. Sort by order count
-- descending.

-- select employee attributes ,group by them and then filter using having
-- order by the total orders cnt
select e.employeeid,e.firstname,e.lastname,count(o.orderid) as total_orders
from employees e join orders o on e.employeeid = o.employeeid
group by e.employeeid,e.firstname,e.lastname
having count(o.orderid) > 20
order by total_orders desc;


-- INTERMEDIATE


-- 6. Retrieve the total sales per category by joining categories, products, and
-- order_details.
-- Show only categories with total sales above 50,000. Sort by total sales
-- descending.

-- get category attributes join categories->products->orderdetails
-- calculate sales , group by category attributes and filter by having
select c.categoryid , c.categoryname ,sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from categories c join products p on c.categoryid = p.categoryid
join order_details d on p.productid = d.productid
group by c.categoryid , c.categoryname
having sum(d.unitprice*d.quantity*(1-d.discount)) > 50000
order by total_sales desc;

-- 7. List suppliers and the number of products they supply.
-- Show only suppliers who supply more than 5 products. Sort by product count
-- descending.

--select suplier attributes and product count using aggregation
-- filter using having and sort based on cnt
select s.supplierid,s.companyname,count(p.productid) as total_products_supplied
from suppliers s join products p on s.supplierid  = p.supplierid
group by s.supplierid,s.companyname
having count(p.productid) > 5
order by count(p.productid) desc;


-- 8. Get the average unit price per category.
-- Show only categories where the average price is above 30. Sort by average
-- price descending.

-- select category attributes , join categories->products 
-- group by the same and filter using having and order
select c.categoryid,c.categoryname,round(avg(p.unitprice),2) as avg_price
from categories c join products p 
on c.categoryid = p.categoryid
group by c.categoryid,c.categoryname 
having avg(p.unitprice) > 30
order by avg_price desc;


-- 9. Display the total revenue generated per employee (orders + order_details).
-- Show only employees generating more than 20,000 in revenue. Sort by
-- revenue descending.

-- select emp attributes and then join the employees->orders->orderdetails 
-- grp by the same and then filter using having and order by
select e.employeeid,e.firstname,e.lastname,sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales
from employees e join orders o on e.employeeid = o.employeeid
join order_details d on o.orderid = d.orderid
group by e.employeeid,e.firstname,e.lastname
having sum(d.unitprice*d.quantity*(1-d.discount)) > 20000

-- 10. Retrieve the number of orders shipped to each country.
-- Show only countries with more than 10 orders. Sort by order count
-- descending.

-- select shipcountry aggregate count and order by count 
-- after filtering using having
select shipcountry , count(orderid) as num_of_orders
from orders group by shipcountry
having count(orderid)>10
order by num_of_orders desc;


-- ADVANCED

-- 11. Find customers and the average order value (orders + order_details).
-- Show only customers with average order value greater than 500. Sort by
-- average descending.

-- select customer attributes and then join orers and orderdetails
select c.customerid,c.contactname,avg(d.unitprice*d.quantity*(1-d.discount)) as avg_order_value
from customers c join orders o on c.customerid = o.customerid
join order_details d on o.orderid = d.orderid
group by c.customerid,c.contactname
having avg(d.unitprice*d.quantity*(1-d.discount)) > 500
order by avg_order_value desc;


-- 12. Get the top-selling products per category (by total quantity sold).
-- Show only products with total quantity sold above 200. Sort within category by
-- quantity descending.

-- get appropriate attributes group by category->then productid
-- do filtering and sorting
select c.categoryid,c.categoryname,p.productid,p.productname,sum(d.quantity) as quantity_sold
from categories c join products p on c.categoryid = p.categoryid
join order_details d on p.productid = d.productid
group by c.categoryid,c.categoryname,p.productid,p.productname
having sum(d.quantity)>200
order by c.categoryname desc , quantity_sold desc;

-- 13. Retrieve the total discount given per product (order_details).
-- Show only products where total discount exceeds 1,000. Sort by discount
-- descending.

-- get product attributes and then join products->orderdetails
-- filter using having and order by calc discount
select p.productid,p.productname,sum(d.quantity*d.unitprice*d.discount) as discount_amount
from products p join order_details d on p.productid = d.productid
group by p.productid,p.productname
having sum(d.quantity*d.unitprice*d.discount) > 1000
order by discount_amount desc;

-- 14. List employees and the number of unique customers they handled.
-- Show only employees who handled more than 15 unique customers. Sort by
-- count descending.

-- use distinct to include unique customers
-- select emp attributes and join orders table and filter and sort
select e.employeeid,e.firstname,e.lastname,count(distinct(o.customerid)) as unique_customers
from employees e join orders o on e.employeeid = o.employeeid
group by e.employeeid,e.firstname,e.lastname
having count(distinct(o.customerid))>15
order by unique_customers desc;

-- 15. Find the monthly total sales (year + month) using orders and order_details.
-- Show only months where total sales exceed 30,000. Sort by year and month
-- ascending.
-- extract year and month and calculate total sales
-- grp by the same and sort by year and month
select extract(year from o.orderdate) as Year , extract(month from o.orderdate) as Month,
sum(d.unitprice*d.quantity*(1-d.discount)) as total_sales 
from orders o join order_details d on o.orderid = d.orderid
group by extract(year from o.orderdate) , extract(month from o.orderdate)
having sum(d.unitprice*d.quantity*(1-d.discount)) > 30000
order by year asc ,month asc;