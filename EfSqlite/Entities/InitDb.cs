namespace EfSqlite.Entities
{
    public static class InitDb
    {
        public const string Query = @"
drop table if exists tree;
drop table if exists leaf;
drop table if exists quote;

create table tree (
    id integer primary key autoincrement,
    name varchar(100) not null    
);

create table leaf (
    id integer primary key autoincrement,
    tree_id integer null,
    name varchar(100) not null    
);

create table quote(
    id integer primary key autoincrement,
    text text not null)
";

        public const string ReCreateTreeTable =
            @"drop table if exists Tree;
                create table Tree (
                Id integer primary key autoincrement,
                Name varchar(100) not null);";

        public const string ReCreateLeafTable = 
            @"drop table if exists Leaf;
                create table Leaf (
                Id integer primary key autoincrement,
                TreeId integer null,
                Name varchar(100) not null);";

    }
}