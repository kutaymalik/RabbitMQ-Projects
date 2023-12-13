using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQ.ExcelCreate.Hubs;
using UdemyRabbitMQ.ExcelCreate.Models;

namespace UdemyRabbitMQ.ExcelCreate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly AppDbContext dbContext;
    private readonly IHubContext<MyHub> hubContext;

    public FilesController(AppDbContext dbContext, IHubContext<MyHub> hubContext)
    {
        this.dbContext = dbContext;
        this.hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, int fileId)
    {
        if(file is not { Length: >0 }) return BadRequest();

        var userFile = await dbContext.UserFiles.FirstAsync(x => x.Id == fileId);

        var filePath = userFile.FileName + Path.GetExtension(file.FileName);

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

        using FileStream stream = new(path, FileMode.Create);

        await file.CopyToAsync(stream);

        userFile.CreatedDate = DateTime.Now;

        userFile.FilePath = filePath;

        userFile.FileStatus = FileStatus.Completed;

        await dbContext.SaveChangesAsync();

        //SignalR notification will create
        await hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

        return Ok();
    }
}
