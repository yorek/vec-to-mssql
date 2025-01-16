drop table if exists dbo.sift_base;
create table dbo.sift_base
(
    id int not null primary key clustered,
    vector vector(128) not null    
)
go

drop table if exists dbo.sift_query;
create table dbo.sift_query
(
    id int not null primary key clustered,
    vector vector(128) not null    
)
go

drop table if exists dbo.sift_groundtruth;
create table dbo.sift_groundtruth
(
    id int not null primary key clustered,
    vector vector(128) not null    
)
go
