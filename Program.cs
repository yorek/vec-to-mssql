using System;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using DotNetEnv;
using System.ComponentModel;

internal class Program
{
    private static readonly string _connectionString;

    static Program()
    {
        Env.Load();

        _connectionString = Environment.GetEnvironmentVariable("MSSQL") ?? "Server=localhost,31433;Database=sift;TrustServerCertificate=True;Integrated Security=True;";
    }

    private static void Main(string[] args)
    {
        string test = "sift";
        string path = @"C:\temp\";

        LoadFiles(path, test);
    }

    private static void LoadFiles(string path, string test)
    {   
        LoadFile(LoadFVECS, Path.Combine(path,test,$"{test}_query.fvecs"), $"{test}_query", num:10000, dim:128);             
        LoadFile(LoadIVECS, Path.Combine(path,test,$"{test}_groundtruth.ivecs"), $"{test}_groundtruth", num:10000, dim:100);
        LoadFile(LoadFVECS, Path.Combine(path,test,$"{test}_base.fvecs"), $"{test}_base", num:1000000, dim:128);        
    }

    private static void LoadFile(Func<BinaryReader, int, string> FileLoader, string file, string tableName, int num, int dim)
    {
        SqlConnectionStringBuilder builder = new(_connectionString);
        Console.WriteLine($"Loading to server: {builder.DataSource}, database: {builder.InitialCatalog}, table: {tableName}");       

        Console.WriteLine($"Reading file {file}...");
        Console.WriteLine($"num: {num}, dim: {dim}");

        Console.WriteLine($"Connecting...");

        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        
        Console.WriteLine($"Truncating table...");
        using var cmd = new SqlCommand($"TRUNCATE TABLE {tableName}", conn);
        cmd.ExecuteNonQuery();

        using var bulk = new SqlBulkCopy(conn);
        bulk.DestinationTableName = tableName;
        bulk.ColumnOrderHints.Add("id", SortOrder.Ascending);

        var dt = CreateSiftDataTable();

        using FileStream fs = new(file, FileMode.Open);
        using BinaryReader br = new(fs);
        for (int i = 0; i < num; i++)
        {
            var d = br.ReadInt32();

            if (d != dim)
                throw new Exception($"Dimension mismatch: {d} != {dim}");

            var data = FileLoader(br, dim);

            dt.Rows.Add(i, data);

            if (i > 0 && i % 10000 == 0)
            {
                Console.WriteLine($"Writing {i} rows...");
                bulk.WriteToServer(dt);
                dt.Clear();
            }
        }
        if (dt.Rows.Count > 0)
        {
            Console.WriteLine($"Writing {dt.Rows.Count} rows...");
            bulk.WriteToServer(dt);
            dt.Clear();
        }
        fs.Close();

        conn.Close();

        Console.WriteLine("Done!");
    }

    private static DataTable CreateSiftDataTable()
    {
        DataTable dt = new();

        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("vector", typeof(string));

        return dt;      
    }

    private static string LoadFVECS(BinaryReader br, int dim)
    {
        var data = new float[dim];
        
        for (int j = 0; j < dim; j++)
        {
            data[j] = br.ReadSingle();
        }

        return "[" + string.Join(",", data) + "]";        
    }

    private static string LoadIVECS(BinaryReader br, int dim)
    {
        var data = new int[dim];

        for (int j = 0; j < dim; j++)
        {
            data[j] = br.ReadInt32();

        }
        return "[" + string.Join(",", data) + "]";        
    }
}