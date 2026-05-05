using FixConnect.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly WorkerService _workerService;

    public CustomerController(WorkerService workerService)
    {
        _workerService = workerService;
    }

    // GET: /Customer/WorkerProfile/5
    [HttpGet]
    public IActionResult WorkerProfile(int id)
    {
        var vm = _workerService.GetPublicProfile(id);
        if (vm == null) return NotFound();
        return View(vm);
    }
}