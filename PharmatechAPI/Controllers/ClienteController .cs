using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PharmatechAPI.Controllers
{
    [Route("api/cliente")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ClienteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetClientes()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        CodAux AS CodigoCliente,
                        NomAux AS NombreCliente,
                        RutAux AS RutCliente,
                        ISNULL(cwtgiro.GirDes,'Sin Giro') AS Giro,
                        ISNULL(PaiAux,'S/P') AS Pais,
                        ISNULL(cwtregion.Descripcion,'Sin Region') AS Region,
                        ISNULL(cwtciud.CiuDes,'Sin Ciudad') AS Ciudad,
                        ISNULL(cwtcomu.ComDes,'Sin Comuna') AS Comuna,
                        ISNULL(DirAux,'Sin Direccion') AS Direccion,
                        ISNULL(DirNum,'Sin Numero') AS NumeracionDireccion,
                        ISNULL(eMailDTE,'Sin Email') AS EmailDTE,  
                        ISNULL(EMail,'Sin Email') AS Email
                    FROM softland.cwtauxi  
                    LEFT JOIN softland.cwtgiro ON cwtgiro.GirCod = cwtauxi.GirAux
                    LEFT JOIN softland.cwtciud ON cwtciud.CiuCod = cwtauxi.CiuAux
                    LEFT JOIN softland.cwtregion ON cwtregion.id_Region = cwtauxi.Region
                    LEFT JOIN softland.cwtcomu ON cwtcomu.ComCod = cwtauxi.ComAux
                    WHERE ActAux = 'S' and ClaCli = 'S'";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<Cliente>();

                    while (reader.Read())
                    {
                        var cliente = new Cliente
                        {
                            CodigoCliente = reader["CodigoCliente"].ToString(),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            RutCliente = reader["RutCliente"].ToString(),
                            Giro = reader["Giro"].ToString(),
                            Pais = reader["Pais"].ToString(),
                            Region = reader["Region"].ToString(),
                            Ciudad = reader["Ciudad"].ToString(),
                            Comuna = reader["Comuna"].ToString(),
                            Direccion = reader["Direccion"].ToString(),
                            NumeracionDireccion = reader["NumeracionDireccion"].ToString(),
                            EmailDTE = reader["EmailDTE"].ToString(),
                            Email = reader["Email"].ToString()
                        };

                        results.Add(cliente);
                    }

                    return Ok(results);
                }
            }
        }
    }
}
