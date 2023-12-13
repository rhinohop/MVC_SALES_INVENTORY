using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLogin.Models;
using ProyectoLogin.Datos;
using System.Diagnostics;
using System.Security.Claims;
using System.Xml.Linq;

namespace ProyectoLogin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    

    {
        DA_Producto _daProducto = new DA_Producto();
        DA_Venta _daVenta = new DA_Venta();

      

        public IActionResult DetalleVenta()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public JsonResult AutoCompleteProducto(string search)
        {
            List<Autocomplete> autocomplete = new List<Autocomplete>();
            autocomplete = _daProducto.Listar()
                .Where(x => string.Concat(x.Codigo.ToUpper(), x.oCategoria.Descripcion.ToUpper(), x.Descripcion.ToUpper()).Contains(search.ToUpper()))
                .Select(m => new Autocomplete
                {
                    label = $"{m.Codigo} - {m.oCategoria.Descripcion} - {m.Descripcion}",
                    value = m.IdProducto
                }
                ).ToList();

            return Json(autocomplete);
        }

        [HttpGet]
        public JsonResult ObtenerProducto(int idproducto)
        {
            Producto? oProducto = new Producto();
            oProducto = _daProducto.Listar().Where(x => x.IdProducto == idproducto).FirstOrDefault();
            return Json(oProducto);
        }

        [HttpPost]
        public JsonResult RegistrarVenta([FromBody] Venta body)
        {

            string rpta = "";

            XElement venta = new XElement("Venta",
                new XElement("TipoPago", body.TipoPago),
                new XElement("NumeroDocumento", "0"),
                new XElement("DocumentoCliente", body.DocumentoCliente),
                new XElement("NombreCliente", body.NombreCliente),
                new XElement("MontoPagoCon", body.MontoPagoCon),
                new XElement("MontoCambio", body.MontoCambio),
                new XElement("MontoSubTotal", body.MontoSubTotal),
                new XElement("MontoIGV", body.MontoIGV),
                new XElement("MontoTotal", body.MontoTotal)
            );
            XElement oDetalleVenta = new XElement("Detalle_Venta");
            foreach (Detalle_Venta item in body.oDetalleVenta)
            {
                oDetalleVenta.Add(new XElement("Item",
                    new XElement("IdProducto", item.oProducto.IdProducto),
                    new XElement("PrecioVenta", item.PrecioVenta),
                    new XElement("Cantidad", item.Cantidad),
                    new XElement("Total", item.Total)
                    ));
            }

            venta.Add(oDetalleVenta);

            rpta = _daVenta.Registrar(venta.ToString());

            return Json(new { respuesta = rpta });
        }

        [HttpGet]
        public JsonResult ObtenerVenta(string nrodocumento)
        {
            Venta? oVenta = new Venta();
            oVenta = _daVenta.Detalle(nrodocumento);
            return Json(oVenta);
        }


        private readonly ILogger<HomeController> _logger;

      
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            string nombreUsuario = "";

            if (claimuser.Identity.IsAuthenticated) {
                nombreUsuario  = claimuser.Claims.Where(c=> c.Type == ClaimTypes.Name)
                    .Select(c=>c.Value).SingleOrDefault();
            }

            ViewData["nombreUsuario"] = nombreUsuario;

            return View();
        }

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task< IActionResult >CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("IniciarSesion", "Inicio");
        }
    }
}