using BancaWeb.Models.ViewModels;
using BancaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancaWeb.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(ApiService apiService, ILogger<ConfigurationController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var config = await _apiService.GetAsync<ConfigurationViewModel>("api/configuration");
                return View(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la configuración");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(ConfigurationViewModel model)
        {
            try
            {
                await _apiService.PutAsync("api/configuration", model);
                TempData["Success"] = "Configuración actualizada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la configuración");
                ModelState.AddModelError("", "Error al actualizar la configuración");
                return View("Index", model);
            }
        }
    }
}