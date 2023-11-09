using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PharmatechAPI.Controllers
{
    [Route("api/resumenventas")]
    [ApiController]
    public class ResumenVentasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ResumenVentasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetResumenVentas()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT  
                        Folio,
                        ISNULL(iw_gsaen.CodAux, 'Sin cliente asociado') AS CodigoCliente,
                        ISNULL(cwtauxi.RutAux, 'Sin RuT asociado') AS RutCliente,
                        ISNULL(cwtauxi.NomAux, 'Sin cliente asociado') AS NombreCliente,
                        CodBode,
                        Concepto,
                        Fecha,
                        iw_gsaen.Proceso AS TipoDocumento,
                        CONVERT(INT, Total) AS TotalDoc
                    FROM softland.iw_gsaen
                    LEFT JOIN softland.cwtauxi ON cwtauxi.CodAux = iw_gsaen.CodAux
                    WHERE Fecha BETWEEN '2023-01-01' AND GETDATE()
                        AND Tipo IN ('F', 'B', 'N', 'D')
                    ORDER BY Fecha DESC;
                ";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<ResumenVenta>();

                    while (reader.Read())
                    {
                        var resumenVenta = new ResumenVenta
                        {
                            Folio = GetNullableString(reader, "Folio"),
                            CodigoCliente = GetNullableString(reader, "CodigoCliente"),
                            RutCliente = GetNullableString(reader, "RutCliente"),
                            NombreCliente = GetNullableString(reader, "NombreCliente"),
                            CodBode = GetNullableString(reader, "CodBode"),
                            Concepto = GetNullableString(reader, "Concepto"),
                            Fecha = GetNullableDateTime(reader, "Fecha"),
                            TipoDocumento = GetNullableString(reader, "TipoDocumento"),
                            TotalDoc = GetNullableInt(reader, "TotalDoc"),
                        };

                        results.Add(resumenVenta);
                    }

                    return Ok(results);
                }
            }
        }

        private string GetNullableString(SqlDataReader reader, string columnName)
        {
            return reader?[columnName] != DBNull.Value ? reader[columnName].ToString() : null;
        }

        private int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            return reader[columnName] != DBNull.Value ? (int?)Convert.ToInt32(reader[columnName]) : null;
        }

        private DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            return reader[columnName] != DBNull.Value ? (DateTime?)reader.GetDateTime(reader.GetOrdinal(columnName)) : null;
        }
    }
}
