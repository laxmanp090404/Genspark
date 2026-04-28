
CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    name TEXT,
    details JSONB
);
select * from products;


-- INSERT INTO products (name, details) VALUES 
-- ('Laptop', '{"brand": "Apple", "specs": {"ram": "16GB", "storage": "512GB"}}'),
-- ('Phone', '{"brand": "Samsung", "specs": {"ram": "8GB", "storage": "128GB"}}');


INSERT INTO products (name, details) VALUES 
('Desktop', '{"brand": "Samsung", "specs": {"ram": "8GB", "storage": "128GB"},"colour":"blue"}');


select * from products;

select details from products;

select details->'brand' as jsonbrand from products;

select * from products
where  details->>'brand' = 'Apple';


select * from products
where  details->'specs'->>'ram' = '8GB';


select * from products
where  details ? 'colour'

select * from employees;

select * from employees where area = 'ABC';

select * from employees where emp_id > 101;
select * from employees where emp_id between 101 and 105;

select area,count(*) as area_cnt from employees group by area having count(*) > 2; 
select * from skills;
select * from employees;
select * from employeeskills;

--avg skill level of every employee
select emp_id , ROUND(AVG(skilllevel),2) as EmployeeAvgSkillLevel from employeeskills group by emp_id;

--avg skill level of each skill
select s.skill , round(avg(e.skilllevel),2) as SkillAverageLevel from skills s join employeeskills e on e.skill=s.skill group by s.skill order by SkillAverageLevel;

