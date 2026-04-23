using Project_X_Data.Data;
using Project_X_Data.Models.Rest;
using Project_X_Data.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Project_X_Data.Data;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(IStorageService storageService, DataContext dataContext, DataAccessor dataAccessor) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;
        //[HttpGet]
        //public RestResponse AllGroups()
        //{
        //    return new()
        //    {
        //        Meta = new()
        //        {
        //            Manipulations = ["GET", "POST"],
        //            Cache = 24 * 60 * 60,
        //            Service = "Shop API: product groups",
        //            DataType = "json/array"
        //        },
        //        Data = _dataAccessor.GetProductGroups()
        //    };
        //}
        //[HttpPost]
        //public object AddGroup(AdminGroupFormModel model)
        //{

        //    if (string.IsNullOrWhiteSpace(model.Name))
        //        return new { status = "Invalid Name", code = 400 };

        //    if (string.IsNullOrWhiteSpace(model.Slug))
        //        return new { status = "Invalid Slug", code = 400 };

        //    if (_dataContext.ProductGroups.Any(g => g.Slug == model.Slug))
        //        return new { status = "Slug already exists", code = 400 };

        //    string? imageUrl = null;
        //    if (model.Image != null)
        //    {
        //        try
        //        {
        //            imageUrl = _storageService.Save(model.Image);
        //        }
        //        catch
        //        {
        //            return new { status = "Invalid Image", code = 400 };
        //        }
        //    }

        //    _dataContext.ProductGroups.Add(new Data.Entities.ProductGroup()
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = model.Name,
        //        Description = model.Description,
        //        Slug = model.Slug,
        //        ImageUrl = _storageService.Save(model.Image)
        //    });
        //    try
        //    {
        //        _dataContext.SaveChanges();
        //        return new { status = "OK", code = 200 };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { status = ex.Message, code = 500 };
        //    }
        //}
    }
}
