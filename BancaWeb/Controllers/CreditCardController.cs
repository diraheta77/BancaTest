using BancaWeb.Models.ViewModels;
using BancaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancaWeb.Controllers
{
    public class CreditCardController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CreditCardController> _logger;

        public CreditCardController(ApiService apiService, ILogger<CreditCardController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int id = 1)
        {
            try
            {
                var creditCard = await _apiService.GetAsync<CreditCardViewModel>($"api/creditcards/{id}");
                if (creditCard == null)
                    return NotFound();

                return View(creditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la tarjeta de crédito");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Statement(int id = 1)
        {
            try
            {
                var statement = await _apiService.GetAsync<CreditCardViewModel>($"api/creditcards/{id}/full-statement");
                if (statement == null)
                    return NotFound();

                return View(statement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de cuenta");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Transactions(int id = 1)
        {
            try
            {
                var transactions = await _apiService.GetAsync<List<TransactionViewModel>>($"api/creditcards/{id}/transactions");
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las transacciones");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> ExportPdf(int id = 1)
        {
            try
            {
                var pdfBytes = await _apiService.GetBytesAsync($"api/creditcards/{id}/statement/pdf");
                return File(pdfBytes, "application/pdf", $"estado-cuenta-{id}-{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar el PDF");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}