using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TuProyecto.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StockController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetStock()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        IW_vsnpMovimStockTipoBod.CodProd AS CodigoProducto,
                        DesProd AS DescripcionProducto,
                        SUM(CASE WHEN CodBode = '18' THEN Ingresos - Egresos ELSE 0 END) AS Bodega18,
                        SUM(CASE WHEN CodBode = '02' THEN Ingresos - Egresos ELSE 0 END) AS Bodega02,
                        SUM((CASE WHEN CodBode = '18' THEN Ingresos - Egresos ELSE 0 END) + (CASE WHEN CodBode = '02' THEN Ingresos - Egresos ELSE 0 END)) AS StockTotal
                    FROM softland.IW_vsnpMovimStockTipoBod
                    INNER JOIN SOFTLAND.iw_tprod ON iw_tprod.CodProd = IW_vsnpMovimStockTipoBod.CodProd
                    GROUP BY IW_vsnpMovimStockTipoBod.CodProd, DesProd, TipoBod
                    HAVING SUM((CASE WHEN CodBode = '18' THEN Ingresos - Egresos ELSE 0 END) + (CASE WHEN CodBode = '02' THEN Ingresos - Egresos ELSE 0 END)) > 0 AND TipoBod = 'D'
                    ORDER BY StockTotal DESC;
                ";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<Stock>();

                    while (reader.Read())
                    {
                        var stock = new Stock
                        {
                            CodigoProducto = GetNullableString(reader, "CodigoProducto"),
                            DescripcionProducto = GetNullableString(reader, "DescripcionProducto"),
                            Bodega18 = GetNullableDecimal(reader, "Bodega18"),
                            Bodega02 = GetNullableDecimal(reader, "Bodega02"),
                            StockTotal = GetNullableDecimal(reader, "StockTotal"),
                        };

                        results.Add(stock);
                    }

                    return Ok(results);
                }
            }
        }

        private string GetNullableString(SqlDataReader reader, string columnName)
        {
            return reader[columnName] != DBNull.Value ? reader[columnName].ToString() : null;
        }

        private decimal GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            return reader[columnName] != DBNull.Value ? Convert.ToDecimal(reader[columnName]) : 0;
        }
    }
}
