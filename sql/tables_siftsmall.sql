drop table if exists dbo.siftsmall_base;
create table dbo.siftsmall_base
(
    id int not null,
    jsonvector nvarchar(max) not null
)
go

drop table if exists dbo.siftsmall_query;
create table dbo.siftsmall_query
(
    id int not null,
    jsonvector nvarchar(max) not null
)
go

drop table if exists dbo.siftsmall_groundtruth;
create table dbo.siftsmall_groundtruth
(
    id int not null,
    jsonvector nvarchar(max) not null
)
go

