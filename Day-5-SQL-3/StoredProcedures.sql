-- PROCEDURES

-- 1.Create a stored procedure to insert a new customer. Use a transaction so that if any required value is missing, the insert is rolled back.
-- drop procedure add_customer;
create or replace procedure add_customer(
    n_customerid char(5),
    n_companyname varchar(40),
    n_contactname varchar(30),
    n_contacttitle varchar(30),
    n_address varchar(60),
    n_city varchar(15),
    n_region varchar(15),
    n_postalcode varchar(10),
    n_country varchar(15),
    n_phone varchar(24),
    n_fax varchar(24)
)
language plpgsql
as $$
begin
    if trim(coalesce(n_customerid, '')) = ''
       or trim(coalesce(n_companyname, '')) = ''
       or trim(coalesce(n_contactname, '')) = '' then

        raise notice 'required fields missing';
        rollback;
        return;
    end if;

    insert into customers (
        customerid,
        companyname,
        contactname,
        contacttitle,
        address,
        city,
        region,
        postalcode,
        country,
        phone,
        fax
    )
    values (
        n_customerid,
        n_companyname,
        n_contactname,
        n_contacttitle,
        n_address,
        n_city,
        n_region,
        n_postalcode,
        n_country,
        n_phone,
        n_fax
    );

    commit;
    raise notice 'customer added successfully';
end;
$$;

-- sample insert

call add_customer('AB123','abc corporation','laxman','manager','12 anna salai','chennai',null,'600001',
    'india','9999999999',null);

-- 2. Create a stored procedure to place a new order for an existing customer with one product. Insert into orders and order_details in a single transaction.

-- select * from order_details;

create or replace procedure place_singleorder(
    n_customerid char(5),
    n_employeeid int,
    n_productid int,
    n_requireddate timestamp,
    n_shipvia int,
    n_freight numeric,
    n_shipname varchar(40),
    n_shipaddress varchar(60),
    n_shipcity varchar(15),
    n_shipregion varchar(15),
    n_shippostalcode varchar(10),
    n_shipcountry varchar(15),
    n_quantity smallint,
    n_discount real
)
language plpgsql
as $$
declare
    n_orderid int;
    n_unitprice numeric;
begin

    -- validate customer
    if not exists (
        select 1
        from customers
        where customerid = n_customerid
    ) then
        raise notice 'customer not found';
        rollback;
        return;
    end if;

    -- validate employee
    if not exists (
        select 1
        from employees
        where employeeid = n_employeeid
    ) then
        raise notice 'employee not found';
        rollback;
        return;
    end if;

    -- validate shipper
    if not exists (
        select 1
        from shippers
        where shipperid = n_shipvia
    ) then
        raise notice 'shipper not found';
        rollback;
        return;
    end if;

    -- fetch product price
    select unitprice
    into n_unitprice
    from products
    where productid = n_productid;

    if not found then
        raise notice 'product not found';
        rollback;
        return;
    end if;

    insert into orders (
        customerid,
        employeeid,
        orderdate,
        requireddate,
        shipvia,
        freight,
        shipname,
        shipaddress,
        shipcity,
        shipregion,
        shippostalcode,
        shipcountry
    )
    values (
        n_customerid,
        n_employeeid,
        current_timestamp,
        n_requireddate,
        n_shipvia,
        n_freight,
        n_shipname,
        n_shipaddress,
        n_shipcity,
        n_shipregion,
        n_shippostalcode,
        n_shipcountry
    )
    returning orderid into n_orderid;

    insert into order_details (
        orderid,
        productid,
        unitprice,
        quantity,
        discount
    )
    values (
        n_orderid,
        n_productid,
        n_unitprice,
        n_quantity,
        n_discount
    );

    commit;
    raise notice 'order placed successfully. order id: %', n_orderid;

end;
$$;
select * from orders;
-- sample insert 
call place_singleorder(
    'ALFKI'::char(5),
    4,
    11,
    '2026-05-01'::timestamp,
    3,
    32.38::numeric,
    'Vins et alcools Chevalier'::varchar,
    '59 rue de l''Abbaye'::varchar,
    'Reims'::varchar,
    null::varchar,
    '51100'::varchar,
    'France'::varchar,
    12::smallint,
    0::real
);

-- 3.Create a stored procedure to update product stock after an order is placed. If stock is not enough, rollback the transaction.

create or replace procedure update_productstock(
    n_productid int,
    n_quantity smallint
)
language plpgsql
as $$
declare
    n_stock smallint;
begin

    -- fetch current stock
    select unitsinstock
    into n_stock
    from products
    where productid = n_productid;

    -- check if product exists
    if not found then
        raise notice 'product not found';
        rollback;
        return;
    end if;

    -- check stock availability
    if n_stock < n_quantity then
        raise notice 'insufficient stock';
        rollback;
        return;
    end if;

    -- update stock
    update products
    set unitsinstock = unitsinstock - n_quantity
    where productid = n_productid;

    commit;
    raise notice 'stock updated successfully';

end;
$$;
-- sample update
call update_productstock(11, 5::smallint);

-- 4.Create a stored procedure to cancel an order. Delete records from order_details first, then from orders, using a transaction.

create or replace procedure cancel_order(
n_orderid int
)
language plpgsql
as $$
begin
	--check if order exists
	if not exists(
		select 1 from orders where orderid = n_orderid
	) then
		raise notice 'Order doesnt exist';
		rollback;
		return;
	end if;

	--delete in orderdetails
	delete from order_details
	where orderid = n_orderid;

	--delete from orders
	delete from orders
	where orderid =n_orderid;
	commit;
	raise notice 'order deleted successfully or cancelled';
	
end
	
$$;
-- select * from orders;
call cancel_order(10248);


-- 5.Create a stored procedure to transfer products from one supplier to another. If the old supplier or new supplier does not exist, rollback.
create or replace procedure transfer_supplierproducts(
    n_old_supplierid int,
    n_new_supplierid int
)
language plpgsql
as $$
begin

    -- check old supplier exists
    if not exists (
        select 1
        from suppliers
        where supplierid = n_old_supplierid
    ) then
        raise notice 'old supplier not found';
        rollback;
        return;
    end if;

    -- check new supplier exists
    if not exists (
        select 1
        from suppliers
        where supplierid = n_new_supplierid
    ) then
        raise notice 'new supplier not found';
        rollback;
        return;
    end if;

    -- transfer products
    update products
    set supplierid = n_new_supplierid
    where supplierid = n_old_supplierid;

    commit;
    raise notice 'products transferred successfully';

end;
$$;
-- sample update
-- select * from suppliers;
call transfer_supplierproducts(1, 2);



-- 6.Create a stored procedure to update the price of all products in a category by a percentage. Rollback if the percentage is less than or equal to zero.
create or replace procedure update_categoryprices(
    n_categoryid int,
    n_percentage numeric
)
language plpgsql
as $$
begin

    -- validate percentage
    if n_percentage <= 0 then
        raise notice 'percentage must be greater than zero';
        rollback;
        return;
    end if;

    -- validate category exists
    if not exists (
        select 1
        from categories
        where categoryid = n_categoryid
    ) then
        raise notice 'category not found';
        rollback;
        return;
    end if;

    -- update prices
    update products
    set unitprice=unitprice+(unitprice*n_percentage/100)
    where categoryid=n_categoryid;

    commit;
    raise notice 'category prices updated successfully';

end;
$$;
-- sample increase in price
-- select * from categories;
call update_categoryprices(1, 10);

-- 7. Create a stored procedure to add a new product under an existing category and supplier. Rollback if the category or supplier does not exist.
create or replace procedure add_product(
    n_productname varchar(40),
    n_supplierid int,
    n_categoryid int,
    n_quantityperunit varchar(20),
    n_unitprice numeric,
    n_unitsinstock smallint,
    n_unitsonorder smallint,
    n_reorderlevel smallint,
    n_discontinued smallint
)
language plpgsql
as $$
begin

    -- validate supplier
    if not exists (
        select 1
        from suppliers
        where supplierid = n_supplierid
    ) then
        raise notice 'supplier not found';
        rollback;
        return;
    end if;

    -- validate category
    if not exists (
        select 1
        from categories
        where categoryid = n_categoryid
    ) then
        raise notice 'category not found';
        rollback;
        return;
    end if;

    -- insert product
    insert into products (
        productname,
        supplierid,
        categoryid,
        quantityperunit,
        unitprice,
        unitsinstock,
        unitsonorder,
        reorderlevel,
        discontinued
    )
    values (
        n_productname,
        n_supplierid,
        n_categoryid,
        n_quantityperunit,
        n_unitprice,
        n_unitsinstock,
        n_unitsonorder,
        n_reorderlevel,
        n_discontinued
    );

    commit;
    raise notice 'product added successfully';

end;
$$;
-- sample add
-- select * from products;
-- select * from categories;
call add_product('Maggie Noodles'::varchar,1,5,'10 boxes x 10 pkts'::varchar,25.50::numeric,100::smallint,20::smallint,10::smallint,0::smallint);

-- 8. Create a stored procedure to delete a customer only if the customer has no orders. Use a transaction and rollback if orders exist.
create or replace procedure delete_customerifnoorders(
    n_customerid char(5)
)
language plpgsql
as $$
begin

    -- check customer exists
    if not exists (
        select 1
        from customers
        where customerid = n_customerid
    ) then
        raise notice 'customer not found';
        rollback;
        return;
    end if;

    -- check for existing orders
    if exists (
        select 1
        from orders
        where customerid = n_customerid
    ) then
        raise notice 'customer has orders and cannot be deleted';
        rollback;
        return;
    end if;

    -- delete customer
    delete from customers
    where customerid = n_customerid;

    commit;
    raise notice 'customer deleted successfully';

end;
$$;
-- sample delete
call delete_customerifnoorders('ALFKI');

-- 9. Create a stored procedure to apply a discount to all order details for a specific order. Rollback if the order does not exist.
create or replace procedure apply_orderdiscount(
    n_orderid int,
    n_discount real
)
language plpgsql
as $$
begin

    -- check order exists
    if not exists (
        select 1
        from orders
        where orderid = n_orderid
    ) then
        raise notice 'order not found';
        rollback;
        return;
    end if;

    -- validate discount
    if n_discount < 0 or n_discount > 1 then
        raise notice 'discount must be between 0 and 1';
        rollback;
        return;
    end if;

    -- update discount for all order items
    update order_details
    set discount = n_discount
    where orderid = n_orderid;

    commit;
    raise notice 'discount applied successfully';

end;
$$;

-- sample update 
select * from orders;
call apply_orderdiscount(10249, 0.15);

Create a stored procedure to place an order with multiple products. Insert the order and all order items in one transaction. If any product is invalid or stock is insufficient, rollback the complete order.

