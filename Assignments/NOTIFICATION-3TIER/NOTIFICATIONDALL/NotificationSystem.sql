create table users(
id serial primary key,
name varchar(60) not null,
email varchar(60) not null unique,
phone varchar(10) not null unique,
isdeleted boolean not null default false
);

create table notifications(
id serial primary key,
message text not null,
sentdate timestamp not null default current_timestamp,
notificationtype varchar(20) not null constraint chk_type check(notificationtype in ('Email','SMS')),
recipientid int not null,
constraint fk_notification_user foreign key(recipientid) references users(id)
);

create index idx_notifications_recipientid on notifications(recipientid);