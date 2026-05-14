select * from accounts;
select * from transac;

-- procedure to add new account
create or replace procedure add_account(
p_accno int,p_balance numeric
)
language plpgsql
as 
$$begin
insert into accounts values(p_accno,p_balance);
end
$$

-- procedure to update account
create or replace procedure update_account(p_accno int , p_balance numeric)
language plpgsql
as
$$begin
	update accounts set balance=p_balance where accno=p_accno;
end
$$

--procedure to add transaction

create or replace procedure add_transaction(from_acc int,to_acc int ,amount numeric)
language plpgsql
as
$$begin
	insert into transac(fromaccno,toaccno,amount) values(from_acc,to_acc,amount);
end$$

-- checking procedures
call add_account(4,2500)
call update_account(4,1500)
call add_transaction(1,2,500)
-- call update_account(1,500)
-- call update_account(2,1000)
