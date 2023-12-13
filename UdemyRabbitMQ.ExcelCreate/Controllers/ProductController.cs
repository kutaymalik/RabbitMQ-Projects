using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQ.ExcelCreate.Models;
using UdemyRabbitMQ.ExcelCreate.Services;

namespace UdemyRabbitMQ.ExcelCreate.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;
    private readonly RabbitMQPublisher rabbitMQPublisher;

    public ProductController(AppDbContext dbContext, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.rabbitMQPublisher = rabbitMQPublisher;
    }

    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> CreateProductExcel()
    {
        var user = await userManager.FindByNameAsync(User.Identity.Name);

        var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

        UserFile userFile = new()
        {
            UserId = user.Id,
            FileName = fileName,
            FileStatus = FileStatus.Creating,
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files")
        };

        await dbContext.UserFiles.AddAsync(userFile);

        await dbContext.SaveChangesAsync();

        // Send message to RabbitMQ Queue
        rabbitMQPublisher.Publish(new Shared.CreateExcelMessage()
        {
            FileId = userFile.Id, 
        });

        TempData["StartCreatingExcel"] = true;

        return RedirectToAction(nameof(Files));
    }

    public async Task<IActionResult> Files()
    {
        var user = await userManager.FindByNameAsync(User.Identity.Name);



        return View(await dbContext.UserFiles.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).ToListAsync());
    }
}
