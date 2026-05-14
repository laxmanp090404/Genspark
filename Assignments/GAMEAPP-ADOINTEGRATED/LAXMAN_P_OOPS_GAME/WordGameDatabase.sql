create table users(
id serial primary key,
username varchar(50) not null unique constraint username_check check(char_length(username)>2),
password varchar(60) not null constraint password_chk check(char_length(password)>=6),
is_active bool default true
);

create table hiddenwords(
    wordid serial constraint pk_words primary key,
    word char(5) not null unique
);
create table games(
gameid serial constraint pk_games primary key,
userid int constraint fk_gametouser references users(id) on delete restrict,
iswon boolean not null,
attemptsused int not null constraint chk_attempts check(attemptsused between 1 and 6),
wordid int constraint fk_gametoword references hiddenwords(wordid) on delete restrict,
createdat timestamp default current_timestamp
);

create table guesses(
guessid serial constraint pk_guesses primary key,
gameid int constraint fk_guesstogames references games(gameid) on delete cascade,
userid int constraint fk_guesstousers references users(id) on delete restrict,
guess char(5) not null,
attemptnumber int not null constraint chk_attemptnumber check(attemptnumber between 1 and 6),
unique(gameid,attemptnumber)
);

alter table guesses
add column feedback char(5) not null
constraint chk_feedback
check(feedback ~ '^[GYX]{5}$');

alter table games
add column score int default 0
constraint chk_score check(score >= 0);

insert into hiddenwords(word)
values
('APPLE'),
('MANGO'),
('GRAPE'),
('TRAIN'),
('PLANT'),
('BRAIN');