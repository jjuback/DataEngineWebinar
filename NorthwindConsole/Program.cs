using System;
using System.Data.SqlClient;
using C1.DataEngine;

namespace NorthwindConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string Invoices = "Invoices";
            const string Products = "Products";
            const string CountryTotal = "CountryTotal";
            const string TofuSalesByCountry = "TofuSalesByCountry";
            const string YearlyTotal = "YearlyTotal";
            const string CategoryTotal = "CategoryTotal";            

            Workspace workspace = new Workspace();
            workspace.Init("workspace");
            workspace.Clear();

            if (!workspace.TableExists(Invoices))
			{
				// change connection string for your environment, if necessary
                SqlConnection conn = new SqlConnection("Server=localhost;Database=NORTHWND;Trusted_Connection=True");
                conn.Open();
                var command = new SqlCommand("select * from Invoices", conn);

                var connector = new DbConnector(workspace, conn, command);
                connector.GetData(Invoices);
                workspace.Save();

                Console.WriteLine("{0}: {1} rows imported.", Invoices, workspace.GetTableData(Invoices).Count);
            }

            if (!workspace.TableExists(Products))
            {
				// change connection string for your environment, if necessary
                SqlConnection conn = new SqlConnection("Server=localhost;Database=NORTHWND;Trusted_Connection=True");
                conn.Open();
                var command = new SqlCommand("select Products.*, CategoryName from Products join Categories on Products.CategoryID = Categories.CategoryID", conn);

                var connector = new DbConnector(workspace, conn, command);
                connector.GetData(Products);
                workspace.Save();

                Console.WriteLine("{0}: {1} rows imported.", Products, workspace.GetTableData(Products).Count);
            }

            if (!workspace.QueryExists(CountryTotal))
            {
                dynamic invoices = workspace.table(Invoices);
                
                dynamic query = workspace.query(CountryTotal, new {
                    invoices.Country,
                    Total = Op.Sum(invoices.ExtendedPrice)
                });
                
                query.Query.Execute();
                var data = workspace.GetQueryData(CountryTotal);
                DataList.Sort(data, "Total", false);
                Console.WriteLine();
                DataList.Write(data, Console.Out);
            }

            if (!workspace.QueryExists(TofuSalesByCountry))
            {
                dynamic invoices = workspace.table(Invoices);
                
                dynamic query = workspace.query(TofuSalesByCountry, new {
                    _filter = invoices.ProductName.Cnt("Tofu"),
                    invoices.ProductName,
                    invoices.Country,
                    Total = Op.Sum(invoices.ExtendedPrice)
                });
                
                query.Query.Execute();
                var data = workspace.GetQueryData(TofuSalesByCountry);
                Console.WriteLine();
                DataList.Write(data, Console.Out);
            }

            if (!workspace.QueryExists(YearlyTotal))
            {
                dynamic invoices = workspace.table(Invoices);

                dynamic parent = workspace.query(new {
                    _base = "*",
                    Year = Op.DtPart(invoices.OrderDate, DateTimeParts.Year)
                });

                dynamic query = workspace.query(YearlyTotal, new {
                    parent.Year,
                    Total = Op.Sum(parent.ExtendedPrice)
                });

                query.Query.Execute();
                var data = workspace.GetQueryData(YearlyTotal);
                Console.WriteLine();
                DataList.Write(data, Console.Out);
            }

            if (!workspace.QueryExists(CategoryTotal))
            {
                dynamic invoices = workspace.table(Invoices);
                dynamic products = workspace.table(Products);

                dynamic join = workspace.join(invoices, new {
                    products = products.CategoryName | invoices.ProductID == products.ProductID
                });

                dynamic query = workspace.query(CategoryTotal, new {
                    join.CategoryName,
                    Total = Op.Sum(join.ExtendedPrice)
                });
 
                query.Query.Execute();
                var data = workspace.GetQueryData(CategoryTotal);
                Console.WriteLine();
                DataList.Write(data, Console.Out);
            }
        }
    }
}
