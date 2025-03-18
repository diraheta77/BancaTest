using BancaWeb.Models.ViewModels;
using BancaWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancaWeb.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ApiService apiService, ILogger<TransactionController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int creditCardId = 1)
        {
            try
            {
                var transactions = await _apiService.GetAsync<List<TransactionViewModel>>($"api/creditcards/{creditCardId}/transactions");
                if (transactions == null)
                    transactions = new List<TransactionViewModel>();

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las transacciones");
                TempData["Error"] = "Error al obtener las transacciones";
                return RedirectToAction("Index", "CreditCard");
            }
        }
        public IActionResult Purchase(int creditCardId)
        {
            return View(new TransactionViewModel
            {
                Date = DateTime.Now,
                Type = TransactionType.Purchase,
                CreditCardId = creditCardId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(TransactionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _apiService.PostAsync("api/transactions/purchase", model);
                TempData["Success"] = "Compra registrada exitosamente";
                return RedirectToAction("Index", "CreditCard", new { id = model.CreditCardId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la compra");
                ModelState.AddModelError("", "Error al registrar la compra");
                return View(model);
            }
        }

        public IActionResult Payment(int creditCardId)
        {
            return View(new TransactionViewModel
            {
                Date = DateTime.Now,
                Type = TransactionType.Payment,
                CreditCardId = creditCardId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Payment(TransactionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _apiService.PostAsync("api/transactions/payment", model);
                TempData["Success"] = "Pago registrado exitosamente";
                return RedirectToAction("Index", "CreditCard", new { id = model.CreditCardId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el pago");
                ModelState.AddModelError("", "Error al registrar el pago");
                return View(model);
            }
        }
    }
}