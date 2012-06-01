﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble;
using Gribble.Expressions;
using Gribble.Model;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class SchemaTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthdate { get; set; }
            public DateTime? Created { get; set; }
            public int Age { get; set; }
            public float Price { get; set; }
            public double Distance { get; set; }
            public byte Flag { get; set; }
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        public const string TableName = "some_table_in_the_db";
        private const string TableName1 = "XLIST_1";
        private const string TableName2 = "XLIST_2";
        private const string TableName3 = "XLIST_3";
        private const string TableName4 = "XLIST_4";

        [Test]
        public void Create_Table__With_Crl_Types_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Id", typeof(Guid), key: Column.KeyType.PrimaryKey, isAutoGenerated: true),
                new Column("Name", typeof(string), length: 500, isNullable: true),
                new Column("Active", typeof(bool), isNullable: false, defaultValue: true),
                new Column("Created", typeof(DateTime), isNullable: false, isAutoGenerated: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Id] uniqueidentifier NOT NULL DEFAULT NEWSEQUENTIALID(), [Name] nvarchar (500) NULL, [Active] bit NOT NULL DEFAULT 1, [Created] datetime NOT NULL DEFAULT GETDATE(), CONSTRAINT [PK_some_table_in_the_db_Id] PRIMARY KEY ([Id] ASC))");
        }

        [Test]
        public void Create_Table__With_Sql_Types_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Id", sqlType: SqlDbType.UniqueIdentifier, key: Column.KeyType.PrimaryKey, isAutoGenerated: true),
                new Column("Name", sqlType: SqlDbType.NVarChar, length: 500, isNullable: true),
                new Column("Active", sqlType: SqlDbType.Bit, isNullable: false, defaultValue: true),
                new Column("Created", sqlType: SqlDbType.DateTime, isNullable: false, isAutoGenerated: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Id] uniqueidentifier NOT NULL DEFAULT NEWSEQUENTIALID(), [Name] nvarchar (500) NULL, [Active] bit NOT NULL DEFAULT 1, [Created] datetime NOT NULL DEFAULT GETDATE(), CONSTRAINT [PK_some_table_in_the_db_Id] PRIMARY KEY ([Id] ASC))");
        }

        [Test]
        public void Create_Table_Identity_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Id", typeof(int), isIdentity: true, key: Column.KeyType.PrimaryKey));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Id] int IDENTITY(1,1) NOT NULL, CONSTRAINT [PK_some_table_in_the_db_Id] PRIMARY KEY ([Id] ASC))");
        }

        [Test]
        public void Create_Table_Clustered_Primary_Key_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Id", typeof(int), isIdentity: true, key: Column.KeyType.ClusteredPrimaryKey));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Id] int IDENTITY(1,1) NOT NULL, CONSTRAINT [PK_some_table_in_the_db_Id] PRIMARY KEY CLUSTERED ([Id] ASC))");
        }

        [Test]
        public void Create_Table_with_decimal_precision_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Value", typeof(decimal), precision: 5));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Value] decimal (5) NOT NULL)");
        }

        [Test]
        public void Create_Table_with_decimal_precision_and_scale_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Value", typeof(decimal), precision: 5, scale: 1));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Value] decimal (5, 1) NOT NULL)");
        }

        [Test]
        public void Create_Table_with_computation_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Value", typeof(string), computation: "1 + 1", isNullable: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Value] AS (1 + 1))");
        }

        [Test]
        public void Create_Table_with_persisted_computation_Test()
        {
            var statement = SchemaWriter.CreateTableCreateStatement(TableName,
                new Column("Value", typeof(string), computation: "1 + 1", computationPersisted: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE TABLE [some_table_in_the_db] ([Value] AS (1 + 1) PERSISTED NOT NULL)");
        }

        [Test]
        public void Table_Exists_Test()
        {
            var statement = SchemaWriter.CreateTableExistsStatement(TableName);
            statement.Result.ShouldEqual(Statement.ResultType.Scalar);
            statement.Text.ShouldEqual("SELECT CAST(CASE WHEN EXISTS (SELECT * FROM [sys].[tables] WHERE [name] = 'some_table_in_the_db') THEN 1 ELSE 0 END AS bit)");
        }

        [Test]
        public void Drop_Table_Test()
        {
            var statement = SchemaWriter.CreateDeleteTableStatement(TableName);
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("IF EXISTS (SELECT * FROM [sys].[tables] WHERE [name] = 'some_table_in_the_db') DROP TABLE [some_table_in_the_db]");
        }

        [Test]
        public void should_generate_proper_create_table_columns_sql()
        {
            var select = new Select { From = { Type = Data.DataType.Query, 
                                                 Queries = new List<Select> { new Select { From = { Type = Data.DataType.Table, 
                                                                                                      Table = new Table { Name = TableName1 }}} }}};
            var statement = SchemaWriter.CreateCreateTableColumnsStatement(select);
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Text.ShouldEqual("SELECT [SC].[name], [SC].[system_type_id], CAST(CASE WHEN [SC].[system_type_id] IN (239, 231, 99) THEN [SC].[max_length] / 2 ELSE [SC].[max_length] END AS smallint) AS [max_length], [SC].[is_nullable], [SC].[is_identity], " + 
                "CAST((CASE OBJECT_DEFINITION([SC].[default_object_id]) WHEN '(GETDATE())' THEN 1 WHEN '(NEWID())' THEN 1 WHEN '(NEWSEQUENTIALID())' THEN 1 ELSE 0 END) AS bit) AS [is_auto_generated], " + 
                "CASE OBJECT_DEFINITION([SC].[default_object_id]) WHEN '(GETDATE())' THEN NULL WHEN '(NEWID())' THEN NULL WHEN '(NEWSEQUENTIALID())' THEN NULL ELSE REPLACE(REPLACE(OBJECT_DEFINITION([SC].[default_object_id]), '(', ''), ')', '') END AS [default_value], " +
                "ISNULL([SI].[is_primary_key], 0) AS [is_primary_key], CAST((CASE [SI].[type] WHEN 1 THEN 1 ELSE 0 END) AS bit) AS [is_primary_key_clustered], [SC].[precision], [SC].[scale], [SCC].[definition] AS [computation], [SCC].[is_persisted] AS [persisted_computation] " + 
                "FROM ( ( ( [sys].[columns] [SC] LEFT JOIN [sys].[index_columns] [SIC] ON [SC].[column_id] = [SIC].[column_id] AND [SC].[object_id] = [SIC].[object_id] ) LEFT JOIN [sys].[indexes] [SI] ON [SI].[index_id] = [SIC].[index_id] AND " + 
                "[SI].[object_id] = [SIC].[object_id] ) LEFT JOIN [sys].[computed_columns] [SCC] ON [SC].[column_id] = [SCC].[column_id] AND [SC].[object_id] = [SCC].[object_id] ) JOIN (SELECT [name], " + 
                "CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] " + 
                "FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_1')) [__SubQuery__] ON [__SubQuery__].[name] = [SC].[name] AND [SC].[object_id] = OBJECT_ID(N'XLIST_1')");
        }

        [Test]
        public void should_generate_proper_get_columns_sql()
        {
            var statement = SchemaWriter.CreateTableColumnsStatement("XLIST_1");
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Text.ShouldEqual("SELECT [SC].[name], [SC].[system_type_id], CAST(CASE WHEN [SC].[system_type_id] IN (239, 231, 99) THEN [SC].[max_length] / 2 ELSE [SC].[max_length] END AS smallint) AS [max_length], [SC].[is_nullable], [SC].[is_identity], " +
                "CAST((CASE OBJECT_DEFINITION([SC].[default_object_id]) WHEN '(GETDATE())' THEN 1 WHEN '(NEWID())' THEN 1 WHEN '(NEWSEQUENTIALID())' THEN 1 ELSE 0 END) AS bit) AS [is_auto_generated], " +
                "CASE OBJECT_DEFINITION([SC].[default_object_id]) WHEN '(GETDATE())' THEN NULL WHEN '(NEWID())' THEN NULL WHEN '(NEWSEQUENTIALID())' THEN NULL ELSE REPLACE(REPLACE(OBJECT_DEFINITION([SC].[default_object_id]), '(', ''), ')', '') END AS [default_value], " +
                "ISNULL([SI].[is_primary_key], 0) AS [is_primary_key], CAST((CASE [SI].[type] WHEN 1 THEN 1 ELSE 0 END) AS bit) AS [is_primary_key_clustered], [SC].[precision], [SC].[scale], [SCC].[definition] AS [computation], [SCC].[is_persisted] AS [persisted_computation] " +
                "FROM ( ( ( [sys].[columns] [SC] LEFT JOIN [sys].[index_columns] [SIC] ON [SC].[column_id] = [SIC].[column_id] AND [SC].[object_id] = [SIC].[object_id] ) LEFT JOIN [sys].[indexes] [SI] ON [SI].[index_id] = [SIC].[index_id] AND " +
                "[SI].[object_id] = [SIC].[object_id] ) LEFT JOIN [sys].[computed_columns] [SCC] ON [SC].[column_id] = [SCC].[column_id] AND [SC].[object_id] = [SCC].[object_id] ) WHERE [SC].[object_id] = OBJECT_ID(N'XLIST_1')");
        }

        [Test]
        public void should_generate_proper_get_indexes_sql()
        {
            var statement = SchemaWriter.CreateGetIndexesStatement("XLIST_1");
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Text.ShouldEqual("SELECT [SI].[name] , [SI].[type] , [SI].[is_unique] , [SI].[is_primary_key] , [SC].[name] AS [column_name] , [SIC].[is_descending_key] FROM [sys].[indexes] [SI] JOIN [sys].[index_columns] [SIC] ON " + 
                "[SI].[object_id] = [SIC].[object_id] AND [SI].[index_id] = [SIC].[index_id] JOIN [sys].[columns] [SC] ON [SIC].[object_id] = [SC].[object_id] AND [SIC].[column_id] = [SC].[column_id] WHERE [SI].[object_id] = OBJECT_ID(N'XLIST_1') ORDER BY [SI].[name]");
        }

        [Test]
        public void Columns_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(5).Union(MockQueryable<Entity>.Create(TableName3).Skip(4).OrderBy(x => x.Active)));
            var statement = SchemaWriter.CreateUnionColumnsStatement(QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_1') INTERSECT " +
                                    "SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_2') INTERSECT " +
                                    "SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_3')");
        }

        [Test]
        public void Insert_Into_Columns_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(5).Union(MockQueryable<Entity>.Create(TableName3).Skip(4).OrderBy(x => x.Active))).CopyTo(MockQueryable<Entity>.Create(TableName4));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>) x).Name);
            var statement = SchemaWriter.CreateSharedColumnsStatement(select.CopyTo.Query, select.CopyTo.Into);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("SELECT [__SubQuery__].[name], " + 
                "CAST(CASE WHEN [SC].[system_type_id] < (SELECT MAX([system_type_id]) FROM [sys].[columns] WHERE [name] = [__SubQuery__].[name] AND [system_type_id] IN (175, 167, 35, 239, 231, 99) AND [object_id] IN (OBJECT_ID(N'XLIST_1'), OBJECT_ID(N'XLIST_2'), OBJECT_ID(N'XLIST_3'))) THEN 1 ELSE 0 END AS bit) AS [NarrowingConversion] " + 
                "FROM (SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] " + 
                        "FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_1') INTERSECT SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] " + 
                            "FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_2') INTERSECT SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] " + 
                                "FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_3') INTERSECT SELECT [name], CASE [system_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [system_type_id] END AS [system_type_id], CASE [user_type_id] WHEN 167 THEN 231 WHEN 175 THEN 239 WHEN 35 THEN 99 ELSE [user_type_id] END AS [user_type_id] " + 
                                    "FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'XLIST_4')) [__SubQuery__] JOIN [sys].[columns] [SC] ON [__SubQuery__].[name] = [SC].[name] AND [SC].[object_id] = OBJECT_ID(N'XLIST_4')");
        }

        [Test]
        public void Add_Clr_Type_Column_Test()
        {
            var statement = SchemaWriter.CreateAddColumnStatement(TableName, 
                new Column("Created", typeof(DateTime), isNullable: false, isAutoGenerated: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("ALTER TABLE [some_table_in_the_db] ADD [Created] datetime NOT NULL DEFAULT GETDATE()");
        }

        [Test]
        public void Add_Sql_Type_Column_Test()
        {
            var statement = SchemaWriter.CreateAddColumnStatement(TableName,
                new Column("Created", sqlType:SqlDbType.DateTime, isNullable: false, isAutoGenerated: true));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("ALTER TABLE [some_table_in_the_db] ADD [Created] datetime NOT NULL DEFAULT GETDATE()");
        }

        [Test]
        public void Drop_Column_Test()
        {
            var statement = SchemaWriter.CreateRemoveColumnStatement(TableName, "Created");
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("IF EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'some_table_in_the_db') AND [name] = 'Created') ALTER TABLE [some_table_in_the_db] DROP COLUMN [Created]");
        }

        [Test]
        public void Create_Non_Clustered_Index_Test()
        {
            var statement = SchemaWriter.CreateAddNonClusteredIndexStatement(TableName, new Index.Column("Created"), new Index.Column("Id"));
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("CREATE NONCLUSTERED INDEX [IX_some_table_in_the_db_Created_Id] ON [some_table_in_the_db] ([Created] ASC, [Id] ASC)");
        }

        [Test]
        public void Drop_Index_Test()
        {
            var statement = SchemaWriter.CreateRemoveNonClusteredIndexStatement(TableName, "Created_Index");
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Text.ShouldEqual("IF EXISTS (SELECT * FROM [sys].[indexes] WHERE [object_id] = OBJECT_ID(N'some_table_in_the_db') AND [name] = 'Created_Index') DROP INDEX [Created_Index] ON [some_table_in_the_db]");
        }
    }
}
